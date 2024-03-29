﻿using System.Threading.Tasks;
using System.Threading;
using System;

namespace HospitalSimulator.TPLAdvanced
{
	public class PauseOrCancelToken
	{
		public CancellationToken CancellationToken => _ct;
		public PauseToken PauseToken => _pt;

		public PauseOrCancelToken(CancellationTokenSource ct, PauseTokenSource pt)
		{
			_ct = ct.Token;
			_pt = pt.Token;
		}

		public async Task PauseIfRequest()
		{
			await _pt.WaitWhilePausedAsync();
		}

		public void CancelIfRequest()
		{
			_ct.ThrowIfCancellationRequested();
		}

		public async Task<bool> PauseOrCancelIfRequest()
		{
			await _pt.WaitWhilePausedAsync();
			try
			{
				_ct.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException)
			{
				return true;
			}
			return false;
		}

		private CancellationToken _ct;
		private PauseToken _pt;

	}
}
