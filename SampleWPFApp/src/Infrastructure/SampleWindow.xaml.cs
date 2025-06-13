using SampleWPFApp.src.Infrastructure;

namespace SampleWPFApp.Infrastructure
{
    public class SampleWindowViewContext : ViewContext<SampleWindow>
    {
        protected override Key[] WriteFlashKey()
        => [
            Key.From<SampleWindowViewModel>(),
            Key.From(this)
        ];
    }

    /// <summary>
    /// SampleWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SampleWindow
    {
        public SampleWindow()
        {
            InitializeComponent();
            DataContext = new SampleWindowViewModel();
        }
    }

    public class SampleWindowViewModel
    {
        public NotifyPropertyChangeValue<string> InputText { get; } = new("");
        public NotifyPropertyChangeValue<string> OutputText { get; } = new("");
        public NotifyPropertyChangeValue<string> ClickCountText { get; } = new("");
        public NotifyPropertyChangeValue<object> OnClickCommand { get; } = new("");
        
        public SampleWindowViewModel()
        {
            var model = SampleWindowViewContext.GetFlashKey<SampleWindowModel>();
            var context = SampleWindowViewContext.GetFlashKey();
            
            InputText
                .Stream(model.InputText)
                .AddTo(context);

            OnClickCommand
                .Stream(model.OnClick)
                .AddTo(context);

            model.OutputValue
                .Stream(OnChangeOutputText)
                .AddTo(context);

            model.ClickCount
                .Stream(OnChangeClickCount)
                .AddTo(context);
        }

        private void OnChangeClickCount(int value)
            => ClickCountText.Value = value.ToString();

        private void OnChangeOutputText(string x)
            => OutputText.Value = x;
    }

    public class SampleWindowModel
    {
        private NotifyValue<string> _OutputValue { get; } = new("");
        private NotifyValue<int> _ClickCount { get; } = new(0);
        public NotifyStream<string> OutputValue => _OutputValue;
        public NotifyStream<int> ClickCount => _ClickCount;

        public void InputText(string x) 
            => _OutputValue.Value = x;

        public void OnClick(object _) 
            => _ClickCount.Value++;
    }
}
