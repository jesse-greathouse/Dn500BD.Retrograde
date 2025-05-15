namespace Dn500BD.Retrograde;

public interface ISerialPortService
{
    bool IsOpen { get; }
    void Open(string portName, int baudRate = 115200);
    void Close();
    Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken);
    void Dispose();
}
