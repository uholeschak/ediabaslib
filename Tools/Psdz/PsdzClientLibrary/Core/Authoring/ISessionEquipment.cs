using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.ComponentModel;

namespace BMW.Authoring.Session
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISessionEquipment : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        NetworkType NetworkConnectionPc { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        NetworkType NetworkConnectionVci { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string NetworkConnectionVciSignalStrength { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string NetworkConnectionPcSignalStrength { get; }

        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool SetVCItoDisconnected { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool DisconnectVCI();

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool TryToReconnectVCI();
    }
}
