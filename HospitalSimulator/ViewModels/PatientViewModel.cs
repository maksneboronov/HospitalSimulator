﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalSimulator.Models;

namespace HospitalSimulator.ViewModels
{
	internal class PatientViewModel : PersonViewModel, IPatient
	{
		public PatientStatus Status { get => _status; set => this.UpdateValue(value, ref _status); }
		
		private PatientStatus _status = PatientStatus.Healthy;
	}
}
