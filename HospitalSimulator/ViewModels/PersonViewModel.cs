using System;
using MVVM.Core;
using HospitalSimulator.Models;
using System.Collections.Generic;

namespace HospitalSimulator.ViewModels
{
	internal class PersonViewModel : NotifyPropertyChanged, IPerson
	{
		public String Name { get => _name; set { Sex = _vowels.Contains(value[value.Length - 1]) ? PersonSex.Female : PersonSex.Male; this.UpdateValue(value, ref _name); } }
		public PersonSex Sex { get => _sex; set => this.UpdateValue(value, ref _sex); }

		private String _name = "NoName";
		private PersonSex _sex = PersonSex.Male;

		private static readonly HashSet<char> _vowels = new HashSet<char> { 'a', 'e', 'o', 'i', 'y' };
	}
}
