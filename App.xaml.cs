using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Dn500BD.Retrograde;
using Dn500BD.Retrograde.Core;
using Dn500BD.Retrograde.Infra;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Dn500BD
{
    public partial class App : Application
    {
        private Window? _window;

        public static DenonRemoteService RemoteService { get; private set; } = null!;

        public App()
        {
            try
            {
                Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent(); // Explicitly initializes WinAppSDK

                this.UnhandledException += OnUnhandledException;

                InitializeComponent();

            }
            catch (Exception ex)
            {
                File.WriteAllText("error.log", ex.ToString());
                throw;
            }
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Initialize logger and serial factory
            ILogger logger = LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger("DenonRemote");

            Func<string, ISerialPortService> serialFactory = _ => new SerialPortService();

            // Create the global service instance
            RemoteService = new DenonRemoteService(logger, serialFactory);

            // Launch MainWindow with service
            _window = new MainWindow(RemoteService);
            _window.Activate();
        }
        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[UNHANDLED EXCEPTION] {e.Exception.Message}");
            System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
        }
    }
}
