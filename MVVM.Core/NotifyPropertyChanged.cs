using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Core
{
	public class NotifyPropertyChanged : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public NotifyPropertyChanged()
		{

		}

		public void UpdateValue<T>(T val, ref T desc, [CallerMemberName] string name = "")
		{
			if (EqualityComparer<T>.Default.Equals(val, desc))
			{
				return;
			}

			desc = val;

			if (!String.IsNullOrWhiteSpace(name))
			{
				RaisePropertyChanged(name);
			}
		}

		public virtual void RaisePropertyChanged([CallerMemberName] string name = "")
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				return;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
