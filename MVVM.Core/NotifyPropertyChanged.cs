using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MVVM.Core
{
	[AttributeUsage(AttributeTargets.Property)]
	public class RaisableAttribute : Attribute
	{
		public RaisableAttribute(string dependProp) => Property = dependProp;
		
		public string Property { get; set; }
	}

	public class NotifyPropertyChanged : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public NotifyPropertyChanged()
		{
			_propDict = this
				.GetType()
				.GetProperties()
				.ToDictionary(
					i => i.Name,
					k => k
						.GetCustomAttributes(typeof(RaisableAttribute), false)
						.Select(j => ((RaisableAttribute)j).Property)
						.ToList()
						);
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
			if (_propDict.ContainsKey(name))
			{
				foreach (var prop in _propDict[name])
				{
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
				}
			}
		}

		private Dictionary<string, List<string>> _propDict = new Dictionary<string, List<string>> ();
	}
}
