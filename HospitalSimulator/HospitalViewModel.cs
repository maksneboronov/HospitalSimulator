using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVM.Core;
using HospitalSimulator.Models;
using HospitalSimulator.ViewModels;
using HospitalSimulator.Services;
using System.Windows.Input;
using System.Collections.ObjectModel;
using HospitalSimulator.Services.Interfaces;
using System.Threading;
using System;
using HospitalSimulator.TPLAdvanced;

namespace HospitalSimulator
{
	internal sealed class HospitalViewModel : NotifyPropertyChanged
	{
		public ObservableCollection<PatientViewModel> Patients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<PatientViewModel> WaitingPatients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<DoctorViewModel> Doctors { get; set; } = new ObservableCollection<DoctorViewModel>();

		public ICommand PauseCommand { get; }
		public ICommand ResumeCommand { get; }
		public ICommand StopCommand { get; }
		public ICommand StartCommand { get; }
		public ICommand OptionCommand { get; }

		public WorkState WorkStatus { get => _workState; set => this.UpdateValue(value, ref _workState); }

		public HospitalViewModel()
		{
			var cf = new RelayCommandFactory();
			PauseCommand = cf.CreateCommand(Pause);
			ResumeCommand = cf.CreateCommand(Resume);
			StartCommand = cf.CreateCommand(Start);
			StopCommand = cf.CreateCommand(Stop);
			OptionCommand = cf.CreateCommand(OptionWindowOpen);

			//OptionCommand.Execute(null);

			_illTimer = new TimerWrapper(TimeSpan.FromSeconds(1), PatientsIll);

			//Start();

		}

		private void PatientsIll()
		{
			if (Patients.Count == 0)
			{
				_illTimer.Stop();
				return;
			}

			if (Patients.All(i => i.Status == PatientStatus.Sick))
			{
				lock (_sync)
				{
					if (Patients.All(i => i.Status == PatientStatus.Sick))
					{
						var keysP = _infection.Keys.ToList();
						foreach (var k in keysP)
						{
							_infection[k] = 0;
						}
						return;
					}
				}
			}

			var keys = _infection.Keys.ToList();
			foreach (var inf in keys)
			{
				_infection[inf]++;

				if (_infection[inf] == _infectionInterval)
				{
					lock (_sync)
					{
						var keysP = _infection.Keys.ToList();
						foreach (var k in keysP)
						{
							_infection[k] = 0;
						}

						foreach (var p in Patients)
						{
							p.Status = PatientStatus.Sick;
						}
					}
					break;
				}
			}
		}

		// Вариант 1 (действующий): семафорслим, но тогда пациенты заходят в смотровую только при генерации пациента 
		//		(при большом периоде генерации никто не зайдет, но позже зайдет та группа, которая не заходила, но это не точно)
		// Вариант 2: убрать семафоры, пусть лучше крутится отдельный поток, который будет проверять, надо ли зайти пациенту, но он будет куртится вечно
		private async void CreatePatients(PauseOrCancelToken pt)
		{
			while (true)
			{
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}
				await Task.Delay(_rand.NextRandomSecond(1, _generationInterval));
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}

				lock (_sync)
				{
					var newPatient = _personFactory.CreateRandomPatient();
					Patients.Add(newPatient);
					if (newPatient.Status == PatientStatus.Sick)
					{
						_infection[newPatient] = 0;
					}
					if (WaitingPatients.Count < _maxWaitingPatientsNum)
					{
						_pat.Release();
					}
					_illTimer.Start();
				}
			}
		}

		private async void PatientsToWaiting(PauseOrCancelToken pt)
		{
			while (true)
			{
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}
				await _pat.WaitAsync();
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}

				lock (_sync)
				{
					var patients = new List<PatientViewModel>();

					if (_lookoutState == LookoutState.Nobody)
					{
						_lookoutState = Patients[0].Status == PatientStatus.Healthy ? LookoutState.OnlyHealthy : LookoutState.OnlySick;
						patients = Patients.Where(i => i.Status == Patients[0].Status).ToList();
					}
					else
					{
						var illState = _lookoutState == LookoutState.OnlyHealthy ? PatientStatus.Healthy : PatientStatus.Sick;
						patients = Patients.Where(i => i.Status == illState).ToList();
					}
					var cnt = patients.Count > _maxWaitingPatientsNum ? _maxWaitingPatientsNum : patients.Count;
					cnt = cnt > _maxWaitingPatientsNum - WaitingPatients.Count ? _maxWaitingPatientsNum - WaitingPatients.Count : cnt;
					for (var i = 0; i < cnt; ++i)
					{
						WaitingPatients.Add(patients[i]);
						Patients.Remove(patients[i]);
						_infection.Remove(patients[i]);
					}

				}
				if (_docs.CurrentCount < _maxDoctorsNum)
				{
					_docs.Release();
				}
			}
		}

		private async void DoctorsWorking(DoctorViewModel doc, PauseOrCancelToken pt)
		{
			while (true)
			{
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}
				await _docs.WaitAsync();
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}

				doc.Status = DoctorStatus.Work;
				if (WaitingPatients.Count > 0)
				{
					WaitingPatients.RemoveAt(0);
				}
				else
				{
					_lookoutState = LookoutState.Nobody;
				}

				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}
				await Task.Delay(_rand.NextRandomSecond(1, _receptionInterval));
				if (await pt.PauseOrCancelIfRequest())
				{
					return;
				}

				if (_rand.Next(10) < 3)
				{
					doc.Status = DoctorStatus.NotWork;

					if (await pt.PauseOrCancelIfRequest())
					{
						return;
					}
					await Task.Delay(_rand.NextRandomSecond(1, _receptionInterval));
				}

				doc.Status = DoctorStatus.Wait;
				await Task.Delay(100);
			}
		}

		private void OptionWindowOpen()
		{
			_uiserv.OpenOptionWindow(ref _maxDoctorsNum, ref _maxWaitingPatientsNum, ref _infectionInterval, ref _generationInterval, ref _receptionInterval);
		}

		private void Start()
		{
			if (WorkStatus != WorkState.Stopped)
			{
				return;
			}

			_pcts = new PCTokenSource();
			WorkStatus = WorkState.Started;
			Doctors = _personFactory.CreateDoctors(_maxDoctorsNum, _maxDoctorsNum);
			this.RaisePropertyChanged(nameof(Doctors));

			_docs = new SemaphoreSlim(0, _maxWaitingPatientsNum);
			_pat = new SemaphoreSlim(0, 1);
			foreach (var doc in Doctors)
			{
				var t = Task.Factory.StartNew(() => DoctorsWorking(doc, _pcts.Token), _pcts.CancellationToken, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
			}
			Task.Factory.StartNew(() => CreatePatients(_pcts.Token), _pcts.CancellationToken, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
			Task.Factory.StartNew(() => PatientsToWaiting(_pcts.Token), _pcts.CancellationToken, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

			_illTimer.Start();
		}

		private void Resume()
		{
			if (WorkStatus != WorkState.Paused)
			{
				return;
			}

			WorkStatus = WorkState.Started;
			_pcts.Resume();
			_illTimer.Start();
		}

		private void Stop()
		{
			if (WorkStatus != WorkState.Started)
			{
				return;
			}
			WorkStatus = WorkState.Stopped;
			_pcts.Cancel();
			_illTimer.Stop();

			Doctors.Clear();
			WaitingPatients.Clear();
			Patients.Clear();
			_lookoutState = LookoutState.Nobody;
			_infection.Clear();
	
		}

		private void Pause()
		{
			if (WorkStatus != WorkState.Started)
			{
				return;
			}

			WorkStatus = WorkState.Paused;
			_pcts.Pause();
			_illTimer.Stop();
		}

		private int _maxDoctorsNum = 11;
		private int _maxWaitingPatientsNum = 11;
		private int _infectionInterval = 11;
		private int _receptionInterval = 11;
		private int _generationInterval = 11;

		private Dictionary<IPatient, int> _infection = new Dictionary<IPatient, int>();

		private SemaphoreSlim _docs;
		private SemaphoreSlim _pat;
		private object _sync = new object();
		private LookoutState _lookoutState = LookoutState.Nobody;
		private WorkState _workState = WorkState.Stopped;
		private PCTokenSource _pcts;

		private IUIService _uiserv = new UIService();
		private ITimerWrapper _illTimer;
		private RandomTimeService _rand = new RandomTimeService();
		private readonly IPersonFactory _personFactory = new PersonFactory();

		private enum LookoutState
		{
			OnlySick,
			OnlyHealthy,
			Nobody
		}

		public enum WorkState
		{
			Started,
			Stopped,
			Paused
		}
	}
}
