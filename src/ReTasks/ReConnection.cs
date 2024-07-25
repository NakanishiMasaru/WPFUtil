namespace WPFUtil.src.ReTasks
{


    public interface IDataMember
    {
        Task syncFields(CancellationToken ct);

        Task commitFields(CancellationToken ct);

        void restore();
    }

    public class AssetEditor
    {
        public static Task<EditHandle<T>> open<T>(T resource, CancellationToken ct) where T : IDataMember
        {
            return Task.FromResult(new EditHandle<T>(resource));
        }
    }

    public class AssetReader
    {
        public static Task<ReadHandle<T>> open<T>(T resource, CancellationToken ct) where T : IDataMember
        {
            return Task.FromResult(new ReadHandle<T>(resource));
        }
    }

    public class EditHandle<T>(T resource) : IDisposable where T : IDataMember
    {
        private bool _IsDisposed;
        private T? _Temp;

        public async Task<T> current(CancellationToken ct)
        {
            ObjectDisposedException.ThrowIf(_IsDisposed, this);

            await resource.syncFields(ct);

            _Temp = resource;

            return _Temp;
        }

        public async Task commit(CancellationToken ct)
        {
            ObjectDisposedException.ThrowIf(_IsDisposed, this);
            await resource.commitFields(ct);
        }

        public void restore()
        {
            ObjectDisposedException.ThrowIf(_IsDisposed, this);
            resource.restore();

            _Temp = default;
        }

        public void Dispose()
        {
            restore();
            _IsDisposed = true;
        }
    }

    public class ReadHandle<T>(T resource) : IDisposable where T : IDataMember
    {

        private bool _IsDisposed;

        public async Task<T> current(CancellationToken ct)
        {
            ObjectDisposedException.ThrowIf(_IsDisposed, this);

            await resource.syncFields(ct);

            return resource;
        }

        public void commit(CancellationToken ct)
        {
            throw new InvalidOperationException("Cannot commit changes in read-only mode.");
        }

        public void Dispose()
        {
            // No specific action needed for read-only handle
            _IsDisposed = true;
        }
    }

}