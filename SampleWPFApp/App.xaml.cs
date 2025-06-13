using SampleWPFApp.src.Infrastructure;
using System.Windows;
using SampleWPFApp.Infrastructure;

namespace SampleWPFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            new SampleWindowViewContext().Run();
        }
    }

}
