using System.Threading.Tasks;

namespace HospitalSimulator.TPLAdvanced
{
	public struct PauseToken
	{
		public PauseToken(PauseTokenSource source) => m_source = source;

		public bool IsPaused => m_source != null && m_source.IsPaused;

		public Task WaitWhilePausedAsync() => IsPaused ?
				m_source.WaitWhilePausedAsync() :
				PauseTokenSource.s_completedTask;

		private readonly PauseTokenSource m_source;


	}
}
