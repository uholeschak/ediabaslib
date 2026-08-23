using PsdzClient.Core;
using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.HighVoltageBattery;

namespace BMW.Authoring.API.Interface.HighVoltageBattery
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IHighVoltageBatteryData : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult ApiResult { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        HighVoltageBatteryPassResponse HighVoltageBatteryPassData { get; set; }
    }
}
