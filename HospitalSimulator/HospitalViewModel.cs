using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVM.Core;
using System.Windows.Input;

namespace HospitalSimulator
{
    internal sealed class HospitalViewModel : NotifyPropertyChanged
    {
		[Raisable("FullName")]
		public string Name { get => _name; set => this.UpdateValue(value, ref _name); }

		public string FullName { get => _name; }

		public ICommand ClearName { get; }

		public HospitalViewModel()
		{
			var cf = new RelayCommandFactory();
			ClearName = cf.CreateCommand(() => Name = String.Empty, () => Name.Length < 5);
		}

		private string _name = String.Empty;
    }
}
