using System;

namespace HospitalSimulator.Models
{
	internal enum PersonSex
	{
		Male,
		Female
	}

	internal interface IPerson
	{
		String Name { get; set; }
		PersonSex Sex { get; set; }
	}

	internal enum DoctorStatus
	{
		Work,
		Wait,
		NotWork
	}

	internal interface IDoctor
	{
		DoctorStatus Status { get; set; }
	}

	internal enum PatienStatus
	{
		Sick,
		Healthy
	}

	internal interface IPatient
	{
		PatienStatus Status { get; set; }
	}
}