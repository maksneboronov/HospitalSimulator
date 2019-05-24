using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using HospitalSimulator.Models;

namespace HospitalSimulator.Converters
{
	public sealed class IllConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (PatientStatus)value == PatientStatus.Healthy ? Visibility.Hidden : Visibility.Visible;
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
