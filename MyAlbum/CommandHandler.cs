using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyAlbum
{
    public class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> _action;
        private Predicate<object> _canExecutePredicate;
        private bool _canExecute = true;

        public CommandHandler(Action<object> action, Predicate<object> canExecute)
        {
            _action = action;
            _canExecutePredicate = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = _canExecutePredicate(parameter);
            if(canExecute != _canExecute)
            {
                canExecute = _canExecute;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
            return _canExecute;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
