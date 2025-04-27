using PsdzClient.Programming;
using System;

namespace BMW.Rheingold.CoreFramework.Programming.Data.Ecu
{
    public class BusObject : IBusObject
    {
        public static readonly BusObject ACan = new BusObject(1, "ACan");

        public static readonly BusObject BodyCan = new BusObject(2, "BodyCan");

        public static readonly BusObject DCan = new BusObject(3, "DCan");

        public static readonly BusObject FaCan = new BusObject(4, "FaCan");

        public static readonly BusObject KCan = new BusObject(5, "KCan");

        public static readonly BusObject Unknown = new BusObject(6, "Unknown");

        public static readonly BusObject Unbekannt = new BusObject(7, "Unbekannt");

        public static readonly BusObject Most = new BusObject(8, "Most");

        public static readonly BusObject FlexRay = new BusObject(9, "FlexRay");

        public static readonly BusObject B2Can = new BusObject(10, "B2Can");

        public static readonly BusObject Ethernet = new BusObject(11, "Ethernet");

        public static readonly BusObject SCan = new BusObject(12, "SCan");

        public static readonly BusObject LeCan = new BusObject(13, "LeCan");

        public static readonly BusObject LpCan = new BusObject(14, "LpCan");

        public static readonly BusObject LrCan = new BusObject(15, "LrCan");

        public static readonly BusObject IkCan = new BusObject(16, "IkCan");

        public static readonly BusObject SfCan = new BusObject(17, "SfCan");

        public static readonly BusObject ZsgCan = new BusObject(18, "ZsgCan");

        public static readonly BusObject ZgwCan = new BusObject(19, "ZgwCan");

        public static readonly BusObject Le2Can = new BusObject(20, "Le2Can");

        public static readonly BusObject InfraCan = new BusObject(21, "InfraCan");

        public static readonly BusObject B3Can = new BusObject(22, "B3Can");

        public static readonly BusObject SystemBusEthernet = new BusObject(23, "SystemBusEthernet");

        public static readonly BusObject AeCanFd = new BusObject(24, "AeCanFd");

        public static readonly BusObject ApCanFd = new BusObject(25, "ApCanFd");

        public static readonly BusObject FasCanFd = new BusObject(26, "FasCanFd");

        public static readonly BusObject UssCanFd = new BusObject(27, "UssCanFd");

        public static readonly BusObject SrrCanFd = new BusObject(28, "SrrCanFd");

        public static readonly BusObject Ipb11CanFd = new BusObject(29, "Ipb11CanFd");

        public static readonly BusObject Ipb12CanFd = new BusObject(30, "Ipb12CanFd");

        public static readonly BusObject Ipb13CanFd = new BusObject(31, "Ipb13CanFd");

        public static readonly BusObject Ipb14CanFd = new BusObject(32, "Ipb14CanFd");

        public static readonly BusObject Ipb15CanFd = new BusObject(33, "Ipb15CanFd");

        public static readonly BusObject Ipb16CanFd = new BusObject(34, "Ipb16CanFd");

        public static readonly BusObject Ipb17CanFd = new BusObject(35, "Ipb17CanFd");

        public static readonly BusObject Zim11CanFd = new BusObject(36, "Zim11CanFd");

        public static readonly BusObject Zim12CanFd = new BusObject(37, "Zim12CanFd");

        public int Id { get; set; }

        public string Name { get; set; }

        public bool DirectAddress { get; set; }

        public BusObject(int id, string name, bool directAddress = false)
        {
            Id = id;
            Name = name;
            DirectAddress = directAddress;
        }

        public BusObject()
        {
        }

        public override string ToString()
        {
            return Name;
        }

        public Bus ConvertToBus()
        {
            if (Enum.TryParse<Bus>(Name, out var result))
            {
                return result;
            }
            return Bus.Unknown;
        }
    }
}