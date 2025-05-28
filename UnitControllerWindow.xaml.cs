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
    public AppWindow? AppWindow { get; private set; }
    private readonly string comPort;

    public UnitControllerWindow(string port)
    {
        this.InitializeComponent();
        comPort = port;
        HeaderText.Text = $"Controlling device on {comPort}";

        WindowHelpers.SetWindowSize(this, 1000, 600);
    }

    public void CenterOnScreen()
    {
        WindowHelpers.CenterOnScreen(this);
    }
}
