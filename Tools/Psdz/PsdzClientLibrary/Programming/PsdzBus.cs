using System.Linq;
using System;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
#if OLD_PSDZ_BUS
#warning OLD_PSDZ_BUS activated. Do not use for release builds.
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
#else
    // Todo: Check on update
    public class PsdzBus : IComparable<PsdzBus>
    {
        public const string ETHERNET_PATTERN = "ETHERNET";

        public const int Unknown = -1;

        public const int KCan = 0;

        public const int BodyCan = 1;

        public const int SfCan = 2;

        public const int Most = 3;

        public const int FaCan = 4;

        public const int FlexRay = 5;

        public const int SCan = 6;

        public const int InfraCan = 7;

        public const int B3Can = 12;

        public const int B2Can = 13;

        public const int Ethernet = 15;

        public const int DCan = 16;

        public const int ACan = 17;

        public const int LeCan = 18;

        public const int LpCan = 19;

        public const int IkCan = 20;

        public const int ZsgCan = 21;

        public const int ZgwCan = 24;

        public const int SystemBusEthernet = 27;

        public const int Le2Can = 28;

        public const int AeCanFd = 29;

        public const int ApCanFd = 30;

        public const int UssCanFd = 31;

        public const int FasCanFd = 32;

        public const int SrrCanFd = 33;

        public const int Ipb11CanFd = 34;

        public const int Ipb12CanFd = 35;

        public const int Ipb13CanFd = 36;

        public const int Ipb14CanFd = 37;

        public const int Ipb15CanFd = 38;

        public const int Ipb16CanFd = 47;

        public const int Zim11CanFd = 48;

        public const int Zim12CanFd = 49;

        public const int Ipb17CanFd = 50;

        public const int LrCan = 51;

        public const int Unbekannt = 255;

        public const int Invalid = 255;

        public static readonly PsdzBus BUSNAME_UNKNOWN = new PsdzBus(-1, "UNKNOWN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_UNBEKANNT = new PsdzBus(255, "UNBEKANNT", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_K_CAN = new PsdzBus(0, "K_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_A_CAN = new PsdzBus(17, "A_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_BODY_CAN = new PsdzBus(1, "BODY_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_MOST = new PsdzBus(3, "MOST", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_FA_CAN = new PsdzBus(4, "FA_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_FLEXRAY = new PsdzBus(5, "FLEXRAY", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_INFRA_CAN = new PsdzBus(7, "INFRA_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_B3_CAN = new PsdzBus(12, "B3_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_B2_CAN = new PsdzBus(13, "B2_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_ETHERNET = new PsdzBus(15, "ETHERNET", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_D_CAN = new PsdzBus(16, "D_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_S_CAN = new PsdzBus(6, "S_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_LE_CAN = new PsdzBus(18, "LE_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_LP_CAN = new PsdzBus(19, "LP_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IK_CAN = new PsdzBus(20, "IK_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_SF_CAN = new PsdzBus(2, "SF_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_ZSG_CAN = new PsdzBus(21, "ZSG_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_ZGW_CAN = new PsdzBus(24, "ZGW_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_SYSTEMBUS_ETHERNET = new PsdzBus(27, "SYSTEMBUS_ETHERNET", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_LE2_CAN = new PsdzBus(28, "LE2_CAN", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_AE_CAN_FD = new PsdzBus(29, "AE_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_AP_CAN_FD = new PsdzBus(30, "AP_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_FAS_CAN_FD = new PsdzBus(32, "FAS_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_USS_CAN_FD = new PsdzBus(31, "USS_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_SRR_CAN_FD = new PsdzBus(33, "SRR_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB11_CAN_FD = new PsdzBus(34, "IPB11_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB12_CAN_FD = new PsdzBus(35, "IPB12_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB13_CAN_FD = new PsdzBus(36, "IPB13_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB14_CAN_FD = new PsdzBus(37, "IPB14_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB15_CAN_FD = new PsdzBus(38, "IPB15_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB16_CAN_FD = new PsdzBus(47, "IPB16_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_IPB17_CAN_FD = new PsdzBus(50, "IPB17_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_ZIM11_CAN_FD = new PsdzBus(48, "ZIM11_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_ZIM12_CAN_FD = new PsdzBus(49, "ZIM12_CAN_FD", pDirectAccess: true);

        public static readonly PsdzBus BUSNAME_LR_CAN = new PsdzBus(51, "LR_CAN", pDirectAccess: true);

        private static readonly string[] VEHICLE_ACCESS_BUSSES = new string[2] { "D_CAN", "ETHERNET" };

        private static readonly PsdzBus[] EMPTY_BUSLIST = new PsdzBus[1] { BUSNAME_UNKNOWN };

        private int id;

        private string name;

        private bool directAccess;

        public int Id
        {
            get
            {
                if (id != -1)
                {
                    return id;
                }
                return 255;
            }
            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                if (id != 255)
                {
                    return name;
                }
                return "UNKNOWN";
            }
            set
            {
                name = value;
            }
        }

        public bool DirectAccess
        {
            get
            {
                return directAccess;
            }
            set
            {
                directAccess = value;
            }
        }

        public PsdzBus()
        {
        }

        public PsdzBus(int pId, string pName, bool pDirectAccess)
        {
            Id = pId;
            Name = pName;
            DirectAccess = pDirectAccess;
        }

        public bool IsDirectAccess()
        {
            return directAccess;
        }

        public bool IsVehicleAccess()
        {
            if (!directAccess)
            {
                return false;
            }
            return VEHICLE_ACCESS_BUSSES.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsEthernet()
        {
            return IsEthernet(name);
        }

        public bool IsExternalEthernet()
        {
            return IsExternalEthernet(name);
        }

        public override bool Equals(object obj)
        {
            if (obj is PsdzBus psdzBus && name == psdzBus.name)
            {
                return id == psdzBus.id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + id;
        }

        public bool IsUnknown()
        {
            return name == BUSNAME_UNKNOWN.name;
        }

        public static bool IsEthernet(string busName)
        {
            return busName?.Contains("ETHERNET") ?? false;
        }

        public static bool IsExternalEthernet(string busName)
        {
            return busName?.StartsWith("ETHERNET") ?? false;
        }

        public static bool IsVehicleAccess(string busName)
        {
            if (busName != null)
            {
                return VEHICLE_ACCESS_BUSSES.Contains(busName);
            }
            return false;
        }

        public static PsdzBus GetBusNameStatic(PsdzBus[] busNames, int busId)
        {
            return busNames?.FirstOrDefault((PsdzBus b) => b.id == busId) ?? BUSNAME_UNKNOWN;
        }

        public static PsdzBus GetBusNameStatic(PsdzBus[] busNames, string busName)
        {
            return busNames?.FirstOrDefault((PsdzBus b) => b.name == busName) ?? BUSNAME_UNKNOWN;
        }

        public static bool IsValidName(string name)
        {
            return name != BUSNAME_UNKNOWN.name;
        }

        public static PsdzBus[] GetEmptyBuslist()
        {
            return (PsdzBus[])EMPTY_BUSLIST.Clone();
        }

        public int CompareTo(PsdzBus other)
        {
            return Id.CompareTo(other.Id);
        }
    }
#endif
}