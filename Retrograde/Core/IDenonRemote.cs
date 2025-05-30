﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dn500BD.Retrograde.Core;

public interface IDenonRemote
{
    Task<bool> SendCommandAsync(DenonCommand command);
    IEnumerable<DenonCommand> GetAvailableCommands();
}
