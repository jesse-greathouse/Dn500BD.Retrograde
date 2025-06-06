using System;
using System.Collections.Generic;

namespace Dn500BD.Retrograde.Core;

public interface IDenonRemoteService
{
    UnitController Connect(string comPort, IEnumerable<Action>? onWindowClosed = null);
    void Disconnect(string comPort);
    UnitController? Get(string comPort);
    IReadOnlyDictionary<string, UnitController> All { get; }
}
