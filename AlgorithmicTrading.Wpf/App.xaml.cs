using System.Windows;
using WpfBindingErrors;

namespace AlgorithmicTrading.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            BindingExceptionThrower.Attach();
        }
    }
}
