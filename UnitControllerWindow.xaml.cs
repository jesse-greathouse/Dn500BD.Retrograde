using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Graphics;
using Windows.System;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        SetWindowSize(1000, 600);
    }
    private void SetWindowSize(int width, int height)
    {
        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        AppWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        AppWindow.Resize(new SizeInt32(width, height));
    }

    public void CenterOnScreen()
    {
        if (AppWindow == null) return;

        var area = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
        if (area == null) return;

        var x = (area.Value.Width - AppWindow.Size.Width) / 2;
        var y = (area.Value.Height - AppWindow.Size.Height) / 2;
        AppWindow.Move(new PointInt32(x, y));
    }
}
