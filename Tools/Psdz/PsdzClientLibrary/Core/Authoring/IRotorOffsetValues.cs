using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API.CalibrationValues
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IRotorOffsetValues : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool Valid { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        double RotorOffsetAMRvalue { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        double RotorOffsetGMRvalue { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string RotorOffsetAMRchecksum { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string RotorOffsetGMRchecksum { get; }
    }
}
