using System.Collections.ObjectModel;

namespace BmwDeepObd
{
    public class VehicleInfo
    {
        // ReSharper disable InconsistentNaming
        public enum BusType
        {
            ROOT,
            ETHERNET,
            MOST,
            KCAN,
            KCAN2,
            KCAN3,
            BCAN,
            BCAN2,
            BCAN3,
            FLEXRAY,
            FACAN,
            FASCAN,
            SCAN,
            NONE,
            SIBUS,
            KBUS,
            FCAN,
            ACAN,
            HCAN,
            LOCAN,
            ZGW,
            DWA,
            BYTEFLIGHT,
            INTERNAL,
            VIRTUAL,
            VIRTUALBUSCHECK,
            VIRTUALROOT,
            IBUS,
            LECAN,
            IKCAN,
            UNKNOWN
        }

        // ReSharper restore InconsistentNaming

        public interface IEcuLogisticsEntry
        {
            int DiagAddress { get; }

            string Name { get; }

            string GroupSgbd { get; }

            BusType Bus { get; }

            int Column { get; }

            int Row { get; }

            string ShortName { get; }

            long? SubDiagAddress { get; }

            BusType[] SubBusList { get; }
        }

        public class EcuLogisticsEntry : IEcuLogisticsEntry
        {
            public int DiagAddress { get; }
            public string Name { get; }
            public BusType Bus { get; }
            public BusType[] SubBusList { get; }
            public string GroupSgbd { get; }
            public int Column { get; }
            public int Row { get; }
            public string ShortName { get; }
            public long? SubDiagAddress { get; }

            public EcuLogisticsEntry()
            {
            }

            public EcuLogisticsEntry(int diagAddress, string name, BusType bus, string groupSgbd, int column, int row)
                : this(diagAddress, null, name, bus, null, groupSgbd, column, row, null)
            {
            }

            public EcuLogisticsEntry(int diagAddress, string name, BusType bus, BusType[] subBusList, string groupSgbd,
                int column, int row) : this(diagAddress, null, name, bus, subBusList, groupSgbd, column, row, null)
            {
            }

            public EcuLogisticsEntry(int diagAddress, int subDiagAddress, string name, BusType bus, string groupSgbd,
                int column, int row)
                : this(diagAddress, subDiagAddress, name, bus, null, groupSgbd, column, row, null)
            {
            }

            public EcuLogisticsEntry(int diagAddress, string name, BusType bus, string groupSgbd, int column, int row,
                string shortName) : this(diagAddress, null, name, bus, null, groupSgbd, column, row, shortName)
            {
            }

            public EcuLogisticsEntry(int diagAddress, long? subDiagAddress, string name, BusType bus,
                BusType[] subBusList, string groupSgbd, int column, int row, string shortName)
            {
                this.DiagAddress = diagAddress;
                this.Name = name;
                this.Bus = bus;
                this.SubBusList = subBusList;
                this.GroupSgbd = groupSgbd;
                this.Column = column;
                this.Row = row;
                this.ShortName = shortName;
                this.SubDiagAddress = subDiagAddress;
            }
        }

        // ReSharper disable RedundantExplicitArrayCreation
        // ReSharper disable CoVariantArrayConversion

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE46 =
            new ReadOnlyCollection<IEcuLogisticsEntry>(new EcuLogisticsEntry[]
            {
                new EcuLogisticsEntry(0, "ZKE", BusType.KBUS, "D_0000|D_ZKE_GM", 1, 1),
                new EcuLogisticsEntry(8, "SHD", BusType.KBUS, "D_0008", 2, 6),
                new EcuLogisticsEntry(13, "KOMBI", BusType.FACAN, "D_000D", -1, -1),
                new EcuLogisticsEntry(16, "DME", BusType.FACAN, "D_0010", 7, 1),
                new EcuLogisticsEntry(17, "ZKE", BusType.KBUS, "D_0011", 1, 2),
                new EcuLogisticsEntry(18, "DME", BusType.FACAN, "D_MOTOR|D_0012", 7, 1),
                new EcuLogisticsEntry(19, "DME", BusType.FACAN, "D_0013", 7, 1),
                new EcuLogisticsEntry(20, "DME2", BusType.FACAN, "D_0014", 7, 2),
                new EcuLogisticsEntry(21, "DDSHD", BusType.KBUS, "D_0015", 1, 7),
                new EcuLogisticsEntry(22, "TOENS", BusType.UNKNOWN, "D_0016", -1, -1),
                new EcuLogisticsEntry(24, "EGS", BusType.FACAN, "D_EGS", 7, 9),
                new EcuLogisticsEntry(25, "ATCU", BusType.UNKNOWN, "D_0019", -1, -1),
                new EcuLogisticsEntry(32, "EML", BusType.UNKNOWN, "D_0020", -1, -1),
                new EcuLogisticsEntry(33, "ZV", BusType.UNKNOWN, "D_0021", -1, -1),
                new EcuLogisticsEntry(34, "EML", BusType.FACAN, "D_0022", 7, 3),
                new EcuLogisticsEntry(36, "HKM", BusType.UNKNOWN, "D_0024", -1, -1),
                new EcuLogisticsEntry(40, "RCC", BusType.UNKNOWN, "D_0028", -1, -1),
                new EcuLogisticsEntry(70, "CID", BusType.FACAN, "D_CID", 7, 7),
                new EcuLogisticsEntry(44, "VNC", BusType.UNKNOWN, "D_002C", -1, -1),
                new EcuLogisticsEntry(46, "EDC", BusType.UNKNOWN, "D_002E", -1, -1),
                new EcuLogisticsEntry(48, "CCM", BusType.KBUS, "D_0030", 0, 5),
                new EcuLogisticsEntry(49, "EHPS", BusType.FACAN, "D_0031", 7, 4),
                new EcuLogisticsEntry(50, "EGS", BusType.FACAN, "D_0032", 7, 4),
                new EcuLogisticsEntry(52, "VGSG", BusType.FACAN, "D_VGSG", 7, 10),
                new EcuLogisticsEntry(53, "LSM/EPS", BusType.FACAN, "D_0035|D_EPS", 6, 1),
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", 7, 5),
                new EcuLogisticsEntry(59, "VIDEO", BusType.KBUS, "D_003b", 5, 4),
                new EcuLogisticsEntry(64, "FBZV", BusType.KBUS, "D_0040", 0, 6),
                new EcuLogisticsEntry(68, "EWS", BusType.KBUS, "D_0044", 2, 1),
                new EcuLogisticsEntry(69, "DWA", BusType.UNKNOWN, "D_0045", -1, -1),
                new EcuLogisticsEntry(41, "DSC/DXC", BusType.FACAN, "D_ABSKWP", 7, 5),
                new EcuLogisticsEntry(71, "CID", BusType.KBUS, "D_0047", -1, -1),
                new EcuLogisticsEntry(72, "JBIT", BusType.KBUS, "D_0048", -1, -1),
                new EcuLogisticsEntry(80, "MFL", BusType.KBUS, "D_0050", 4, 3),
                new EcuLogisticsEntry(86, "DSC", BusType.FACAN, "D_0056", 7, 5),
                new EcuLogisticsEntry(87, "LWS", BusType.FACAN, "D_0057", 7, 6),
                new EcuLogisticsEntry(89, "HKA", BusType.KBUS, "D_0059", -1, -1),
                new EcuLogisticsEntry(90, "ELV", BusType.UNKNOWN, "D_005A", -1, -1),
                new EcuLogisticsEntry(91, "IHKA", BusType.KBUS, "D_005B", 0, 1),
                new EcuLogisticsEntry(96, "PDC", BusType.KBUS, "D_0060", 2, 3),
                new EcuLogisticsEntry(101, "EKP", BusType.FACAN, "D_0065", 7, 11),
                new EcuLogisticsEntry(102, "ALC", BusType.FACAN, "D_0066", 7, 13),
                new EcuLogisticsEntry(104, "RADIO", BusType.KBUS, "D_0068", 4, 1),
                new EcuLogisticsEntry(105, "EKM", BusType.UNKNOWN, "D_0069", -1, -1),
                new EcuLogisticsEntry(106, "DSP", BusType.KBUS, "D_006A", 1, 4),
                new EcuLogisticsEntry(107, "STH", BusType.KBUS, "D_ZUHEIZ", 1, 5),
                new EcuLogisticsEntry(108, "EGS", BusType.UNKNOWN, "D_006C", -1, -1),
                new EcuLogisticsEntry(112, "RDC", BusType.KBUS, "D_0070", 0, 3),
                new EcuLogisticsEntry(113, "SM/SPM", BusType.UNKNOWN, "D_0071", -1, -1),
                new EcuLogisticsEntry(114, "SM", BusType.KBUS, "D_0072|D_0072b", 0, 2),
                new EcuLogisticsEntry(116, "OC", BusType.KBUS, "D_0074", 2, 8),
                new EcuLogisticsEntry(118, "CDC", BusType.KBUS, "D_0076", 5, 1),
                new EcuLogisticsEntry(127, "NAVI", BusType.KBUS, "D_007f", 5, 2),
                new EcuLogisticsEntry(128, "KOMBI", BusType.ROOT, "D_0080|D_KOMBI", 5, 0),
                new EcuLogisticsEntry(129, "RIP", BusType.KBUS, "D_0081", 4, 4),
                new EcuLogisticsEntry(134, "AHK", BusType.UNKNOWN, "D_0086", -1, -1),
                new EcuLogisticsEntry(153, "NO_DME", BusType.VIRTUAL, "D_0099", -1, -1),
                new EcuLogisticsEntry(154, "LWR", BusType.KBUS, "D_009a", 2, 2),
                new EcuLogisticsEntry(155, "CVM", BusType.KBUS, "D_009b", -1, -1),
                new EcuLogisticsEntry(157, "ETS", BusType.UNKNOWN, "D_009d", -1, -1),
                new EcuLogisticsEntry(156, "CVM", BusType.KBUS, "D_009c", 2, 4),
                new EcuLogisticsEntry(158, "UEB", BusType.KBUS, "D_009e", 2, 7),
                new EcuLogisticsEntry(160, "FOND_ZIS", BusType.UNKNOWN, "D_00a0", -1, -1),
                new EcuLogisticsEntry(161, "SBSL", BusType.SIBUS, "D_SBSL2", -1, -1),
                new EcuLogisticsEntry(162, "SBSR", BusType.SIBUS, "D_SBSR2", -1, -1),
                new EcuLogisticsEntry(164, "ZAE", BusType.KBUS, "D_00a4|D_SIM", 2, 5),
                new EcuLogisticsEntry(166, "GR", BusType.FACAN, "D_00a6", 6, 5),
                new EcuLogisticsEntry(167, "FHK", BusType.UNKNOWN, "D_00a7", -1, -1),
                new EcuLogisticsEntry(172, "EHC2", BusType.FACAN, "D_00AC", 7, 12),
                new EcuLogisticsEntry(173, "STVL", BusType.SIBUS, "D_STVL", -1, -1),
                new EcuLogisticsEntry(174, "STVR", BusType.SIBUS, "D_STVR", -1, -1),
                new EcuLogisticsEntry(176, "RADIO", BusType.KBUS, "D_00b0", -1, -1),
                new EcuLogisticsEntry(185, "IRS", BusType.UNKNOWN, "D_00b9", -1, -1),
                new EcuLogisticsEntry(187, "JNAV", BusType.KBUS, "D_00bb", -1, -1),
                new EcuLogisticsEntry(192, "ZIS", BusType.KBUS, "D_00c0", 0, 7),
                new EcuLogisticsEntry(194, "SVT", BusType.KBUS, "D_00c2", 1, 6),
                new EcuLogisticsEntry(200, "TELEFON", BusType.KBUS, "D_00c8", 4, 5),
                new EcuLogisticsEntry(205, "BC", BusType.KBUS, "D_00cd", 2, 9),
                new EcuLogisticsEntry(206, "RDC/SBE", BusType.SIBUS, "D_00ce", -1, -1),
                new EcuLogisticsEntry(208, "LM", BusType.FACAN, "D_00d0", 7, 8),
                new EcuLogisticsEntry(218, "SM", BusType.KBUS, "D_00da", 0, 4),
                new EcuLogisticsEntry(224, "IRIS", BusType.UNKNOWN, "D_00e0", -1, -1),
                new EcuLogisticsEntry(232, "RLS", BusType.KBUS, "D_00e8", 1, 3),
                new EcuLogisticsEntry(234, "DSP_BT", BusType.UNKNOWN, "D_00ea", -1, -1),
                new EcuLogisticsEntry(237, "VIDEO", BusType.KBUS, "D_00ed", 4, 7),
                new EcuLogisticsEntry(240, "CIDB", BusType.KBUS, "D_00f0", 4, 6),
                new EcuLogisticsEntry(245, "LKM", BusType.KBUS, "D_00f5|d_szm", 4, 2)
            });

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE53 =
            new ReadOnlyCollection<IEcuLogisticsEntry>(new EcuLogisticsEntry[]
            {
                new EcuLogisticsEntry(0, "ZKE", BusType.KBUS, "D_ZKE_GM|D_0000", 1, 1),
                new EcuLogisticsEntry(8, "SHD", BusType.KBUS, "D_0008", 2, 6),
                new EcuLogisticsEntry(13, "KOMBI", BusType.FACAN, "D_000D", -1, -1),
                new EcuLogisticsEntry(16, "DME", BusType.FACAN, "D_0010", 7, 1),
                new EcuLogisticsEntry(17, "ZKE", BusType.KBUS, "D_0011", 1, 2),
                new EcuLogisticsEntry(18, "DME", BusType.FACAN, "D_MOTOR|D_0012", 7, 1),
                new EcuLogisticsEntry(19, "DME", BusType.FACAN, "D_0013", 7, 1),
                new EcuLogisticsEntry(20, "DME2", BusType.FACAN, "D_0014", 7, 2),
                new EcuLogisticsEntry(21, "DDSHD", BusType.KBUS, "D_0015", 1, 7),
                new EcuLogisticsEntry(22, "TOENS", BusType.UNKNOWN, "D_0016", -1, -1),
                new EcuLogisticsEntry(24, "EGS", BusType.FACAN, "D_EGS", 7, 9),
                new EcuLogisticsEntry(25, "ATCU", BusType.UNKNOWN, "D_0019", -1, -1),
                new EcuLogisticsEntry(32, "EML", BusType.UNKNOWN, "D_0020", -1, -1),
                new EcuLogisticsEntry(33, "ZV", BusType.UNKNOWN, "D_0021", -1, -1),
                new EcuLogisticsEntry(34, "EML", BusType.FACAN, "D_0022", 7, 3),
                new EcuLogisticsEntry(36, "HKM", BusType.UNKNOWN, "D_0024", -1, -1),
                new EcuLogisticsEntry(40, "RCC", BusType.UNKNOWN, "D_0028", -1, -1),
                new EcuLogisticsEntry(41, "DXC", BusType.FACAN, "D_ABSKWP", 6, 5),
                new EcuLogisticsEntry(70, "CID", BusType.FACAN, "D_CID", 7, 7),
                new EcuLogisticsEntry(44, "VNC", BusType.UNKNOWN, "D_002C", -1, -1),
                new EcuLogisticsEntry(46, "EDC", BusType.UNKNOWN, "D_002E", -1, -1),
                new EcuLogisticsEntry(48, "CCM", BusType.KBUS, "D_0030", 0, 5),
                new EcuLogisticsEntry(49, "EHPS", BusType.FACAN, "D_0031", 7, 4),
                new EcuLogisticsEntry(50, "EGS", BusType.FACAN, "D_0032", 7, 4),
                new EcuLogisticsEntry(52, "VGSG", BusType.FACAN, "D_VGSG", 7, 10),
                new EcuLogisticsEntry(53, "LSM/EPS", BusType.FACAN, "D_0035|D_EPS", 6, 1),
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", -1, -1),
                new EcuLogisticsEntry(59, "VIDEO", BusType.KBUS, "D_003b", 5, 4),
                new EcuLogisticsEntry(64, "FBZV", BusType.KBUS, "D_0040", 0, 6),
                new EcuLogisticsEntry(68, "EWS", BusType.KBUS, "D_0044", 2, 1),
                new EcuLogisticsEntry(69, "DWA", BusType.UNKNOWN, "D_0045", -1, -1),
                new EcuLogisticsEntry(71, "CID", BusType.KBUS, "D_0047", -1, -1),
                new EcuLogisticsEntry(72, "JBIT", BusType.KBUS, "D_0048", -1, -1),
                new EcuLogisticsEntry(80, "MFL", BusType.KBUS, "D_0050", 4, 3),
                new EcuLogisticsEntry(86, "DSC", BusType.FACAN, "D_0056", 7, 5),
                new EcuLogisticsEntry(87, "LWS", BusType.FACAN, "D_0057", 7, 6),
                new EcuLogisticsEntry(89, "HKA", BusType.KBUS, "D_0059", -1, -1),
                new EcuLogisticsEntry(90, "ELV", BusType.UNKNOWN, "D_005A", -1, -1),
                new EcuLogisticsEntry(91, "IHKA", BusType.KBUS, "D_005B", 0, 1),
                new EcuLogisticsEntry(96, "PDC", BusType.KBUS, "D_0060", 2, 3),
                new EcuLogisticsEntry(101, "EKP", BusType.FACAN, "D_0065", 7, 11),
                new EcuLogisticsEntry(102, "ALC", BusType.FACAN, "D_0066", 7, 13),
                new EcuLogisticsEntry(104, "RADIO", BusType.KBUS, "D_0068", 4, 1),
                new EcuLogisticsEntry(105, "EKM", BusType.UNKNOWN, "D_0069", -1, -1),
                new EcuLogisticsEntry(106, "DSP", BusType.KBUS, "D_006A", 1, 4),
                new EcuLogisticsEntry(107, "STH", BusType.KBUS, "D_ZUHEIZ", 1, 5),
                new EcuLogisticsEntry(108, "EGS", BusType.UNKNOWN, "D_006C", -1, -1),
                new EcuLogisticsEntry(112, "RDC", BusType.KBUS, "D_0070", 0, 3),
                new EcuLogisticsEntry(113, "SM/SPM", BusType.UNKNOWN, "D_0071", -1, -1),
                new EcuLogisticsEntry(114, "SM", BusType.KBUS, "D_0072|D_0072b", 0, 2),
                new EcuLogisticsEntry(116, "OC", BusType.KBUS, "D_0074", 2, 8),
                new EcuLogisticsEntry(118, "CDC", BusType.KBUS, "D_0076", 5, 1),
                new EcuLogisticsEntry(127, "NAVI", BusType.KBUS, "D_007f", 5, 2),
                new EcuLogisticsEntry(128, "KOMBI", BusType.ROOT, "D_0080|D_KOMBI", 5, 0),
                new EcuLogisticsEntry(129, "RIP", BusType.KBUS, "D_0081", 4, 4),
                new EcuLogisticsEntry(134, "AHK", BusType.UNKNOWN, "D_0086", -1, -1),
                new EcuLogisticsEntry(153, "NO_DME", BusType.VIRTUAL, "D_0099", -1, -1),
                new EcuLogisticsEntry(154, "LWR", BusType.KBUS, "D_009a", 2, 2),
                new EcuLogisticsEntry(155, "CVM", BusType.KBUS, "D_009b", -1, -1),
                new EcuLogisticsEntry(157, "ETS", BusType.UNKNOWN, "D_009d", -1, -1),
                new EcuLogisticsEntry(156, "CVM", BusType.KBUS, "D_009c", 2, 4),
                new EcuLogisticsEntry(158, "UEB", BusType.KBUS, "D_009e", 2, 7),
                new EcuLogisticsEntry(160, "FOND_ZIS", BusType.UNKNOWN, "D_00a0", -1, -1),
                new EcuLogisticsEntry(161, "SBSL", BusType.SIBUS, "D_SBSL2", -1, -1),
                new EcuLogisticsEntry(162, "SBSR", BusType.SIBUS, "D_SBSR2", -1, -1),
                new EcuLogisticsEntry(164, "ZAE", BusType.KBUS, "D_00a4|D_SIM", 2, 5),
                new EcuLogisticsEntry(166, "GR", BusType.FACAN, "D_00a6", 6, 5),
                new EcuLogisticsEntry(167, "FHK", BusType.UNKNOWN, "D_00a7", -1, -1),
                new EcuLogisticsEntry(172, "EHC2", BusType.FACAN, "D_00AC", 7, 12),
                new EcuLogisticsEntry(173, "STVL", BusType.SIBUS, "D_STVL", -1, -1),
                new EcuLogisticsEntry(174, "STVR", BusType.SIBUS, "D_STVR", -1, -1),
                new EcuLogisticsEntry(176, "RADIO", BusType.KBUS, "D_00b0", -1, -1),
                new EcuLogisticsEntry(185, "IRS", BusType.UNKNOWN, "D_00b9", -1, -1),
                new EcuLogisticsEntry(187, "JNAV", BusType.KBUS, "D_00bb", -1, -1),
                new EcuLogisticsEntry(192, "ZIS", BusType.KBUS, "D_00c0", 0, 7),
                new EcuLogisticsEntry(194, "SVT", BusType.KBUS, "D_00c2", 1, 6),
                new EcuLogisticsEntry(200, "TELEFON", BusType.KBUS, "D_00c8", 4, 5),
                new EcuLogisticsEntry(205, "BC", BusType.KBUS, "D_00cd", 2, 9),
                new EcuLogisticsEntry(206, "RDC/SBE", BusType.SIBUS, "D_00ce", -1, -1),
                new EcuLogisticsEntry(208, "LM", BusType.KBUS, "D_00d0", 0, 8),
                new EcuLogisticsEntry(218, "SM", BusType.KBUS, "D_00da", 0, 4),
                new EcuLogisticsEntry(224, "IRIS", BusType.UNKNOWN, "D_00e0", -1, -1),
                new EcuLogisticsEntry(232, "RLS", BusType.KBUS, "D_00e8", 1, 3),
                new EcuLogisticsEntry(234, "DSP_BT", BusType.UNKNOWN, "D_00ea", -1, -1),
                new EcuLogisticsEntry(237, "VIDEO", BusType.KBUS, "D_00ed", 4, 7),
                new EcuLogisticsEntry(240, "CIDB", BusType.KBUS, "D_00f0", 4, 6),
                new EcuLogisticsEntry(245, "LKM", BusType.KBUS, "D_00f5|d_szm", 4, 2)

            });

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE36 =
            new ReadOnlyCollection<IEcuLogisticsEntry>(new EcuLogisticsEntry[]
            {
                new EcuLogisticsEntry(0, "ZKE", BusType.KBUS, "D_0000|D_ZKE_GM", 1, 1),
                new EcuLogisticsEntry(8, "SHD", BusType.KBUS, "D_0008", 2, 6),
                new EcuLogisticsEntry(13, "KOMBI", BusType.ROOT, "D_000D", 5, 0),
                new EcuLogisticsEntry(16, "DME", BusType.FACAN, "D_0010", 7, 1),
                new EcuLogisticsEntry(17, "ZKE", BusType.KBUS, "D_0011", 1, 2),
                new EcuLogisticsEntry(18, "DME", BusType.FACAN, "D_MOTOR|D_0012", 7, 1),
                new EcuLogisticsEntry(19, "DME", BusType.FACAN, "D_0013", 7, 1),
                new EcuLogisticsEntry(20, "DME2", BusType.FACAN, "D_0014", 7, 2),
                new EcuLogisticsEntry(21, "DDSHD", BusType.KBUS, "D_0015", 1, 7),
                new EcuLogisticsEntry(22, "TOENS", BusType.UNKNOWN, "D_0016", -1, -1),
                new EcuLogisticsEntry(24, "EGS", BusType.FACAN, "D_EGS", 7, 9),
                new EcuLogisticsEntry(25, "ATCU", BusType.UNKNOWN, "D_0019", -1, -1),
                new EcuLogisticsEntry(32, "EML", BusType.UNKNOWN, "D_0020", -1, -1),
                new EcuLogisticsEntry(33, "ZV", BusType.UNKNOWN, "D_0021", -1, -1),
                new EcuLogisticsEntry(34, "EML", BusType.FACAN, "D_0022", 7, 3),
                new EcuLogisticsEntry(36, "HKM", BusType.UNKNOWN, "D_0024", -1, -1),
                new EcuLogisticsEntry(40, "RCC", BusType.UNKNOWN, "D_0028", -1, -1),
                new EcuLogisticsEntry(70, "CID", BusType.FACAN, "D_CID", 7, 7),
                new EcuLogisticsEntry(44, "VNC", BusType.UNKNOWN, "D_002C", -1, -1),
                new EcuLogisticsEntry(46, "EDC", BusType.UNKNOWN, "D_002E", -1, -1),
                new EcuLogisticsEntry(48, "CCM", BusType.KBUS, "D_0030", 0, 5),
                new EcuLogisticsEntry(49, "EHPS", BusType.FACAN, "D_0031", 7, 4),
                new EcuLogisticsEntry(50, "EGS", BusType.FACAN, "D_0032", 7, 4),
                new EcuLogisticsEntry(52, "VGSG", BusType.FACAN, "D_VGSG", 7, 10),
                new EcuLogisticsEntry(53, "LSM/EPS", BusType.FACAN, "D_0035|D_EPS", 6, 1),
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", 7, 5),
                new EcuLogisticsEntry(59, "VIDEO", BusType.KBUS, "D_003b", 5, 4),
                new EcuLogisticsEntry(64, "FBZV", BusType.KBUS, "D_0040", 0, 6),
                new EcuLogisticsEntry(68, "EWS", BusType.KBUS, "D_0044", 2, 1),
                new EcuLogisticsEntry(69, "DWA", BusType.UNKNOWN, "D_0045", -1, -1),
                new EcuLogisticsEntry(41, "DSC/DXC", BusType.KBUS, "D_ABSKWP", 1, 5),
                new EcuLogisticsEntry(71, "CID", BusType.KBUS, "D_0047", -1, -1),
                new EcuLogisticsEntry(72, "JBIT", BusType.KBUS, "D_0048", -1, -1),
                new EcuLogisticsEntry(80, "MFL", BusType.KBUS, "D_0050", 4, 3),
                new EcuLogisticsEntry(86, "DSC", BusType.FACAN, "D_0056", 7, 5),
                new EcuLogisticsEntry(87, "LWS", BusType.FACAN, "D_0057", 7, 6),
                new EcuLogisticsEntry(89, "HKA", BusType.KBUS, "D_0059", -1, -1),
                new EcuLogisticsEntry(90, "ELV", BusType.UNKNOWN, "D_005A", -1, -1),
                new EcuLogisticsEntry(91, "IHKA", BusType.KBUS, "D_005B", 0, 1),
                new EcuLogisticsEntry(96, "PDC", BusType.KBUS, "D_0060", 2, 3),
                new EcuLogisticsEntry(101, "EKP", BusType.FACAN, "D_0065", 7, 11),
                new EcuLogisticsEntry(102, "ALC", BusType.FACAN, "D_0066", 7, 13),
                new EcuLogisticsEntry(104, "RADIO", BusType.KBUS, "D_0068", 4, 1),
                new EcuLogisticsEntry(105, "EKM", BusType.UNKNOWN, "D_0069", -1, -1),
                new EcuLogisticsEntry(106, "DSP", BusType.KBUS, "D_006A", 1, 4),
                new EcuLogisticsEntry(107, "STH", BusType.KBUS, "D_ZUHEIZ", 1, 5),
                new EcuLogisticsEntry(108, "EGS", BusType.UNKNOWN, "D_006C", -1, -1),
                new EcuLogisticsEntry(112, "RDC", BusType.KBUS, "D_0070", 0, 3),
                new EcuLogisticsEntry(113, "SM/SPM", BusType.UNKNOWN, "D_0071", -1, -1),
                new EcuLogisticsEntry(114, "SM", BusType.KBUS, "D_0072|D_0072b", 0, 2),
                new EcuLogisticsEntry(116, "OC", BusType.KBUS, "D_0074", 2, 8),
                new EcuLogisticsEntry(118, "CDC", BusType.KBUS, "D_0076", 5, 1),
                new EcuLogisticsEntry(127, "NAVI", BusType.KBUS, "D_007f", 5, 2),
                new EcuLogisticsEntry(128, "KOMBI", BusType.ROOT, "D_0080|D_KOMBI", 5, 0),
                new EcuLogisticsEntry(129, "RIP", BusType.KBUS, "D_0081", 4, 4),
                new EcuLogisticsEntry(134, "AHK", BusType.UNKNOWN, "D_0086", -1, -1),
                new EcuLogisticsEntry(153, "NO_DME", BusType.VIRTUAL, "D_0099", -1, -1),
                new EcuLogisticsEntry(154, "LWR", BusType.KBUS, "D_009a", 2, 2),
                new EcuLogisticsEntry(155, "CVM", BusType.KBUS, "D_009b", 2, 4),
                new EcuLogisticsEntry(157, "ETS", BusType.UNKNOWN, "D_009d", -1, -1),
                new EcuLogisticsEntry(156, "CVM", BusType.KBUS, "D_009c", 2, 4),
                new EcuLogisticsEntry(158, "UEB", BusType.KBUS, "D_009e", 2, 7),
                new EcuLogisticsEntry(160, "FOND_ZIS", BusType.KBUS, "D_00a0", -1, -1),
                new EcuLogisticsEntry(161, "SBSL", BusType.SIBUS, "D_SBSL2", -1, -1),
                new EcuLogisticsEntry(162, "SBSR", BusType.SIBUS, "D_SBSR2", -1, -1),
                new EcuLogisticsEntry(164, "ZAE", BusType.KBUS, "D_00a4|D_SIM", 2, 5),
                new EcuLogisticsEntry(166, "GR", BusType.FACAN, "D_00a6", 6, 5),
                new EcuLogisticsEntry(167, "FHK", BusType.KBUS, "D_00a7", -1, -1),
                new EcuLogisticsEntry(172, "EHC2", BusType.FACAN, "D_00AC", 7, 12),
                new EcuLogisticsEntry(173, "STVL", BusType.SIBUS, "D_STVL", -1, -1),
                new EcuLogisticsEntry(174, "STVR", BusType.SIBUS, "D_STVR", -1, -1),
                new EcuLogisticsEntry(176, "RADIO", BusType.KBUS, "D_00b0", -1, -1),
                new EcuLogisticsEntry(185, "IRS", BusType.KBUS, "D_00b9", 0, 8),
                new EcuLogisticsEntry(187, "JNAV", BusType.KBUS, "D_00bb", -1, -1),
                new EcuLogisticsEntry(192, "ZIS", BusType.KBUS, "D_00c0", 0, 7),
                new EcuLogisticsEntry(194, "SVT", BusType.KBUS, "D_00c2", 1, 6),
                new EcuLogisticsEntry(200, "TELEFON", BusType.KBUS, "D_00c8", 4, 5),
                new EcuLogisticsEntry(205, "BC", BusType.KBUS, "D_00cd", 2, 9),
                new EcuLogisticsEntry(206, "RDC/SBE", BusType.SIBUS, "D_00ce", -1, -1),
                new EcuLogisticsEntry(208, "LM", BusType.FACAN, "D_00d0", 7, 8),
                new EcuLogisticsEntry(218, "SM", BusType.KBUS, "D_00da", 0, 4),
                new EcuLogisticsEntry(224, "IRIS", BusType.UNKNOWN, "D_00e0", -1, -1),
                new EcuLogisticsEntry(232, "RLS", BusType.KBUS, "D_00e8", 1, 3),
                new EcuLogisticsEntry(234, "DSP_BT", BusType.KBUS, "D_00ea", -1, -1),
                new EcuLogisticsEntry(237, "VIDEO", BusType.KBUS, "D_00ed", 4, 7),
                new EcuLogisticsEntry(240, "CIDB", BusType.KBUS, "D_00f0", 4, 6),
                new EcuLogisticsEntry(245, "LKM", BusType.KBUS, "D_00f5|d_szm", 4, 2)
            });

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE38 =
            new ReadOnlyCollection<IEcuLogisticsEntry>(new EcuLogisticsEntry[]
            {
                new EcuLogisticsEntry(0, "ZKE", BusType.KBUS, "D_ZKE_GM", 1, 1),
                new EcuLogisticsEntry(8, "SHD", BusType.KBUS, "D_0008", 2, 6),
                new EcuLogisticsEntry(13, "KOMBI", BusType.FACAN, "D_000D", -1, -1),
                new EcuLogisticsEntry(16, "DME", BusType.FACAN, "D_0010", 7, 1),
                new EcuLogisticsEntry(18, "DME", BusType.FACAN, "D_MOTOR|D_0012", 7, 1),
                new EcuLogisticsEntry(19, "DME", BusType.FACAN, "D_0013", 7, 1),
                new EcuLogisticsEntry(20, "DME2", BusType.FACAN, "D_0014", 7, 2),
                new EcuLogisticsEntry(21, "DDSHD", BusType.KBUS, "D_0015", 1, 7),
                new EcuLogisticsEntry(22, "TOENS", BusType.UNKNOWN, "D_0016", -1, -1),
                new EcuLogisticsEntry(24, "EGS", BusType.FACAN, "D_EGS", 7, 9),
                new EcuLogisticsEntry(25, "ATCU", BusType.UNKNOWN, "D_0019", -1, -1),
                new EcuLogisticsEntry(32, "EML", BusType.UNKNOWN, "D_0020", -1, -1),
                new EcuLogisticsEntry(33, "ZV", BusType.UNKNOWN, "D_0021", -1, -1),
                new EcuLogisticsEntry(34, "EML", BusType.FACAN, "D_0022", 7, 3),
                new EcuLogisticsEntry(36, "HKM", BusType.UNKNOWN, "D_0024", -1, -1),
                new EcuLogisticsEntry(40, "RCC", BusType.IBUS, "D_0028", 4, 8),
                new EcuLogisticsEntry(70, "CID", BusType.FACAN, "D_CID", 7, 7),
                new EcuLogisticsEntry(44, "VNC", BusType.UNKNOWN, "D_002C", -1, -1),
                new EcuLogisticsEntry(46, "EDC", BusType.UNKNOWN, "D_002E", -1, -1),
                new EcuLogisticsEntry(48, "CCM", BusType.IBUS, "D_0030", 5, 5),
                new EcuLogisticsEntry(49, "EHPS", BusType.FACAN, "D_0031", 7, 4),
                new EcuLogisticsEntry(50, "EGS", BusType.FACAN, "D_0032", 7, 4),
                new EcuLogisticsEntry(52, "VGSG", BusType.FACAN, "D_VGSG", 7, 10),
                new EcuLogisticsEntry(53, "LSM/EPS", BusType.FACAN, "D_0035|D_EPS", 6, 1),
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", -1, -1),
                new EcuLogisticsEntry(59, "VIDEO", BusType.IBUS, "D_003b", 5, 4),
                new EcuLogisticsEntry(64, "FBZV", BusType.KBUS, "D_0040", 0, 6),
                new EcuLogisticsEntry(68, "EWS", BusType.KBUS, "D_0044", 2, 1),
                new EcuLogisticsEntry(69, "DWA", BusType.UNKNOWN, "D_0045", -1, -1),
                new EcuLogisticsEntry(41, "DSC/DXC", BusType.KBUS, "D_ABSKWP", 1, 5),
                new EcuLogisticsEntry(71, "CID", BusType.KBUS, "D_0047", -1, -1),
                new EcuLogisticsEntry(72, "JBIT", BusType.KBUS, "D_0048", -1, -1),
                new EcuLogisticsEntry(80, "MFL", BusType.IBUS, "D_0050", 4, 3),
                new EcuLogisticsEntry(86, "DSC", BusType.FACAN, "D_0056", 7, 5),
                new EcuLogisticsEntry(87, "LWS", BusType.FACAN, "D_0057", 7, 6),
                new EcuLogisticsEntry(89, "HKA", BusType.KBUS, "D_0059", -1, -1),
                new EcuLogisticsEntry(90, "ELV", BusType.UNKNOWN, "D_005A", -1, -1),
                new EcuLogisticsEntry(91, "IHKA", BusType.KBUS, "D_005B", 0, 1),
                new EcuLogisticsEntry(96, "PDC", BusType.IBUS, "D_0060", 5, 3),
                new EcuLogisticsEntry(101, "EKP", BusType.FACAN, "D_0065", 7, 11),
                new EcuLogisticsEntry(102, "ALC", BusType.FACAN, "D_0066", 7, 13),
                new EcuLogisticsEntry(104, "RADIO", BusType.IBUS, "D_0068", 4, 1),
                new EcuLogisticsEntry(105, "EKM", BusType.UNKNOWN, "D_0069", -1, -1),
                new EcuLogisticsEntry(106, "DSP", BusType.IBUS, "D_006A", 1, 4),
                new EcuLogisticsEntry(107, "STH", BusType.KBUS, "D_ZUHEIZ", 1, 5),
                new EcuLogisticsEntry(108, "EGS", BusType.UNKNOWN, "D_006C", -1, -1),
                new EcuLogisticsEntry(112, "RDC", BusType.KBUS, "D_0070", 0, 3),
                new EcuLogisticsEntry(113, "SM/SPM", BusType.UNKNOWN, "D_0071", -1, -1),
                new EcuLogisticsEntry(114, "SM", BusType.KBUS, "D_0072|D_0072b", 0, 2),
                new EcuLogisticsEntry(116, "OC", BusType.KBUS, "D_0074", 2, 8),
                new EcuLogisticsEntry(118, "CDC", BusType.IBUS, "D_0076", 5, 1),
                new EcuLogisticsEntry(127, "NAVI", BusType.IBUS, "D_007f", 5, 2),
                new EcuLogisticsEntry(128, "KOMBI", BusType.ROOT, "D_0080|D_KOMBI", 5, 0),
                new EcuLogisticsEntry(129, "RIP", BusType.IBUS, "D_0081", 4, 4),
                new EcuLogisticsEntry(134, "AHK", BusType.UNKNOWN, "D_0086", -1, -1),
                new EcuLogisticsEntry(153, "NO_DME", BusType.VIRTUAL, "D_0099", -1, -1),
                new EcuLogisticsEntry(154, "LWR", BusType.KBUS, "D_009a", 2, 2),
                new EcuLogisticsEntry(155, "CVM", BusType.KBUS, "D_009b", -1, -1),
                new EcuLogisticsEntry(157, "ETS", BusType.UNKNOWN, "D_009d", -1, -1),
                new EcuLogisticsEntry(156, "CVM", BusType.KBUS, "D_009c", 2, 4),
                new EcuLogisticsEntry(158, "UEB", BusType.KBUS, "D_009e", 2, 7),
                new EcuLogisticsEntry(160, "FOND_ZIS", BusType.KBUS, "D_00a0", -1, -1),
                new EcuLogisticsEntry(161, "SBSL", BusType.SIBUS, "D_SBSL2", -1, -1),
                new EcuLogisticsEntry(162, "SBSR", BusType.SIBUS, "D_SBSR2", -1, -1),
                new EcuLogisticsEntry(164, "ZAE", BusType.KBUS, "D_00a4|D_SIM", 2, 5),
                new EcuLogisticsEntry(166, "GR", BusType.FACAN, "D_00a6", 6, 5),
                new EcuLogisticsEntry(167, "FHK", BusType.KBUS, "D_00a7", -1, -1),
                new EcuLogisticsEntry(172, "EHC2", BusType.FACAN, "D_00AC", 7, 12),
                new EcuLogisticsEntry(173, "STVL", BusType.SIBUS, "D_STVL", -1, -1),
                new EcuLogisticsEntry(174, "STVR", BusType.SIBUS, "D_STVR", -1, -1),
                new EcuLogisticsEntry(176, "RADIO", BusType.KBUS, "D_00b0", -1, -1),
                new EcuLogisticsEntry(184, "ACC", BusType.FACAN, "D_b8_d0", 6, 6),
                new EcuLogisticsEntry(185, "IRS", BusType.KBUS, "D_00b9", -1, -1),
                new EcuLogisticsEntry(187, "JNAV", BusType.KBUS, "D_00bb", -1, -1),
                new EcuLogisticsEntry(192, "ZIS", BusType.KBUS, "D_00c0", 0, 7),
                new EcuLogisticsEntry(194, "SVT", BusType.KBUS, "D_00c2", 1, 6),
                new EcuLogisticsEntry(200, "TELEFON", BusType.IBUS, "D_00c8", 4, 5),
                new EcuLogisticsEntry(205, "BC", BusType.KBUS, "D_00cd", 2, 9),
                new EcuLogisticsEntry(206, "RDC/SBE", BusType.SIBUS, "D_00ce", -1, -1),
                new EcuLogisticsEntry(208, "LM", BusType.IBUS, "D_00d0", 5, 6),
                new EcuLogisticsEntry(218, "SM", BusType.KBUS, "D_00da", 0, 4),
                new EcuLogisticsEntry(224, "IRIS", BusType.UNKNOWN, "D_00e0", -1, -1),
                new EcuLogisticsEntry(232, "RLS", BusType.KBUS, "D_00e8", 1, 3),
                new EcuLogisticsEntry(234, "DSP_BT", BusType.KBUS, "D_00ea", -1, -1),
                new EcuLogisticsEntry(237, "VIDEO", BusType.IBUS, "D_00ed", 4, 7),
                new EcuLogisticsEntry(240, "CIDB", BusType.IBUS, "D_00f0", 4, 6),
                new EcuLogisticsEntry(245, "LKM", BusType.IBUS, "D_00f5|d_szm", 4, 2)
            });

        // ReSharper restore CoVariantArrayConversion
        // ReSharper restore RedundantExplicitArrayCreation

        public static IEcuLogisticsEntry GetEcuLogisticsByGroupName(ReadOnlyCollection<IEcuLogisticsEntry> ecuLogisticsList, string name)
        {
            string nameLower = name.ToLowerInvariant();
            foreach (IEcuLogisticsEntry entry in ecuLogisticsList)
            {
                if (entry.GroupSgbd.ToLowerInvariant().Contains(nameLower))
                {
                    return entry;
                }
            }
            return null;
        }
    }
}
