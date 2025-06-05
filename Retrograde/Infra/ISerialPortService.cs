using System.Threading;

namespace Dn500BD.Retrograde.Infra;

public interface ISerialPortService
{
    bool IsOpen { get; }
    void Open(string portName, int baudRate = 115200);
    void Close();
    bool SendCommand(string command, CancellationToken cancellationToken);
    string SendCommandAndRead(string command, CancellationToken cancellationToken);
    void Dispose();
}
