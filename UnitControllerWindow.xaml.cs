using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Dn500BD.Retrograde.UI;
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

namespace Dn500BD.Retrograde;

public sealed partial class UnitControllerWindow : Window
{
    private readonly string comPort;
    private bool _initialized = false;

    public string PositionLabel { get; } = "00:00";

    public UnitControllerWindow(string port)
    {
        InitializeComponent();
        comPort = port;

        // Extend content into title bar area
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar); // AppTitleBar is the Grid named in the XAML

        WindowHelpers.ForceDarkTitleBarColors(this);

        Activated += OnActivated;
    }
    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        if (_initialized)
            return;

        _initialized = true;

        try
        {
            WindowHelpers.SetWindowSize(this, 1020, 620);
            WindowHelpers.CenterOnScreen(this);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Window Init] Failed to set size or center: {ex.Message}");
        }
    }

    public void CenterOnScreen()
    {
        WindowHelpers.CenterOnScreen(this);
    }
    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
