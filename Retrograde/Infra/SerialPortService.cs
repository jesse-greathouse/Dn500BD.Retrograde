using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Dn500BD.Retrograde.Infra;

public class SerialPortService : ISerialPortService, IDisposable
{
    private SerialPort? serialPort;

    private const int ResponseTimeoutMs = 300;

    public bool IsOpen => serialPort != null && serialPort.IsOpen;

    public void Open(string portName, int baudRate = 115200)
    {
        Close(); // close previous port if needed

        serialPort = new SerialPort(portName, baudRate)
        {
            ReadTimeout = ResponseTimeoutMs,
            WriteTimeout = ResponseTimeoutMs
        };

        serialPort.Open();
    }

    public void Close()
    {
        if (serialPort != null)
        {
            if (serialPort.IsOpen)
                serialPort.Close();

            serialPort.Dispose();
            serialPort = null;
        }
    }

    public bool SendCommand(string command, CancellationToken cancellationToken)
    {
        if (serialPort == null || !serialPort.IsOpen)
            return false;

        try
        {
            var buffer = Encoding.ASCII.GetBytes(command);
            using var reg = cancellationToken.Register(() => serialPort?.DiscardOutBuffer());
            serialPort.Write(buffer, 0, buffer.Length);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string SendCommandAndRead(string command, CancellationToken cancellationToken)
    {
        if (serialPort == null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        var buffer = Encoding.ASCII.GetBytes(command);
        var response = new StringBuilder();

        using var reg = cancellationToken.Register(() => serialPort?.DiscardInBuffer());

        try
        {
            serialPort.Write(buffer, 0, buffer.Length);

            var deadline = DateTime.UtcNow + TimeSpan.FromMilliseconds(ResponseTimeoutMs);

            while (DateTime.UtcNow < deadline)
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new TimeoutException("Cancelled while waiting for response.");

                if (serialPort.BytesToRead > 0)
                {
                    int read = serialPort.ReadByte();
                    if (read == -1) break;

                    char received = (char)read;
                    if (received == (char)255) break;

                    response.Append(received);
                }
                else
                {
                    Thread.Sleep(10); // poll delay
                }
            }

            return response.ToString();
        }
        catch (TimeoutException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new IOException("Error during SendCommandAndRead", ex);
        }
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
