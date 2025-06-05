using System.Collections.Generic;
using System.Threading;

namespace Dn500BD.Retrograde.Core;

public interface IDenonRemote
{
    bool SendCommand(DenonCommand command);
    bool ValidateConnection(CancellationToken token = default);
    IEnumerable<DenonCommand> GetAvailableCommands();
}
