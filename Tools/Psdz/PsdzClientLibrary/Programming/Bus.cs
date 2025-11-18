using PsdzClient.Core;
using PsdzClientLibrary;

namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "Obsolete removed", AttributesModified = true)]
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum Bus
    {
        ACan,
        BodyCan,
        DCan,
        FaCan,
        KCan,
        Unknown,
        Unbekannt,
        Most,
        FlexRay,
        B2Can,
        Ethernet,
        SCan,
        LeCan,
        LpCan,
        LrCan,
        IkCan,
        SfCan,
        ZsgCan,
        ZgwCan,
        Le2Can,
        InfraCan,
        B3Can,
        SystemBusEthernet,
        AeCanFd,
        ApCanFd,
        FasCanFd,
        UssCanFd,
        SrrCanFd,
        Ipb11CanFd,
        Ipb12CanFd,
        Ipb13CanFd,
        Ipb14CanFd,
        Ipb15CanFd,
        Ipb16CanFd,
        Ipb17CanFd,
        Zim11CanFd,
        Zim12CanFd
    }
}