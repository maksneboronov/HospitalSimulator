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
			Patients = _personFactory.CreatePatients(50, 200);
			WaitingPatients = _personFactory.CreatePatients(10, 20);
			Doctors = _personFactory.CreateDoctors(5, 10);

			SynchronizationContext.SetSynchronizationContext(SynchronizationContext.Current);

			var t = Task.Factory.StartNew(CreatePatiens, new CancellationToken(false), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
		}
		private async void CreatePatiens()
		{
			while (Patients.Count > 1)
			{
				Patients.RemoveAt(0);

				await Task.Delay(100);
			}
		}

		private readonly IPersonFactory _personFactory = new PersonFactory();
    }
}
