using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Dn500BD.Retrograde.Infra;

namespace Dn500BD.Retrograde.Core;

public class DenonRemoteService
{
    private readonly Dictionary<string, UnitController> _controllers = new();
    private readonly ILogger _logger;
    private readonly Func<string, ISerialPortService> _serialFactory;

    public DenonRemoteService(ILogger logger, Func<string, ISerialPortService> serialFactory)
    {
        _logger = logger;
        _serialFactory = serialFactory;
    }
    public UnitController Connect(string comPort, IEnumerable<Action>? onWindowClosed = null)
    {
        if (_controllers.ContainsKey(comPort))
            throw new InvalidOperationException($"COM port {comPort} is already connected.");

        var serial = _serialFactory(comPort);
        serial.Open(comPort);

        var remote = new DenonRemoteController(serial, _logger);

        // Validate connection
        if (!remote.ValidateConnection())
        {
            serial.Close();
            throw new InvalidOperationException($"Device on {comPort} is not a DN-500BD MKII.");
        }

        var window = new UnitControllerWindow(comPort);
        var controller = new UnitController(remote, window);
        _controllers[comPort] = controller;

        window.Closed += (_, _) =>
        {
            Disconnect(comPort);
            if (onWindowClosed != null)
            {
                foreach (var callback in onWindowClosed)
                {
                    try { callback(); } catch (Exception ex) { _logger.LogWarning(ex, "Callback failed on window close."); }
                }
            }
        };

        window.Activate();
        window.CenterOnScreen();

        return controller;
    }

    public void Disconnect(string comPort)
    {
        if (_controllers.TryGetValue(comPort, out var controller))
        {
            try
            {
                controller.Remote?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing remote for {ComPort}", comPort);
            }
            _controllers.Remove(comPort);
        }
    }

    public UnitController? Get(string comPort)
    {
        _controllers.TryGetValue(comPort, out var controller);
        return controller;
    }

    public IReadOnlyDictionary<string, UnitController> All => _controllers;
}
