using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVM.Core;

namespace HospitalSimulator
{
    internal sealed class HospitalViewModel : NotifyPropertyChanged
    {
		public string Name { get => _name; set => this.UpdateValue(value, ref _name); }

		public HospitalViewModel()
		{

		}

		private string _name = String.Empty;
    }
}
