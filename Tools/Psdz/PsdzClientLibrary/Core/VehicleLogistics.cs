using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Utility;

namespace PsdzClient.Core
{
    public class VehicleLogistics
    {
        private static ConcurrentDictionary<object, BaseEcuCharacteristics> ecuCharacteristics = new ConcurrentDictionary<object, BaseEcuCharacteristics>();
        [PreserveSource(Hint = "Modified")]
        private static Lazy<bool> isGui = new Lazy<bool>(false);
        public const string FallbackBordnetName = "BNT-XML-FALLBACK.xml";
        public static BaseEcuCharacteristics CallGetCharacteristics(Vehicle vecInfo)
        {
            return GetCharacteristics(vecInfo);
        }

        public static void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning("VehicleLogistics.CalculateECUConfiguration()", "vecInfo was null");
                return;
            }

            if (vecInfo.BNType == BNType.UNKNOWN)
            {
                Log.Warning("VehicleLogistics.CalculateECUConfiguration()", "BNType was unknown");
                return;
            }

            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                characteristics.CalculateECUConfiguration(vecInfo, ffmResolver);
            }
            else
            {
                Log.Warning("vehicleLogistics.CaCalculateECUConfiguration()", "no fitting BaseEcuCharacteristics found");
            }
        }

        public static void ShapeECUConfiguration(Vehicle vecInfo)
        {
            if ("E36,E38,E46,E53,E83,E85,R50".Contains(vecInfo.Ereihe))
            {
                ValidateIfDiagnosticsHasValidLicense();
                GetCharacteristics(vecInfo)?.ShapeECUConfiguration(vecInfo);
            }
        }

        [PreserveSource(Hint = "XEP_SALAPAS replaced")]
        public static ObservableCollectionEx<PsdzDatabase.SaLaPa> GetAvailableSALAPAs(Vehicle vecInfo)
        {
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                return characteristics.GetAvailableSALAPAs(vecInfo);
            }

            return new ObservableCollectionEx<PsdzDatabase.SaLaPa>();
        }

        public static ICollection<IBusLogisticsEntry> GetBusTable(Vehicle vecInfo)
        {
            return GetCharacteristics(vecInfo)?.GetBusTable();
        }

        public static string GetBusAlias(Vehicle vecInfo, BusType bus)
        {
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                return characteristics.GetBusAlias(bus);
            }

            return bus.ToString();
        }

        public static ICollection<ICombinedEcuHousingEntry> GetCombinedEcuHousingTable(Vehicle vecInfo)
        {
            return GetCharacteristics(vecInfo)?.GetCombinedEcuHousingTable();
        }

        public static double? GetRootHorizontalBusStep(Vehicle vecInfo)
        {
            return GetCharacteristics(vecInfo)?.rootHorizontalBusStep;
        }

        public static bool CasNeedsZgmRepair(Vehicle vecInfo)
        {
            string baureihenverbund = vecInfo.Baureihenverbund;
            string[] source = new string[3]
            {
                "F001",
                "F010",
                "F025"
            };
            bool flag = !string.IsNullOrEmpty(baureihenverbund) && source.Contains(baureihenverbund);
            Log.Info("VehicleLogistics.CasNeedsZgmRepair()", "Return {0} for BRV=\"{1}\".", flag, baureihenverbund);
            return flag;
        }

        public static IEcuLogisticsEntry GetEcuLogisticsEntry(Vehicle vecInfo, IEcu ecu)
        {
            return GetCharacteristics(vecInfo)?.GetEcuLogisticsEntry(vecInfo, ecu);
        }

        public static ICollection<IBusInterConnectionEntry> GetInterConnectionTable(Vehicle vecInfo)
        {
            return GetCharacteristics(vecInfo)?.GetInterConnectionTable();
        }

        public static bool HasBus(BusType busType, Vehicle vecInfo, ECU ecu)
        {
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (vecInfo == null)
            {
                Log.Warning("VehicleLogistics.HasBus()", "vecInfo was null");
                return false;
            }

            if (ecu == null)
            {
                Log.Warning("VehicleLogistics.HasBus()", "ecu was null");
                return false;
            }

            if (busType.Equals(ecu.BUS) || (ecu.SubBUS != null && ecu.SubBUS.Contains(busType)))
            {
                return true;
            }

            return characteristics?.HasBus(busType, vecInfo, ecu) ?? false;
        }

        public static string getBrSgbd(Vehicle vecInfo)
        {
            IDiagnosticsBusinessData service = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            ValidateIfDiagnosticsHasValidLicense();
            string text = service.GetMainSeriesSgbd(vecInfo);
            if (string.IsNullOrEmpty(text))
            {
                text = GetCharacteristics(vecInfo)?.brSgbd;
            }

            return text;
        }

        public static BusType getECUBus(Vehicle vecInfo, long? iD_SG_ADR, string ecuGroup = null)
        {
            ValidateIfDiagnosticsHasValidLicense();
            return GetCharacteristics(vecInfo)?.GetBus(iD_SG_ADR, vecInfo.VCI?.VCIType, ecuGroup) ?? BusType.UNKNOWN;
        }

        public static bool getECUTreeCoordinates(Vehicle vecInfo, ECU ecu, out int column, out int row)
        {
            ValidateIfDiagnosticsHasValidLicense();
            column = -1;
            row = -1;
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                characteristics.GetEcuCoordinates(ecu.ID_SG_ADR, ecu.ID_LIN_SLAVE_ADR, out column, out row);
                return true;
            }

            return false;
        }

        public static string getECU_GROBNAME(Vehicle vecInfo, long? sgAdr)
        {
            ValidateIfDiagnosticsHasValidLicense();
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                return characteristics.GetECU_GROBNAME(sgAdr);
            }

            return string.Empty;
        }

        public static string getECU_GROBNAMEByEcuGroup(Vehicle vecInfo, string ecuGroup)
        {
            ValidateIfDiagnosticsHasValidLicense();
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                return characteristics.GetECU_GROBNAMEByEcuGroup(ecuGroup);
            }

            return string.Empty;
        }

        public static string getFASTAConfig(string produktart)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (string.IsNullOrEmpty(produktart))
            {
                return string.Empty;
            }

            string text = produktart.ToUpper();
            if (!(text == "M"))
            {
                if (text == "P")
                {
                    return "fasta6_pkw.cfg";
                }

                return null;
            }

            return "fasta6_ux.cfg";
        }

        private static BaseEcuCharacteristics GetCharacteristics(Vehicle vecInfo)
        {
            int customHashCode = vecInfo.GetCustomHashCode();
            if (ecuCharacteristics.TryGetValue(customHashCode, out var value))
            {
                return value;
            }

            if (!string.IsNullOrEmpty(vecInfo.Baureihenverbund))
            {
                switch (vecInfo.Baureihenverbund.ToUpper())
                {
                    case "K01X":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "K024":
                    case "KH24":
                        return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "K001":
                    case "KS01":
                    case "KE01":
                        return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                }
            }

            IDiagnosticsBusinessData service = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
            if (!string.IsNullOrEmpty(vecInfo.Ereihe))
            {
                switch (vecInfo.Ereihe.ToUpper())
                {
                    case "F01":
                    case "F02":
                    case "F03":
                    case "F04":
                    case "F06":
                    case "F07":
                    case "F10":
                    case "F11":
                    case "F12":
                    case "F13":
                    case "F18":
                        if (!vecInfo.C_DATETIME.HasValue || vecInfo.C_DATETIME < service.DTimeF01Lci)
                        {
                            return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        if (vecInfo.ECU != null)
                        {
                            ECU eCU = vecInfo.getECU(16L);
                            if (eCU != null && eCU.SubBUS != null && eCU.SubBUS.Contains(BusType.MOST))
                            {
                                return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                            }
                        }

                        return GetEcuCharacteristics<F01_1307EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F14":
                    case "F15":
                    case "F16":
                    case "F85":
                    case "F86":
                        return GetEcuCharacteristics<F15EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F25":
                        if (!vecInfo.C_DATETIME.HasValue || vecInfo.C_DATETIME < service.DTimeF25Lci)
                        {
                            return GetEcuCharacteristics<F25EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F26":
                        return GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F20":
                    case "F21":
                    case "F22":
                    case "F23":
                    case "F30":
                    case "F31":
                    case "F32":
                    case "F33":
                    case "F34":
                    case "F35":
                    case "F36":
                    case "F80":
                    case "F81":
                    case "F82":
                    case "F83":
                    case "F87":
                        return GetEcuCharacteristics<F20EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F01BN2K":
                        return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "RR4":
                    case "RR5":
                    case "RR6":
                        return GetEcuCharacteristics<RR6EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F40":
                    case "F44":
                    case "G22":
                    case "G23":
                    case "G42":
                    case "G80":
                    case "G81":
                    case "G82":
                    case "G83":
                    case "G87":
                    case "G20":
                    case "G21":
                    case "G26":
                    case "G28":
                    case "G29":
                    case "J29":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F45":
                    case "F46":
                    case "F47":
                    case "F48":
                    case "F49":
                    case "F52":
                    case "F54":
                    case "F55":
                    case "F57":
                    case "F60":
                    case "M13":
                    case "F39":
                        return GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F56":
                        if (vecInfo.IsBev())
                        {
                            return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "I01":
                    case "I12":
                    case "I15":
                    case "I20":
                    case "U06":
                    case "U10":
                    case "U11":
                    case "U12":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "G01":
                    case "G02":
                    case "F97":
                    case "F98":
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "G08":
                        if (vecInfo.IsBev() || vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "G30":
                    case "G31":
                    case "G32":
                    case "G38":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "F90":
                    case "G11":
                    case "G12":
                        if (vecInfo.HasNbtevo())
                        {
                            return GetEcuCharacteristics<BNT_G11_G12_G3X_SP2015>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "G05":
                    case "G06":
                    case "G07":
                    case "G18":
                    case "F95":
                    case "F96":
                    case "G14":
                    case "G15":
                    case "G16":
                    case "F91":
                    case "F92":
                    case "F93":
                    case "RR11":
                    case "RR12":
                    case "RR31":
                    case "RR21":
                    case "RR22":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "M12":
                        return GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
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
                        return GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E60":
                    case "E61":
                    case "E63":
                    case "E64":
                        return GetEcuCharacteristics<E60EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "R55":
                    case "R56":
                    case "R57":
                    case "R58":
                    case "R59":
                    case "R60":
                    case "R61":
                        return GetEcuCharacteristics<R55EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E70":
                    case "E71":
                        if (vecInfo.HasAmpt70())
                        {
                            return GetEcuCharacteristics<E70EcuCharacteristicsAMPT>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        if (vecInfo.HasAmph70())
                        {
                            return GetEcuCharacteristics<E70EcuCharacteristicsAMPH>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<E70EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E72":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "RR1":
                    case "RR2":
                    case "RR3":
                        if (!vecInfo.C_DATETIME.HasValue || vecInfo.C_DATETIME < service.DTimeRR_S2)
                        {
                            return GetEcuCharacteristics<RREcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<RR2EcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E65":
                    case "E66":
                    case "E67":
                    case "E68":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E83":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E46":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E85":
                    case "E86":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E52":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E53":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E36":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E38":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E39":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "R50":
                    case "R52":
                    case "R53":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "K18":
                    case "K19":
                    case "K21":
                        if (service.GetBNType(vecInfo) == BNType.BN2000_MOTORBIKE)
                        {
                            return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "A67":
                    case "K25":
                    case "K26":
                    case "K27":
                    case "K28":
                    case "K29":
                    case "K40":
                    case "K42":
                    case "K43":
                    case "K44":
                    case "K70":
                    case "K71":
                    case "K72":
                    case "K73":
                    case "K75":
                    case "MRK24":
                    case "V98":
                        return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "K46":
                        if (service.GetBNType(vecInfo) == BNType.BN2020_MOTORBIKE)
                        {
                            return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                        }

                        return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "H61":
                    case "H91":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "C01":
                    case "247":
                    case "247E":
                    case "248":
                    case "259":
                    case "259C":
                    case "259E":
                    case "259R":
                    case "259S":
                    case "E169":
                    case "E189":
                    case "K14":
                    case "K15":
                    case "K16":
                    case "K41":
                    case "K569":
                    case "K589":
                    case "K599":
                    case "R13":
                    case "R21":
                    case "R22":
                    case "R28":
                    case "K30":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "X_K001":
                    case "K02":
                    case "K03":
                    case "K08":
                    case "K09":
                    case "K22":
                    case "K23":
                    case "K32":
                    case "K33":
                    case "K34":
                    case "K35":
                    case "K47":
                    case "K48":
                    case "K49":
                    case "K50":
                    case "K51":
                    case "K52":
                    case "K53":
                    case "K54":
                    case "K60":
                    case "K63":
                    case "K66":
                    case "K80":
                    case "K81":
                    case "K82":
                    case "K83":
                    case "K84":
                    case "K61":
                    case "K67":
                    case "K69":
                    case "V99":
                        return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                    case "K07":
                    case "K17":
                        return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    case "E30":
                    case "E31":
                    case "E32":
                    case "E34":
                        return GetEcuCharacteristicsFromFallback("iBusEcuCharacteristics.xml", vecInfo);
                }

                Log.Info("VehicleLogistics.GetCharacteristics()", "cannot retrieve bordnet configuration using ereihe");
            }

            switch (vecInfo.BNType)
            {
                case BNType.BN2000_MOTORBIKE:
                    return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                case BNType.BNK01X_MOTORBIKE:
                    return GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                case BNType.BN2020_MOTORBIKE:
                    return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-FALLBACK.xml", vecInfo);
                case BNType.IBUS:
                    return GetEcuCharacteristics("iBusEcuCharacteristics.xml", vecInfo);
                default:
                {
                    BaseEcuCharacteristics baseEcuCharacteristics = GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                    if (baseEcuCharacteristics != null)
                    {
                        return baseEcuCharacteristics;
                    }

                    Log.Warning("VehicleLogistics.GetCharacteristics()", $"No configuration found for vehicle with ereihe: {vecInfo.Ereihe}, bn type: {vecInfo.BNType}");
                    return null;
                }
            }
        }

        public static void CalculateMaxAssembledECUList(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            if (vecInfo == null)
            {
                Log.Warning("VehicleLogistics.CalculateECUConfiguration()", "vecInfo was null");
                return;
            }

            if (vecInfo.BNType == BNType.UNKNOWN)
            {
                Log.Warning("VehicleLogistics.CalculateECUConfiguration()", "BNType was unknown");
                return;
            }

            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                characteristics.CalculateMaxAssembledECUList(vecInfo, ffmResolver);
            }
            else
            {
                Log.Warning("VehicleLogistics.CalculateMaxAssembledECUList()", "no fitting BaseEcuCharacteristics found");
            }
        }

        [PreserveSource(Hint = "vehicle added")]
        public static void DecodeVCMBackupFA(byte[] faAsByteArray, Vehicle vehicle)
        {
            if (faAsByteArray == null || faAsByteArray.Length < 160)
            {
                Log.Warning("VehicleLogistics.DecodeVCMBackupFA()", "fa byte stream was null or too short");
                return;
            }

            FA fA = new FA();
            try
            {
                fA.C_DATE = FormatConverter.Convert6BitNibblesTo4DigitString(faAsByteArray, 1u);
                fA.BR = FormatConverter.Convert6BitNibblesTo4DigitString(faAsByteArray, 4u);
                fA.TYPE = FormatConverter.Convert6BitNibblesTo4DigitString(faAsByteArray, 7u);
                fA.LACK = FormatConverter.Convert6BitNibblesTo4DigitString(faAsByteArray, 10u);
                fA.POLSTER = FormatConverter.Convert6BitNibblesTo4DigitString(faAsByteArray, 13u);
                string text = string.Empty;
                for (int i = 16; i < faAsByteArray.Length; i++)
                {
                    text += Convert.ToString(faAsByteArray[i], 2).PadLeft(8, '0');
                }

                string value = text.Substring(0, 4);
                string text2 = text;
                if ("1000".Equals(value))
                {
                    text2 = text.Substring(4, text.Length - 4);
                    int j;
                    for (j = 0; j < text2.Length; j += 18)
                    {
                        byte b = Convert.ToByte(text2.Substring(j, 6), 2);
                        if ((b & 0xF0) == 0)
                        {
                            j += 2;
                            break;
                        }

                        byte inChar = Convert.ToByte(text2.Substring(j + 6, 6), 2);
                        byte inChar2 = Convert.ToByte(text2.Substring(j + 12, 6), 2);
                        string item = $"{FormatConverter.DecodeFAChar((char)b)}{FormatConverter.DecodeFAChar((char)inChar)}{FormatConverter.DecodeFAChar((char)inChar2)}";
                        fA.SA.AddIfNotContains(item);
                    }

                    text2 = text2.Substring(j, text2.Length - j);
                }

                value = text2.Substring(0, 4);
                text2 = text2.Substring(4, text2.Length - 4);
                if ("0100".Equals(value))
                {
                    int j;
                    for (j = 0; j < text2.Length; j += 24)
                    {
                        byte b2 = Convert.ToByte(text2.Substring(j, 6), 2);
                        if ((b2 & 0xF0) == 0)
                        {
                            j += 2;
                            break;
                        }

                        byte inChar3 = Convert.ToByte(text2.Substring(j + 6, 6), 2);
                        byte inChar4 = Convert.ToByte(text2.Substring(j + 12, 6), 2);
                        byte inChar5 = Convert.ToByte(text2.Substring(j + 18, 6), 2);
                        string item2 = $"{FormatConverter.DecodeFAChar((char)b2)}{FormatConverter.DecodeFAChar((char)inChar3)}{FormatConverter.DecodeFAChar((char)inChar4)}{FormatConverter.DecodeFAChar((char)inChar5)}";
                        fA.E_WORT.AddIfNotContains(item2);
                    }

                    text2 = text2.Substring(j, text2.Length - j);
                }

                value = text2.Substring(0, 4);
                text2 = text2.Substring(4, text2.Length - 4);
                if ("1100".Equals(value))
                {
                    int j;
                    for (j = 0; j < text2.Length; j += 24)
                    {
                        byte b3 = Convert.ToByte(text2.Substring(j, 6), 2);
                        if ((b3 & 0xF0) == 0)
                        {
                            j += 2;
                            break;
                        }

                        byte inChar6 = Convert.ToByte(text2.Substring(j + 6, 6), 2);
                        byte inChar7 = Convert.ToByte(text2.Substring(j + 12, 6), 2);
                        byte inChar8 = Convert.ToByte(text2.Substring(j + 18, 6), 2);
                        string item3 = $"{FormatConverter.DecodeFAChar((char)b3)}{FormatConverter.DecodeFAChar((char)inChar6)}{FormatConverter.DecodeFAChar((char)inChar7)}{FormatConverter.DecodeFAChar((char)inChar8)}";
                        fA.HO_WORT.AddIfNotContains(item3);
                    }

                    text2 = text2.Substring(j, text2.Length - j);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("VehicleLogistics.DecodeVCMBackupFA()", exception);
            }

            string text3 = string.Format(CultureInfo.InvariantCulture, "{0}#{1}*{2}%{3}&{4}", fA.BR, fA.C_DATE, fA.TYPE, fA.LACK, fA.POLSTER);
            foreach (string item4 in fA.SA)
            {
                text3 += string.Format(CultureInfo.InvariantCulture, "${0}", item4);
            }

            foreach (string item5 in fA.E_WORT)
            {
                text3 += string.Format(CultureInfo.InvariantCulture, "-{0}", item5);
            }

            foreach (string item6 in fA.HO_WORT)
            {
                text3 += string.Format(CultureInfo.InvariantCulture, "+{0}", item6);
            }

            fA.STANDARD_FA = text3;
            fA.SA_ANZ = (short)fA.SA.Count;
            fA.E_WORT_ANZ = (short)fA.HO_WORT.Count;
            fA.HO_WORT_ANZ = (short)fA.E_WORT.Count;
            fA.ZUSBAU_ANZ = 0;
            fA.AlreadyDone = true;
            vehicle.FA = fA; // [UH] [IGNORE] replaced
        }

        [PreserveSource(Hint = "Unchanged")]
        private static BaseEcuCharacteristics GetEcuCharacteristics(string storedXmlFileName, Vehicle vecInfo)
        {
            return GetEcuCharacteristics<GenericEcuCharacteristics>(storedXmlFileName, vecInfo);
        }

        [PreserveSource(Hint = "Database replaced")]
        public static BaseEcuCharacteristics GetEcuCharacteristics<T>(string storedXmlFileName, Vehicle vecInfo)
            where T : BaseEcuCharacteristics
        {
            Log.Info(Log.CurrentMethod(), $"Reading bordnet configuration with ereihe: {vecInfo.Ereihe}, bn type: {vecInfo.BNType}, target type: {typeof(T).Name}. The fallback xml file is: {storedXmlFileName}");
            PsdzDatabase database = ClientContext.GetDatabase(vecInfo);
            if (database == null)
            {
                return null;
            }

            try
            {
                PsdzDatabase.BordnetsData bordnetsData = ((!isGui.Value || vecInfo.BordnetsData == null) ? GetBordnetXmlFromDatabase(vecInfo) : vecInfo.BordnetsData);
                if (bordnetsData != null && !string.IsNullOrWhiteSpace(bordnetsData.DocData))
                {
                    BaseEcuCharacteristics baseEcuCharacteristics = CreateCharacteristicsInstance<GenericEcuCharacteristics>(vecInfo, bordnetsData.DocData, bordnetsData.InfoObjIdent);
                    if (baseEcuCharacteristics != null)
                    {
                        vecInfo.BordnetsData = bordnetsData;
                        return baseEcuCharacteristics;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMissingDatabaseBordnet(vecInfo, ex.Message);
            }

            if (!string.IsNullOrEmpty(storedXmlFileName))
            {
                string xml = database.GetEcuCharacteristicsXml(storedXmlFileName);
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    BaseEcuCharacteristics ecuCharacteristicsFromFallback = GetEcuCharacteristicsFromFallback<T>(storedXmlFileName, vecInfo);
                    if (ecuCharacteristicsFromFallback != null)
                    {
                        return ecuCharacteristicsFromFallback;
                    }
                }
            }

            Log.Error(Log.CurrentMethod(), "No bordnet could be loaded.");
            return null;
        }

        private static BaseEcuCharacteristics GetEcuCharacteristicsFromFallback(string storedXmlFileName, Vehicle vecInfo)
        {
            Log.Info(Log.CurrentMethod(), "Using fallback xml file: " + storedXmlFileName);
            return CreateCharacteristicsInstance<GenericEcuCharacteristics>(vecInfo, storedXmlFileName, storedXmlFileName);
        }

        private static BaseEcuCharacteristics GetEcuCharacteristicsFromFallback<T>(string storedXmlFileName, Vehicle vecInfo)
            where T : BaseEcuCharacteristics
        {
            Log.Info(Log.CurrentMethod(), "Using fallback xml file: " + storedXmlFileName);
            return CreateCharacteristicsInstance<T>(vecInfo, storedXmlFileName, storedXmlFileName);
        }

        [PreserveSource(Hint = "Changed to public")]
        public static BaseEcuCharacteristics CreateCharacteristicsInstance<T>(Vehicle vehicle, string xml, string name)
            where T : BaseEcuCharacteristics
        {
            try
            {
                Type typeFromHandle = typeof(T);
                object[] args = new string[1]
                {
                    xml
                };
                T val = (T)Activator.CreateInstance(typeFromHandle, args);
                if (val != null)
                {
                    ecuCharacteristics.TryAdd(vehicle.GetCustomHashCode(), val);
                    val.BordnetName = name;
                    Log.Info(Log.CurrentMethod(), "Found characteristics with group sgdb: " + val.brSgbd);
                }

                return val;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        private static void LogMissingDatabaseBordnet(Vehicle vec, string error)
        {
            string text = (ConfigSettings.IsOssModeActive ? "JA" : "NEIN");
            string text2 = "BRV: " + vec.Baureihenverbund + ", E-Reihe: " + vec.Ereihe + ", Error: " + error + ", AOS: " + text;
            string bNT01_BnTopologieNotFound_nu_LF = ServiceCodes.BNT01_BnTopologieNotFound_nu_LF;
            Log.Info(Log.CurrentMethod(), bNT01_BnTopologieNotFound_nu_LF + ": " + text2);
            IFasta2Service service = ServiceLocator.Current.GetService<IFasta2Service>();
            if (service != null)
            {
                service.AddServiceCode(bNT01_BnTopologieNotFound_nu_LF, text2, LayoutGroup.X);
            }
            else
            {
                Log.Error(Log.CurrentMethod(), "IFasta2Service could not be fetched from the ServiceLocator");
            }
        }

        [PreserveSource(Hint = "Modified")]
        private static PsdzDatabase.BordnetsData GetBordnetXmlFromDatabase(Vehicle vecInfo)
        {
            Log.Info(Log.CurrentMethod(), "Reading bordnet configuration from the database");
            string text = "Es gibt zu viele gültige Bordnetze: ";
            try
            {
                PsdzDatabase database = ClientContext.GetDatabase(vecInfo);
                if (database == null)
                {
                    return null;
                }

                List<PsdzDatabase.BordnetsData> collection = database.LoadBordnetsData(vecInfo);
                if (collection != null && collection.Count == 1)
                {
                    return collection.First();
                }

                if (collection != null && collection.Count > 1)
                {
                    foreach (PsdzDatabase.BordnetsData item in collection)
                    {
                        text = text + item.InfoObjIdent + " ";
                    }

                    throw new Exception(text);
                }
            }
            catch (Exception ex)
            {
                Log.Error(Log.CurrentMethod(), $"Reading bordnet configuration from the database failed: {ex}");
                throw;
            }

            return null;
        }

        [PreserveSource(Hint = "Cleaned")]
        private static void ValidateIfDiagnosticsHasValidLicense()
        {
        }

        [PreserveSource(Hint = "Added")]
        public static BaseEcuCharacteristics GetCharacteristicsPublic(Vehicle vecInfo)
        {
            return GetCharacteristics(vecInfo);
        }
    }
}