using System;
using System.Windows.Input;

namespace MVVM.Core
{
	public interface ICommandFactory
	{
		ICommand CreateCommand(Action action);
		ICommand CreateCommand(Action action, Func<bool> canExec);

		ICommand CreateCommand<T>(Action<T> action);
		ICommand CreateCommand<T>(Action<T> action, Func<T, bool> canExec);

		ICommand CreateCommand(Action<object> action);
		ICommand CreateCommand(Action<object> action, Func<object, bool> canExec);
	}

	public class RelayCommandFactory : ICommandFactory
	{
		public ICommand CreateCommand(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new RelayCommand<object>(o => action());
		}

		public ICommand CreateCommand(Action action, Func<bool> canExec)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (canExec == null)
			{
				throw new ArgumentNullException(nameof(canExec));
			}

			return new RelayCommand<object>(o => action(), o => canExec());
		}

		public ICommand CreateCommand<T>(Action<T> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new RelayCommand<T>(action);
		}

		public ICommand CreateCommand<T>(Action<T> action, Func<T, bool> canExec)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (canExec == null)
			{
				throw new ArgumentNullException(nameof(canExec));
			}

			return new RelayCommand<T>(action, canExec);
		}

		public ICommand CreateCommand(Action<object> action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return new RelayCommand<object>(action);
		}

		public ICommand CreateCommand(Action<object> action, Func<object, bool> canExec)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (canExec == null)
			{
				throw new ArgumentNullException(nameof(canExec));
			}

			return new RelayCommand<object>(action, canExec);
		}
	}
}
