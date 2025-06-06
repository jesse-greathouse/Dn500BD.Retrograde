
namespace Dn500BD.Retrograde.Core;

public class UnitController
{
    public DenonRemoteController Remote { get; }
    public UnitControllerWindow Window { get; }

    public UnitController(DenonRemoteController remote, UnitControllerWindow window)
    {
        Remote = remote;
        Window = window;
    }
}
