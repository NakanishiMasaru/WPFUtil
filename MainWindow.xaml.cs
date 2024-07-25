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
            ReTaskExtension.setMainThreadContext();
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

                ct.check();
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
            using EditHandle<TESTClass> editHandle = await AssetEditor.open(resource, ct);

            try
            {
                //現在状態を取得
                var currentData = await editHandle.current(ct);

                currentData.DataA = "Edited DataA";
                currentData.DataB = "Edited DataB";

                //適応
                await editHandle.commit(ct);

                //違うデータ
                var currentData2 = await editHandle.current(ct);

                currentData2.DataA = "Edited DataA";
                currentData2.DataB = "Edited DataB";


                // ここで例外を発生させてみる
                throw new Exception("An error occurred");


                await editHandle.commit(ct);

            }
            catch (Exception)
            {
                editHandle.restore();
            }
        }


        static async void Main2()
        {
            var cts = new CancellationTokenSource();
            var ct = cts.Token;

            var resource = new TESTClass { DataA = "Original Data" };
            using ReadHandle<TESTClass> readHandle = await AssetReader.open(resource, ct);

            try
            {
                //現在状態を取得
                var currentData = await readHandle.current(ct);

                currentData.DataA = "Edited DataA";
                currentData.DataB = "Edited DataB";

                //await readHandle.commit(ct);//できない
            }
            catch (Exception)
            {
            }
        }

        public class TESTClass : IDataMember
        {
            public bool Test;

            public string DataA { get; internal set; }

            public string DataB { get; internal set; }

            public Task commitFields(CancellationToken ct)
            {
                throw new NotImplementedException();
            }

            public void restore()
            {
                throw new NotImplementedException();
            }

            public Task syncFields(CancellationToken ct)
            {
                throw new NotImplementedException();
            }
        }

    }
}
