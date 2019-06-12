using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HospitalSimulator
{
	public class ExtButton
	{
		public static readonly DependencyProperty TextProperty;

		static ExtButton()
		{
			var metadata = new FrameworkPropertyMetadata(null);
			TextProperty =  DependencyProperty.RegisterAttached("Text", typeof(String), typeof(ExtButton), null);
		}

		public static String GetIcon(DependencyObject obj)
		{
			return (String)obj.GetValue(TextProperty);
		}

		public static void SetValue(DependencyObject obj, String value)
		{
			obj.SetValue(TextProperty, value);
		}
	}
}
