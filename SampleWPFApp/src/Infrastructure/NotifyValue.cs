using System.ComponentModel;
using System.Windows.Input;

namespace SampleWPFApp.src.Infrastructure
{   
    public class NotifyStream<T>
    {
        private readonly List<Action<T>> _notifies = [];

        protected void OnNotify(T val)
        {
            var count = _notifies.Count;
            
            for (var i = 0; i < count; i++)
            {
                _notifies[i](val);
            }
        }

        public UpdateDisposable Stream(Action<T> value)
        {
            _notifies.Add(value);
            
            return new UpdateDisposable(OnDispose);

            void OnDispose() => _notifies.Remove(value);
        }
    }

    public readonly struct UpdateDisposable(Action onDispose) : IDisposable
    {
        public void Dispose()
            => onDispose();

        public void AddTo(CancellationToken ct)
            => ct.Register(onDispose);

        public void AddTo(ICollection<IDisposable> disposables)
            => disposables.Add(this);
    }

    public class NotifyValue<T> : NotifyStream<T>
    {
        private T _value;

        public NotifyValue(T defaultValue)
        {
            _value = defaultValue;
        }

        public T Value 
        {
            get => _value;
            set
            {
               _value = value;
               OnNotify(_value);
            }
        }
    }

    public sealed class NotifyPropertyChangeValue<T> : NotifyValue<T>,INotifyPropertyChanged , ICommand
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        bool ICommand.CanExecute(object? parameter) => true;

        void ICommand.Execute(object? parameter)
        {
            if (parameter is not T value) return;
            Value = value;
        }

        public event EventHandler? CanExecuteChanged;

        public NotifyPropertyChangeValue(T defaultValue) : base(defaultValue)
            => Stream(Action);

        private const string ParamName = nameof(Value);

        private void Action(T _)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(ParamName));
        }
    }
}
