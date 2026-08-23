using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API.Interface.SeamLM2Demand
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISeamLM2BatteryDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        ISeamLM2BatteryData GetBatteryDataFromBackend(string vin);
    }
}
