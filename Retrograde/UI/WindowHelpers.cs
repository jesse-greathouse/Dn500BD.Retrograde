using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;

namespace Dn500BD.Retrograde.UI;

public static class WindowHelpers
{
    public static void SetWindowSize(Window window, int width, int height)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow?.Resize(new SizeInt32(width, height));
    }

    public static void CenterOnScreen(Window window)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow == null) return;

        var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Nearest)?.WorkArea;
        if (area == null) return;

        int x = (area.Value.Width - appWindow.Size.Width) / 2;
        int y = (area.Value.Height - appWindow.Size.Height) / 2;
        appWindow.Move(new PointInt32(x, y));
    }
    public static void ForceDarkTitleBarColors(Window window)
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow?.TitleBar is AppWindowTitleBar titleBar)
        {
            var dark = Microsoft.UI.ColorHelper.FromArgb(255, 18, 18, 18);
            var darker = Microsoft.UI.ColorHelper.FromArgb(255, 28, 28, 28);
            var white = Microsoft.UI.Colors.White;
            var lightGray = Microsoft.UI.Colors.LightGray;

            titleBar.BackgroundColor = dark;
            titleBar.ForegroundColor = white;
            titleBar.ButtonBackgroundColor = dark;
            titleBar.ButtonForegroundColor = white;

            titleBar.InactiveBackgroundColor = darker;
            titleBar.InactiveForegroundColor = lightGray;
            titleBar.ButtonInactiveBackgroundColor = darker;
            titleBar.ButtonInactiveForegroundColor = lightGray;
        }
    }
}
