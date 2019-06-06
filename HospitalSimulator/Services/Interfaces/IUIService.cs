namespace HospitalSimulator
{
	public interface IUIService
	{
		void OpenOptionWindow(ref int doctorsNum, ref int patientsNum, ref int infectionInterval, ref int generationInterval, ref int receptionInterval);
	}
}
