using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using Dn500BD.Retrograde.Core;
using Dn500BD.Retrograde.UI;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using WinRT;
using WinRT.Interop;

namespace Dn500BD.Retrograde
{
    public sealed partial class MainWindow : Window
    {
        private MicaController? _micaController;
        private SystemBackdropConfiguration? _backdropConfig;
        private readonly IDenonRemoteService _remoteService;

        public MainWindow(IDenonRemoteService remoteService)
        {
            this.InitializeComponent();
            _remoteService = remoteService;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            WindowHelpers.ForceDarkTitleBarColors(this);
            TrySetMicaBackdrop();

            RefreshComPortList();
            WindowHelpers.SetWindowSize(this, 800, 640);
        }

        private void TrySetMicaBackdrop()
        {
            var darkBackground = new SolidColorBrush(ColorHelper.FromArgb(255, 18, 18, 18));
            RootGrid.Background = darkBackground;

            if (MicaController.IsSupported())
            {
                _backdropConfig = new SystemBackdropConfiguration
                {
                    IsInputActive = true,
                    Theme = SystemBackdropTheme.Dark
                };

                _micaController = new MicaController();
                var backdropTarget = this.As<ICompositionSupportsSystemBackdrop>();
                _micaController.AddSystemBackdropTarget(backdropTarget);
                _micaController.SetSystemBackdropConfiguration(_backdropConfig);

                this.Activated += (_, args) =>
                {
                    _backdropConfig.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
                };
            }
        }

        private void RefreshComPortList()
        {
            try
            {
                var allPorts = SerialPort.GetPortNames().Distinct().OrderBy(x => x).ToList();
                var connectedPorts = _remoteService.All.Keys;
                var availablePorts = allPorts.Except(connectedPorts).ToList();

                ComPortComboBox.ItemsSource = availablePorts;

                if (availablePorts.Count > 0)
                    ComPortComboBox.SelectedIndex = 0;
                else
                    ComPortComboBox.SelectedItem = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"COM port enumeration failed: {ex.Message}");
                ComPortComboBox.ItemsSource = new[] { "Error retrieving COM ports" };
            }
        }
        private void OnOpenControllerClick(object sender, RoutedEventArgs e)
        {
            if (ComPortComboBox.SelectedItem is not string selectedPort)
                return;

            try
            {
                var controller = _remoteService.Connect(selectedPort, new List<Action>
                {
                    () =>
                    {
                        try
                        {
                            RefreshComPortList();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[RefreshComPortList] failed: {ex.Message}");
                        }
                    }
                });

                RefreshComPortList();
            }
            catch (TimeoutException tex)
            {
                Debug.WriteLine($"[OnOpenControllerClick] Port {selectedPort} timed out: {tex.Message}");

                ContentDialog timeoutDialog = new()
                {
                    Title = "Connection Timeout",
                    Content = $"The port {selectedPort} did not respond in time.\n\n" +
                              $"Make sure the device is powered on and connected properly.\n" +
                              $"Technical details:\n{tex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                _ = timeoutDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OnOpenControllerClick] Failed to open controller for {selectedPort}: {ex}");

                ContentDialog errorDialog = new()
                {
                    Title = "Connection Error",
                    Content = $"Failed to connect to {selectedPort}.\n\n{ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };

                _ = errorDialog.ShowAsync();
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            foreach (var port in _remoteService.All.Keys.ToList())
            {
                try
                {
                    _remoteService.Disconnect(port);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Exit] Failed to disconnect {port}: {ex.Message}");
                }
            }

            this.Close();
        }
    }
}
