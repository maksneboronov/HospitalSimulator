using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HospitalSimulator
{
	/// <summary>
	/// Логика взаимодействия для OptionWindow.xaml
	/// </summary>
	public partial class OptionWindow : Window
	{
		public int DoctorsNum { get => (int)DoctorsSlider.Value; set => DoctorsSlider.Value = value; }
		public int WaitingPatientsNum { get => (int)PatientsSlider.Value; set => PatientsSlider.Value = value; }
		public int InfectionInterval { get => (int)InfectionSlider.Value; set => InfectionSlider.Value = value; }
		public int GenerationInterval { get => (int)GenerationSlider.Value; set => GenerationSlider.Value = value; }
		public int ReceptionInterval { get => (int)ReceptionSlider.Value; set => ReceptionSlider.Value = value; }

		public OptionWindow()
		{
			InitializeComponent();
		}

		private void SaveClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			this.Close();
		}

		private void CancelClick(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			this.Close();
		}
	}
}
