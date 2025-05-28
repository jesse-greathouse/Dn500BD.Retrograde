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
}
