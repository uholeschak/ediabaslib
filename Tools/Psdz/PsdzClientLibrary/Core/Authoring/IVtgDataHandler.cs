using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API.VTG
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IVtgDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        void SendVtgDataToBackend(string vtgClass, string vtgVersion, string vtgSerialNumber, int vtgCalculatedCharacteristicsCurve);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IVtgData GetVtgDataFromBackend();
    }
}
