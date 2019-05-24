using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HospitalSimulator.Models;

namespace HospitalSimulator.Converters
{
	public sealed class DoctorStatusConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (DoctorStatus)value == DoctorStatus.NotWork ? notWork : ((DoctorStatus)value == DoctorStatus.Wait ? wait : work);
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

		private static readonly SolidColorBrush notWork = new SolidColorBrush(Color.FromRgb(169, 169, 169));
		private static readonly SolidColorBrush wait = new SolidColorBrush(Color.FromRgb(152, 251, 152));
		private static readonly SolidColorBrush work = new SolidColorBrush(Color.FromRgb(250, 128, 114));
	}
}
