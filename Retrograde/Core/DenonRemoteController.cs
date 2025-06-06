using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dn500BD.Retrograde.Infra;
using Microsoft.Extensions.Logging;

namespace Dn500BD.Retrograde.Core;

public class DenonRemoteController : IDenonRemote, IDisposable
{
    private readonly ISerialPortService _serial;
    private readonly ILogger _logger;

    private static readonly List<DenonCommand> _defaultCommands =
    [
        new DenonCommand("Home", "@0PCHM\r"),
        new DenonCommand("Up", "@0PCCUSR3\r"),
        new DenonCommand("Down", "@0PCCUSR4\r"),
        new DenonCommand("Left", "@0PCCUSR1\r"),
        new DenonCommand("Right", "@0PCCUSR2\r"),
        new DenonCommand("Enter", "@0PCENTR\r"),
        new DenonCommand("ConnectionTest", "@0?PCCT\r")
    ];

    private readonly Dictionary<string, DenonCommand> _commandMap = _defaultCommands.ToDictionary(c => c.Label);

    public DenonRemoteController(ISerialPortService serial, ILogger logger)
    {
        _serial = serial;
        _logger = logger;
    }

    public async Task<bool> SendCommandAsync(DenonCommand command, CancellationToken token = default)
    {
        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            linkedCts.CancelAfter(3000);

            bool success = await _serial.SendCommandAsync(command.Code, linkedCts.Token);

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

    public async Task<bool> ValidateConnectionAsync(CancellationToken token = default)
    {
        try
        {
            string response = await _serial.SendCommandAndReadAsync(_commandMap["ConnectionTest"].Code, token);
            _logger.LogInformation("Connection test response: {Response}", response);
            return response.Contains("PCCT00");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection validation failed");
            return false;
        }
    }

    public IEnumerable<DenonCommand> GetAvailableCommands() => _commandMap.Values;

    public void Dispose()
    {
        _logger.LogInformation("Disposing DenonRemoteController and closing serial connection.");
        _serial.Dispose();
    }
}
