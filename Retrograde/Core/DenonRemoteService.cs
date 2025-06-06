using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dn500BD.Retrograde.Infra;
using Microsoft.Extensions.Logging;

namespace Dn500BD.Retrograde.Core;

public class DenonRemoteService : IDenonRemoteService
{
    private readonly Dictionary<string, UnitController> _controllers = new();
    private readonly object _sync = new(); // synchronization lock

    private readonly ILogger _logger;
    private readonly Func<string, ISerialPortService> _serialFactory;

    public DenonRemoteService(ILogger logger, Func<string, ISerialPortService> serialFactory)
    {
        _logger = logger;
        _serialFactory = serialFactory;
    }

    public UnitController Connect(string comPort, IEnumerable<Action>? onWindowClosed = null)
    {
        lock (_sync)
        {
            if (_controllers.ContainsKey(comPort))
                throw new InvalidOperationException($"COM port {comPort} is already connected.");
        }

        ISerialPortService? serial = null;

        try
        {
            serial = _serialFactory(comPort);
            serial.Open(comPort);

            var remote = new DenonRemoteController(serial, _logger);

            try
            {
                var validated = Task.Run(() => remote.ValidateConnectionAsync()).GetAwaiter().GetResult();
                if (!validated)
                    throw new InvalidOperationException($"Device on {comPort} is not a DN-500BD MKII.");
            }
            catch (Exception ex)
            {
                serial.Close();
                _logger.LogWarning(ex, "Validation failed during Connect()");
                throw;
            }

            var window = new UnitControllerWindow(comPort);
            var controller = new UnitController(remote, window);

            lock (_sync)
            {
                _controllers[comPort] = controller;
            }

            window.Closed += (_, _) =>
            {
                Disconnect(comPort);
                if (onWindowClosed != null)
                {
                    foreach (var callback in onWindowClosed)
                    {
                        try { callback(); }
                        catch (Exception ex) { _logger.LogWarning(ex, "Callback failed on window close."); }
                    }
                }
            };

            window.Activate();
            window.CenterOnScreen();

            return controller;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to {ComPort}", comPort);
            serial?.Close();
            throw;
        }
    }

    public void Disconnect(string comPort)
    {
        UnitController? controller = null;

        lock (_sync)
        {
            if (_controllers.TryGetValue(comPort, out controller))
            {
                _controllers.Remove(comPort);
            }
        }

        if (controller != null)
        {
            try
            {
                controller.Remote?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing remote for {ComPort}", comPort);
            }
        }
    }

    public UnitController? Get(string comPort)
    {
        lock (_sync)
        {
            _controllers.TryGetValue(comPort, out var controller);
            return controller;
        }
    }

    public IReadOnlyDictionary<string, UnitController> All
    {
        get
        {
            lock (_sync)
            {
                return new Dictionary<string, UnitController>(_controllers);
            }
        }
    }
}
