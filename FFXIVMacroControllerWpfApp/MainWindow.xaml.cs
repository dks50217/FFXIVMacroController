using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FFXIVMacroController.Grunt;
using FFXIVMacroController.Pigeonhole;
using FFXIVMacroController.Seer;
using FFXIVMacroController.Seer.Events;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace FFXIVMacroControllerWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // DI
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddRadzenComponents();
            serviceCollection.AddRadzenCookieThemeService();

            Resources.Add("services", serviceCollection.BuildServiceProvider());

            InitializeApp();
        }

        private void InitializeApp()
        {
            BmpPigeonhole.Initialize(AppContext.BaseDirectory + @"\Grunt.ApiTest.json");
            BmpSeer.Instance.SetupFirewall("FFXIVMacroController");
            BmpSeer.Instance.Start();
            BmpGrunt.Instance.Start();

            //Task.Delay(2000);
            //BmpSeer.Instance.IsGotChatLog += OnGotChatLog;
        }

        protected void OnGotChatLog(EnsembleNone seerEvent)
        {
            foreach (var chat in seerEvent.ChatLog)
            {
                this.logText.Text += chat + "\r\n";
            }
        }
    }
}