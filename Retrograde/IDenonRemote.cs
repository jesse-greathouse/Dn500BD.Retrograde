namespace Dn500BD.Retrograde;

public interface IDenonRemote
{
    Task<bool> SendCommandAsync(DenonCommand command);
    IEnumerable<DenonCommand> GetAvailableCommands();
}
