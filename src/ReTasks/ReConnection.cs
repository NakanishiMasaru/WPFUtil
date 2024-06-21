using static WPFUtil.MainWindow;

namespace WPFUtil.src.ReTasks
{

    public class AssetEditor
    {
        public static Task<EditHandle<T>> Open<T>(T resource, CancellationToken ct) where T : IDataMember
        {
            return Task.FromResult(new EditHandle<T>(resource));
        }
    }

    public class AssetReader
    {
        public static Task<ReadHandle<T>> Open<T>(T resource, CancellationToken ct) where T : IDataMember
        {
            return Task.FromResult(new ReadHandle<T>(resource));
        }
    }

    public class EditHandle<T>(T resource) : IDisposable where T : IDataMember
    {
        private bool _isDisposed;
        private T? _temp;

        public async Task<T> Current(CancellationToken ct)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            await resource.SyncFields(ct);

            _temp = resource;

            return _temp;
        }

        public async Task Commit(CancellationToken ct)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            await resource.CommitFields(ct);
        }

        public void Restore()
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            resource.Restore();

            _temp = default;
        }

        public void Dispose()
        {
            Restore();
            _isDisposed = true;
        }
    }

    public class ReadHandle<T>(T resource) : IDisposable where T : IDataMember
    {

        private bool _isDisposed;

        public async Task<T> Current(CancellationToken ct)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            await resource.SyncFields(ct);

            return resource;
        }

        public void Commit(CancellationToken ct)
        {
            throw new InvalidOperationException("Cannot commit changes in read-only mode.");
        }

        public void Dispose()
        {
            // No specific action needed for read-only handle
            _isDisposed = true;
        }
    }

}