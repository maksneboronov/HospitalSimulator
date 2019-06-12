using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HospitalSimulator.Converters
{
	public sealed class WorkStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var parse = (HospitalViewModel.WorkState) Enum.Parse(typeof(HospitalViewModel.WorkState), (string)parameter);
			return parse == (HospitalViewModel.WorkState)value ? Visibility.Visible : Visibility.Collapsed; 
		}
			
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
