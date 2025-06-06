using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dn500BD.Retrograde.Core;

public interface IDenonRemote
{
    Task<bool> SendCommandAsync(DenonCommand command, CancellationToken token = default);
    Task<bool> ValidateConnectionAsync(CancellationToken token = default);
    IEnumerable<DenonCommand> GetAvailableCommands();
}
