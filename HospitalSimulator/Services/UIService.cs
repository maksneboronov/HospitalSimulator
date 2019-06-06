using System;

namespace HospitalSimulator
{
	public class UIService : IUIService
	{
		public void OpenOptionWindow(ref int doctorsNum, ref int patientsNum, ref int infectionInterval, ref int generationInterval, ref int receptionInterval)
		{
			var ow = new OptionWindow()
			{
				DoctorsNum = doctorsNum,
				WaitingPatientsNum = patientsNum,
				InfectionInterval = infectionInterval,
				GenerationInterval = generationInterval,
				ReceptionInterval = receptionInterval

			};

			if (ow.ShowDialog() == true)
			{
				doctorsNum = ow.DoctorsNum;
				patientsNum = ow.WaitingPatientsNum;
				infectionInterval = ow.InfectionInterval;
				generationInterval = ow.GenerationInterval;
				receptionInterval = ow.ReceptionInterval;
			}
		}
	}
}
