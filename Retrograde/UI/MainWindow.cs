using Gdk;
using Gtk;

namespace Dn500BD.Retrograde.UI;

public class MainWindow : Gtk.Window
{
    private readonly System.Action _onShutdown;

    public MainWindow(System.Action onShutdown) : base("DN 500BD Retrograde")
    {
        _onShutdown = onShutdown;
        SetDefaultSize(800, 640); // Set window size to 800x640
    }
    protected override bool OnDeleteEvent(Event e)
    {
        _onShutdown(); // Dispose the serial connection explicitly
        Application.Quit();
        return true;
    }
}
