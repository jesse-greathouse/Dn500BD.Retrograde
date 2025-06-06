using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Dn500BD.Retrograde.Infra;

public class SerialPortService : ISerialPortService, IDisposable
{
    private readonly ILogger _logger;
    private SerialPort? serialPort;

    private const int ResponseTimeoutMs = 300;
    private readonly object _lock = new();

    public bool IsOpen => serialPort != null && serialPort.IsOpen;

    public SerialPortService(ILogger logger)
    {
        _logger = logger;
    }

    public void Open(string portName, int baudRate)
    {
        lock (_lock)
        {
            if (serialPort != null)
            {
                try { serialPort.Close(); } catch { }
                serialPort.Dispose();
            }

            var newSerialPort = new SerialPort(portName, baudRate)
            {
                ReadTimeout = ResponseTimeoutMs,
                WriteTimeout = ResponseTimeoutMs
            };

            SerialPort? openedPort = null;

            var openTask = Task.Run(() =>
            {
                newSerialPort.Open();
                openedPort = newSerialPort;
            });

            _logger.LogInformation("Opening port {PortName}...", portName);
            if (!openTask.Wait(ResponseTimeoutMs))
            {
                _logger.LogWarning("Timed out opening port {PortName}", portName);
                throw new TimeoutException($"Timed out opening serial port {portName} after {ResponseTimeoutMs}ms.");
            }

            _logger.LogInformation("Successfully opened port {PortName}", portName);
            serialPort = openedPort!;
        }
    }

    public void Close()
    {
        if (serialPort != null)
        {
            try
            {
                if (serialPort.IsOpen)
                    serialPort.Close();
                _logger.LogInformation("Closed port {PortName}", serialPort.PortName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing port");
            }

            serialPort.Dispose();
            serialPort = null;
        }
    }

    public async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        if (serialPort == null || !serialPort.IsOpen)
            return false;

        var buffer = Encoding.ASCII.GetBytes(command);

        try
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    using var reg = cancellationToken.Register(() => serialPort?.DiscardOutBuffer());
                    serialPort!.Write(buffer, 0, buffer.Length);
                }
            }, cancellationToken);

            _logger.LogInformation("Command sent: {Command}", command);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send command asynchronously: {Command}", command);
            return false;
        }
    }

    public async Task<string> SendCommandAndReadAsync(string command, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            if (serialPort == null || !serialPort.IsOpen)
                throw new InvalidOperationException("Serial port is not open.");

            serialPort.ReadTimeout = ResponseTimeoutMs;
            serialPort.WriteTimeout = ResponseTimeoutMs;
        }

        var buffer = Encoding.ASCII.GetBytes(command);
        var response = new StringBuilder();

        try
        {
            _logger.LogInformation("Sending command: {Command}", command);

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    try
                    {
                        serialPort!.Write(buffer, 0, buffer.Length);
                    }
                    catch (Exception ex)
                    {
                        throw new IOException($"Write failed on port {serialPort?.PortName}", ex);
                    }
                }
            }, cancellationToken);

            var readBuffer = new byte[1];

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesRead = await Task.Run(() =>
                {
                    try
                    {
                        lock (_lock)
                        {
                            return serialPort!.BaseStream.Read(readBuffer, 0, 1);
                        }
                    }
                    catch (TimeoutException)
                    {
                        return 0;
                    }
                }, cancellationToken);

                if (bytesRead == 0)
                    break;

                char received = (char)readBuffer[0];
                if (received == (char)255) break;

                response.Append(received);
            }

            _logger.LogInformation("Received response: {Response}", response.ToString());
            return response.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SendCommandAndReadAsync on {Port}", serialPort?.PortName);
            throw new IOException($"Error during SendCommandAndReadAsync on {serialPort?.PortName}", ex);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            try
            {
                serialPort?.DiscardInBuffer();
                serialPort?.DiscardOutBuffer();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error discarding buffers during Dispose");
            }

            Close();
        }

        GC.SuppressFinalize(this);
    }
}
