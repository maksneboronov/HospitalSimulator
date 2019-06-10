using System.Threading;

namespace HospitalSimulator.TPLAdvanced
{
	public class PCTokenSource
	{
		public PauseOrCancelToken Token => new PauseOrCancelToken(_cts, _pts);
		public CancellationToken CancellationToken => _cts.Token;

		public void Pause()
		{
			_pts.IsPaused = true;
		}

		public void Resume()
		{
			_pts.IsPaused = false;
		}

		public void Cancel()
		{
			_cts.Cancel();
		}

		private PauseTokenSource _pts = new PauseTokenSource();
		private CancellationTokenSource _cts = new CancellationTokenSource();
	}
}
