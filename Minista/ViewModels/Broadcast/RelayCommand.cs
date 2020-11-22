using System;
using System.Windows.Input;

namespace Minista.ViewModels.Broadcast
{
    public class RelayCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<T> ExecuteAction { get; }

        public RelayCommand(Action<T> executeAction) => ExecuteAction = executeAction;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
       
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => ExecuteAction?.Invoke((T)parameter);
    }
}
