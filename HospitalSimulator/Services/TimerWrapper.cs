using System;
using System.Windows.Threading;
using HospitalSimulator.Services.Interfaces;

namespace HospitalSimulator.Services
{
	public class TimerWrapper : ITimerWrapper
	{
		public TimerWrapper(TimeSpan interval, Action tick)
		{
			if (tick == null)
			{
				throw new ArgumentNullException(nameof(tick));
			}

			_timer = new DispatcherTimer()
			{
				Interval = interval
			};

			_timer.Tick += (o, e) => tick();
		}

		public void Start()
		{
			if (_timer.IsEnabled)
			{
				return;
			}

			_timer.Start();
		}

		public void Stop()
		{
			if (!_timer.IsEnabled)
			{
				return;
			}

			_timer.Stop();
		}

		private DispatcherTimer _timer;
	}
}
