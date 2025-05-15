using System.IO.Ports;
using System.Text;

namespace Dn500BD.Retrograde;

public class SerialPortService : ISerialPortService, IDisposable
{
    private SerialPort? serialPort;

    public bool IsOpen => serialPort != null && serialPort.IsOpen;

    public void Open(string portName, int baudRate = 115200)
    {
        Close(); // close previous port if needed

        serialPort = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 400
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

    public async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        if (serialPort == null || !serialPort.IsOpen)
            return false;

        var commandBytes = Encoding.ASCII.GetBytes(command);

        try
        {
            await serialPort.BaseStream.WriteAsync(commandBytes.AsMemory(), cancellationToken);
            await serialPort.BaseStream.FlushAsync(cancellationToken);
            string _ = await ReadWithTimeoutAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> ReadWithTimeoutAsync(CancellationToken token)
    {
        if (serialPort == null || !serialPort.IsOpen)
            throw new InvalidOperationException("Serial port is not open.");

        var buffer = new byte[1];
        var result = new StringBuilder();

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token);

        Task readTask = Task.Run(async () =>
        {
            while (!linkedCts.Token.IsCancellationRequested)
            {
                if (serialPort.BytesToRead > 0)
                {
                    int bytesRead = await serialPort.BaseStream.ReadAsync(buffer.AsMemory(0, 1), linkedCts.Token);
                    if (bytesRead > 0)
                    {
                        char received = (char)buffer[0];
                        if (received == (char)255) break;
                        result.Append(received);
                    }
                }
                else
                {
                    await Task.Delay(100, linkedCts.Token);
                }
            }
        }, linkedCts.Token);

        if (await Task.WhenAny(readTask, Task.Delay(3000, linkedCts.Token)) != readTask)
            throw new TimeoutException("Device did not respond within timeout.");

        return result.ToString();
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
