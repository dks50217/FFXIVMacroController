﻿using System.Reflection;
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
using FFXIVMacroControllerApp.Helper;
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
        private readonly UpdateChecker _updateChecker = new UpdateChecker();

        public MainWindow()
        {

            InitializeComponent();

            CheckForUpdate();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            VersionLabel.Content = $"Version: {version}";

            // DI
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddRadzenComponents();
            serviceCollection.AddRadzenCookieThemeService();

            //serviceCollection.AddSingleton<IFileProvider>(provider =>
            //{
            //    return new ManifestEmbeddedFileProvider(Assembly.GetExecutingAssembly(), "wwwroot");
            //});

            Resources.Add("services", serviceCollection.BuildServiceProvider());

            InitializeApp();
        }

        private async void CheckForUpdate()
        {
            await _updateChecker.CheckForUpdateAsync();
        }

        private void InitializeApp()
        {
            BmpPigeonhole.Initialize(AppContext.BaseDirectory + @"\Grunt.ApiTest.json");
            BmpSeer.Instance.SetupFirewall("FFXIVMacroController");
            BmpSeer.Instance.Start();
            BmpGrunt.Instance.Start();
        }
    }
}