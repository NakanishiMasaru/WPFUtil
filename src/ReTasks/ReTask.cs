using System.Runtime.CompilerServices;

namespace WPFUtil.src.ReTasks
{

    public static class ReTaskExtension
    {
        private static SynchronizationContext? _MainThreadContext;

        public static void check(this CancellationToken ct)
        {
            checkIsMainThread();
            ct.ThrowIfCancellationRequested();
        }

        public static void checkIsMainThread()
        {
            if (Environment.CurrentManagedThreadId != 1)
            {
                throw new InvalidOperationException("メインスレッド外で実行されています");
            }
        }

        public static void setMainThreadContext()
        {
            checkIsMainThread();

            var current = SynchronizationContext.Current;

            _MainThreadContext = current ?? throw new InvalidOperationException();
        }

        // メインスレッドでアクションを実行
        public static void post(Action action)
        {
            if (_MainThreadContext == null)
                throw new InvalidOperationException();

            _MainThreadContext.Post(_ => action(), null);
        }

        public static Task<TResult> runAsync<TResult>(Func<Task<TResult>> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            post(async () =>
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

        public static Task runAsync(Func<Task> func)
        {
            var tcs = new TaskCompletionSource<object?>();
            post(async () =>
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

        public static async Task main(this ConfiguredTaskAwaitable conf)
        {
            await runAsync(async () =>
            {
                await conf;
            }).ConfigureAwait(false);
        }

        public static async Task<T> main<T>(this ConfiguredTaskAwaitable<T> conf)
        {
            return await runAsync(async () =>
            {
                return await conf;
            }).ConfigureAwait(false);
        }

        public static async Task main(this Task task)
        {
            await runAsync(async () =>
            {
                await task.ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
        public static async Task<T> main<T>(this Task<T> task)
        {
            return await runAsync(async () =>
            {
                return await task.ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
