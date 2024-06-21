using System.Resources;
using System.Windows;
using WPFUtil.src.ReTasks;

namespace WPFUtil
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ReTaskExtension.SetMainThreadContext();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            TestNoDeadlock();
        }
        public static void TestNoDeadlock()
        {
            // Run the Main method in a single-threaded context to simulate potential deadlock
            var thread = new Thread(() =>
            {
                var cts = new CancellationTokenSource();
                var ct = cts.Token;


                // Expecting this to complete without deadlock
                var task = Task.Delay(1000,ct).main();

                Console.WriteLine(task);

                task.Wait();//エラー

                ct.Check();
            });

            thread.SetApartmentState(ApartmentState.STA); // Ensure the thread is STA for UI operations
            thread.Start();

            // Give some time to detect potential issues
            if (!thread.Join(5000))
            {
                thread.Interrupt();
                throw new InvalidOperationException("デッドロックが発生しています");
            }
        }

        static async void Main()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var resource = new TESTClass { DataA = "Original Data" };
            using EditHandle<TESTClass> editHandle = await AssetEditor.Open(resource, ct);

            try
            {
                //現在状態を取得
                var currentData = await editHandle.Current(ct);

                currentData.DataA = "Edited DataA";
                currentData.DataB = "Edited DataB";

                //適応
                await editHandle.Commit(ct);

                //違うデータ
                var currentData2 = await editHandle.Current(ct);

                currentData2.DataA = "Edited DataA";
                currentData2.DataB = "Edited DataB";


                // ここで例外を発生させてみる
                throw new Exception("An error occurred");


                await editHandle.Commit(ct);

            }
            catch (Exception)
            {
                editHandle.Restore();
            }
        }


        static async void Main2()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var resource = new TESTClass { DataA = "Original Data" };
            using ReadHandle<TESTClass> readHandle = await AssetReader.Open(resource, ct);

            try
            {
                //現在状態を取得
                var currentData = await readHandle.Current(ct);

                currentData.DataA = "Edited DataA";
                currentData.DataB = "Edited DataB";

                await readHandle.Commit(ct);//できない
            }
            catch (Exception)
            {
            }
        }


        public interface IDataMember
        {
            Task SyncFields(CancellationToken ct);

            Task CommitFields(CancellationToken ct);

            void Restore();
        }


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


        public class TESTClass : IDataMember
        {
            public bool Test;

            public string DataA { get; internal set; }

            public string DataB { get; internal set; }
        }

    }
}
