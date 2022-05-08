using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public class VehicleLogistics
    {
        private class E46EcuCharacteristics { }
		private class E36EcuCharacteristics { }
		private class E39EcuCharacteristics { }
        private class E38EcuCharacteristics { }
        private class E52EcuCharacteristics { }
		private class E53EcuCharacteristics { }
        private class F01EcuCharacteristics { }
        private class F25_1404EcuCharacteristics { }
        private class F25EcuCharacteristics { }
        private class R50EcuCharacteristics { }
        private class RR6EcuCharacteristics { }
        private class R55EcuCharacteristics { }
        private class RR2EcuCharacteristics { }
        private class RREcuCharacteristics { }
        private class BNT_G11_G12_G3X_SP2015 { }
        private class MRXEcuCharacteristics { }
        private class MREcuCharacteristics { }
        private class E70EcuCharacteristicsAMPT { }
        private class E70EcuCharacteristicsAMPH { }
        private class E70EcuCharacteristics { }
        private class E60EcuCharacteristics { }
        private class E83EcuCharacteristics { }
        private class E85EcuCharacteristics { }
        private class F15EcuCharacteristics { }
        private class F01_1307EcuCharacteristics { }
        private class BNT_G01_G02_G08_F97_F98_SP2015 { }
        private class E89EcuCharacteristics { }
        private class F56EcuCharacteristics { }
        private class F20EcuCharacteristics { }

		private static string GetEcuCharacteristics(string storedXmlFileName, Vehicle vecInfo)
        {
            return storedXmlFileName;
        }

        private static string GetEcuCharacteristics<T>(string storedXmlFileName, Vehicle vecInfo)
        {
            return storedXmlFileName;
        }

        // ToDo: Check on update
        public static string GetCharacteristicsName(Vehicle vecInfo)
		{
            int customHashCode = vecInfo.GetCustomHashCode();
            if (!string.IsNullOrEmpty(vecInfo.Baureihenverbund))
            {
                switch (vecInfo.Baureihenverbund.ToUpper())
                {
                    case "K01X":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
                    case "K024":
                    case "KH24":
                        return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                    case "K001":
                    case "KS01":
                    case "KE01":
                        return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                }
            }
            if (!string.IsNullOrEmpty(vecInfo.Ereihe))
            {
                switch (vecInfo.Ereihe.ToUpper())
                {
                    case "247":
                    case "247E":
                    case "248":
                    case "259":
                    case "259C":
                    case "259E":
                    case "259R":
                    case "259S":
                    case "C01":
                    case "E169":
                    case "E189":
                    case "K14":
                    case "K15":
                    case "K16":
                    case "K30":
                    case "K41":
                    case "K569":
                    case "K589":
                    case "K599":
                    case "R13":
                    case "R21":
                    case "R22":
                    case "R28":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
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
                        return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                    case "E36":
                        return VehicleLogistics.GetEcuCharacteristics<E36EcuCharacteristics>("BNT-XML-E36.xml", vecInfo);
                    case "E38":
                        return VehicleLogistics.GetEcuCharacteristics<E38EcuCharacteristics>("BNT-XML-E38.xml", vecInfo);
                    case "E39":
                        return VehicleLogistics.GetEcuCharacteristics<E39EcuCharacteristics>("BNT-XML-E39.xml", vecInfo);
                    case "E46":
                        return VehicleLogistics.GetEcuCharacteristics<E46EcuCharacteristics>("BNT-XML-E46.xml", vecInfo);
                    case "E52":
                        return VehicleLogistics.GetEcuCharacteristics<E52EcuCharacteristics>("BNT-XML-E52.xml", vecInfo);
                    case "E53":
                        return VehicleLogistics.GetEcuCharacteristics<E53EcuCharacteristics>("BNT-XML-E53.xml", vecInfo);
                    case "E60":
                    case "E61":
                    case "E63":
                    case "E64":
                        return VehicleLogistics.GetEcuCharacteristics<E60EcuCharacteristics>("BNT-XML-E60.xml", vecInfo);
                    case "E65":
                    case "E66":
                    case "E67":
                    case "E68":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-E65.xml", vecInfo);
                    case "E70":
                    case "E71":
                        if (vecInfo.HasAmpt70())
                            return VehicleLogistics.GetEcuCharacteristics<E70EcuCharacteristicsAMPT>("BNT-XML-E70-AMPH70_AMPT70.xml", vecInfo);
                        return vecInfo.HasAmph70() ? VehicleLogistics.GetEcuCharacteristics<E70EcuCharacteristicsAMPH>("BNT-XML-E70-AMPH70_AMPT70.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics<E70EcuCharacteristics>("BNT-XML-E70_NOAMPT_NOAMPH.xml", vecInfo);
                    case "E72":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-E72.xml", vecInfo);
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
                        int num = vecInfo.Baureihenverbund == "E89X" ? 1 : 0;
                        return VehicleLogistics.GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
                    case "E83":
                        return VehicleLogistics.GetEcuCharacteristics<E83EcuCharacteristics>("BNT-XML-E83.xml", vecInfo);
                    case "E85":
                    case "E86":
                        return VehicleLogistics.GetEcuCharacteristics<E85EcuCharacteristics>("BNT-XML-E85.xml", vecInfo);
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
                        if (vecInfo.C_DATETIME.HasValue)
                        {
                            DateTime? cDatetime = vecInfo.C_DATETIME;
                            DateTime dtimeF01Lci = BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.DTimeF01Lci;
                            if ((cDatetime.HasValue ? (cDatetime.GetValueOrDefault() < dtimeF01Lci ? 1 : 0) : 0) == 0)
                            {
                                if (vecInfo.ECU != null)
                                {
                                    ECU ecu = vecInfo.getECU(new long?(16L));
                                    if (ecu != null && ecu.SubBUS != null && ecu.SubBUS.Contains(BusType.MOST))
                                        return VehicleLogistics.GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
                                }
                                return VehicleLogistics.GetEcuCharacteristics<F01_1307EcuCharacteristics>("BNT-XML-F01_1307.xml", vecInfo);
                            }
                        }
                        return VehicleLogistics.GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
                    case "F01BN2K":
                        return VehicleLogistics.GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
                    case "F14":
                    case "F15":
                    case "F16":
                    case "F85":
                    case "F86":
                        return VehicleLogistics.GetEcuCharacteristics<F15EcuCharacteristics>("BNT-XML-F15.xml", vecInfo);
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
                        return VehicleLogistics.GetEcuCharacteristics<F20EcuCharacteristics>("BNT-XML-F20.xml", vecInfo);
                    case "F25":
                        if (vecInfo.C_DATETIME.HasValue)
                        {
                            DateTime? cDatetime = vecInfo.C_DATETIME;
                            DateTime dtimeF25Lci = BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.DTimeF25Lci;
                            if ((cDatetime.HasValue ? (cDatetime.GetValueOrDefault() < dtimeF25Lci ? 1 : 0) : 0) == 0)
                                return VehicleLogistics.GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
                        }
                        return VehicleLogistics.GetEcuCharacteristics<F25EcuCharacteristics>("BNT-XML-F25.xml", vecInfo);
                    case "F26":
                        return VehicleLogistics.GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
                    case "F39":
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
                        return VehicleLogistics.GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
                    case "F40":
                    case "F44":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-F40_F44.xml", vecInfo);
                    case "F56":
                        return vecInfo.IsBev() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-F56BEV.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
                    case "F90":
                    case "G11":
                    case "G12":
                        if (vecInfo.HasHuMgu())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
                        return vecInfo.HasNbtevo() ? VehicleLogistics.GetEcuCharacteristics<BNT_G11_G12_G3X_SP2015>("BNT-XML-G11_G12_G3X_SP2015.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
                    case "F91":
                    case "F92":
                    case "F93":
                    case "G14":
                    case "G15":
                    case "G16":
                        return vecInfo.HasHuMgu() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
                    case "F95":
                    case "F96":
                    case "G05":
                    case "G06":
                    case "G07":
                    case "G18":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G05_G06_G07.xml", vecInfo);
                    case "F97":
                    case "F98":
                    case "G01":
                    case "G02":
                        if (vecInfo.HasEnavevoOrNbtevo())
                            return VehicleLogistics.GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                        return vecInfo.HasHuMgu() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                    case "G08":
                        if (vecInfo.IsBev())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G08BEV.xml", vecInfo);
                        if (vecInfo.HasEnavevo())
                            return VehicleLogistics.GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                        return vecInfo.HasHuMgu() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                    case "G20":
                    case "G21":
                        if (vecInfo.IsPhev())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G21_PHEV.xml", vecInfo);
                        if (vecInfo.HasHuMgu())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                        return vecInfo.HasEnavevo() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "G22":
                    case "G23":
                    case "G42":
                        if (!vecInfo.IsPhev())
                        {
                            if (vecInfo.HasHuMgu())
                                return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                            if (vecInfo.HasEnavevo())
                                return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
                        }
                        return VehicleLogistics.GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "G26":
                    case "G28":
                        if (vecInfo.IsBev())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G26_G28_BEV.xml", vecInfo);
                        if (vecInfo.HasHuMgu())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                        return vecInfo.HasEnavevo() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "G29":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G29.xml", vecInfo);
                    case "G30":
                    case "G31":
                    case "G32":
                    case "G38":
                        if (vecInfo.HasEnavevoOrNbtevo())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_NOMGU.xml", vecInfo);
                        return vecInfo.HasHuMgu() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
                    case "G80":
                    case "G81":
                    case "G82":
                    case "G83":
                    case "G87":
                        return !vecInfo.IsPhev() && vecInfo.HasHuMgu() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "H61":
                    case "H91":
                        return VehicleLogistics.GetEcuCharacteristics("H61EcuCharacteristics.xml", vecInfo);
                    case "I01":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I01.xml", vecInfo);
                    case "I12":
                    case "I15":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I12_I15.xml", vecInfo);
                    case "I20":
                        if (vecInfo.HasMrr30())
                            return VehicleLogistics.GetEcuCharacteristics("BNT-XML-I20_FRS.xml", vecInfo);
                        return vecInfo.HasFrr30v() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-I20_FRSF.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT_I20.xml", vecInfo);
                    case "J29":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-J29.xml", vecInfo);
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
                    case "K61":
                    case "K63":
                    case "K66":
                    case "K67":
                    case "K69":
                    case "K80":
                    case "K81":
                    case "K82":
                    case "K83":
                    case "K84":
                    case "V99":
                    case "X_K001":
                        return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                    case "K07":
                    case "K17":
                        return VehicleLogistics.GetEcuCharacteristics("MRKE01EcuCharacteristics.xml", vecInfo);
                    case "K18":
                    case "K19":
                    case "K21":
                        return VehicleLogistics.getBNType(vecInfo) == BNType.BN2000_MOTORBIKE ? VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                    case "K46":
                        return VehicleLogistics.getBNType(vecInfo) == BNType.BN2020_MOTORBIKE ? VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                    case "M12":
                        return VehicleLogistics.GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
                    case "R50":
                    case "R52":
                    case "R53":
                        return VehicleLogistics.GetEcuCharacteristics<R50EcuCharacteristics>("BNT-XML-R50.xml", vecInfo);
                    case "R55":
                    case "R56":
                    case "R57":
                    case "R58":
                    case "R59":
                    case "R60":
                    case "R61":
                        return VehicleLogistics.GetEcuCharacteristics<R55EcuCharacteristics>("BNT-XML-R55.xml", vecInfo);
                    case "RR1":
                    case "RR2":
                    case "RR3":
                        if (vecInfo.C_DATETIME.HasValue)
                        {
                            DateTime? cDatetime = vecInfo.C_DATETIME;
                            DateTime dtimeRrS2 = BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.DTimeRR_S2;
                            if ((cDatetime.HasValue ? (cDatetime.GetValueOrDefault() < dtimeRrS2 ? 1 : 0) : 0) == 0)
                                return VehicleLogistics.GetEcuCharacteristics<RR2EcuCharacteristics>("BNT-XML-RR2.xml", vecInfo);
                        }
                        return VehicleLogistics.GetEcuCharacteristics<RREcuCharacteristics>("BNT-XML-RR.xml", vecInfo);
                    case "RR11":
                    case "RR12":
                    case "RR21":
                    case "RR22":
                    case "RR31":
                        return VehicleLogistics.GetEcuCharacteristics("BNT-XML-RR1X_RR3X_RRNM.xml", vecInfo);
                    case "RR4":
                    case "RR5":
                    case "RR6":
                        return VehicleLogistics.GetEcuCharacteristics<RR6EcuCharacteristics>("BNT-XML-RR6.xml", vecInfo);
                    case "U06":
                    case "U10":
                    case "U11":
                    case "U12":
                        if (vecInfo.HasMrr30())
                            return !vecInfo.IsPhev() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_FRS.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_PHEV_FRS.xml", vecInfo);
                        if (!vecInfo.HasFrr30v())
                            return VehicleLogistics.GetEcuCharacteristics("BNT_U06-Fallback.xml", vecInfo);
                        return !vecInfo.IsPhev() ? VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_FRSF.xml", vecInfo) : VehicleLogistics.GetEcuCharacteristics("BNT-XML-U06_PHEV_FRSF.xml", vecInfo);
                    default:
                        //Log.Info("VehicleLogistics.GetCharacteristics()", "cannot retrieve bordnet configuration using ereihe");
                        break;
                }
            }
            switch (vecInfo.BNType)
            {
                case BNType.IBUS:
                    return VehicleLogistics.GetEcuCharacteristics("iBusEcuCharacteristics.xml", vecInfo);
                case BNType.BN2000_MOTORBIKE:
                    return VehicleLogistics.GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                case BNType.BN2020_MOTORBIKE:
                    return VehicleLogistics.GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                case BNType.BNK01X_MOTORBIKE:
                    return VehicleLogistics.GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
                case BNType.BN2000_WIESMANN:
                    return VehicleLogistics.GetEcuCharacteristics("WiesmannEcuCharacteristics.xml", vecInfo);
                case BNType.BN2000_RODING:
                    return VehicleLogistics.GetEcuCharacteristics("RodingEcuCharacteristics.xml", vecInfo);
                case BNType.BN2000_PGO:
                    return VehicleLogistics.GetEcuCharacteristics("PGOEcuCharacteristics.xml", vecInfo);
                case BNType.BN2000_GIBBS:
                    return VehicleLogistics.GetEcuCharacteristics("GibbsEcuCharacteristics.xml", vecInfo);
                case BNType.BN2020_CAMPAGNA:
                    return VehicleLogistics.GetEcuCharacteristics("CampagnaEcuCharacteristics.xml", vecInfo);
                default:
                    return null;
            }
        }

		// ToDo: Check on update
		public static BNType getBNType(Vehicle vecInfo)
		{
			if (vecInfo == null)
			{
				//Log.Warning("VehicleLogistics.getBNType()", "vehicle was null");
				return BNType.UNKNOWN;
			}
			if (string.IsNullOrEmpty(vecInfo.Ereihe))
				return BNType.UNKNOWN;
			switch (vecInfo.Ereihe.ToUpper())
			{
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
				case "K30":
				case "K41":
				case "K569":
				case "K589":
				case "K599":
				case "R21":
				case "R22":
				case "R28":
					return BNType.BNK01X_MOTORBIKE;
				case "A67":
				case "C01":
				case "H61":
				case "H91":
				case "K14":
				case "K15":
				case "K16":
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
				case "R13":
					return BNType.BN2000_MOTORBIKE;
				case "AERO":
					return BNType.BN2000_MORGAN;
				case "E36":
				case "E38":
				case "E39":
				case "E46":
				case "E52":
				case "E53":
				case "E83":
				case "E85":
				case "E86":
				case "R50":
				case "R52":
				case "R53":
					return BNType.IBUS;
				case "E60":
				case "E61":
				case "E62":
				case "E63":
				case "E64":
				case "E65":
				case "E66":
				case "E67":
				case "E68":
				case "E70":
				case "E71":
				case "E72":
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
					return BNType.BN2000;
				case "F01":
				case "F02":
				case "F03":
				case "F04":
				case "F05":
				case "F06":
				case "F07":
				case "F10":
				case "F11":
				case "F12":
				case "F13":
				case "F14":
				case "F15":
				case "F16":
				case "F18":
				case "F20":
				case "F21":
				case "F22":
				case "F23":
				case "F25":
				case "F26":
				case "F30":
				case "F31":
				case "F32":
				case "F33":
				case "F34":
				case "F35":
				case "F39":
				case "F40":
				case "F44":
				case "F45":
				case "F46":
				case "F48":
				case "F49":
				case "F52":
				case "F54":
				case "F55":
				case "F56":
				case "F57":
				case "F60":
				case "F80":
				case "F81":
				case "F82":
				case "F83":
				case "F85":
				case "F86":
				case "F87":
				case "F93":
				case "F95":
				case "F96":
				case "F97":
				case "F98":
				case "G01":
				case "G02":
				case "G05":
				case "G06":
				case "G07":
				case "G08":
				case "G15":
				case "G16":
				case "G20":
				case "G21":
				case "G28":
				case "G29":
				case "I01":
				case "I12":
				case "I15":
				case "I20":
				case "J29":
				case "M13":
					return BNType.BN2020;
				case "G11":
				case "G12":
				case "G14":
				case "G30":
				case "G31":
				case "G32":
				case "G38":
				case "RR11":
				case "RR12":
				case "RR21":
				case "RR22":
				case "RR31":
					return BNType.BN2020;
				case "GT1":
					return BNType.BN2000_GIBBS;
				case "K02":
				case "K03":
				case "K07":
				case "K08":
				case "K09":
				case "K17":
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
				case "K61":
				case "K63":
				case "K66":
				case "K67":
				case "K69":
				case "K80":
				case "K81":
				case "K82":
				case "K83":
				case "K84":
					return BNType.BN2020_MOTORBIKE;
				case "K18":
					return !string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0C04") || vecInfo.VINType.Equals("0C14")) ? BNType.BN2020_MOTORBIKE : BNType.BN2000_MOTORBIKE;
				case "K19":
					return !string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0C05") || vecInfo.VINType.Equals("0C15")) ? BNType.BN2020_MOTORBIKE : BNType.BN2000_MOTORBIKE;
				case "K21":
					return !string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0A06") || vecInfo.VINType.Equals("0A16")) ? BNType.BN2000_MOTORBIKE : BNType.BN2020_MOTORBIKE;
				case "K46":
					return !string.IsNullOrEmpty(vecInfo.VINType) && !"XXXX".Equals(vecInfo.VINType, StringComparison.OrdinalIgnoreCase) && (vecInfo.VINType.Equals("0D10") || vecInfo.VINType.Equals("0D21") || vecInfo.VINType.Equals("0D30") || vecInfo.VINType.Equals("0D40") || vecInfo.VINType.Equals("0D50") || vecInfo.VINType.Equals("0D60") || vecInfo.VINType.Equals("0D70") || vecInfo.VINType.Equals("0D80") || vecInfo.VINType.Equals("0D90")) ? BNType.BN2020_MOTORBIKE : BNType.BN2000_MOTORBIKE;
				case "M12":
					return BNType.BEV2010;
				case "MF25":
				case "MF28":
				case "MF3":
				case "MF30":
				case "MF35":
				case "MF4":
				case "MF4-S":
				case "MF5":
					return BNType.BN2000_WIESMANN;
				case "N18":
					return BNType.BN2000_PGO;
				case "R55":
				case "R56":
				case "R57":
				case "R58":
				case "R59":
				case "R60":
				case "R61":
					return BNType.BN2000;
				case "RODING_ROADSTER":
					return BNType.BN2000_RODING;
				case "RR1":
				case "RR2":
				case "RR3":
					return BNType.BN2000;
				case "RR4":
				case "RR5":
				case "RR6":
					return BNType.BN2020;
				case "U06":
				case "U11":
					return BNType.BN2020;
				case "V98":
					return BNType.BN2000_MOTORBIKE;
				case "V99":
					return BNType.BN2020_CAMPAGNA;
				default:
					return !vecInfo.Ereihe.ToUpper().StartsWith("F", StringComparison.Ordinal) && !vecInfo.Ereihe.ToUpper().StartsWith("G", StringComparison.Ordinal) && !vecInfo.Ereihe.ToUpper().StartsWith("U", StringComparison.Ordinal) ? BNType.UNKNOWN : BNType.BN2020;
			}
		}
	}
}
