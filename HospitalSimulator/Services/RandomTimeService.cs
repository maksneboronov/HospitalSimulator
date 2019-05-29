using System;

namespace HospitalSimulator.Services
{
	public class RandomTimeService
	{
		public int NextRandomSecond(int min, int max) => _rand.Next(min * 1000, max * 1000);
		public int NextRandomSecond(int max) => _rand.Next(max * 1000);

		public int NextRandomMillisecond(int min, int max) => _rand.Next(min, max);
		public int NextRandomMillisecond(int max) => _rand.Next(max);

		public int Next(int min, int max) => _rand.Next(min, max);
		public int Next(int max) => _rand.Next(max);

		private Random _rand = new Random(DateTime.Now.GetHashCode());
	}
}
