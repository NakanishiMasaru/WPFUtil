using System.Windows;

namespace SampleWPFApp.src.Infrastructure
{
    public abstract class ViewContext : List<IDisposable>, IDisposable 
    {
        private static readonly Dictionary<Type, object> ServiceCollection = [];

        private static object _LockObject = new();

        public void Run()
        {
            lock (_LockObject)
            {
                ServiceCollection.Clear();

                ConfigureServices(this);
                var window = CreateWindow();
                window.Show();

                window.Closed += (s, e) =>
                {
                    Dispose();
                };

                ServiceCollection.Clear();
            }
        }

        protected abstract void ConfigureServices(ViewContext provider);

        protected abstract Window CreateWindow();

        public void Dispose()
        {
            foreach (var item in this)
            {
                item?.Dispose();
            }
        }

        public void add<T>() where T : new() 
            => ServiceCollection.Add(typeof(T), new T());

        public static T InitalizingRequest<T>()
        {
            try
            {
                return (T)ServiceCollection[typeof(T)];
            }
            catch(KeyNotFoundException)
            {
                throw new ArgumentOutOfRangeException("コンストラクタ外では使用できません");
            }
        }
    }
}
