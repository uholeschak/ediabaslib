using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
    public enum LayoutGroup
    {
        X,
        D,
        P,
        PB,
        PK,
        PR,
        PBV,
        L,
        R,
        F,
        PA,
        PS,
        PAP,
        PAV,
        PAE,
        PAN
    }

    public delegate object DoECUIdentDelegate(IVehicle vecInfo, ECU mECU, IEcuKom ecuKom, ref bool resetMOSTGWdone, IProgressMonitor monitor, int retry, bool forceReRead, bool tryReanimation, bool IdentForceOnUnidentified = false);

    public class DiagnosticsBusinessData : DiagnosticsBusinessDataCore, IDiagnosticsBusinessData
    {
        internal class EcuKomConfig
        {
            private string ecu;

            private string job;

            private string param;

            private ushort set;

            private string result;

            public string Ecu
            {
                get
                {
                    return ecu;
                }
                set
                {
                    ecu = value;
                }
            }

            public string Job
            {
                get
                {
                    return job;
                }
                set
                {
                    job = value;
                }
            }

            public string Param
            {
                get
                {
                    return param;
                }
                set
                {
                    param = value;
                }
            }

            public ushort Set
            {
                get
                {
                    return set;
                }
                set
                {
                    set = value;
                }
            }

            public string Result
            {
                get
                {
                    return result;
                }
                set
                {
                    result = value;
                }
            }

            internal EcuKomConfig(string ecu, string job, string param, ushort set, string result)
            {
                this.ecu = ecu;
                this.job = job;
                this.param = param;
                this.set = set;
                this.result = result;
            }
        }

        private const string ILevelBN2020RegexPattern = "([A-Z0-9]{4}|[A-Z0-9]{3})-[0-9]{2}[-_](0[1-9]|1[0-2])[-_][0-9]{3}";

        //private string ServiceCodeValuePattern = "{0}_{1}";

        //private LayoutGroup layoutGroup = LayoutGroup.D;

        private readonly List<string> fsLesenExpertVariants = new List<string>
        {
            "PCU48", "DME9FF_R", "DME98_R", "D94BX7A0", "DME98_L", "IB_I20", "IB_G70", "GSMA02QZ", "GSMA02PU", "GSZF04GD",
            "GSZF04GF", "SCR04", "SCR05", "CCU_P1", "HVS_02"
        };

        private readonly List<string> specificModelsNoPopUp = new List<string>
        {
            "H61", "H91", "M12", "M13", "N18", "J29", "A67", "C01", "X_K001", "AERO",
            "GT1", "247", "247E", "248", "259", "259C", "259E", "259R", "259S"
        };

        private readonly List<string> placeholderModelsNopPopUp = new List<string>
        {
            "Vxx", "Rxx", "Fxx", "Exx", "Exxx", "RRx", "RRxx", "Kxx", "Kxxx", "MFx",
            "MFxx", "MFx-S", "MRKxx"
        };

        private const string rsuStopModuleName = "ABL-DIT-AG6510_RSU_STOP";

        private const string rsuStartModuleName = "ABL-DIT-AG6510_RSU_START";

        private const string checkVoltageModuleName = "ABL-LIF-FAHRZEUGDATEN__BATTERIE";

        private const string requestApplicationNumberAndUpgradeModuleName = "ABL-GEN-DETERMINE_REFURBISH_SWID";

        private const string serviceHistoryActionModuleName = "ABL-WAR-AS6100_SERVICEHISTORIE_AIR";

        private readonly string[] VarianteListFor14DigitSerialNumber = new string[9] { "NBT", "NBTEVO", "ENTRYNAV", "ENAVEVO", "ENTRY", "HU_MGU", "BIS01", "MGU_02_L", "MGU_02_A" };

        private string[] maxGrpListMINI = new string[31]
        {
            "D_0008", "D_0012", "D_0013", "D_0031", "D_003B", "D_0044", "D_0050", "D_0057", "D_005B", "D_0060",
            "D_0068", "D_006A", "D_0070", "D_0074", "D_0076", "D_007F", "D_0080", "D_0081", "D_009A", "D_009C",
            "D_00A4", "D_00BB", "D_00C8", "D_00CE", "D_00E8", "D_00ED", "D_00F0", "D_ABSKWP", "D_EGS", "D_MOTOR",
            "D_ZKE_GM"
        };

        private string[] maxGrpListBMWRest = new string[60]
        {
            "D_0014", "D_009A", "D_0022", "D_0040", "D_0013", "D_0000", "D_0008", "D_0012", "D_0032", "D_003B",
            "D_0044", "D_0048", "D_0050", "D_0056", "D_0057", "D_005B", "D_0060", "D_0065", "D_0066", "D_0068",
            "D_006A", "D_0070", "D_0072", "D_0074", "D_0076", "D_007F", "D_0080", "D_0081", "D_009C", "D_009B",
            "D_00A4", "D_00B0", "D_00BB", "D_00C0", "D_00C2", "D_00C8", "D_00CE", "D_00D0", "D_00E8", "D_00EA",
            "D_00ED", "D_00F0", "D_ABSKWP", "D_AHM", "D_BFS", "D_CID", "D_EGS", "D_EKP", "D_EPS", "D_FAS",
            "D_FLA", "D_MOTOR", "D_SIM", "D_STVL2", "D_STVR2", "D_SZM", "D_VGSG", "D_VVT", "D_ZKE_GM", "D_ZUHEIZ"
        };

        private List<string> ereiheForGrpListBMWRest = new List<string>
        {
            "E30", "E31", "E32", "E34", "E36", "E38", "E39", "E46", "E83", "E85",
            "E86"
        };

        private string[] maxGrpListBMW = new string[37]
        {
            "D_0000", "D_0012", "D_0032", "D_003B", "D_0044", "D_0056", "D_0057", "D_005B", "D_0068", "D_006A",
            "D_0070", "D_0072", "D_0074", "D_007F", "D_0081", "D_0080", "D_009C", "D_00A4", "D_00B0", "D_00BB",
            "D_00C0", "D_00C8", "D_00CE", "D_00D0", "D_00E8", "D_00EA", "D_00ED", "D_00F0", "D_ABSKWP", "D_EGS",
            "D_MOTOR", "D_SZM", "D_VGSG", "D_VVT", "D_VVT2", "D_ZKE_GM", "D_ZUHEIZ"
        };

        private List<string> ereiheForGrpListBMW = new List<string> { "E52", "E53" };

        private string[] maxGrpListFull = new string[96]
        {
            "d_egs", "d_0000", "d_0008", "d_000d", "d_0010", "d_0011", "d_0012", "d_motor", "d_0013", "d_0014",
            "d_0015", "d_0016", "d_0020", "d_0021", "d_0022", "d_0024", "d_0028", "d_002c", "d_002e", "d_0030",
            "d_0032", "d_0035", "d_0036", "d_003b", "d_0040", "d_0044", "d_0045", "d_0050", "d_0056", "d_0057",
            "d_0059", "d_005a", "d_005b", "d_0060", "d_0068", "d_0069", "d_006a", "d_006c", "d_0070", "d_0071",
            "d_0072", "d_007f", "d_0080", "d_0086", "d_0099", "d_009a", "d_009b", "d_009c", "d_009d", "d_009e",
            "d_00a0", "d_00a4", "d_00a6", "d_00a7", "d_00ac", "d_00b0", "d_00b9", "d_00bb", "d_00c0", "d_00c8",
            "d_00cd", "d_00d0", "d_00da", "d_00e0", "d_00e8", "d_00ed", "d_00f0", "d_00f5", "d_00ff", "d_b8_d0",
            "", "d_m60_10", "d_m60_12", "d_spmbt", "d_spmft", "d_szm", "d_zke3bt", "d_zke3ft", "d_zke3pm", "d_zke3sb",
            "d_zke3sd", "d_zke_gm", "d_zuheiz", "d_sitz_f", "d_sitz_b", "d_0047", "d_0048", "d_00ce", "d_00ea", "d_abskwp",
            "d_0031", "d_0019", "d_smac", "d_0081", "d_xen_l", "d_xen_r"
        };

        private string[] newFaultMemoryEnabledESeriesLifeCycles = new string[8] { "F95-1", "F96-1", "G05-1", "G06-1", "G07-1", "G09-0", "G18-1", "RR25-0" };

        private readonly List<string> ereiheWithoutFA = new List<string>
        {
            "E36", "E38", "E39", "E52", "E53", "R50", "R52", "R53", "E83", "E85",
            "E86", "E30", "E31", "E32", "E34"
        };

        private static IDictionary<string, string> Mapping = new Dictionary<string, string>
        {
            { "STATUS_VCM_BACKUP_FAHRZEUGAUFTRAG_LESEN", "STATUS_VCM_BACKUP_FAHRZEUGAUFTRAG_LESEN_SP2021" },
            { "STATUS_LESEN", "STATUS_LESEN" },
            { "STATUS_I_STUFE_LESEN_OHNE_SIGNATUR", "STATUS_I_STUFE_LESEN_OHNE_SIGNATUR" },
            { "STATUS_GWSZ_ANZEIGE", "STATUS_LESEN" },
            { "CBS_DATEN_LESEN", "STATUS_CBS_DATEN_LESEN" },
            { "CBS_INFO", "STATUS_CBS_INFO" }
        };


        public List<string> ProductLinesEpmBlacklist => new List<string> { "PL0", "PL2", "PL3", "PL3-alt", "PL4", "PL5", "PL5-alt", "PL6", "PL6-alt", "PL7" };

        public DateTime DTimeF25Lci => DateTime.ParseExact("01.04.2014", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTimeF01BN2020MostDomain => DateTime.ParseExact("30.06.2010", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTime2022_07 => DateTime.ParseExact("01.07.2022", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTime2023_03 => DateTime.ParseExact("01.03.2023", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTime2023_07 => DateTime.ParseExact("01.07.2023", "dd.MM.yyyy", new CultureInfo("de-DE"));

        DateTime IDiagnosticsBusinessData.DTimeRR_S2 => DiagnosticsBusinessDataCore.DTimeRR_S2;

        DateTime IDiagnosticsBusinessData.DTimeF01Lci => DiagnosticsBusinessDataCore.DTimeF01Lci;

        public decimal? ReadGwszForGroupCars(IVehicle vecInfo, IEcuKom ecuKom)
        {
            Dictionary<string, EcuKomConfig> dictionary = new Dictionary<string, EcuKomConfig>();
            dictionary.Add("G_KOMBI_V1", new EcuKomConfig("G_KOMBI", "STATUS_GWSZ_ANZEIGE", string.Empty, 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("G_KOMBI_V2", new EcuKomConfig("G_KOMBI", "STATUS_LESEN", "ARG;GWSZ_ANZEIGE_WERT", 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("G_VIP", new EcuKomConfig("G_VIP", "STATUS_LESEN", "ARG;GWSZ_ANZEIGE_WERT", 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("G_ZGW", new EcuKomConfig("G_ZGW", "STATUS_LESEN", "ARG;GWSZ_ANZEIGE_WERT", 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("G_MMI", new EcuKomConfig("G_MMI", "STATUS_LESEN", "ARG;GWSZ_ANZEIGE_WERT", 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("G_CAS", new EcuKomConfig("G_CAS", "STATUS_KILOMETERSTAND", string.Empty, 1, "STAT_KILOMETERSTAND_WERT"));
            dictionary.Add("IPF1_FAR", new EcuKomConfig("IPF1_FAR", "status_lesen", "ARG;GWSZ_ANZEIGE_WERT", 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("KOMBI65", new EcuKomConfig("KOMBI65", "STATUS_ANGEZEIGTER_GWSZ", string.Empty, 1, "STAT_GWSZ"));
            dictionary.Add("KOMB65_2", new EcuKomConfig("KOMB65_2", "STATUS_ANGEZEIGTER_GWSZ", string.Empty, 1, "STAT_GWSZ"));
            dictionary.Add("KOMBRR", new EcuKomConfig("KOMBRR", "STATUS_ANGEZEIGTER_GWSZ", string.Empty, 1, "STAT_GWSZ"));
            dictionary.Add("KOMBRR_2", new EcuKomConfig("KOMBRR_2", "STATUS_ANGEZEIGTER_GWSZ", string.Empty, 1, "STAT_GWSZ"));
            dictionary.Add("KOMB56", new EcuKomConfig("KOMB56", "STATUS_GWSZ_ANZEIGE", string.Empty, 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("KOMB60", new EcuKomConfig("KOMB60", "STATUS_GWSZ_ANZEIGE", string.Empty, 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("KOMB70", new EcuKomConfig("KOMB70", "STATUS_GWSZ_ANZEIGE", string.Empty, 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("KOMB87", new EcuKomConfig("KOMB87", "STATUS_GWSZ_ANZEIGE", string.Empty, 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("D_KOMBI_FB1", new EcuKomConfig("D_KOMBI", "STATUS_ANGEZEIGTER_GWSZ", string.Empty, 1, "STAT_GWSZ"));
            dictionary.Add("D_KOMBI_FB2", new EcuKomConfig("D_KOMBI", "STATUS_GWSZ_ANZEIGE", string.Empty, 1, "STAT_GWSZ_ANZEIGE_WERT"));
            dictionary.Add("D_0080", new EcuKomConfig("D_0080", "GWSZ_MINUS_OFFSET_LESEN", string.Empty, 1, "STAT_GWSZ_MINUS_OFFSET_WERT"));
            dictionary.Add("KOMBI36C", new EcuKomConfig("KOMBI36C", "AIF_GWSZ_LESEN", string.Empty, 1, "STAT_GWSZ_WERT"));
            dictionary.Add("KOMBI85", new EcuKomConfig("KOMBI85", "STATUS_AIF_GWSZ_LESEN", string.Empty, 1, "STAT_GWSZ_WERT"));
            List<EcuKomConfig> list = new List<EcuKomConfig>();
            if (vecInfo != null)
            {
                if (vecInfo.BNType == BNType.BN2020)
                {
                    if (vecInfo.Produktlinie == "PL5-alt")
                    {
                        IEcu eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE("D_KOMBI");
                        if (eCUbyECU_GRUPPE != null && eCUbyECU_GRUPPE.VARIANTE.ToUpper() == "KOMBRR_2")
                        {
                            list.Add(dictionary["KOMBRR_2"]);
                        }
                    }
                    if (vecInfo.Sp2021Enabled)
                    {
                        list.Add(dictionary["G_VIP"]);
                        list.Add(dictionary["G_ZGW"]);
                    }
                    if (IsEES25Vehicle(vecInfo))
                    {
                        list.Add(dictionary["IPF1_FAR"]);
                    }
                    if (vecInfo.getECUbyECU_GRUPPE("G_KOMBI") == null)
                    {
                        list.Add(dictionary["G_MMI"]);
                    }
                    list.Add(dictionary["G_KOMBI_V1"]);
                    list.Add(dictionary["G_KOMBI_V2"]);
                    list.Add(dictionary["G_CAS"]);
                }
                else if (vecInfo.BNType == BNType.BEV2010 || vecInfo.BNType == BNType.BN2000)
                {
                    IEcu eCUbyECU_GRUPPE2 = vecInfo.getECUbyECU_GRUPPE("D_KOMBI");
                    if (eCUbyECU_GRUPPE2 != null)
                    {
                        string key = eCUbyECU_GRUPPE2.VARIANTE.ToUpper();
                        list.Add(dictionary[key]);
                    }
                    else
                    {
                        list.Add(dictionary["D_KOMBI_FB1"]);
                        list.Add(dictionary["D_KOMBI_FB2"]);
                    }
                }
                else if (vecInfo.BNType == BNType.IBUS)
                {
                    IEcu eCUbyECU_GRUPPE3 = vecInfo.getECUbyECU_GRUPPE("D_0080");
                    if (eCUbyECU_GRUPPE3 != null)
                    {
                        string text = eCUbyECU_GRUPPE3.VARIANTE?.ToUpper();
                        if (text == "KOMBI36C")
                        {
                            list.Add(dictionary["KOMBI36C"]);
                        }
                        else if (text == "KOMBI85")
                        {
                            list.Add(dictionary["KOMBI85"]);
                        }
                        else
                        {
                            list.Add(dictionary["D_0080"]);
                        }
                    }
                }
            }
            if (list.Count <= 0)
            {
                foreach (KeyValuePair<string, EcuKomConfig> item in dictionary)
                {
                    list.Add(item.Value);
                }
            }
            return ReadGwszFromEcus(ecuKom, list);
        }

        public decimal? ReadGwszForGroupMotorbike(IVehicle vehicle, IEcuKom ecuKom, int retryCount, Action<string> protocolUnit, Action<IVehicle, string, string> logIfEcuMissing)
        {
            Vehicle vehicle2 = (Vehicle)vehicle;
            switch (vehicle2.BNType)
            {
                case BNType.BN2000_MOTORBIKE:
                    try
                    {
                        if (!string.IsNullOrEmpty(vehicle2.getECUbyECU_GRUPPE("D_MRKOMB")?.VARIANTE))
                        {
                            IEcuJob ecuJob2 = ecuKom.ApiJob("D_MRKOMB", "STATUS_GWSZ_ANZEIGE", string.Empty, string.Empty, retryCount);
                            if (ecuJob2.IsOkay())
                            {
                                decimal num = 1m;
                                if (ecuJob2.getResultFormat("STAT_GWSZ_ANZEIGE_WERT") != -1)
                                {
                                    string stringResult = ecuJob2.getStringResult("STAT_GWSZ_ANZEIGE_EINH");
                                    protocolUnit(stringResult);
                                    if (!string.IsNullOrEmpty(stringResult) && string.Compare("miles", stringResult, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        num = 1.609344m;
                                    }
                                    decimal value = num;
                                    decimal? mileageFromJob = GetMileageFromJob(ecuJob2, "STAT_GWSZ_ANZEIGE_WERT", (ushort)1);
                                    return (decimal?)value * mileageFromJob;
                                }
                                if (ecuJob2.getResultFormat("GWSZ") != -1)
                                {
                                    string stringResult2 = ecuJob2.getStringResult("STAT_GWSZ_ANZEIGE_EINH");
                                    protocolUnit(stringResult2);
                                    if ("miles".Equals(stringResult2, StringComparison.OrdinalIgnoreCase))
                                    {
                                        num = 1.609344m;
                                    }
                                    decimal value = num;
                                    decimal? mileageFromJob2 = GetMileageFromJob(ecuJob2, "GWSZ", (ushort)1);
                                    return (decimal?)value * mileageFromJob2;
                                }
                                if (ecuJob2.getResultFormat("STAT_GWSZ") != -1)
                                {
                                    string stringResult3 = ecuJob2.getStringResult("STAT_GWSZ_ANZEIGE_EINH");
                                    protocolUnit(stringResult3);
                                    if ("miles".Equals(stringResult3, StringComparison.OrdinalIgnoreCase))
                                    {
                                        num = 1.609344m;
                                    }
                                    decimal value = num;
                                    decimal? mileageFromJob2 = GetMileageFromJob(ecuJob2, "STAT_GWSZ", (ushort)1);
                                    vehicle2.Gwsz = (decimal?)value * mileageFromJob2;
                                }
                            }
                            else
                            {
                                Log.Warning("VehicleIdent.doReadGwsz()", "GWSZ readout failed for BN2000_MOTORBIKE");
                            }
                        }
                        else
                        {
                            Log.Warning("VehicleIdent.doReadGwsz()", "Failed to identify KOMBI for BN2000_MOTORBIKE");
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException("VehicleIdent.doReadGwsz()", exception);
                    }
                    break;
                case BNType.BN2020_MOTORBIKE:
                    {
                        logIfEcuMissing(vehicle2, "G_MRKOMB", "STATUS_LESEN");
                        IEcuJob ecuJob = ecuKom.ApiJob("G_MRKOMB", "STATUS_LESEN", "ARG;GWSZ_MR", string.Empty, cacheAdding: false);
                        if (!ecuJob.IsOkay())
                        {
                            ecuJob = ecuKom.ApiJob("G_MRZGW", "STATUS_LESEN", "ARG;GWSZ_MR", string.Empty, cacheAdding: false);
                        }
                        if (ecuJob.IsOkay())
                        {
                            return GetMileageFromJob(ecuJob, "STAT_GWSZ_WERT", null).GetValueOrDefault();
                        }
                        break;
                    }
                case BNType.BNK01X_MOTORBIKE:
                    Log.Info("VehicleIdent.doReadGwsz()", "no gwsz readout available for BNK10X motor cycles");
                    break;
            }
            return null;
        }

        private decimal? GetMileageFromJob(IEcuJob job, string resultName, ushort? set = null)
        {
            object obj = ((!set.HasValue) ? job.getResult(resultName) : job.getResult(set.Value, resultName));
            if (obj != null)
            {
                try
                {
                    return Convert.ToDecimal(obj);
                }
                catch (Exception ex)
                {
                    Log.Warning(Log.CurrentMethod(), string.Format("Error converting mileage value '{0}'. Job: {1}, result name: '{2}'. Error: {3}", (obj == null) ? "null" : obj.ToString(), job.JobName, resultName, ex));
                }
            }
            else
            {
                Log.Info(Log.CurrentMethod(), "Mileage could not be retrieved from job: '" + job.JobName + "' using result name: '" + resultName + "'");
            }
            return null;
        }


        internal decimal? ReadGwszFromEcus(IEcuKom ecuKom, List<EcuKomConfig> ecuKomconfigs)
        {
            foreach (EcuKomConfig ecuKomconfig in ecuKomconfigs)
            {
                try
                {
                    Log.Info("VehicleIdent.GetGwsz()", "trying to get Gwsz from {0} with {1}", ecuKomconfig.Ecu, ecuKomconfig.Job);
                    IEcuJob ecuJob = ecuKom.ApiJob(ecuKomconfig.Ecu, ecuKomconfig.Job, ecuKomconfig.Param, string.Empty);
                    if (!ecuJob.IsOkay())
                    {
                        continue;
                    }
                    object result = ecuJob.getResult(ecuKomconfig.Set, ecuKomconfig.Result);
                    if (result != null)
                    {
                        decimal num = Convert.ToDecimal(result);
                        if (ecuKomconfig.Ecu == "KOMBI85")
                        {
                            EcuKomConfig ecuKomConfig = new EcuKomConfig("KOMBI85", "STATUS_GWSZ_OFFSET_LESEN", string.Empty, 1, "STAT_GWSZ_OFFSET_WERT");
                            IEcuJob ecuJob2 = ecuKom.ApiJob(ecuKomConfig.Ecu, ecuKomConfig.Job, ecuKomConfig.Param, string.Empty);
                            if (ecuJob2.IsOkay())
                            {
                                object result2 = ecuJob2.getResult(ecuKomConfig.Result);
                                if (result2 != null)
                                {
                                    decimal num2 = Convert.ToDecimal(result2);
                                    return num - num2;
                                }
                            }
                        }
                        return num;
                    }
                    Log.Warning("ReadGwszFromEcus()", "(Ecu: {0}, Job: {1}, Parameter: {2}, set: {3}) - JobResult {4} was null.", ecuKomconfig.Ecu, ecuKomconfig.Job, ecuKomconfig.Param, ecuKomconfig.Set, ecuKomconfig.Result);
                }
                catch (Exception exception)
                {
                    Log.WarningException("DiagnosticsBusinessData.ReadGwszFromEcus()", exception);
                }
            }
            return null;
        }


        public void ReadILevelBn2020(Vehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
            // [UH] get reactor from vehicle
            Reactor instance = (vecInfo as Vehicle)?.Reactor;
            if (instance == null)
            {
                return;
            }
            IEcuJob ecuJob = new ECUJob();
            if (IsSp2021Gateway(vecInfo, ecuKom, retryCount))
            {
                ecuJob = ecuKom.ApiJobWithRetries("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR", string.Empty, string.Empty, retryCount);
                if (!ecuJob.IsOkay())
                {
                    HandleReadILevelForSp2021Fallback(instance, vecInfo, ecuKom, retryCount);
                }
                else if (!ProcessILevelJobResults(instance, vecInfo, ecuJob))
                {
                    HandleReadILevelForSp2021Fallback(instance, vecInfo, ecuKom, retryCount);
                }
            }
            else if (IsEES25Vehicle(vecInfo))
            {
                ecuJob = ecuKom.ApiJobWithRetries("IPB_APP2", "STATUS_LESEN", "ARG;VCM_DID_ISTUFE", string.Empty, retryCount);
                if (ecuJob.IsOkay())
                {
                    ProcessILevelJobResultsEES25(instance, vecInfo, ecuJob);
                }
            }
            else
            {
                ecuJob = ecuKom.ApiJobWithRetries("g_zgw", "STATUS_VCM_I_STUFE_LESEN", string.Empty, string.Empty, retryCount);
                if (!ecuJob.IsOkay())
                {
                    HandleReadIlevelBackup(instance, vecInfo, ecuKom, retryCount);
                }
                else if (!ProcessILevelJobResults(instance, vecInfo, ecuJob))
                {
                    HandleReadIlevelBackup(instance, vecInfo, ecuKom, retryCount);
                }
            }
        }

        public bool ProcessILevelJobResultsEES25(Reactor reactor, IVehicle vecInfo, IEcuJob iJob)
        {
            string text = ReadingOutILevelJobResult(iJob, "I_STUFE_HO.SERIES_GROUP", "I_STUFE_HO.YEAR", "I_STUFE_HO.MONTH", "I_STUFE_HO.I_LEVEL_IDENTIFICATION_NUMBER");
            string text2 = ReadingOutILevelJobResult(iJob, "I_STUFE_WERK.SERIES_GROUP", "I_STUFE_WERK.YEAR", "I_STUFE_WERK.MONTH", "I_STUFE_WERK.I_LEVEL_IDENTIFICATION_NUMBER");
            string text3 = ReadingOutILevelJobResult(iJob, "I_STUFE_HO_BACKUP.SERIES_GROUP", "I_STUFE_HO_BACKUP.YEAR", "I_STUFE_HO_BACKUP.MONTH", "I_STUFE_HO_BACKUP.I_LEVEL_IDENTIFICATION_NUMBER");
            if (IsExcludedFromILevelValidation(iJob) || (ValidateILevelWithRegexPattern(text2, "ILevelWerk") && ValidateILevelWithRegexPattern(text, "ILevelHO") && ValidateILevelWithRegexPattern(text3, "ILevelHOBackup")))
            {
                reactor.SetILevelWerk(text2, DataSource.Vehicle);
                reactor.SetILevel(text, DataSource.Vehicle);
                vecInfo.ILevelBackup = text3;
                return true;
            }
            return false;
        }

        public bool ProcessILevelJobResults(Reactor reactor, IVehicle vecInfo, IEcuJob iJob)
        {
            string stringResult = iJob.getStringResult(1, "STAT_I_STUFE_WERK");
            string stringResult2 = iJob.getStringResult(1, "STAT_I_STUFE_HO");
            string stringResult3 = iJob.getStringResult(1, "STAT_I_STUFE_HO_BACKUP");
            if (IsExcludedFromILevelValidation(iJob) || (ValidateILevelWithRegexPattern(stringResult, "ILevelWerk") && ValidateILevelWithRegexPattern(stringResult2, "ILevelHO") && ValidateILevelWithRegexPattern(stringResult3, "ILevelHOBackup")))
            {
                reactor.SetILevelWerk(stringResult, DataSource.Vehicle);
                reactor.SetILevel(stringResult2, DataSource.Vehicle);
                vecInfo.ILevelBackup = stringResult3;
                return true;
            }
            return false;
        }

        private static string ReadingOutILevelJobResult(IEcuJob iJob, string seriesResultName, string yearResultName, string monthResultName, string i_level_identification_numberResultName)
        {
            string stringResult = iJob.getStringResult(1, seriesResultName);
            string text = iJob.getuintResult(1, yearResultName).ToString();
            string text2 = iJob.getuintResult(1, monthResultName).ToString();
            string text3 = iJob.getuintResult(1, i_level_identification_numberResultName).ToString();
            text2 = ((text2.Length == 1) ? ("0" + text2) : text2);
            return stringResult + "-" + text + "-" + text2 + "-" + text3;
        }

        private void HandleReadIlevelBackup(Reactor reactor, IVehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
            IEcuJob ecuJob = ecuKom.ApiJobWithRetries("g_frm", "STATUS_VCM_I_STUFE_LESEN", string.Empty, string.Empty, retryCount);
            if (ecuJob.IsOkay())
            {
                ProcessILevelJobResults(reactor, vecInfo, ecuJob);
            }
        }

        private void HandleReadILevelForSp2021Fallback(Reactor reactor, IVehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
            IEcuJob ecuJob = SendJobToKombiOrMmi(vecInfo, ecuKom, "STATUS_I_STUFE_LESEN_OHNE_SIGNATUR", string.Empty, string.Empty, retryCount);
            if (ecuJob.IsOkay())
            {
                ProcessILevelJobResults(reactor, vecInfo, ecuJob);
            }
        }

        private bool IsExcludedFromILevelValidation(IEcuJob iJob)
        {
            string method = Log.CurrentMethod() + "()";
            Log.Info(method, "Check if iLevel is excluded from validation");
            string[] source = new string[1] { "REM_20" };
            string ecuVariante = iJob.getStringResult(0, "VARIANTE");
            if (source.Any((string x) => x.Equals(ecuVariante, StringComparison.OrdinalIgnoreCase)))
            {
                Log.Info(method, "Vehicle is excluded from I-Level validation");
                return true;
            }
            return false;
        }

        private bool ValidateILevelWithRegexPattern(string ilevelInput, string iLevelDescription)
        {
            Log.Info(Log.CurrentMethod(), "Validation of ILevel " + ilevelInput + " for type " + iLevelDescription + " started");
            if (!Regex.IsMatch(ilevelInput, "([A-Z0-9]{4}|[A-Z0-9]{3})-[0-9]{2}[-_](0[1-9]|1[0-2])[-_][0-9]{3}"))
            {
                Log.Warning(Log.CurrentMethod(), "Validation of ILevel " + ilevelInput + " for type " + iLevelDescription + " failed");
                return false;
            }
            return true;
        }

        public void SetSp2021Enabled(IVehicle vecInfo)
        {
            if (string.IsNullOrEmpty(vecInfo.Produktlinie) && ClientContext.GetBrand((Vehicle) vecInfo) == CharacteristicExpression.EnumBrand.BMWMotorrad)
            {
                vecInfo.Produktlinie = "-";
            }
            vecInfo.Sp2021Enabled = vecInfo.Produktlinie.StartsWith("21");
        }

        // ToDo: Check on update
        public bool IsSp2021Gateway(IVehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
            string text = "";
            IEcuJob ecuJob = ecuKom.ApiJobWithRetries("G_ZGW", "IDENT", string.Empty, string.Empty, retryCount);
            if (ecuJob.IsOkay())
            {
                text = ecuJob.getStringResult(0, "VARIANTE");
                if (text == null)
                {
                    text = "";
                }
            }
            if (!vecInfo.Sp2021Enabled && !text.Equals("BCP_SP21", StringComparison.OrdinalIgnoreCase))
            {
                Log.Info(Log.CurrentMethod(), "Vehicle gateway is not bcp_sp21!");
                return false;
            }
            Log.Info(Log.CurrentMethod(), "Vehicle gateway is bcp_sp21!");
            return true;
        }

        public string GetMainSeriesSgbdAdditional(IIdentVehicle vecInfo)
        {
            return GetMainSeriesSgbdAdditional(vecInfo, new NugetLogger());
        }

        public string[] GetMaxEcuList(BrandName brand, string ereihe)
        {
            switch (brand)
            {
                case BrandName.MINIPKW:
                    return maxGrpListMINI;
                case BrandName.BMWPKW:
                    if (ereiheForGrpListBMWRest.Contains(ereihe))
                    {
                        return maxGrpListBMWRest;
                    }
                    if (ereiheForGrpListBMW.Contains(ereihe))
                    {
                        return maxGrpListBMW;
                    }
                    break;
            }
            return maxGrpListFull;
        }


        // ToDo: Check on update
        public List<int> GetGatewayEcuAdresses(IVehicle vecInfo)
        {
            List<int> list = new List<int>();
            if (vecInfo.Prodart == "P")
            {
                if (!string.IsNullOrEmpty(vecInfo.Produktlinie))
                {
                    switch (vecInfo.Produktlinie.ToUpper())
                    {
                        case "PL0":
                            if (vecInfo.Ereihe == "E36")
                            {
                                foreach (ECU item in vecInfo.ECU)
                                {
                                    if (item.ID_SG_ADR == 128)
                                    {
                                        list.Add(128);
                                    }
                                    else if (item.ID_SG_ADR == 13)
                                    {
                                        list.Add(13);
                                    }
                                }
                            }
                            else if (vecInfo.Ereihe == "E65" || vecInfo.Ereihe == "E66" || vecInfo.Ereihe == "E67" || vecInfo.Ereihe == "E68")
                            {
                                list.Add(0);
                                list.Add(1);
                            }
                            else
                            {
                                list.Add(128);
                            }
                            break;
                        case "PL3-ALT":
                            list.Add(128);
                            list.Add(0);
                            break;
                        case "PL2":
                        case "PL3":
                        case "PL6-ALT":
                            list.Add(0);
                            break;
                        case "PL4":
                            list.Add(0);
                            list.Add(100);
                            break;
                        case "PL5-ALT":
                            list.Add(0);
                            list.Add(1);
                            break;
                        case "PL6":
                            if (vecInfo.Ereihe == "F15" || vecInfo.Ereihe == "F16" || vecInfo.Ereihe == "F85" || vecInfo.Ereihe == "F86")
                            {
                                list.Add(16);
                                list.Add(64);
                            }
                            else
                            {
                                list.Add(16);
                            }
                            break;
                        case "PL7":
                            if (vecInfo.Ereihe == "F25" || vecInfo.Ereihe == "F26")
                            {
                                list.Add(16);
                                break;
                            }
                            list.Add(16);
                            list.Add(64);
                            break;
                        case "PLLI":
                        case "PLLU":
                        case "35LU":
                        case "35LK":
                            list.Add(16);
                            list.Add(64);
                            break;
                        case "35LG":
                        case "35LR":
                            if (vecInfo.Ereihe == "G09")
                            {
                                list.Add(16);
                            }
                            else if (vecInfo.Ereihe == "G07")
                            {
                                if (!vecInfo.C_DATETIME.HasValue)
                                {
                                    Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                                    list.Add(16);
                                    list.Add(64);
                                }
                                else if (vecInfo.C_DATETIME >= DTime2022_07)
                                {
                                    list.Add(16);
                                }
                                else
                                {
                                    list.Add(16);
                                    list.Add(64);
                                }
                            }
                            else if (vecInfo.Ereihe == "F95" || vecInfo.Ereihe == "F96" || vecInfo.Ereihe == "G05" || vecInfo.Ereihe == "G06")
                            {
                                if (!vecInfo.C_DATETIME.HasValue)
                                {
                                    Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                                    list.Add(16);
                                    list.Add(64);
                                }
                                else if (vecInfo.C_DATETIME >= DTime2023_03)
                                {
                                    list.Add(16);
                                }
                                else
                                {
                                    list.Add(16);
                                    list.Add(64);
                                }
                            }
                            else if (vecInfo.Ereihe == "G18")
                            {
                                if (!vecInfo.C_DATETIME.HasValue)
                                {
                                    Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                                    list.Add(16);
                                    list.Add(64);
                                }
                                else if (vecInfo.C_DATETIME >= DTime2023_07)
                                {
                                    list.Add(16);
                                }
                                else
                                {
                                    list.Add(16);
                                    list.Add(64);
                                }
                            }
                            else if (vecInfo.Ereihe == "RR25")
                            {
                                list.Add(16);
                            }
                            else
                            {
                                list.Add(16);
                                list.Add(64);
                            }
                            break;
                        case "21LI":
                        case "21LU":
                        case "21LG":
                            list.Add(16);
                            if (IsEES25Vehicle(vecInfo))
                            {
                                list.Add(64);
                            }
                            break;
                        case "25LN":
                        case "25XNF":
                            list.Add(16);
                            list.Add(64);
                            break;
                        default:
                            list.Add(16);
                            break;
                    }
                }
                return list;
            }
            if (vecInfo.Prodart == "M")
            {
                if (!string.IsNullOrEmpty(vecInfo.Baureihe))
                {
                    switch (vecInfo.Baureihenverbund.ToUpper())
                    {
                        case "K01X":
                        case "K024":
                        case "KH24":
                        case "K001":
                        case "KS01":
                        case "KE01":
                            list.Add(18);
                            break;
                        default:
                            list.Add(16);
                            break;
                    }
                }
                return list;
            }
            Log.Info(Log.CurrentMethod(), "Returning null for product line: " + vecInfo?.Produktlinie + ", ereihe: " + vecInfo.Ereihe);
            return null;
        }

        public bool IsEES25Vehicle(IVehicle vecInfo)
        {
            if (vecInfo.Sp2025Enabled)
            {
                return true;
            }
            if (vecInfo.LifeCycle != "BAS")
            {
                if (vecInfo.Ereihe == "G60" || vecInfo.Ereihe == "G61" || vecInfo.Ereihe == "G68" || vecInfo.Ereihe == "G70" || vecInfo.Ereihe == "G90" || vecInfo.Ereihe == "G99")
                {
                    return true;
                }
            }
            else if (vecInfo.Ereihe == "G50" || vecInfo.Ereihe == "G58" || vecInfo.Ereihe == "G72")
            {
                return true;
            }
            return false;
        }

        // ToDo: Check on update
        public BNType GetBNType(IVehicle vecInfo)
        {
            return (BNType)GetBordnetType(vecInfo.Baureihenverbund, vecInfo.Prodart, vecInfo.Ereihe, new NugetLogger());
        }


        public void BN2000HandleKMMFixes(IVehicle vecInfo, IEcuKom ecuKom, bool resetMOSTDone, IProgressMonitor monitor, int retryCount, DoECUIdentDelegate doECUIdentDelegate)
        {
            if ((vecInfo.hasSA("6VC") || vecInfo.hasSA("612") || vecInfo.hasSA("633")) && vecInfo.getECU(54L) == null)
            {
                ECU eCU = new ECU();
                eCU.BUS = BusType.MOST;
                eCU.ID_SG_ADR = 54L;
                eCU.ECU_GRUPPE = "D_TEL";
                eCU.ECU_GROBNAME = "TEL";
                vecInfo.AddEcu(eCU);
                doECUIdentDelegate(vecInfo, eCU, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.hasSA("610") && vecInfo.getECU(61L) == null)
            {
                ECU eCU2 = new ECU();
                eCU2.BUS = BusType.MOST;
                eCU2.ID_SG_ADR = 61L;
                eCU2.ECU_GRUPPE = "D_HUD";
                eCU2.ECU_GROBNAME = "HUD";
                vecInfo.AddEcu(eCU2);
                doECUIdentDelegate(vecInfo, eCU2, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.hasSA("672") && vecInfo.getECU(60L) == null)
            {
                ECU eCU3 = new ECU();
                eCU3.BUS = BusType.MOST;
                eCU3.ID_SG_ADR = 60L;
                eCU3.ECU_GRUPPE = "D_CDC";
                eCU3.ECU_GROBNAME = "CDC";
                vecInfo.AddEcu(eCU3);
                doECUIdentDelegate(vecInfo, eCU3, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.hasSA("696") && vecInfo.getECU(49L) == null)
            {
                ECU eCU4 = new ECU();
                eCU4.BUS = BusType.MOST;
                eCU4.ID_SG_ADR = 49L;
                eCU4.ECU_GRUPPE = "D_MMC";
                eCU4.ECU_GROBNAME = "MMC";
                vecInfo.AddEcu(eCU4);
                doECUIdentDelegate(vecInfo, eCU4, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.hasBusType(BusType.MOST) && vecInfo.getECUbyECU_GRUPPE("D_MOSTGW") != null)
            {
                ECU eCU5 = new ECU();
                eCU5.BUS = BusType.VIRTUALBUSCHECK;
                eCU5.ID_SG_ADR = 255L;
                eCU5.ECU_GRUPPE = "D_MOST";
                eCU5.ECU_GROBNAME = "MOST";
                vecInfo.AddEcu(eCU5);
                doECUIdentDelegate(vecInfo, eCU5, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
        }


        public void HandleECUGroups(IVehicle vecInfo, IEcuKom ecuKom, List<IEcu> ecusToRemoveKMM)
        {
            IEcu eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE("D_RLS");
            if (eCUbyECU_GRUPPE == null && "e89x".Equals(vecInfo.MainSeriesSgbd, StringComparison.OrdinalIgnoreCase))
            {
                if (vecInfo.hasSA("521"))
                {
                    eCUbyECU_GRUPPE = new ECU();
                    eCUbyECU_GRUPPE.ECU_GRUPPE = "D_RLS";
                    eCUbyECU_GRUPPE.ID_SG_ADR = 86L;
                    eCUbyECU_GRUPPE.ID_LIN_SLAVE_ADR = 128L;
                    eCUbyECU_GRUPPE.ECU_GROBNAME = "RLS";
                    eCUbyECU_GRUPPE.TITLE_ECUTREE = "RLS";
                    vecInfo.AddEcu(eCUbyECU_GRUPPE);
                }
                else if ((ecuKom.DefaultApiJob("D_RLS", "IDENT", string.Empty, string.Empty) as ECUJob).IsOkay())
                {
                    eCUbyECU_GRUPPE = new ECU();
                    eCUbyECU_GRUPPE.ECU_GRUPPE = "D_RLS";
                    eCUbyECU_GRUPPE.ID_SG_ADR = 86L;
                    eCUbyECU_GRUPPE.ID_LIN_SLAVE_ADR = 128L;
                    eCUbyECU_GRUPPE.ECU_GROBNAME = "RLS";
                    eCUbyECU_GRUPPE.TITLE_ECUTREE = "RLS";
                    vecInfo.AddEcu(eCUbyECU_GRUPPE);
                }
            }
            IEcu eCUbyECU_GRUPPE2 = vecInfo.getECUbyECU_GRUPPE("D_ISPB");
            if (eCUbyECU_GRUPPE2 != null && !eCUbyECU_GRUPPE2.IDENT_SUCCESSFULLY)
            {
                IEcu eCUbyECU_GRUPPE3 = vecInfo.getECUbyECU_GRUPPE("D_MMI");
                if ((eCUbyECU_GRUPPE3.IDENT_SUCCESSFULLY && string.Compare(eCUbyECU_GRUPPE3.VARIANTE, "RAD2", StringComparison.OrdinalIgnoreCase) == 0) || vecInfo.hasSA("6VC") || vecInfo.getECUbyECU_SGBD("CMEDIAR") != null)
                {
                    Log.Info("VehicleIdent.doECUIdent()", "found RAD2 with built-in USB/audio (SA 6FL/6ND/6NE)");
                    ecusToRemoveKMM.Add(eCUbyECU_GRUPPE2);
                }
            }
            foreach (IEcu item in ecusToRemoveKMM)
            {
                Log.Info("VehicleIdent.doECUIdent()", "remove ECU at address: {0} due to KMM error.", item.ID_SG_ADR);
                vecInfo.RemoveEcu(item);
            }
        }

        public void SetVehicleLifeStartDate(IVehicle vehicle, IEcuKom ecuKom)
        {
            if (vehicle.BrandName == BrandName.BMWMOTORRAD)
            {
                if (vehicle.BNType != BNType.BN2000_MOTORBIKE && vehicle.BNType != BNType.BNK01X_MOTORBIKE)
                {
                    ExecuteVehicleLifeStartDateJobAndProcessResults("G_MRKOMB", "STATUS_LESEN", "ID;0x1701", 3, "STAT_SYSTEMZEIT_WERT", ecuKom, vehicle);
                }
                else
                {
                    Log.Warning(Log.CurrentMethod(), "Found BN2000 Motorbike. VehicleLifeStartdate cannot be read out of the vehicle");
                }
            }
            else if (!ExecuteVehicleLifeStartDateJobAndProcessResults("G_ZGW", "STATUS_LESEN", "ID;0x1701", 3, "STAT_SYSTEMZEIT_WERT", ecuKom, vehicle))
            {
                ExecuteVehicleLifeStartDateJobAndProcessResults("BCP_SP21", "STATUS_LESEN", "ID;0x1769", 3, "STAT_SYSTIME_SECONDS_WERT", ecuKom, vehicle, "STAT_SYSTIME_SECONDS");
            }
        }

        public bool IsEPMEnabled(IVehicle vehicle)
        {
            if (vehicle != null && !string.IsNullOrEmpty(vehicle.Produktlinie))
            {
                if (vehicle.BrandName != BrandName.BMWMOTORRAD)
                {
                    return !ProductLinesEpmBlacklist.Contains(vehicle.Produktlinie);
                }
                return false;
            }
            return false;
        }

        public IEcuJob ExecuteFSLesenExpert(IEcuKom ecuKom, string variant, int retries)
        {
            if (fsLesenExpertVariants.Any((string v) => v.Equals(variant, StringComparison.InvariantCultureIgnoreCase)))
            {
                return ecuKom.ApiJobWithRetries(variant, "FS_LESEN_EXPERT", ";0x2C;0x20", string.Empty, retries);
            }
            return null;
        }

        public void MaskResultsFromFSLesenExpertForFSLesenDetail(IEcuJob ecuJob)
        {
            MaskResultFASTARelevant(ecuJob, 1, 1, new List<string>
        {
            "F_ORT_NR", "F_EREIGNIS_DTC", "F_UEBERLAUF", "F_VORHANDEN_NR", "F_READY_NR", "F_WARNUNG_NR", "F_HFK", "F_HLZ", "F_SAE_CODE_STRING", "F_HEX_CODE",
            "F_FEHLERKLASSE"
        });
            MaskResultFASTARelevant(ecuJob, 1, -2, new List<string> { "F_UW_KM", "F_UW_KM_SUPREME", "F_UW_ZEIT", "F_UW_ZEIT_SUPREME", "F_UW_ANZ", "F_UW*_NR", "F_UW*_WERT", "F_UW_BN", "F_UW_TN" });
        }

        public bool CheckForSpecificModelPopUpForElectricalChecks(string ereihe)
        {
            if (!specificModelsNoPopUp.Contains(ereihe))
            {
                string item = Regex.Replace(ereihe, "[0-9]", "x");
                if (!placeholderModelsNopPopUp.Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        public string GetFourCharEreihe(string ereihe)
        {
            if (ereihe != null && ereihe.Length == 3)
            {
                return ereihe.Insert(1, "0");
            }
            if (ereihe != null && ereihe.Length == 4)
            {
                return ereihe;
            }
            return string.Empty;
        }

        public void ShowAdapterHintMotorCycle(IProgressMonitor monitor, IOperationServices services, string eReihe, string basicType)
        {
            switch (eReihe)
            {
                case "K16":
                    if (!monitor.RequestConfirmation(ProgressRequestConfirmationType.Information, FormatedData.Localize("#Warning"), FormatedData.Localize("#064")))
                    {
                        Log.Info("CommandVINVehicleData.DoExecute()", "No adapter confirmed for motorcycle {0}", eReihe);
                    }
                    break;
                case "259S":
                case "R22":
                case "R21":
                case "R28":
                case "259C":
                case "K589":
                case "K30":
                case "K41":
                    if ("0308,0309,0318,0319,0329,0362,0379,0391,0405,0414,0415,0417,0419,0421,0422,0424,0428,0429,0431,0432,0433,0434,0438,0439,0441,0442,0447,0477,0492,0495,0496,0498,0499,0544,0545,0547,0548,0549,0554,0555,0557,0558,0559".Contains(basicType))
                    {
                        InteractionMotorcycleMRMA24Model model = new InteractionMotorcycleMRMA24Model();
                        services.InteractionService.Register(model);
                    }
                    break;
            }
        }

        private void SetVehicleLifeStartDateWithJobResult(IVehicle vehicle, int? result)
        {
            vehicle.VehicleLifeStartDate = DateTime.Now.AddSeconds(-result.Value);
        }

        private void MaskResultFASTARelevant(IEcuJob ecuJob, ushort startSet, int stopSet, IList<string> fsLesenExpertResultNames)
        {
            fsLesenExpertResultNames.ForEach(delegate (string x)
            {
                ecuJob.maskResultFASTARelevant(startSet, stopSet, x);
            });
        }

        private bool SetVehicleLifeStartDateWithAlternativeResult(IVehicle vehicle, string alternativeResult, ECUJob job)
        {
            bool result = false;
            if (string.IsNullOrEmpty(alternativeResult))
            {
                Log.Warning(Log.CurrentMethod(), "VehicleLifeStartdate could not be read out of the vehicle because the alternative Resultname was empty!");
            }
            else
            {
                int? result2 = job.getintResult(1, alternativeResult);
                if (result2.HasValue)
                {
                    SetVehicleLifeStartDateWithJobResult(vehicle, result2);
                    result = true;
                }
                else
                {
                    Log.Warning(Log.CurrentMethod(), "VehicleLifeStartdate could not be read out of the vehicle with the Job {0} , params {1} and resultname {2}", job.JobName, job.JobParam, alternativeResult);
                }
            }
            return result;
        }

        private bool ExecuteVehicleLifeStartDateJobAndProcessResults(string sgbd, string jobName, string jobParams, int retryCount, string resultname, IEcuKom ecuKom, IVehicle vehicle, string alternativeResult = "")
        {
            bool result = false;
            ECUJob eCUJob = ecuKom.ApiJobWithRetries(sgbd, jobName, jobParams, string.Empty, retryCount) as ECUJob;
            if (eCUJob.IsOkay())
            {
                int? result2 = eCUJob.getintResult(1, resultname);
                if (result2.HasValue)
                {
                    SetVehicleLifeStartDateWithJobResult(vehicle, result2);
                    result = true;
                }
                else
                {
                    result = SetVehicleLifeStartDateWithAlternativeResult(vehicle, alternativeResult, eCUJob);
                }
            }
            return result;
        }

        public IEcuJob ClampShutdownManagement(IVehicle vecInfo, IEcuKom ecuKom, int retryCount = 2, int i_geschw_schwelle = 30)
        {
            IEcuJob clampJob = null;
            if (ecuKom != null && vecInfo != null)
            {
                switch (vecInfo.BNType)
                {
                    case BNType.BN2020:
                        {
                            string variante = null;
                            DetermineBn2020CentralEcuVariant(vecInfo, ecuKom, retryCount, ref clampJob, ref variante);
                            switch (variante)
                            {
                                default:
                                    Log.Info(Log.CurrentMethod(), "Unexpected Variant for clamp shutdown management appeared: " + variante);
                                    ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ZUSTAND_FAHRZEUG", "PRUEFEN_ANALYSE_DIAGNOSE", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ARG;STEUERN_KLEMME15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "BCP_SP21":
                                    vecInfo.PADVehicle = true;
                                    ecuKom.ApiJobWithRetries("G_ZGW", "STEUERN_ZUSTAND_FAHRZEUG", "PRUEFEN_ANALYSE_DIAGNOSE", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("G_ZGW", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "BDC_G11":
                                case "BDC_G05":
                                    vecInfo.PADVehicle = true;
                                    ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ZUSTAND_FAHRZEUG", "PRUEFEN_ANALYSE_DIAGNOSE", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "BDC":
                                case "FEM_20":
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "CAS4_2":
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ID;0xAC51;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                            }
                            break;
                        }
                    case BNType.BN2000:
                    case BNType.BEV2010:
                        clampJob = ecuKom.ApiJobWithRetries("D_CAS", "STEUERN_KL15_ABSCHALTUNG", i_geschw_schwelle.ToString(CultureInfo.InvariantCulture), string.Empty, retryCount);
                        break;
                }
            }
            return clampJob;
        }

        private void DetermineBn2020CentralEcuVariant(IVehicle vecInfo, IEcuKom ecuKom, int retryCount, ref IEcuJob clampJob, ref string variante)
        {
            if (IsSp2021Gateway(vecInfo, ecuKom, retryCount))
            {
                IEcu eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE("G_ZGW");
                if (eCUbyECU_GRUPPE != null)
                {
                    variante = ((!string.IsNullOrEmpty(eCUbyECU_GRUPPE.VARIANTE)) ? eCUbyECU_GRUPPE.VARIANTE.ToUpper() : null);
                }
                if (string.IsNullOrEmpty(variante))
                {
                    IEcuJob ecuJob = ecuKom.DefaultApiJob("G_ZGW", "IDENT", string.Empty, string.Empty);
                    if (ecuJob.IsDone())
                    {
                        variante = ecuJob.getStringResult("VARIANTE");
                    }
                }
                return;
            }
            IEcu eCUbyECU_GRUPPE2 = vecInfo.getECUbyECU_GRUPPE("G_CAS");
            if (eCUbyECU_GRUPPE2 != null)
            {
                variante = ((!string.IsNullOrEmpty(eCUbyECU_GRUPPE2.VARIANTE)) ? eCUbyECU_GRUPPE2.VARIANTE.ToUpper() : null);
            }
            if (string.IsNullOrEmpty(variante))
            {
                clampJob = ecuKom.DefaultApiJob("G_CAS", "IDENT", string.Empty, string.Empty);
                if (clampJob.IsDone())
                {
                    variante = clampJob.getStringResult("VARIANTE");
                }
            }
        }

        public IEcuJob SendJobToKombiOrMmi(IVehicle vecInfo, IEcuKom ecuKom, string job, string param, string resultFilter, int retries)
        {
            IEcu eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE("G_MMI");
            IEcu eCUbyECU_GRUPPE2 = vecInfo.getECUbyECU_GRUPPE("G_KOMBI");
            if (eCUbyECU_GRUPPE != null && eCUbyECU_GRUPPE2 == null)
            {
                if (job == "STATUS_GWSZ_ANZEIGE")
                {
                    param = "ARG;GWSZ_ANZEIGE_WERT";
                }
                return ecuKom.ApiJobWithRetries("G_MMI", Mapping[job], param, resultFilter, retries);
            }
            return ecuKom.ApiJobWithRetries("G_KOMBI", job, param, resultFilter, retries);
        }

        public IEcuJob SendStatusLesenCcmJobToKombiOrMmi(IVehicle vecInfo, IEcuKom ecuKom)
        {
            string job = "STATUS_LESEN";
            string param = "ARG;BMW_CC_DATENSAETZE";
            IEcu ecu = vecInfo.getECUbyECU_GRUPPE("G_KOMBI") ?? vecInfo.getECUbyECU_GRUPPE("G_MMI");
            if (ecu == null)
            {
                Log.Info(Log.CurrentMethod(), "No G_KOMBI and G_MMI ecu group exists in the vehicle. CCM readout will be skipped.");
                return null;
            }
            Log.Info(Log.CurrentMethod(), "CCM readout will use '" + ecu.VARIANTE + "' ecu.");
            return ecuKom.ApiJobWithRetries(ecu.VARIANTE, job, param, string.Empty, 1);
        }

        public string ReadVinForGroupCars(BNType bNType, IEcuKom ecuKom)
        {
            Dictionary<string, EcuKomConfig> dictionary = new Dictionary<string, EcuKomConfig>();
            dictionary.Add("G_ZGW", new EcuKomConfig("G_ZGW", "STATUS_VIN_LESEN", string.Empty, 1, "STAT_VIN"));
            dictionary.Add("G_CAS", new EcuKomConfig("G_CAS", "STATUS_FAHRGESTELLNUMMER", string.Empty, 1, "STAT_FGNR17_WERT"));
            dictionary.Add("G_FRM", new EcuKomConfig("G_FRM", "STATUS_VCM_VIN", string.Empty, 1, "STAT_VIN_EINH"));
            dictionary.Add("D_LM", new EcuKomConfig("D_LM", "READ_FVIN", string.Empty, 1, "FVIN"));
            dictionary.Add("FRM_87", new EcuKomConfig("FRM_87", "READ_FVIN", string.Empty, 1, "FVIN"));
            dictionary.Add("D_CAS", new EcuKomConfig("D_CAS", "STATUS_FAHRGESTELLNUMMER", string.Empty, 1, "FGNUMMER"));
            dictionary.Add("ZCS_ALL", new EcuKomConfig("ZCS_ALL", "FGNR_LESEN", string.Empty, 1, "FG_NR"));
            dictionary.Add("D_0080", new EcuKomConfig("D_0080", "AIF_FG_NR_LESEN", string.Empty, 1, "AIF_FG_NR"));
            dictionary.Add("D_0010", new EcuKomConfig("D_0080", "AIF_LESEN", string.Empty, 1, "AIF_FG_NR"));
            dictionary.Add("EWS3", new EcuKomConfig("EWS3", "FGNR_LESEN", string.Empty, 1, "FG_NR"));
            dictionary.Add("D_ZGM", new EcuKomConfig("D_ZGM", "C_FG_LESEN", string.Empty, 1, "FG_NR"));
            List<EcuKomConfig> list = new List<EcuKomConfig>();
            switch (bNType)
            {
                case BNType.BN2020:
                    list.Add(dictionary["G_ZGW"]);
                    list.Add(dictionary["G_CAS"]);
                    list.Add(dictionary["G_FRM"]);
                    break;
                case BNType.BEV2010:
                    list.Add(dictionary["D_CAS"]);
                    list.Add(dictionary["D_LM"]);
                    list.Add(dictionary["FRM_87"]);
                    list.Add(dictionary["D_ZGM"]);
                    break;
                case BNType.BN2000:
                    list.Add(dictionary["D_CAS"]);
                    list.Add(dictionary["D_LM"]);
                    list.Add(dictionary["FRM_87"]);
                    list.Add(dictionary["D_ZGM"]);
                    break;
                case BNType.IBUS:
                    list.Add(dictionary["ZCS_ALL"]);
                    list.Add(dictionary["D_0080"]);
                    list.Add(dictionary["D_0010"]);
                    list.Add(dictionary["EWS3"]);
                    break;
                default:
                    foreach (KeyValuePair<string, EcuKomConfig> item in dictionary)
                    {
                        list.Add(item.Value);
                    }
                    break;
            }
            return ReadVinFromEcus(ecuKom, list);
        }

        public string ReadVinForGroupCarsNcar(BNType bNType, IEcuKom ecuKom)
        {
            Dictionary<string, EcuKomConfig> dictionary = new Dictionary<string, EcuKomConfig>();
            List<EcuKomConfig> list = new List<EcuKomConfig>();
            dictionary.Add("IPB_APP1", new EcuKomConfig("IPB_APP1", "STATUS_LESEN", "ARG;VIN", 1, "STAT_VIN_TEXT"));
            list.Add(dictionary["IPB_APP1"]);
            return ReadVinFromEcus(ecuKom, list);
        }

        public string ReadVinForMotorcycles(BNType bNType, IEcuKom ecuKom)
        {
            Dictionary<string, EcuKomConfig> dictionary = new Dictionary<string, EcuKomConfig>();
            dictionary.Add("D_MRMOT", new EcuKomConfig("D_MRMOT", "STATUS_FAHRGESTELLNUMMER", string.Empty, 1, "STAT_FGNUMMER"));
            dictionary.Add("MRBMSMP1", new EcuKomConfig("D_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", 1, "STAT_FAHRGESTELLNUMMER_TEXT"));
            dictionary.Add("G_MRMOT", new EcuKomConfig("G_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", 1, "STAT_FAHRGESTELLNUMMER_TEXT"));
            dictionary.Add("X_K001", new EcuKomConfig("X_K001", "prog_fg_nr_lesen_funktional", "18", 1, "FG_NR_LANG"));
            dictionary.Add("X_KS01", new EcuKomConfig("X_KS01", "prog_fg_nr_lesen_funktional", "18", 1, "FG_NR_LANG"));
            List<EcuKomConfig> list = new List<EcuKomConfig>();
            switch (bNType)
            {
                case BNType.BN2020_MOTORBIKE:
                    list.Add(dictionary["G_MRMOT"]);
                    list.Add(dictionary["X_K001"]);
                    list.Add(dictionary["X_KS01"]);
                    break;
                case BNType.BN2000_MOTORBIKE:
                    list.Add(dictionary["D_MRMOT"]);
                    list.Add(dictionary["MRBMSMP1"]);
                    break;
                default:
                    foreach (KeyValuePair<string, EcuKomConfig> item in dictionary)
                    {
                        list.Add(item.Value);
                    }
                    break;
            }
            return ReadVinFromEcus(ecuKom, list);
        }

        internal string ReadVinFromEcus(IEcuKom ecuKom, List<EcuKomConfig> ecuKomconfigs)
        {
            string text = null;
            foreach (EcuKomConfig ecuKomconfig in ecuKomconfigs)
            {
                try
                {
                    Log.Info("VehicleIdent.GetVin17()", "trying to get VIN from {0} with {1}", ecuKomconfig.Ecu, ecuKomconfig.Job);
                    IEcuJob ecuJob = ecuKom.ApiJob(ecuKomconfig.Ecu, ecuKomconfig.Job, ecuKomconfig.Param, string.Empty);
                    if (ecuJob.IsOkay())
                    {
                        text = ecuJob.getStringResult(ecuKomconfig.Set, ecuKomconfig.Result);
                        if (!string.IsNullOrEmpty(text) && text != "00000000000000000")
                        {
                            Log.Info("VehicleIdent.GetVin17()", "getting VIN was successfully: {0}", text);
                            return text;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("VehicleIdent.GetVin17()", exception);
                }
            }
            return null;
        }
    }
}
