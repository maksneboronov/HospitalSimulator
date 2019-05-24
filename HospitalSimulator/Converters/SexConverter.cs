using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using HospitalSimulator.Models;

namespace HospitalSimulator.Converters
{
	public sealed class SexConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> (PersonSex)value == PersonSex.Male ? male : female;
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();

		private static readonly SolidColorBrush male = new SolidColorBrush(Color.FromRgb(176, 224, 230));
		private static readonly SolidColorBrush female = new SolidColorBrush(Color.FromRgb(255, 182, 193));
	}
}
