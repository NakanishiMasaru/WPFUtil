using System.Runtime.CompilerServices;

namespace WPFUtil.src.ReTasks
{

    public static class ReTaskExtension
    {
        private static SynchronizationContext? _mainThreadContext;

        public static void Check(this CancellationToken ct)
        {
            CheckIsMainThread();
            ct.ThrowIfCancellationRequested();
        }

        public static void CheckIsMainThread()
        {
            if (Environment.CurrentManagedThreadId != 1)
            {
                throw new InvalidOperationException("メインスレッド外で実行されています");
            }
        }

        public static void SetMainThreadContext()
        {
            CheckIsMainThread();

            var current = SynchronizationContext.Current;

            _mainThreadContext = current ?? throw new InvalidOperationException();
        }

        // メインスレッドでアクションを実行
        public static void Post(Action action)
        {
            if (_mainThreadContext == null)
                throw new InvalidOperationException();

            _mainThreadContext.Post(_ => action(), null);
        }

        public static Task<TResult> RunAsync<TResult>(Func<Task<TResult>> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            Post(async () =>
            {
                try
                {
                    var res = await func().ConfigureAwait(false);
                    tcs.SetResult(res);
                }
                catch (Exception)
                {
                    throw;
                }
            });

            return tcs.Task;
        }

        public static Task RunAsync(Func<Task> func)
        {
            var tcs = new TaskCompletionSource<object?>();
            Post(async () =>
            {
                try
                {
                    await func().ConfigureAwait(false);
                    tcs.SetResult(null);
                }
                catch (Exception)
                {
                    throw;
                }
            });

            return tcs.Task;
        }

        public static async Task Main(this ConfiguredTaskAwaitable conf)
        {
            await RunAsync(async () =>
            {
                await conf;
            }).ConfigureAwait(false);
        }

        public static async Task<T> Main<T>(this ConfiguredTaskAwaitable<T> conf)
        {
            return await RunAsync(async () =>
            {
                return await conf;
            }).ConfigureAwait(false);
        }

        public static async Task main(this Task task)
        {
            await RunAsync(async () =>
            {
                await task.ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
        public static async Task<T> Main<T>(this Task<T> task)
        {
            return await RunAsync(async () =>
            {
                return await task.ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
