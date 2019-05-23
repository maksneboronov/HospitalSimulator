using System;
using HospitalSimulator.ViewModels;
using HospitalSimulator.Models;
using System.Collections.ObjectModel;
using HospitalSimulator.Services.Interfaces;

namespace HospitalSimulator.Services
{
	internal class PersonFactory : IPersonFactory
	{
		public PatientViewModel CreatePatient(String name = "NoName", PatientStatus status = PatientStatus.Healthy, PersonSex sex = PersonSex.Male)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			return new PatientViewModel()
			{
				Name = name,
				Sex = sex,
				Status = status
			};
		}

		public DoctorViewModel CreateDoctor(String name = "NoName", DoctorStatus status = DoctorStatus.NotWork, PersonSex sex = PersonSex.Male)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			return new DoctorViewModel()
			{
				Name = name,
				Sex = sex,
				Status = status
			};
		}

		public PatientViewModel CreateRandomPatient() => new PatientViewModel
		{
			Name = $"{_firstNames[_rand.Next(_firstNames.Length)]} {_lastNames[_rand.Next(_lastNames.Length)]}",
			Sex = _rand.Next(2) == 1 ? PersonSex.Female : PersonSex.Male,
			Status = _rand.Next(10) < 7 ? PatientStatus.Sick : PatientStatus.Healthy
		};

		public DoctorViewModel CreateRandomDoctor() => new DoctorViewModel
		{
			Name = $"{_firstNames[_rand.Next(_firstNames.Length)]} {_lastNames[_rand.Next(_lastNames.Length)]}",
			Sex = _rand.Next(2) == 1 ? PersonSex.Female : PersonSex.Male,
			Status = _rand.Next(10) < 7 ? DoctorStatus.Wait : DoctorStatus.NotWork
		};

		public ObservableCollection<DoctorViewModel> CreateDoctors(int min = 0, int max = 10) => CreatePersons(min, max, CreateRandomDoctor);

		public ObservableCollection<PatientViewModel> CreatePatients(int min = 0, int max = 10) => CreatePersons(min, max, CreateRandomPatient);

		private ObservableCollection<T> CreatePersons<T>(int min, int max, Func<T> createFunc) where T : PersonViewModel
		{
			var result = new ObservableCollection<T>();

			var size = _rand.Next(min, max);
			for (var i = 0; i < size; ++i)
			{
				result.Add(createFunc());
			}

			return result;
		}

		private readonly Random _rand = new Random(DateTime.Now.Millisecond);
		private readonly String[] _firstNames = new[] { "Peter", "Ivan", "Max", "Stive", "Kate", "Jane" };
		private readonly String[] _lastNames = new[] { "Parker", "Klein", "Forbes", "Silkens", "Clarkson", "Crag", "Williams" };
	}
}
