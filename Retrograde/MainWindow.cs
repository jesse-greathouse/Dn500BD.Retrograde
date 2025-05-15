using Gdk;
using Gtk;

namespace Dn500BD.Retrograde;

public class MainWindow : Gtk.Window
{
    public MainWindow() : base("DN 500BD Retrograde")
    {
        SetDefaultSize(800, 640); // Set window size to 800x640
    }

    protected override bool OnDeleteEvent(Event e)
    {
        Application.Quit();
        return true;
    }
}
