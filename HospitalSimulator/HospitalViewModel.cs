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

namespace HospitalSimulator
{
	public class PCTokenSource
	{
		public PauseOrCancelToken Token => new PauseOrCancelToken(_cts, _pts);
		public CancellationToken CancellationToken => _cts.Token;

		public void Pause()
		{
			_pts.IsPaused = true;
		}

		public void Resume()
		{
			_pts.IsPaused = false;
		}

		public void Cancel()
		{
			_cts.Cancel();
		}

		private PauseTokenSource _pts = new PauseTokenSource();
		private CancellationTokenSource _cts = new CancellationTokenSource();
	}

	public class PauseTokenSource
	{
		public bool IsPaused
		{
			get => m_paused != null;
			set
			{
				if (value)
				{
					Interlocked.CompareExchange(
						ref m_paused, new TaskCompletionSource<bool>(), null);
				}
				else
				{
					while (true)
					{
						var tcs = m_paused;
						if (tcs == null)
						{
							return;
						}

						if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
						{
							tcs.SetResult(true);
							break;
						}
					}
				}
			}
		}

		public PauseToken Token => new PauseToken(this);

		public Task WaitWhilePausedAsync()
		{
			var cur = m_paused;
			return cur != null ? cur.Task : s_completedTask;
		}

		private volatile TaskCompletionSource<bool> m_paused;
		internal static readonly Task s_completedTask = Task.FromResult(true);
	}

	public struct PauseToken
	{
		public PauseToken(PauseTokenSource source) => m_source = source;

		public bool IsPaused => m_source != null && m_source.IsPaused;

		public Task WaitWhilePausedAsync() => IsPaused ?
				m_source.WaitWhilePausedAsync() :
				PauseTokenSource.s_completedTask;

		private readonly PauseTokenSource m_source;


	}

	public class PauseOrCancelToken
	{
		public CancellationToken CancellationToken => _ct;
		public PauseToken PauseToken => _pt;

		public PauseOrCancelToken(CancellationTokenSource ct, PauseTokenSource pt)
		{
			_ct = ct.Token;
			_pt = pt.Token;
		}

		public async Task PauseIfRequest()
		{
			await _pt.WaitWhilePausedAsync();
		}

		public void CancelIfRequest()
		{
			_ct.ThrowIfCancellationRequested();
		}

		public async Task PauseOrCancelIfRequest()
		{
			await _pt.WaitWhilePausedAsync();
			_ct.ThrowIfCancellationRequested();
		}

		private CancellationToken _ct;
		private PauseToken _pt;

	}

	internal sealed class HospitalViewModel : NotifyPropertyChanged
	{
		public ObservableCollection<PatientViewModel> Patients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<PatientViewModel> WaitingPatients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<DoctorViewModel> Doctors { get; set; } = new ObservableCollection<DoctorViewModel>();

		public ICommand PauseCommand { get; }
		public ICommand ResumeCommand { get; }

		public HospitalViewModel()
		{
			_uiserv.OpenOptionWindow(ref _maxDoctorsNum, ref _maxWaitingPatientsNum, ref _infectionInterval, ref _generationInterval, ref _receptionInterval);
			
			var cf = new RelayCommandFactory();
			PauseCommand = cf.CreateCommand(Pause);
			ResumeCommand = cf.CreateCommand(Resume);

			_illTimer = new TimerWrapper(TimeSpan.FromSeconds(1), PatientsIll);

			Start();

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
				await pt.PauseOrCancelIfRequest();
				await Task.Delay(_rand.NextRandomSecond(1, _generationInterval));
				await pt.PauseOrCancelIfRequest();

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
				await pt.PauseOrCancelIfRequest();
				await _pat.WaitAsync();
				await pt.PauseOrCancelIfRequest();

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
				await pt.PauseOrCancelIfRequest();
				await _docs.WaitAsync();
				await pt.PauseOrCancelIfRequest();

				doc.Status = DoctorStatus.Work;
				WaitingPatients.RemoveAt(0);
				if (WaitingPatients.Count == 0)
				{
					_lookoutState = LookoutState.Nobody;
				}

				await pt.PauseOrCancelIfRequest();
				await Task.Delay(_rand.NextRandomSecond(1, _receptionInterval));
				await pt.PauseOrCancelIfRequest();

				if (_rand.Next(10) < 3)
				{
					doc.Status = DoctorStatus.NotWork;

					await pt.PauseOrCancelIfRequest();
					await Task.Delay(_rand.NextRandomSecond(1, _receptionInterval));
					await pt.PauseOrCancelIfRequest();
				}

				doc.Status = DoctorStatus.Wait;
				await Task.Delay(100);
			}
		}

		private void Start()
		{
			Doctors = _personFactory.CreateDoctors(_maxDoctorsNum, _maxDoctorsNum);

			_docs = new SemaphoreSlim(0, _maxWaitingPatientsNum);
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
			_pcts.Resume();
			_illTimer.Start();
		}

		private void Stop()
		{
			//_pcts.Cancel();
			//_illTimer.Stop();
		}

		private void Pause()
		{
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
		private SemaphoreSlim _pat = new SemaphoreSlim(0, 1);
		private object _sync = new object();
		private LookoutState _lookoutState = LookoutState.Nobody;
		private PCTokenSource _pcts = new PCTokenSource();

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
	}
}
