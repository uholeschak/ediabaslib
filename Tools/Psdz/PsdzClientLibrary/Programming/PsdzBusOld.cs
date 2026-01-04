using PsdzClient;

#if OLD_PSDZ_BUS
#warning OLD_PSDZ_BUS activated. Do not use for release builds.
namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [PreserveSource(Hint = "Old implementation", Removed = true)]
    public enum PsdzBus
    {
        Unknown = -1,
        KCan = 0,
        BodyCan = 1,
        SfCan = 2,
        Most = 3,
        FaCan = 4,
        FlexRay = 5,
        SCan = 6,
        InfraCan = 7,
        B3Can = 12,
        B2Can = 13,
        Ethernet = 15,
        DCan = 16,
        ACan = 17,
        LeCan = 18,
        LpCan = 19,
        IkCan = 20,
        ZsgCan = 21,
        ZgwCan = 24,
        SystemBusEthernet = 27,
        Le2Can = 28,
        AeCanFd = 29,
        ApCanFd = 30,
        UssCanFd = 31,
        FasCanFd = 32,
        SrrCanFd = 33,
        Ipb11CanFd = 34,
        Ipb12CanFd = 35,
        Ipb13CanFd = 36,
        Ipb14CanFd = 37,
        Ipb15CanFd = 38,
        Ipb16CanFd = 47,
        Zim11CanFd = 48,
        Zim12CanFd = 49,
        Ipb17CanFd = 50,
    }
}
#endif
