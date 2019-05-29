using System;
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
using System.Windows.Threading;

namespace HospitalSimulator
{
	internal sealed class HospitalViewModel : NotifyPropertyChanged
	{
		public ObservableCollection<PatientViewModel> Patients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<PatientViewModel> WaitingPatients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<DoctorViewModel> Doctors { get; set; } = new ObservableCollection<DoctorViewModel>();

		public HospitalViewModel()
		{
			Doctors = _personFactory.CreateDoctors(_maxDoctorsNum, _maxDoctorsNum);

			SynchronizationContext.SetSynchronizationContext(SynchronizationContext.Current);

			_docs = new SemaphoreSlim(0, _maxWaitingPatientsNum);
			foreach (var doc in Doctors)
			{
				var t = Task.Factory.StartNew(DoctorsWorking, doc, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
			}
			Task.Factory.StartNew(CreatePatients, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
			Task.Factory.StartNew(PatientsToWaiting, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

			_illTimer = new DispatcherTimer()
			{
				Interval = TimeSpan.FromSeconds(1),
				IsEnabled = true
			};

			_illTimer.Tick += (o, e) => PatientsIll();
			_illTimer.Start();
		}

		private void PatientsIll()
		{
			if (Patients.Count == 0)
			{
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

				if (_infection[inf] == 5)
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
		private async void CreatePatients()
		{
			while (true)
			{
				await Task.Delay(_rand.Next(100, 1100));

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
				}
			}
		}

		private async void PatientsToWaiting()
		{
			while (true)
			{
				await _pat.WaitAsync();

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

		private async void DoctorsWorking(object doctor)
		{
			var doc = (DoctorViewModel)doctor;
			while (true)
			{
				await _docs.WaitAsync();
				WaitingPatients.RemoveAt(0);
				if (WaitingPatients.Count == 0)
				{
					_lookoutState = LookoutState.Nobody;
				}
				await Task.Delay(_rand.Next(15000, 20000));
			}
		}

		private int _maxDoctorsNum = 5;
		private int _maxWaitingPatientsNum = 10;

		private Dictionary<IPatient, int> _infection = new Dictionary<IPatient, int>();

		private SemaphoreSlim _docs;
		private SemaphoreSlim _pat = new SemaphoreSlim(0, 1); // _maxWaitingPatients
		private object _sync = new object();
		private LookoutState _lookoutState = LookoutState.Nobody;

		private DispatcherTimer _illTimer;

		private Random _rand = new Random();
		private readonly IPersonFactory _personFactory = new PersonFactory();

		private enum LookoutState
		{
			OnlySick,
			OnlyHealthy,
			Nobody
		}
	}
}
