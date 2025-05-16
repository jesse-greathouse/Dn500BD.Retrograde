using Dn500BD.Retrograde.Infra;
using Microsoft.Extensions.Logging;

namespace Dn500BD.Retrograde.Core;

public class DenonRemoteController(ISerialPortService serial, ILogger logger) : IDenonRemote
{
    private readonly ISerialPortService _serial = serial;
    private readonly ILogger _logger = logger;

    private static readonly List<DenonCommand> _defaultCommands =
    [
        new DenonCommand("Home", "@0PCHM\r"),
        new DenonCommand("Up", "@0PCCUSR3\r"),
        new DenonCommand("Down", "@0PCCUSR4\r"),
        new DenonCommand("Left", "@0PCCUSR1\r"),
        new DenonCommand("Right", "@0PCCUSR2\r"),
        new DenonCommand("Enter", "@0PCENTR\r")
    ];

    private readonly Dictionary<string, DenonCommand> _commandMap = _defaultCommands.ToDictionary(cmd => cmd.Label);

    public async Task<bool> SendCommandAsync(DenonCommand command)
    {
        try
        {
            using var cts = new CancellationTokenSource(3000);
            bool success = await _serial.SendCommandAsync(command.Code, cts.Token);

            if (success)
                _logger.LogInformation("Command {Label} sent successfully.", command.Label);
            else
                _logger.LogWarning("Failed to send command {Label}.", command.Label);

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while sending command {Label}", command.Label);
            return false;
        }
    }

    public IEnumerable<DenonCommand> GetAvailableCommands()
    {
        return _commandMap.Values;
    }
}
