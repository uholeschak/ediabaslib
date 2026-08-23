using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API.VTG
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IVtgData : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult ApiResult { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string VtgClass { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string VtgVersion { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string VtgSerialNumber { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int VtgCalculatedCharacteristicCurve { get; }
    }
}
