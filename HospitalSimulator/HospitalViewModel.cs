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

namespace HospitalSimulator
{
	internal sealed class HospitalViewModel : NotifyPropertyChanged
	{
		public ObservableCollection<PatientViewModel> Patients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<PatientViewModel> WaitingPatients { get; set; } = new ObservableCollection<PatientViewModel>();
		public ObservableCollection<DoctorViewModel> Doctors { get; set; } = new ObservableCollection<DoctorViewModel>();

		public HospitalViewModel()
		{
			Doctors = _personFactory.CreateDoctors(_maxDoctotsNum, _maxDoctotsNum);

			SynchronizationContext.SetSynchronizationContext(SynchronizationContext.Current);

			_docs = new SemaphoreSlim(0, _maxWaitingPatientsNum);
			foreach (var doc in Doctors)
			{
				var t = Task.Factory.StartNew(DoctorsWorking, doc, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
			}
			Task.Factory.StartNew(CreatePatients, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
			Task.Factory.StartNew(PatientsToWaiting, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

		}

		private async void CreatePatients()
		{
			while (true)
			{
				await Task.Delay(_rand.Next(1000, 2000));

				lock (_sync)
				{
					Patients.Add(_personFactory.CreateRandomPatient());

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
					WaitingPatients.Add(Patients[0]);
					Patients.RemoveAt(0);
				}
				if (_docs.CurrentCount < _maxDoctotsNum)
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
				await Task.Delay(_rand.Next(10000, 20000));
			}
		}

		private int _maxDoctotsNum = 5;
		private int _maxWaitingPatientsNum = 10;

		private SemaphoreSlim _docs;
		private SemaphoreSlim _pat = new SemaphoreSlim(0, 1);
		private object _sync = new object();

		private Random _rand = new Random();
		private readonly IPersonFactory _personFactory = new PersonFactory();
	}
}
