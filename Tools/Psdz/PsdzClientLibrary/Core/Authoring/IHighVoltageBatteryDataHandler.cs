using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.Interface.HighVoltageBattery
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IHighVoltageBatteryDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IHighVoltageBatteryData GetHightVoltageBatteryPass(string vin);
    }
}
