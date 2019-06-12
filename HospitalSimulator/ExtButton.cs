using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HospitalSimulator.ExtensionAnimationButtonColor
{
	public class ExtButton
	{
		public static readonly DependencyProperty SizeProperty;

		static ExtButton()
		{
			var metadata = new FrameworkPropertyMetadata(null);
			SizeProperty = DependencyProperty.RegisterAttached("Size", typeof(int), typeof(ExtButton), metadata);
		}

		public static int GetSize(DependencyObject obj)
		{
			return (int)obj.GetValue(SizeProperty);
		}

		public static void SetSize(DependencyObject obj, int value)
		{
			obj.SetValue(SizeProperty, value);
		}
	}
}
