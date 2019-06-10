using System.Threading.Tasks;
using System.Threading;

namespace HospitalSimulator.TPLAdvanced
{
	public class PauseTokenSource
	{
		public bool IsPaused
		{
			get => m_paused != null;
			set
			{
				if (value)
				{
					Interlocked.CompareExchange(
						ref m_paused, new TaskCompletionSource<bool>(), null);
				}
				else
				{
					while (true)
					{
						var tcs = m_paused;
						if (tcs == null)
						{
							return;
						}

						if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
						{
							tcs.SetResult(true);
							break;
						}
					}
				}
			}
		}

		public PauseToken Token => new PauseToken(this);

		public Task WaitWhilePausedAsync()
		{
			var cur = m_paused;
			return cur != null ? cur.Task : s_completedTask;
		}

		private volatile TaskCompletionSource<bool> m_paused;
		internal static readonly Task s_completedTask = Task.FromResult(true);
	}
}
