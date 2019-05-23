using HospitalSimulator.Models;

namespace HospitalSimulator.ViewModels
{
	internal class DoctorViewModel : PersonViewModel, IDoctor
	{
		public DoctorStatus Status { get => _status; set => this.UpdateValue(value, ref _status); }

		private DoctorStatus _status;
	}
}
