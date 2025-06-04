using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Dn500BD.Retrograde.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.System;
using WinRT;
using WinRT.Interop;

namespace Dn500BD.Retrograde
{
    public sealed partial class MainWindow : Window
    {
        private MicaController? _micaController;
        private SystemBackdropConfiguration? _backdropConfig;
        private readonly Dictionary<string, UnitControllerWindow> _controllerWindows = new();

        public MainWindow()
        {
            this.InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Apply Mica effect if supported
            TrySetMicaBackdrop();

            // Populate available COM ports
            PopulateComPorts();

            // Set window size to 800x640
            WindowHelpers.SetWindowSize(this, 800, 640);
        }

        private void TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported())
            {
                _backdropConfig = new SystemBackdropConfiguration
                {
                    IsInputActive = true,
                    Theme = SystemBackdropTheme.Default
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
            else
            {
                // Fallback visual treatment
                RootGrid.Background = new SolidColorBrush(Microsoft.UI.Colors.LightGray);
            }
        }

        private void PopulateComPorts()
        {
            try
            {
                var ports = SerialPort.GetPortNames();
                ComPortComboBox.ItemsSource = ports;
                if (ports.Length > 0)
                    ComPortComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"COM port enumeration failed: {ex.Message}");
                ComPortComboBox.ItemsSource = new[] { "Error retrieving COM ports" };
            }
        }
        private void OnOpenControllerClick(object sender, RoutedEventArgs e)
        {
            if (ComPortComboBox.SelectedItem is string selectedPort)
            {
                if (_controllerWindows.TryGetValue(selectedPort, out var existingWindow))
                {
                    // Reactivate if already open
                    existingWindow.Activate();
                    return;
                }

                var controllerWindow = new UnitControllerWindow(selectedPort);
                _controllerWindows[selectedPort] = controllerWindow;

                controllerWindow.Closed += (_, _) => _controllerWindows.Remove(selectedPort);

                controllerWindow.Activate();
                controllerWindow.CenterOnScreen();
            }
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
