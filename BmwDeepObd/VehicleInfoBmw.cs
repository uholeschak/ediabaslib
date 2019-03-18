using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using EdiabasLib;
using ICSharpCode.SharpZipLib.Zip;

namespace BmwDeepObd
{
    public class VehicleInfoBmw
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
                DiagAddress = diagAddress;
                Name = name;
                Bus = bus;
                SubBusList = subBusList;
                GroupSgbd = groupSgbd;
                Column = column;
                Row = row;
                ShortName = shortName;
                SubDiagAddress = subDiagAddress;
            }
        }

        private const string DatabaseFileName = @"Database.zip";

        // ReSharper disable RedundantExplicitArrayCreation
        // ReSharper disable CoVariantArrayConversion

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

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE39 =
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
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", -1, -1),
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
                new EcuLogisticsEntry(185, "IRS", BusType.KBUS, "D_00b9", -1, -1),
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

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE52 =
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
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", -1, -1),
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

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE83 =
            new ReadOnlyCollection<IEcuLogisticsEntry>(new EcuLogisticsEntry[]
            {
                new EcuLogisticsEntry(128, "KOMBI", BusType.ROOT, "D_0080|D_KOMBI", 5, 0),
                new EcuLogisticsEntry(129, "RIP", BusType.INTERNAL, "D_0081", 5, 1),
                new EcuLogisticsEntry(70, "CID", BusType.KBUS, "D_CID", 1, 1),
                new EcuLogisticsEntry(106, "DSP", BusType.KBUS, "D_006A", 1, 2),
                new EcuLogisticsEntry(112, "RDC", BusType.KBUS, "D_0070", 1, 3),
                new EcuLogisticsEntry(59, "VIDEO", BusType.KBUS, "D_003b", 2, 1),
                new EcuLogisticsEntry(200, "TELEFON", BusType.KBUS, "D_00c8", 2, 2),
                new EcuLogisticsEntry(245, "LKM", BusType.KBUS, "D_00f5|d_SZM", 2, 3),
                new EcuLogisticsEntry(127, "NAVI", BusType.KBUS, "D_007f", 2, 4),
                new EcuLogisticsEntry(118, "CDC", BusType.KBUS, "D_0076", 2, 5),
                new EcuLogisticsEntry(68, "EWS", BusType.KBUS, "D_0044", 3, 1),
                new EcuLogisticsEntry(0, "ZKE", BusType.KBUS, "D_0000|D_ZKE_GM", 3, 2),
                new EcuLogisticsEntry(91, "IHKA", BusType.KBUS, "D_005B", 3, 3),
                new EcuLogisticsEntry(208, "LM", BusType.KBUS, "D_00d0", 3, 4),
                new EcuLogisticsEntry(80, "MFL", BusType.KBUS, "D_0050", 3, 5),
                new EcuLogisticsEntry(176, "SES", BusType.KBUS, "D_00b0", 3, 6),
                new EcuLogisticsEntry(96, "PDC", BusType.KBUS, "D_0060", 3, 7),
                new EcuLogisticsEntry(104, "RADIO", BusType.KBUS, "D_0068", 3, 8),
                new EcuLogisticsEntry(116, "OC", BusType.KBUS, "D_0074", 4, 1),
                new EcuLogisticsEntry(114, "SM", BusType.KBUS, "D_0072|D_0072b", 4, 3),
                new EcuLogisticsEntry(102, "ALC", BusType.KBUS, "D_0066", 4, 4),
                new EcuLogisticsEntry(8, "SHD", BusType.KBUS, "D_0008", 4, 5),
                new EcuLogisticsEntry(164, "ZAE", BusType.KBUS, "D_00a4|D_SIM", 4, 6),
                new EcuLogisticsEntry(232, "RLS", BusType.KBUS, "D_00e8", 4, 8),
                new EcuLogisticsEntry(41, "ABS/DSC", BusType.FACAN, "D_ABSKWP", 7, 1),
                new EcuLogisticsEntry(194, "SVT", BusType.FACAN, "D_00c2", 7, 2),
                new EcuLogisticsEntry(87, "LWS", BusType.FACAN, "D_0057", 7, 3),
                new EcuLogisticsEntry(101, "EKP", BusType.FACAN, "D_0065", 7, 7),
                new EcuLogisticsEntry(16, "DME", BusType.FACAN, "D_0010", 8, 1),
                new EcuLogisticsEntry(18, "DME", BusType.FACAN, "D_MOTOR|D_0012", 8, 1),
                new EcuLogisticsEntry(19, "DME", BusType.FACAN, "D_0013", 8, 1),
                new EcuLogisticsEntry(24, "EGS", BusType.FACAN, "D_EGS", 8, 3),
                new EcuLogisticsEntry(52, "VGSG", BusType.FACAN, "D_VGSG", 8, 5),
                new EcuLogisticsEntry(49, "EHPS", BusType.FACAN, "D_0031", 7, 2),
                new EcuLogisticsEntry(13, "KOMBI", BusType.FACAN, "D_000D", -1, -1),
                new EcuLogisticsEntry(17, "ZKE", BusType.KBUS, "D_0011", 2, 2),
                new EcuLogisticsEntry(237, "VIDEO", BusType.KBUS, "D_00ed", 2, 1),
                new EcuLogisticsEntry(21, "DDSHD", BusType.KBUS, "D_0015", 2, 7),
                new EcuLogisticsEntry(22, "TOENS", BusType.UNKNOWN, "D_0016", -1, -1),
                new EcuLogisticsEntry(25, "ATCU", BusType.UNKNOWN, "D_0019", -1, -1),
                new EcuLogisticsEntry(32, "EML", BusType.UNKNOWN, "D_0020", -1, -1),
                new EcuLogisticsEntry(33, "ZV", BusType.UNKNOWN, "D_0021", -1, -1),
                new EcuLogisticsEntry(34, "EML", BusType.FACAN, "D_0022", 7, 3),
                new EcuLogisticsEntry(36, "HKM", BusType.UNKNOWN, "D_0024", -1, -1),
                new EcuLogisticsEntry(40, "RCC", BusType.UNKNOWN, "D_0028", -1, -1),
                new EcuLogisticsEntry(187, "JNAV", BusType.KBUS, "D_00bb", 2, 10),
                new EcuLogisticsEntry(44, "VNC", BusType.UNKNOWN, "D_002C", -1, -1),
                new EcuLogisticsEntry(46, "EDC", BusType.UNKNOWN, "D_002E", -1, -1),
                new EcuLogisticsEntry(48, "CCM", BusType.KBUS, "D_0030", 1, 5),
                new EcuLogisticsEntry(50, "EGS", BusType.FACAN, "D_0032", 7, 4),
                new EcuLogisticsEntry(154, "LWR", BusType.KBUS, "D_009a", 2, 1),
                new EcuLogisticsEntry(53, "LSM/EPS", BusType.FACAN, "D_0035|D_EPS", 6, 1),
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", -1, -1),
                new EcuLogisticsEntry(64, "FBZV", BusType.KBUS, "D_0040", 1, 6),
                new EcuLogisticsEntry(69, "DWA", BusType.UNKNOWN, "D_0045", -1, -1),
                new EcuLogisticsEntry(71, "CID", BusType.KBUS, "D_0047", -1, -1),
                new EcuLogisticsEntry(72, "JBIT", BusType.KBUS, "D_0048", -1, -1),
                new EcuLogisticsEntry(86, "DSC", BusType.FACAN, "D_0056", 7, 5),
                new EcuLogisticsEntry(89, "HKA", BusType.KBUS, "D_0059", -1, -1),
                new EcuLogisticsEntry(90, "ELV", BusType.UNKNOWN, "D_005A", -1, -1),
                new EcuLogisticsEntry(240, "CIDB", BusType.KBUS, "D_00f0", 2, 7),
                new EcuLogisticsEntry(105, "EKM", BusType.UNKNOWN, "D_0069", -1, -1),
                new EcuLogisticsEntry(107, "STH", BusType.KBUS, "D_ZUHEIZ", 2, 5),
                new EcuLogisticsEntry(108, "EGS", BusType.UNKNOWN, "D_006C", -1, -1),
                new EcuLogisticsEntry(113, "SM/SPM", BusType.UNKNOWN, "D_0071", -1, -1),
                new EcuLogisticsEntry(134, "AHK", BusType.UNKNOWN, "D_0086", -1, -1),
                new EcuLogisticsEntry(153, "NO_DME", BusType.VIRTUAL, "D_0099", -1, -1),
                new EcuLogisticsEntry(155, "CVM", BusType.KBUS, "D_009b", -1, -1),
                new EcuLogisticsEntry(157, "ETS", BusType.UNKNOWN, "D_009d", -1, -1),
                new EcuLogisticsEntry(158, "UEB", BusType.KBUS, "D_009e", 3, 7),
                new EcuLogisticsEntry(160, "FOND_ZIS", BusType.UNKNOWN, "D_00a0", -1, -1),
                new EcuLogisticsEntry(161, "SBSL", BusType.SIBUS, "D_SBSL", -1, -1),
                new EcuLogisticsEntry(162, "SBSR", BusType.SIBUS, "D_SBSR", -1, -1),
                new EcuLogisticsEntry(166, "GR", BusType.UNKNOWN, "D_00a6", -1, -1),
                new EcuLogisticsEntry(167, "FHK", BusType.UNKNOWN, "D_00a7", -1, -1),
                new EcuLogisticsEntry(172, "EHC2", BusType.FACAN, "D_00AC", 7, 12),
                new EcuLogisticsEntry(173, "STVL", BusType.SIBUS, "D_STVL", -1, -1),
                new EcuLogisticsEntry(174, "STVR", BusType.SIBUS, "D_STVR", -1, -1),
                new EcuLogisticsEntry(185, "IRS", BusType.UNKNOWN, "D_00b9", -1, -1),
                new EcuLogisticsEntry(192, "ZIS", BusType.KBUS, "D_00c0", 1, 7),
                new EcuLogisticsEntry(205, "BC", BusType.KBUS, "D_00cd", 3, 9),
                new EcuLogisticsEntry(206, "RDC/SBE", BusType.SIBUS, "D_00ce", -1, -1),
                new EcuLogisticsEntry(218, "SM", BusType.KBUS, "D_00da", 1, 4),
                new EcuLogisticsEntry(224, "IRIS", BusType.UNKNOWN, "D_00e0", -1, -1),
                new EcuLogisticsEntry(156, "CVM", BusType.KBUS, "D_009c", 2, 3),
                new EcuLogisticsEntry(234, "DSP_BT", BusType.UNKNOWN, "D_00ea", -1, -1)
            });

        public static ReadOnlyCollection<IEcuLogisticsEntry> EcuLogisticsE85 =
            new ReadOnlyCollection<IEcuLogisticsEntry>(new EcuLogisticsEntry[]
            {
                new EcuLogisticsEntry(0, "ZKE", BusType.KBUS, "D_0000|D_ZKE_GM", 1, 1),
                new EcuLogisticsEntry(8, "SHD", BusType.KBUS, "D_0008", 2, 6),
                new EcuLogisticsEntry(13, "KOMBI", BusType.FACAN, "D_000D", -1, -1),
                new EcuLogisticsEntry(16, "DME", BusType.FACAN, "D_0010", 9, 1),
                new EcuLogisticsEntry(17, "ZKE", BusType.KBUS, "D_0011", 1, 2),
                new EcuLogisticsEntry(18, "DME", BusType.FACAN, "D_MOTOR|D_0012", 9, 1),
                new EcuLogisticsEntry(19, "DME", BusType.FACAN, "D_0013", 9, 1),
                new EcuLogisticsEntry(20, "DME2", BusType.FACAN, "D_0014", 9, 2),
                new EcuLogisticsEntry(21, "DDSHD", BusType.KBUS, "D_0015", 1, 7),
                new EcuLogisticsEntry(22, "TOENS", BusType.UNKNOWN, "D_0016", -1, -1),
                new EcuLogisticsEntry(24, "EGS", BusType.FACAN, "D_EGS", 9, 9),
                new EcuLogisticsEntry(25, "ATCU", BusType.UNKNOWN, "D_0019", -1, -1),
                new EcuLogisticsEntry(32, "EML", BusType.UNKNOWN, "D_0020", -1, -1),
                new EcuLogisticsEntry(33, "ZV", BusType.UNKNOWN, "D_0021", -1, -1),
                new EcuLogisticsEntry(34, "EML", BusType.FACAN, "D_0022", 9, 3),
                new EcuLogisticsEntry(36, "HKM", BusType.UNKNOWN, "D_0024", -1, -1),
                new EcuLogisticsEntry(40, "RCC", BusType.UNKNOWN, "D_0028", -1, -1),
                new EcuLogisticsEntry(70, "CID", BusType.FACAN, "D_CID", 9, 7),
                new EcuLogisticsEntry(44, "VNC", BusType.UNKNOWN, "D_002C", -1, -1),
                new EcuLogisticsEntry(46, "EDC", BusType.UNKNOWN, "D_002E", -1, -1),
                new EcuLogisticsEntry(48, "CCM", BusType.KBUS, "D_0030", 0, 5),
                new EcuLogisticsEntry(49, "EHPS", BusType.FACAN, "D_0031", 9, 4),
                new EcuLogisticsEntry(50, "EGS", BusType.FACAN, "D_0032", 9, 4),
                new EcuLogisticsEntry(52, "VGSG", BusType.FACAN, "D_VGSG", 9, 10),
                new EcuLogisticsEntry(53, "LSM/EPS", BusType.FACAN, "D_0035|D_EPS", 8, 1),
                new EcuLogisticsEntry(54, "ABS", BusType.FACAN, "D_0036", -1, -1),
                new EcuLogisticsEntry(55, "EPS", BusType.FACAN, "D_EPS", -1, -1),
                new EcuLogisticsEntry(59, "VIDEO", BusType.KBUS, "D_003b", 5, 4),
                new EcuLogisticsEntry(64, "FBZV", BusType.KBUS, "D_0040", 0, 6),
                new EcuLogisticsEntry(68, "EWS", BusType.KBUS, "D_0044", 2, 1),
                new EcuLogisticsEntry(69, "DWA", BusType.UNKNOWN, "D_0045", -1, -1),
                new EcuLogisticsEntry(41, "DSC/DXC", BusType.FACAN, "D_ABSKWP", 8, 2),
                new EcuLogisticsEntry(71, "CID", BusType.KBUS, "D_0047", -1, -1),
                new EcuLogisticsEntry(72, "JBIT", BusType.KBUS, "D_0048", -1, -1),
                new EcuLogisticsEntry(80, "MFL", BusType.KBUS, "D_0050", 4, 3),
                new EcuLogisticsEntry(86, "DSC", BusType.FACAN, "D_0056", 9, 5),
                new EcuLogisticsEntry(87, "LWS", BusType.FACAN, "D_0057", 9, 6),
                new EcuLogisticsEntry(89, "HKA", BusType.KBUS, "D_0059", -1, -1),
                new EcuLogisticsEntry(90, "ELV", BusType.UNKNOWN, "D_005A", -1, -1),
                new EcuLogisticsEntry(91, "IHKA", BusType.KBUS, "D_005B", 0, 1),
                new EcuLogisticsEntry(96, "PDC", BusType.KBUS, "D_0060", 2, 3),
                new EcuLogisticsEntry(101, "EKP", BusType.FACAN, "D_0065", 9, 11),
                new EcuLogisticsEntry(102, "ALC", BusType.FACAN, "D_0066", 9, 13),
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
                new EcuLogisticsEntry(161, "SBSL", BusType.BYTEFLIGHT, "D_SBSL2", 3, 5),
                new EcuLogisticsEntry(162, "SBSR", BusType.BYTEFLIGHT, "D_SBSR2", 3, 6),
                new EcuLogisticsEntry(164, "ZAE", BusType.KBUS, new BusType[] {BusType.BYTEFLIGHT}, "D_00a4|D_SIM", 2, 5),
                new EcuLogisticsEntry(166, "GR", BusType.FACAN, "D_00a6", 8, 5),
                new EcuLogisticsEntry(167, "FHK", BusType.UNKNOWN, "D_00a7", -1, -1),
                new EcuLogisticsEntry(172, "EHC2", BusType.FACAN, "D_00AC", 9, 12),
                new EcuLogisticsEntry(173, "STVL", BusType.KBUS, "D_STVL2", 0, 8),
                new EcuLogisticsEntry(174, "STVR", BusType.KBUS, "D_STVR2", 1, 8),
                new EcuLogisticsEntry(176, "RADIO", BusType.KBUS, "D_00b0", -1, -1),
                new EcuLogisticsEntry(185, "IRS", BusType.UNKNOWN, "D_00b9", -1, -1),
                new EcuLogisticsEntry(187, "JNAV", BusType.KBUS, "D_00bb", -1, -1),
                new EcuLogisticsEntry(192, "ZIS", BusType.KBUS, "D_00c0", 0, 7),
                new EcuLogisticsEntry(194, "SVT", BusType.KBUS, "D_00c2", 1, 6),
                new EcuLogisticsEntry(200, "TELEFON", BusType.KBUS, "D_00c8", 4, 5),
                new EcuLogisticsEntry(205, "BC", BusType.KBUS, "D_00cd", 2, 9),
                new EcuLogisticsEntry(206, "RDC/SBE", BusType.BYTEFLIGHT, "D_00ce", -1, -1),
                new EcuLogisticsEntry(208, "LM", BusType.FACAN, "D_00d0", 9, 8),
                new EcuLogisticsEntry(218, "SM", BusType.KBUS, "D_00da", 0, 4),
                new EcuLogisticsEntry(224, "IRIS", BusType.UNKNOWN, "D_00e0", -1, -1),
                new EcuLogisticsEntry(232, "RLS", BusType.KBUS, "D_00e8", 1, 3),
                new EcuLogisticsEntry(234, "DSP_BT", BusType.UNKNOWN, "D_00ea", -1, -1),
                new EcuLogisticsEntry(237, "VIDEO", BusType.KBUS, "D_00ed", 4, 7),
                new EcuLogisticsEntry(240, "CIDB", BusType.KBUS, "D_00f0", 4, 6),
                new EcuLogisticsEntry(245, "LKM", BusType.KBUS, "D_00f5|d_szm", 4, 2)
            });

        // ReSharper restore CoVariantArrayConversion
        // ReSharper restore RedundantExplicitArrayCreation

        private static Dictionary<string, string> _typeKeyDict;

        public static Dictionary<string, string> GetTypeKeyDict(EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Extract type key dict");

            try
            {
                Dictionary<string, string> typeKeyDict = new Dictionary<string, string>();
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, DatabaseFileName)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, "typekeys.txt", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (StreamReader sr = new StreamReader(zipStream))
                                {
                                    while (sr.Peek() >= 0)
                                    {
                                        string line = sr.ReadLine();
                                        if (line == null)
                                        {
                                            break;
                                        }
                                        string[] lineArray = line.Split(',');
                                        if (lineArray.Length == 2)
                                        {
                                            if (!typeKeyDict.ContainsKey(lineArray[0]))
                                            {
                                                typeKeyDict.Add(lineArray[0], lineArray[1]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Extract type key dict done");
                        return typeKeyDict;
                    }
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            catch (Exception ex)
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Extract type key dict exception: {0}", ex.Message);
                return null;
            }
        }

        public static string GetTypeKeyFromVin(string vin, EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type key from VIN: {0}", vin ?? "No VIN");
            if (vin == null)
            {
                return null;
            }
            string serialNumber;
            if (vin.Length == 7)
            {
                serialNumber = vin;
            }
            else if (vin.Length == 17)
            {
                serialNumber = vin.Substring(10, 7);
            }
            else
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "VIN length invalid");
                return null;
            }

            try
            {
                ZipFile zf = null;
                try
                {
                    using (FileStream fs = File.OpenRead(Path.Combine(databaseDir, DatabaseFileName)))
                    {
                        zf = new ZipFile(fs);
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue; // Ignore directories
                            }
                            if (string.Compare(zipEntry.Name, "vinranges.txt", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                Stream zipStream = zf.GetInputStream(zipEntry);
                                using (StreamReader sr = new StreamReader(zipStream))
                                {
                                    while (sr.Peek() >= 0)
                                    {
                                        string line = sr.ReadLine();
                                        if (line == null)
                                        {
                                            break;
                                        }
                                        string[] lineArray = line.Split(',');
                                        if (lineArray.Length == 3 &&
                                            lineArray[0].Length == 7 && lineArray[1].Length == 7)
                                        {
                                            if (string.Compare(serialNumber, lineArray[0], StringComparison.OrdinalIgnoreCase) >= 0 &&
                                                string.Compare(serialNumber, lineArray[1], StringComparison.OrdinalIgnoreCase) <= 0)
                                            {
                                                return lineArray[2];
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Type key not found in vin ranges");
                        return null;
                    }
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            catch (Exception ex)
            {
                ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type key from VIN exception: {0}", ex.Message);
                return null;
            }
        }

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

        public static string GetVehicleTypeFromVin(string vin, EdiabasNet ediabas, string databaseDir)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle type from VIN: {0}", vin ?? "No VIN");
            string typeKey = GetTypeKeyFromVin(vin, ediabas, databaseDir);
            if (typeKey == null)
            {
                return null;
            }
            if (_typeKeyDict == null)
            {
                _typeKeyDict = GetTypeKeyDict(ediabas, databaseDir);
            }
            if (_typeKeyDict == null)
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "No type key dict present");
                return null;
            }
            if (!_typeKeyDict.TryGetValue(typeKey.ToUpperInvariant(), out string vehicleType))
            {
                ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle type not found");
                return null;
            }
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle type: {0}", vehicleType);
            return vehicleType;
        }

        public static ReadOnlyCollection<IEcuLogisticsEntry> GetEcuLogisticsFromVehicleType(string vehicleType, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ECU logistics from vehicle type: {0}", vehicleType ?? "No type");
            if (vehicleType == null)
            {
                return null;
            }
            switch (vehicleType.ToUpperInvariant())
            {
                case "E36":
                    return EcuLogisticsE36;

                case "E38":
                    return EcuLogisticsE38;

                case "E39":
                    return EcuLogisticsE39;

                case "E46":
                    return EcuLogisticsE46;

                case "E52":
                    return EcuLogisticsE52;

                case "E53":
                    return EcuLogisticsE53;

                case "E83":
                    return EcuLogisticsE83;

                case "E85":
                case "E86":
                    return EcuLogisticsE85;
            }
            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle type unknown");
            return null;
        }

        public static string GetVehicleTypeFromBrName(string brName, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Vehicle type from BR name: {0}", brName ?? "No name");
            if (brName == null)
            {
                return null;
            }
            if (string.Compare(brName, "UNBEK", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return null;
            }
            if (brName.Length != 4)
            {
                return null;
            }
            if (brName.EndsWith("_", StringComparison.Ordinal))
            {
                string vehicleType = brName.TrimEnd('_');
                if (Regex.Match(vehicleType, "[ERKHM]\\d\\d").Success)
                {
                    return vehicleType;
                }
            }
            if (brName.StartsWith("RR", StringComparison.OrdinalIgnoreCase))
            {
                string vehicleType = brName.TrimEnd('_');
                if (Regex.Match(vehicleType, "^RR\\d$").Success)
                {
                    return vehicleType;
                }
                if (Regex.Match(vehicleType, "^RR0\\d$").Success)
                {
                    return "RR" + brName.Substring(3, 1);
                }
                if (Regex.Match(vehicleType, "^RR1\\d$").Success)
                {
                    return vehicleType;
                }
            }
            return brName.Substring(0, 1) + brName.Substring(2, 2);
        }

        public static string GetGroupSgbdFromVehicleType(string vehicleType, EdiabasNet ediabas)
        {
            ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group SGBD from vehicle type: {0}", vehicleType ?? "No type");
            if (vehicleType == null)
            {
                return null;
            }
            string typeUpper = vehicleType.ToUpperInvariant();
            if (typeUpper.StartsWith("F") || typeUpper.StartsWith("I"))
            {
                return "f01";
            }
            switch (typeUpper)
            {
                case "E60":
                case "E61":
                case "E62":
                case "E63":
                    return "e60";

                case "E65":
                case "E66":
                case "E67":
                case "E68":
                    return "e65";

                case "E70":
                case "E71":
                case "E72": 
                    return "e70";

                case "M12":
                case "E89X":
                case "E81":
                case "E82":
                case "E84":
                case "E87":
                case "E88":
                case "E89":
                case "E90":
                case "E91":
                case "E92":
                case "E93":
                    return "e89x";

                case "R55":
                case "R56":
                case "R57":
                case "R58":
                case "R59":
                case "R60":
                case "R61":
                    return "r56";
            }
            ediabas?.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle type unknown");
            return null;
        }
    }
}
