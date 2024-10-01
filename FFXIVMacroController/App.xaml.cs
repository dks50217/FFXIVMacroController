using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace FFXIVMacroControllerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception, "Application.Current.DispatcherUnhandledException");
            e.Handled = true;
        }

        private void LogException(Exception ex, string source)
        {
            try
            {
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
                string errorMessage = $"[{DateTime.Now}] {source}:\n{ex}\n\n";
                File.AppendAllText(logFilePath, errorMessage);
                MessageBox.Show($"An error occurred. Please check the log file at:\n{logFilePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show($"A critical error occurred: {ex.Message}", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
