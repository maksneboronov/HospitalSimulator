using System;
using System.Windows.Input;

namespace MVVM.Core
{
	public class RelayCommand<T> : ICommand
	{
		public event EventHandler CanExecuteChanged
		{
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		public RelayCommand(Action<T> action, Func<T, bool> canExecute = null)
		{
			_action = action ?? throw new ArgumentNullException(nameof(action));
			_canExecute = canExecute;
		}

		public bool CanExecute(T parameter) => _canExecute?.Invoke(parameter) ?? true;
		public void Execute(T parameter) => _action(parameter);


		bool ICommand.CanExecute(object parameter) => this.CanExecute((T)parameter);
		void ICommand.Execute(object parameter) => this.Execute((T)parameter);

		private Func<T, bool> _canExecute;
		private Action<T> _action;
	}
}
