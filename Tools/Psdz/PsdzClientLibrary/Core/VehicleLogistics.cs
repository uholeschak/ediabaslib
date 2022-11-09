using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BmwFileReader;
using PsdzClient.Core;
using PsdzClient.Utility;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public class VehicleLogistics
    {
        public class E46EcuCharacteristics : BaseEcuCharacteristics { }
        public class E36EcuCharacteristics : BaseEcuCharacteristics { }
        public class E39EcuCharacteristics : BaseEcuCharacteristics { }
        public class E38EcuCharacteristics : BaseEcuCharacteristics { }
        public class E52EcuCharacteristics : BaseEcuCharacteristics { }
        public class E53EcuCharacteristics : BaseEcuCharacteristics { }
        public class F01EcuCharacteristics : BaseEcuCharacteristics { }
        public class F25_1404EcuCharacteristics : BaseEcuCharacteristics { }
        public class F25EcuCharacteristics : BaseEcuCharacteristics { }
        public class R50EcuCharacteristics : BaseEcuCharacteristics { }
        public class RR6EcuCharacteristics : BaseEcuCharacteristics { }
        public class R55EcuCharacteristics : BaseEcuCharacteristics { }
        public class RR2EcuCharacteristics : BaseEcuCharacteristics { }
        public class RREcuCharacteristics : BaseEcuCharacteristics { }
        public class BNT_G11_G12_G3X_SP2015 : BaseEcuCharacteristics { }
        public class MRXEcuCharacteristics : BaseEcuCharacteristics { }
        public class MREcuCharacteristics : BaseEcuCharacteristics { }
        public class E70EcuCharacteristicsAMPT : BaseEcuCharacteristics { }
        public class E70EcuCharacteristicsAMPH : BaseEcuCharacteristics { }
        public class E70EcuCharacteristics : BaseEcuCharacteristics { }
        public class E60EcuCharacteristics : BaseEcuCharacteristics { }
        public class E83EcuCharacteristics : BaseEcuCharacteristics { }
        public class E85EcuCharacteristics : BaseEcuCharacteristics { }
        public class F15EcuCharacteristics : BaseEcuCharacteristics { }
        public class F01_1307EcuCharacteristics : BaseEcuCharacteristics { }
        public class BNT_G01_G02_G08_F97_F98_SP2015 : BaseEcuCharacteristics { }
        public class E89EcuCharacteristics : BaseEcuCharacteristics { }
        public class F56EcuCharacteristics : BaseEcuCharacteristics { }
        public class F20EcuCharacteristics : BaseEcuCharacteristics { }

        private static ConcurrentDictionary<object, BaseEcuCharacteristics> ecuCharacteristics;

        static VehicleLogistics()
        {
            ecuCharacteristics = new ConcurrentDictionary<object, BaseEcuCharacteristics>();
        }

        public static BaseEcuCharacteristics GetEcuCharacteristics(string storedXmlFileName, Vehicle vecInfo)
        {
            return GetEcuCharacteristics<GenericEcuCharacteristics>(storedXmlFileName, vecInfo);
        }

        public static BaseEcuCharacteristics GetEcuCharacteristics<T>(string storedXmlFileName, Vehicle vecInfo) where T : BaseEcuCharacteristics
		{
            PdszDatabase database = ClientContext.GetDatabase(vecInfo);
            if (database == null)
            {
                return null;
            }

            PdszDatabase.BordnetsData bordnetsData = database.GetBordnetFromDatabase(vecInfo);
            if (bordnetsData != null && !string.IsNullOrWhiteSpace(bordnetsData.DocData))
            {
                BaseEcuCharacteristics baseEcuCharacteristics = CreateCharacteristicsInstance<GenericEcuCharacteristics>(vecInfo, bordnetsData.DocData, bordnetsData.InfoObjIdent);
                if (baseEcuCharacteristics != null)
                {
                    return baseEcuCharacteristics;
                }
            }

            if (!string.IsNullOrEmpty(storedXmlFileName))
            {
                string xml = database.GetEcuCharacteristicsXml(storedXmlFileName);
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    BaseEcuCharacteristics baseEcuCharacteristics = CreateCharacteristicsInstance<GenericEcuCharacteristics>(vecInfo, xml, storedXmlFileName);
                    if (baseEcuCharacteristics != null)
                    {
                        return baseEcuCharacteristics;
                    }
                }
			}

			return null;
        }

        public static BaseEcuCharacteristics CreateCharacteristicsInstance<T>(Vehicle vehicle, string xml, string name) where T : BaseEcuCharacteristics
        {
            try
            {
                Type typeFromHandle = typeof(T);
                object[] args = new string[1] { xml };
                T val = (T)Activator.CreateInstance(typeFromHandle, args);
                if (val != null)
                {
                    ecuCharacteristics.TryAdd(vehicle.GetCustomHashCode(), val);
                    val.BordnetName = name;
                    //Log.Info(Log.CurrentMethod(), "Found characteristics with group sgdb: " + val.brSgbd);
                }
                return val;
            }
            catch (Exception)
            {
                //Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public static void DecodeVCMBackupFA(byte[] fa, Vehicle vehicle)
        {
            if (vehicle == null)
            {
                //Log.Warning("VehicleLogistics.DecodeVCMBackupFA()", "vehicle was null");
            }
            else if (fa != null && fa.Length >= 160)
            {
                try
                {
                    if (vehicle.FA == null)
                    {
                        vehicle.FA = new FA();
                    }
                    if (vehicle.FA.SA == null)
                    {
                        vehicle.FA.SA = new ObservableCollection<string>();
                    }
                    if (vehicle.FA.HO_WORT == null)
                    {
                        vehicle.FA.HO_WORT = new ObservableCollection<string>();
                    }
                    if (vehicle.FA.E_WORT == null)
                    {
                        vehicle.FA.E_WORT = new ObservableCollection<string>();
                    }
                    if (vehicle.FA.ZUSBAU_WORT == null)
                    {
                        vehicle.FA.ZUSBAU_WORT = new ObservableCollection<string>();
                    }
                    vehicle.FA.C_DATE = FormatConverter.Convert6BitNibblesTo4DigitString(fa, 1u);
                    vehicle.FA.BR = FormatConverter.Convert6BitNibblesTo4DigitString(fa, 4u);
                    vehicle.FA.TYPE = FormatConverter.Convert6BitNibblesTo4DigitString(fa, 7u);
                    vehicle.FA.LACK = FormatConverter.Convert6BitNibblesTo4DigitString(fa, 10u);
                    vehicle.FA.POLSTER = FormatConverter.Convert6BitNibblesTo4DigitString(fa, 13u);
                    string text = string.Empty;
                    for (int i = 16; i < fa.Length; i++)
                    {
                        text += Convert.ToString(fa[i], 2).PadLeft(8, '0');
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
                            if ((b & 0xF0u) != 0)
                            {
                                byte inChar = Convert.ToByte(text2.Substring(j + 6, 6), 2);
                                byte inChar2 = Convert.ToByte(text2.Substring(j + 12, 6), 2);
                                string item = $"{FormatConverter.DecodeFAChar((char)b)}{FormatConverter.DecodeFAChar((char)inChar)}{FormatConverter.DecodeFAChar((char)inChar2)}";
                                vehicle.FA.SA.AddIfNotContains(item);
                                continue;
                            }
                            j += 2;
                            break;
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
                            if ((b2 & 0xF0u) != 0)
                            {
                                byte inChar3 = Convert.ToByte(text2.Substring(j + 6, 6), 2);
                                byte inChar4 = Convert.ToByte(text2.Substring(j + 12, 6), 2);
                                byte inChar5 = Convert.ToByte(text2.Substring(j + 18, 6), 2);
                                string item2 = $"{FormatConverter.DecodeFAChar((char)b2)}{FormatConverter.DecodeFAChar((char)inChar3)}{FormatConverter.DecodeFAChar((char)inChar4)}{FormatConverter.DecodeFAChar((char)inChar5)}";
                                vehicle.FA.E_WORT.AddIfNotContains(item2);
                                continue;
                            }
                            j += 2;
                            break;
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
                            if ((b3 & 0xF0u) != 0)
                            {
                                byte inChar6 = Convert.ToByte(text2.Substring(j + 6, 6), 2);
                                byte inChar7 = Convert.ToByte(text2.Substring(j + 12, 6), 2);
                                byte inChar8 = Convert.ToByte(text2.Substring(j + 18, 6), 2);
                                string item3 = $"{FormatConverter.DecodeFAChar((char)b3)}{FormatConverter.DecodeFAChar((char)inChar6)}{FormatConverter.DecodeFAChar((char)inChar7)}{FormatConverter.DecodeFAChar((char)inChar8)}";
                                vehicle.FA.HO_WORT.AddIfNotContains(item3);
                                continue;
                            }
                            j += 2;
                            break;
                        }
                        text2 = text2.Substring(j, text2.Length - j);
                    }
                }
                catch (Exception)
                {
                    //Log.WarningException("VehicleLogistics.DecodeVCMBackupFA()", exception);
                }
                string text3 = string.Format(CultureInfo.InvariantCulture, "{0}#{1}*{2}%{3}&{4}", vehicle.FA.BR, vehicle.FA.C_DATE, vehicle.FA.TYPE, vehicle.FA.LACK, vehicle.FA.POLSTER);
                foreach (string item4 in vehicle.FA.SA)
                {
                    text3 += string.Format(CultureInfo.InvariantCulture, "${0}", item4);
                }
                foreach (string item5 in vehicle.FA.E_WORT)
                {
                    text3 += string.Format(CultureInfo.InvariantCulture, "-{0}", item5);
                }
                foreach (string item6 in vehicle.FA.HO_WORT)
                {
                    text3 += string.Format(CultureInfo.InvariantCulture, "+{0}", item6);
                }
                vehicle.FA.STANDARD_FA = text3;
                vehicle.FA.SA_ANZ = (short)vehicle.FA.SA.Count;
                vehicle.FA.E_WORT_ANZ = (short)vehicle.FA.HO_WORT.Count;
                vehicle.FA.HO_WORT_ANZ = (short)vehicle.FA.E_WORT.Count;
                vehicle.FA.ZUSBAU_ANZ = 0;
                vehicle.FA.AlreadyDone = true;
            }
            else
            {
                //Log.Warning("VehicleLogistics.DecodeVCMBackupFA()", "fa byte stream was null or too short");
            }
        }

        public static ObservableCollectionEx<PdszDatabase.SaLaPa> GetAvailableSALAPAs(Vehicle vecInfo)
        {
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                return characteristics.GetAvailableSALAPAs(vecInfo);
            }
            return new ObservableCollectionEx<PdszDatabase.SaLaPa>();
        }

        // ToDo: Check on update
        public static BaseEcuCharacteristics GetCharacteristics(Vehicle vecInfo)
        {
            IDiagnosticsBusinessData service = DiagnosticsBusinessData.Instance;
            int customHashCode = vecInfo.GetCustomHashCode();
            if (ecuCharacteristics.TryGetValue(customHashCode, out var value))
            {
                return value;
            }
            if (!string.IsNullOrEmpty(vecInfo.Baureihenverbund))
            {
                switch (vecInfo.Baureihenverbund.ToUpper())
                {
                    case "K001":
                    case "KS01":
                    case "KE01":
                        return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                    case "K024":
                    case "KH24":
                        return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                    case "K01X":
                        return GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
                }
            }
            if (!string.IsNullOrEmpty(vecInfo.Ereihe))
            {
                switch (vecInfo.Ereihe.ToUpper())
                {
                    case "M12":
                        return GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
                    case "E39":
                        return GetEcuCharacteristics<E39EcuCharacteristics>("BNT-XML-E39.xml", vecInfo);
                    case "K17":
                    case "K07":
                        return GetEcuCharacteristics("MRKE01EcuCharacteristics.xml", vecInfo);
                    case "E38":
                        return GetEcuCharacteristics<E38EcuCharacteristics>("BNT-XML-E38.xml", vecInfo);
                    case "E53":
                        return GetEcuCharacteristics<E53EcuCharacteristics>("BNT-XML-E53.xml", vecInfo);
                    case "E36":
                        return GetEcuCharacteristics<E36EcuCharacteristics>("BNT-XML-E36.xml", vecInfo);
                    case "E46":
                        return GetEcuCharacteristics<E46EcuCharacteristics>("BNT-XML-E46.xml", vecInfo);
                    case "E52":
                        return GetEcuCharacteristics<E52EcuCharacteristics>("BNT-XML-E52.xml", vecInfo);
                    case "I20":
                        if (vecInfo.HasMrr30())
                        {
                            return GetEcuCharacteristics("BNT-XML-I20_FRS.xml", vecInfo);
                        }
                        if (vecInfo.HasFrr30v())
                        {
                            return GetEcuCharacteristics("BNT-XML-I20_FRSF.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_I20.xml", vecInfo);
                    case "F01BN2K":
                        return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
                    case "I15":
                    case "I12":
                        return GetEcuCharacteristics("BNT-XML-I12_I15.xml", vecInfo);
                    case "R50":
                    case "R52":
                    case "R53":
                        return GetEcuCharacteristics<R50EcuCharacteristics>("BNT-XML-R50.xml", vecInfo);
                    case "F25":
                        if (vecInfo.C_DATETIME.HasValue && !(vecInfo.C_DATETIME < service.DTimeF25Lci))
                        {
                            return GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<F25EcuCharacteristics>("BNT-XML-F25.xml", vecInfo);
                    case "F56":
                        if (vecInfo.IsBev())
                        {
                            return GetEcuCharacteristics("BNT-XML-F56BEV.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
                    case "F26":
                        return GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
                    case "RR5":
                    case "RR4":
                    case "RR6":
                        return GetEcuCharacteristics<RR6EcuCharacteristics>("BNT-XML-RR6.xml", vecInfo);
                    case "F44":
                    case "F40":
                        return GetEcuCharacteristics("BNT-XML-F40_F44.xml", vecInfo);
                    case "U12":
                    case "U06":
                    case "U11":
                    case "U10":
                        if (vecInfo.HasMrr30())
                        {
                            if (!vecInfo.IsPhev())
                            {
                                return GetEcuCharacteristics("BNT-XML-U06_FRS.xml", vecInfo);
                            }
                            return GetEcuCharacteristics("BNT-XML-U06_PHEV_FRS.xml", vecInfo);
                        }
                        if (vecInfo.HasFrr30v())
                        {
                            if (!vecInfo.IsPhev())
                            {
                                return GetEcuCharacteristics("BNT-XML-U06_FRSF.xml", vecInfo);
                            }
                            return GetEcuCharacteristics("BNT-XML-U06_PHEV_FRSF.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_U06-Fallback.xml", vecInfo);
                    case "RR1":
                    case "RR3":
                    case "RR2":
                        if (vecInfo.C_DATETIME.HasValue && !(vecInfo.C_DATETIME < service.DTimeRR_S2))
                        {
                            return GetEcuCharacteristics<RR2EcuCharacteristics>("BNT-XML-RR2.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<RREcuCharacteristics>("BNT-XML-RR.xml", vecInfo);
                    case "R55":
                    case "R56":
                    case "R57":
                    case "R58":
                    case "R59":
                    case "R61":
                    case "R60":
                        return GetEcuCharacteristics<R55EcuCharacteristics>("BNT-XML-R55.xml", vecInfo);
                    case "G08":
                        if (vecInfo.IsBev())
                        {
                            return GetEcuCharacteristics("BNT-XML-G08BEV.xml", vecInfo);
                        }
                        if (vecInfo.HasEnavevo())
                        {
                            return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                        }
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                    case "G83":
                    case "G82":
                    case "G81":
                    case "G80":
                    case "G87":
                        if (!vecInfo.IsPhev() && vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "J29":
                        return GetEcuCharacteristics("BNT-XML-J29.xml", vecInfo);
                    case "G38":
                    case "G31":
                    case "G32":
                    case "G30":
                        if (vecInfo.HasEnavevoOrNbtevo())
                        {
                            return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_NOMGU.xml", vecInfo);
                        }
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
                    case "K19":
                    case "K18":
                    case "K21":
                        if (service.GetBNType(vecInfo) == BNType.BN2000_MOTORBIKE)
                        {
                            return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                    case "E61":
                    case "E60":
                    case "E63":
                    case "E64":
                        return GetEcuCharacteristics<E60EcuCharacteristics>("BNT-XML-E60.xml", vecInfo);
                    case "E71":
                    case "E70":
                        if (vecInfo.HasAmpt70())
                        {
                            return GetEcuCharacteristics<E70EcuCharacteristicsAMPT>("BNT-XML-E70-AMPH70_AMPT70.xml", vecInfo);
                        }
                        if (vecInfo.HasAmph70())
                        {
                            return GetEcuCharacteristics<E70EcuCharacteristicsAMPH>("BNT-XML-E70-AMPH70_AMPT70.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<E70EcuCharacteristics>("BNT-XML-E70_NOAMPT_NOAMPH.xml", vecInfo);
                    case "K46":
                        if (service.GetBNType(vecInfo) == BNType.BN2020_MOTORBIKE)
                        {
                            return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                    case "V98":
                    case "MRK24":
                    case "A67":
                    case "K29":
                    case "K28":
                    case "K75":
                    case "K73":
                    case "K72":
                    case "K71":
                    case "K70":
                    case "K42":
                    case "K43":
                    case "K40":
                    case "K25":
                    case "K26":
                    case "K27":
                    case "K44":
                        return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                    case "E72":
                        return GetEcuCharacteristics("BNT-XML-E72.xml", vecInfo);
                    case "E66":
                    case "E67":
                    case "E65":
                    case "E68":
                        return GetEcuCharacteristics("BNT-XML-E65.xml", vecInfo);
                    case "E83":
                        return GetEcuCharacteristics<E83EcuCharacteristics>("BNT-XML-E83.xml", vecInfo);
                    case "E85":
                    case "E86":
                        return GetEcuCharacteristics<E85EcuCharacteristics>("BNT-XML-E85.xml", vecInfo);
                    case "E88":
                    case "E89":
                    case "E81":
                    case "E82":
                    case "E84":
                    case "E93":
                    case "E92":
                    case "E91":
                    case "E87":
                    case "E90":
                        _ = vecInfo.Baureihenverbund == "E89X";
                        return GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
                    case "H61":
                    case "H91":
                        return GetEcuCharacteristics("H61EcuCharacteristics.xml", vecInfo);
                    case "I01":
                        return GetEcuCharacteristics("BNT-XML-I01.xml", vecInfo);
                    case "G11":
                    case "G12":
                    case "F90":
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
                        }
                        if (vecInfo.HasNbtevo())
                        {
                            return GetEcuCharacteristics<BNT_G11_G12_G3X_SP2015>("BNT-XML-G11_G12_G3X_SP2015.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
                    case "K66":
                    case "K67":
                    case "K60":
                    case "K61":
                    case "K69":
                    case "K63":
                    case "K09":
                    case "K08":
                    case "K03":
                    case "K02":
                    case "V99":
                    case "K84":
                    case "K82":
                    case "K83":
                    case "K48":
                    case "K80":
                    case "K81":
                    case "K49":
                    case "K22":
                    case "K33":
                    case "K23":
                    case "K32":
                    case "K54":
                    case "K47":
                    case "K51":
                    case "K35":
                    case "K50":
                    case "K34":
                    case "K52":
                    case "K53":
                    case "X_K001":
                        return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                    case "G14":
                    case "G15":
                    case "G16":
                    case "F92":
                    case "F91":
                    case "F93":
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
                    case "G06":
                    case "G07":
                    case "G05":
                    case "G18":
                    case "F95":
                    case "F96":
                        return GetEcuCharacteristics("BNT-XML-G05_G06_G07.xml", vecInfo);
                    case "G02":
                    case "G01":
                    case "F97":
                    case "F98":
                        if (vecInfo.HasEnavevoOrNbtevo())
                        {
                            return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                        }
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
                    case "F86":
                    case "F85":
                    case "F15":
                    case "F14":
                    case "F16":
                        return GetEcuCharacteristics<F15EcuCharacteristics>("BNT-XML-F15.xml", vecInfo);
                    case "F10":
                    case "F18":
                    case "F12":
                    case "F11":
                    case "F03":
                    case "F13":
                    case "F02":
                    case "F01":
                    case "F06":
                    case "F07":
                    case "F04":
                        if (vecInfo.C_DATETIME.HasValue && !(vecInfo.C_DATETIME < service.DTimeF01Lci))
                        {
                            if (vecInfo.ECU != null)
                            {
                                ECU eCU = vecInfo.getECU(16L);
                                if (eCU != null && eCU.SubBUS != null && eCU.SubBUS.Contains(BusType.MOST))
                                {
                                    return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
                                }
                            }
                            return GetEcuCharacteristics<F01_1307EcuCharacteristics>("BNT-XML-F01_1307.xml", vecInfo);
                        }
                        return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
                    case "M13":
                    case "F54":
                    case "F55":
                    case "F57":
                    case "F47":
                    case "F46":
                    case "F45":
                    case "F52":
                    case "F49":
                    case "F48":
                    case "F39":
                    case "F60":
                        return GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
                    case "F21":
                    case "F23":
                    case "F20":
                    case "F22":
                    case "F83":
                    case "F82":
                    case "F80":
                    case "F81":
                    case "F87":
                    case "F36":
                    case "F35":
                    case "F34":
                    case "F32":
                    case "F33":
                    case "F31":
                    case "F30":
                        return GetEcuCharacteristics<F20EcuCharacteristics>("BNT-XML-F20.xml", vecInfo);
                    case "K14":
                    case "K15":
                    case "K16":
                    case "259":
                    case "247":
                    case "K599":
                    case "248":
                    case "259R":
                    case "259S":
                    case "R21":
                    case "259C":
                    case "R22":
                    case "259E":
                    case "R28":
                    case "K41":
                    case "K30":
                    case "K569":
                    case "C01":
                    case "K589":
                    case "247E":
                    case "E189":
                    case "E169":
                    case "R13":
                        return GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
                    case "RR11":
                    case "RR12":
                    case "RR31":
                    case "RR21":
                    case "RR22":
                        return GetEcuCharacteristics("BNT-XML-RR1X_RR3X_RRNM.xml", vecInfo);
                    case "G28":
                    case "G26":
                        if (vecInfo.IsBev())
                        {
                            return GetEcuCharacteristics("BNT-XML-G26_G28_BEV.xml", vecInfo);
                        }
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                        }
                        if (vecInfo.HasEnavevo())
                        {
                            return GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "G29":
                        return GetEcuCharacteristics("BNT-XML-G29.xml", vecInfo);
                    case "G21":
                    case "G20":
                        if (vecInfo.IsPhev())
                        {
                            return GetEcuCharacteristics("BNT-XML-G20_G21_PHEV.xml", vecInfo);
                        }
                        if (vecInfo.HasHuMgu())
                        {
                            return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                        }
                        if (vecInfo.HasEnavevo())
                        {
                            return GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
                        }
                        return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                    case "G42":
                    case "G23":
                    case "G22":
                        if (!vecInfo.IsPhev())
                        {
                            if (vecInfo.HasHuMgu())
                            {
                                return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
                            }
                            if (vecInfo.HasEnavevo())
                            {
                                return GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
                            }
                        }
                        return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
                }
                //Log.Info("VehicleLogistics.GetCharacteristics()", "cannot retrieve bordnet configuration using ereihe");
            }
            switch (vecInfo.BNType)
            {
                default:
#if false
                    if (readFromDatabase)
                    {
                        BaseEcuCharacteristics baseEcuCharacteristics = GetEcuCharacteristics("BNT-XML-FALLBACK.xml", vecInfo);
                        if (baseEcuCharacteristics != null)
                        {
                            return baseEcuCharacteristics;
                        }
                    }
                    Log.Warning("VehicleLogistics.GetCharacteristics()", $"No configuration found for vehicle with ereihe: {vecInfo.Ereihe}, bn type: {vecInfo.BNType}");
                    return null;
#endif
                    return GetEcuCharacteristics(string.Empty, vecInfo);
                case BNType.IBUS:
                    return GetEcuCharacteristics("iBusEcuCharacteristics.xml", vecInfo);
                case BNType.BN2000_MOTORBIKE:
                    return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
                case BNType.BN2020_MOTORBIKE:
                    return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
                case BNType.BNK01X_MOTORBIKE:
                    return GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
            }
        }

        // ToDo: Check on update
        public static BNMixed getBNMixed(string br, FA fa)
        {
            IDiagnosticsBusinessData service = DiagnosticsBusinessData.Instance;
            if (string.IsNullOrEmpty(br))
            {
                return BNMixed.UNKNOWN;
            }
            switch (br.ToUpper())
            {
                case "RODING_ROADSTER":
                case "M12":
                case "AERO":
                    return BNMixed.HETEROGENEOUS;
                case "RR1":
                case "RR2":
                case "RR3":
                    if (fa != null && fa.AlreadyDone && fa.C_DATETIME >= service.DTimeRR_S2)
                    {
                        return BNMixed.HETEROGENEOUS;
                    }
                    return BNMixed.HOMOGENEOUS;
                case "F01":
                case "F02":
                case "F03":
                case "F04":
                case "F06":
                case "F07":
                    if (fa != null && fa.AlreadyDone && fa.C_DATETIME > service.DTimeF01BN2020MostDomain)
                    {
                        return BNMixed.HETEROGENEOUS;
                    }
                    return BNMixed.HOMOGENEOUS;
                default:
                    return BNMixed.HOMOGENEOUS;
            }
        }

        public static string getBrSgbd(Vehicle vecInfo)
        {
            IDiagnosticsBusinessData service = DiagnosticsBusinessData.Instance;
            string text = service.GetMainSeriesSgbd(vecInfo);
            if (string.IsNullOrEmpty(text))
            {
                text = GetCharacteristics(vecInfo)?.brSgbd;
            }
            return text;
        }

        public static string getECU_GROBNAME(Vehicle vecInfo, long? sgAdr)
        {
            BaseEcuCharacteristics characteristics = GetCharacteristics(vecInfo);
            if (characteristics != null)
            {
                return characteristics.getECU_GROBNAME(sgAdr);
            }
            return string.Empty;
        }

        public static int getECUAdrByECU_GRUPPE(Vehicle vecInfo, string grp)
        {
            if (vecInfo != null && !string.IsNullOrEmpty(grp))
            {
                PdszDatabase.EcuGroup ecuGroupByName = ClientContext.GetDatabase(vecInfo)?.GetEcuGroupByName(grp);
                if (ecuGroupByName != null)
                {
                    Int64 diagAddrValue = ecuGroupByName.DiagAddr.ConvertToInt();
                    if (diagAddrValue != -1)
                    {
                        return (int) diagAddrValue;
                    }
                }
                //Log.Info(Log.CurrentMethod(), "No diagnostic address can be retrieved from database by group name: " + grp);
                //BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.AddServiceCode(Log.CurrentMethod(), 1);
                return GetCharacteristics(vecInfo)?.getECUAdrByECU_GRUPPE(grp) ?? (-1);
            }
            return -1;
        }

    }
}
