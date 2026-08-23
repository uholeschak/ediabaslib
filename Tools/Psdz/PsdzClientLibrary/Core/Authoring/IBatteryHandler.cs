using BMW.Authoring;
using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.API.Interface.BatteryService
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    [Obsolete("This can be removed in ISTA Version 4.62")]
    public interface IBatteryHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        [Obsolete("This can be removed in ISTA Version 4.62")]
        IBatteryData GetBatteryDataFromBackend(string vin);
    }
}
