using System;
using HospitalSimulator.ViewModels;
using HospitalSimulator.Models;
using System.Collections.ObjectModel;

namespace HospitalSimulator.Services.Interfaces
{
	internal interface IPersonFactory
	{
		PatientViewModel CreatePatient(String name = "NoName", PatientStatus status = PatientStatus.Healthy, PersonSex sex = PersonSex.Male);
		DoctorViewModel CreateDoctor(String name = "NoName", DoctorStatus status = DoctorStatus.NotWork, PersonSex sex = PersonSex.Male);
		PatientViewModel CreateRandomPatient();
		DoctorViewModel CreateRandomDoctor();
		ObservableCollection<DoctorViewModel> CreateDoctors(int min = 0, int max = 10);
		ObservableCollection<PatientViewModel> CreatePatients(int min = 0, int max = 10);
	}
}
