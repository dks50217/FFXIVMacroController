using System.ComponentModel;
using System.Reflection;
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
using FFXIVMacroController.Seer;
using FFXIVMacroController.Seer.Events;
using FFXIVMacroControllerApp.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Radzen;

namespace FFXIVMacroControllerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Content = $"Version: {version}";

            // DI
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
            serviceCollection.AddRadzenComponents();
            serviceCollection.AddRadzenCookieThemeService();
            serviceCollection.AddScoped<IUpdateService, UpdateService>();
            serviceCollection.AddScoped<IAutoClickService, AutoClickService>();

            Resources.Add("services", serviceCollection.BuildServiceProvider());

            Loaded += (s, e) => Dispatcher.BeginInvoke((Action)(() =>
            {
                BmpSeer.Instance.SetupFirewall("FFXIVMacroController");
                BmpSeer.Instance.Start();
                BmpGrunt.Instance.Start();
            }));
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e) 
        { 
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            this.MaximizeTextBlock.Text = this.WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
        }
           

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}