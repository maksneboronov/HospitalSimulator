using System;
using MVVM.Core;
using HospitalSimulator.Models;

namespace HospitalSimulator.ViewModels
{
	internal class PersonViewModel : NotifyPropertyChanged, IPerson
	{
		public String Name { get => _name; set => this.UpdateValue(value, ref _name); }
		public PersonSex Sex { get => _sex; set => this.UpdateValue(value, ref _sex); }

		private String _name = "NoName";
		private PersonSex _sex = PersonSex.Male;
	}
}
