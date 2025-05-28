using System.Threading;
using System.Threading.Tasks;

namespace Dn500BD.Retrograde.Infra;

public interface ISerialPortService
{
    bool IsOpen { get; }
    void Open(string portName, int baudRate = 115200);
    void Close();
    Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken);
    void Dispose();
}
