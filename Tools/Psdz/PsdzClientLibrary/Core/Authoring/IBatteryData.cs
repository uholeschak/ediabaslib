using PsdzClient.Core;
using System;
using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.BatteryDemandService;

namespace BMW.Authoring.API.Interface.BatteryService
{
    [Obsolete("This can be removed in ISTA Version 4.62")]
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IBatteryData : IHideObjectMembers
    {
        [Obsolete("This can be removed in ISTA Version 4.62")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult ApiResult { get; }

        [Obsolete("This can be removed in ISTA Version 4.62")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        VehicleDemandResponse BatteryDemandData { get; set; }
    }
}
