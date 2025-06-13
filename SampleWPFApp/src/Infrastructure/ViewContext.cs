using System.Windows;

namespace SampleWPFApp.src.Infrastructure
{
    public abstract class ViewContext<U> :List<IDisposable> where U : Window, new()
    {
        protected readonly record struct Key(object Instance)
        {
            public static Key From<T>() where T : new() 
                => new(new T());

            public static Key From(object instance)
                => new(instance);

            public readonly bool Is<T>()
                => Instance is T;
        }

        private static Key[] ServiceCollection = [];

        private static object _LockObject = new();

        private readonly CancellationTokenSource _Cts = new();
        public CancellationToken OnFinalizedCt => _Cts.Token;

        public void Run()
        {
            lock (_LockObject)
            {
                ServiceCollection = WriteFlashKey();
                var window = new U();
                window.Show();

                window.Closed += (s, e) =>
                {
                    onClose();
                };

                ServiceCollection = [];
            }
        }

        protected abstract Key[] WriteFlashKey();

        private void onClose()
        {
            _Cts.Cancel();
            _Cts.Dispose();

            foreach (var item in this)
            {
                item?.Dispose();
            }
        }

        public static T GetFlashKey<T>()
        {
            for (int i = 0; i < ServiceCollection.Length; i++)
            {
                var collection = ServiceCollection[i];

                if (collection.Is<T>())
                {
                    return (T)collection.Instance;
                }
            }

            throw new KeyNotFoundException($"Not registered with Provider　Type:{typeof(T)}");
        }

        public static ViewContext<U> GetFlashKey()
            => GetFlashKey<ViewContext<U>>();
    }
}
