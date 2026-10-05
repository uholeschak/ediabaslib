using PsdzClient.Core;
using System.ComponentModel;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.SeamLM2;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.Interface.SeamLM2Demand
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISeamLM2BatteryData
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult ApiResult { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        SeamLM2DemandByCategoryResponse BatteryDemandData { get; set; }
    }
}
