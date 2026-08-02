using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IFault : IHideObjectMembers, IEquatable<IFault>
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        FaultType FaultType { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtc Dtc { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtcVirtuell DtcVirtuell { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDtcSammel DtcSammel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        decimal IsarID { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        new bool Equals(IFault other);
    }
}
