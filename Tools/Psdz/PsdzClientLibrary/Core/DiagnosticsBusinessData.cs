using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core.Container;
using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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

        internal class EcuKomSamples
        {
            private string ecu;

            private string job;

            private string param;

            private ushort set;

            private string type;

            private string result;

            private int satz;

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

            public string Type
            {
                get
                {
                    return type;
                }
                set
                {
                    type = value;
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

            public int Satz
            {
                get
                {
                    return satz;
                }
                set
                {
                    satz = value;
                }
            }

            internal EcuKomSamples(string ecu, string job, string param, ushort set, string type, string result, int satz)
            {
                this.ecu = ecu;
                this.job = job;
                this.param = param;
                this.set = set;
                this.type = type;
                this.result = result;
                this.satz = satz;
            }
        }

        private const string ILevelBN2020RegexPattern = "([A-Z0-9]{4}|[A-Z0-9]{3})-[0-9]{2}[-_](0[1-9]|1[0-2])[-_][0-9]{3}";

        private static readonly DateTime LciDateE36 = DateTime.Parse("1998-03-01", CultureInfo.InvariantCulture);

        private static readonly DateTime LciDateE60 = DateTime.Parse("2005-09-01", CultureInfo.InvariantCulture);

        //private string ServiceCodeValuePattern = "{0}_{1}";

        //private LayoutGroup layoutGroup = LayoutGroup.D;

        private readonly List<string> fsLesenExpertVariants = new List<string>
        {
            "PCU48", "DME9FF_R", "DME98_R", "D94BX7A0", "DME98_L", "IB_I20", "IB_G70", "GSMA02QZ", "GSMA02PU", "GSZF04GD",
            "GSZF04GF", "SCR04", "SCR05", "CCU_P1", "HVS_02", "GSMA02PL", "GSZF04GA"
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

        private readonly string[] maxGrpListMINI = new string[31]
        {
            "D_0008", "D_0012", "D_0013", "D_0031", "D_003B", "D_0044", "D_0050", "D_0057", "D_005B", "D_0060",
            "D_0068", "D_006A", "D_0070", "D_0074", "D_0076", "D_007F", "D_0080", "D_0081", "D_009A", "D_009C",
            "D_00A4", "D_00BB", "D_00C8", "D_00CE", "D_00E8", "D_00ED", "D_00F0", "D_ABSKWP", "D_EGS", "D_MOTOR",
            "D_ZKE_GM"
        };

        private readonly string[] maxGrpListBMWRest = new string[60]
        {
            "D_0014", "D_009A", "D_0022", "D_0040", "D_0013", "D_0000", "D_0008", "D_0012", "D_0032", "D_003B",
            "D_0044", "D_0048", "D_0050", "D_0056", "D_0057", "D_005B", "D_0060", "D_0065", "D_0066", "D_0068",
            "D_006A", "D_0070", "D_0072", "D_0074", "D_0076", "D_007F", "D_0080", "D_0081", "D_009C", "D_009B",
            "D_00A4", "D_00B0", "D_00BB", "D_00C0", "D_00C2", "D_00C8", "D_00CE", "D_00D0", "D_00E8", "D_00EA",
            "D_00ED", "D_00F0", "D_ABSKWP", "D_AHM", "D_BFS", "D_CID", "D_EGS", "D_EKP", "D_EPS", "D_FAS",
            "D_FLA", "D_MOTOR", "D_SIM", "D_STVL2", "D_STVR2", "D_SZM", "D_VGSG", "D_VVT", "D_ZKE_GM", "D_ZUHEIZ"
        };

        private readonly List<string> ereiheForGrpListBMWRest = new List<string>
        {
            "E30", "E31", "E32", "E34", "E36", "E38", "E39", "E46", "E83", "E85",
            "E86"
        };

        private readonly string[] maxGrpListBMW = new string[37]
        {
        "D_0000", "D_0012", "D_0032", "D_003B", "D_0044", "D_0056", "D_0057", "D_005B", "D_0068", "D_006A",
        "D_0070", "D_0072", "D_0074", "D_007F", "D_0081", "D_0080", "D_009C", "D_00A4", "D_00B0", "D_00BB",
        "D_00C0", "D_00C8", "D_00CE", "D_00D0", "D_00E8", "D_00EA", "D_00ED", "D_00F0", "D_ABSKWP", "D_EGS",
        "D_MOTOR", "D_SZM", "D_VGSG", "D_VVT", "D_VVT2", "D_ZKE_GM", "D_ZUHEIZ"
        };

        private readonly List<string> ereiheForGrpListBMW = new List<string> { "E52", "E53" };

        private readonly string[] maxGrpListFull = new string[96]
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

        private readonly string[] varianteListFor14DigitSerialNumber = new string[9] { "NBT", "NBTEVO", "ENTRYNAV", "ENAVEVO", "ENTRY", "HU_MGU", "BIS01", "MGU_02_L", "MGU_02_A" };

        private readonly string[] newFaultMemoryEnabledESeriesLifeCycles = new string[8] { "F95-1", "F96-1", "G05-1", "G06-1", "G07-1", "G09-0", "G18-1", "RR25-0" };

        private readonly List<string> ereiheWithoutFA = new List<string>
    {
        "E36", "E38", "E39", "E52", "E53", "R50", "R52", "R53", "E83", "E85",
        "E86", "E30", "E31", "E32", "E34"
    };

        private const string RsuStopModuleName = "ABL-DIT-AG6510_RSU_STOP";

        private const string RsuStartModuleName = "ABL-DIT-AG6510_RSU_START";

        private const string CheckVoltageModuleName = "ABL-LIF-FAHRZEUGDATEN__BATTERIE";

        private const string RequestApplicationNumberAndUpgradeModuleName = "ABL-GEN-DETERMINE_REFURBISH_SWID";

        private const string ServiceHistoryActionModuleName = "ABL-WAR-AS6100_SERVICEHISTORIE_AIR";

        private static IDictionary<string, string> Mapping = new Dictionary<string, string>
    {
        { "STATUS_VCM_BACKUP_FAHRZEUGAUFTRAG_LESEN", "STATUS_VCM_BACKUP_FAHRZEUGAUFTRAG_LESEN_SP2021" },
        { "STATUS_LESEN", "STATUS_LESEN" },
        { "STATUS_I_STUFE_LESEN_OHNE_SIGNATUR", "STATUS_I_STUFE_LESEN_OHNE_SIGNATUR" },
        { "STATUS_GWSZ_ANZEIGE", "STATUS_LESEN" },
        { "CBS_DATEN_LESEN", "STATUS_CBS_DATEN_LESEN" },
        { "CBS_INFO", "STATUS_CBS_INFO" }
    };

        public List<string> ProductLinesEpmBlacklist => new List<string> { "PL0", "PL2", "PL3", "PL3-ALT", "PL4", "PL5", "PL5-ALT", "PL6", "PL6-ALT", "PL7" };

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
            dictionary.Add("D_KOMBI", new EcuKomConfig("D_KOMBI", "STATUS_ANGEZEIGTER_GWSZ", string.Empty, 1, "STAT_GWSZ"));
            dictionary.Add("D_0080", new EcuKomConfig("D_0080", "GWSZ_MINUS_OFFSET_LESEN", string.Empty, 1, "STAT_GWSZ_MINUS_OFFSET_WERT"));
            dictionary.Add("KOMBI36C", new EcuKomConfig("KOMBI36C", "AIF_GWSZ_LESEN", string.Empty, 1, "STAT_GWSZ_WERT"));
            dictionary.Add("KOMBI85", new EcuKomConfig("KOMBI85", "STATUS_AIF_GWSZ_LESEN", string.Empty, 1, "STAT_GWSZ_WERT"));
            List<EcuKomConfig> list = new List<EcuKomConfig>();
            if (vecInfo != null)
            {
                if (vecInfo.BNType == BNType.BN2020)
                {
                    if (vecInfo.Produktlinie.Equals("PL5-alt", StringComparison.InvariantCultureIgnoreCase))
                    {
                        IEcu eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE("D_KOMBI");
                        if (eCUbyECU_GRUPPE != null && eCUbyECU_GRUPPE.VARIANTE.ToUpper() == "KOMBRR_2")
                        {
                            list.Add(dictionary["KOMBRR_2"]);
                            list.Add(dictionary["D_KOMBI"]);
                        }
                    }
                    if (vecInfo.Classification.IsSp2021)
                    {
                        list.Add(dictionary["G_VIP"]);
                        list.Add(dictionary["G_ZGW"]);
                    }
                    if (vecInfo.Classification.IsNCar)
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
                            return GetMileageFromJob(ecuJob, "STAT_GWSZ_WERT").GetValueOrDefault();
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

        [PreserveSource(Hint = "Using rector from vehicle")]
        public void ReadILevelBn2020(IVehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
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
            else if (vecInfo.Classification.IsNCar)
            {
                ecuJob = ecuKom.ApiJobWithRetries("IPB_APP2", "STATUS_LESEN", "ARG;VCM_DID_ISTUFE", string.Empty, retryCount);
                if (ecuJob.IsOkay())
                {
                    ProcessILevelJobResultsEES25(instance, vecInfo, ecuJob);
                }
                else
                {
                    HandleReadILevelForGzgwFallback(instance, vecInfo, ecuKom, retryCount);
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

        private void HandleReadILevelForGzgwFallback(Reactor reactor, IVehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
            IEcuJob ecuJob = SendStatusLesenNcarFallbackJobMmi(ecuKom, retryCount);
            if (ecuJob.IsOkay())
            {
                ProcessILevelJobResultsEES25(reactor, vecInfo, ecuJob);
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

        public new string GetMainSeriesSgbd(IIdentVehicle vecInfo)
        {
            return base.GetMainSeriesSgbd(vecInfo);
        }

        public string GetMainSeriesSgbdAdditional(IIdentVehicle vecInfo)
        {
            return GetMainSeriesSgbdAdditional(vecInfo, new NugetLogger());
        }

        public bool IsSp2021Enabled(IVehicle vecInfo)
        {
            if (string.IsNullOrEmpty(vecInfo.Produktlinie) && ConfigSettings.SelectedBrand == UiBrand.BMWMotorrad)
            {
                vecInfo.Produktlinie = "-";
            }
            return vecInfo.Produktlinie?.StartsWith("21") ?? false;
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
            if (vecInfo.Classification.IsSp2021 || text.Equals("BCP_SP21", StringComparison.OrdinalIgnoreCase))
            {
                Log.Info(Log.CurrentMethod(), "Vehicle gateway is bcp_sp21!");
                return true;
            }
            Log.Info(Log.CurrentMethod(), "Vehicle gateway is not bcp_sp21!");
            return false;
        }

        public bool IsSp2025Enabled(IVehicle vecInfo)
        {
            if (ConfigSettings.SelectedBrand == UiBrand.BMWMotorrad && string.IsNullOrEmpty(vecInfo.Produktlinie))
            {
                vecInfo.Produktlinie = "-";
            }
            return vecInfo.Produktlinie?.StartsWith("25") ?? false;
        }

        public bool IsNewFaultMemoryEnabled(IVehicle vecInfo)
        {
            if (!ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.NewFaultMemory.NewActivationCondition", defaultValue: true))
            {
                Log.Info(Log.CurrentMethod(), "New condition for enabling new fault memory is disabled. Using old method (sp2021).");
                return vecInfo.Classification.IsSp2021;
            }
            if (!vecInfo.Classification.IsSp2021 && !vecInfo.Classification.IsSp2025)
            {
                return newFaultMemoryEnabledESeriesLifeCycles.Any((string eslc) => eslc.Equals(vecInfo.ESeriesLifeCycle, StringComparison.InvariantCultureIgnoreCase));
            }
            return true;
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
                                foreach (IEcu item in vecInfo.ECU)
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
                            if (vecInfo.Classification.IsNCar)
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

        // ToDo: Check on update
        public bool IsEES25Vehicle(IVehicle vecInfo)
        {
            if (vecInfo.Classification.IsSp2025)
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
        public BNType GetBNType(IVehicle vehicle)
        {
            return (BNType)GetBordnetType(vehicle.Baureihenverbund, vehicle.Prodart, vehicle.Ereihe, new NugetLogger());
        }

        // ToDo: Check on update
        public void BN2000HandleKMMFixes(IVehicle vecInfo, IEcuKom ecuKom, bool resetMOSTDone, IProgressMonitor monitor, int retryCount, DoECUIdentDelegate doECUIdentDelegate)
        {
            if ((vecInfo.HasSA("6VC") || vecInfo.HasSA("612") || vecInfo.HasSA("633")) && vecInfo.getECU(54L) == null)
            {
                ECU eCU = new ECU();
                eCU.BUS = BusType.MOST;
                eCU.ID_SG_ADR = 54L;
                eCU.ECU_GRUPPE = "D_TEL";
                eCU.ECU_GROBNAME = "TEL";
                vecInfo.AddEcu(eCU);
                doECUIdentDelegate(vecInfo, eCU, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.HasSA("610") && vecInfo.getECU(61L) == null)
            {
                ECU eCU2 = new ECU();
                eCU2.BUS = BusType.MOST;
                eCU2.ID_SG_ADR = 61L;
                eCU2.ECU_GRUPPE = "D_HUD";
                eCU2.ECU_GROBNAME = "HUD";
                vecInfo.AddEcu(eCU2);
                doECUIdentDelegate(vecInfo, eCU2, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.HasSA("672") && vecInfo.getECU(60L) == null)
            {
                ECU eCU3 = new ECU();
                eCU3.BUS = BusType.MOST;
                eCU3.ID_SG_ADR = 60L;
                eCU3.ECU_GRUPPE = "D_CDC";
                eCU3.ECU_GROBNAME = "CDC";
                vecInfo.AddEcu(eCU3);
                doECUIdentDelegate(vecInfo, eCU3, ecuKom, ref resetMOSTDone, monitor, retryCount, forceReRead: false, tryReanimation: true);
            }
            if (vecInfo.HasSA("696") && vecInfo.getECU(49L) == null)
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

        public void HandleECUGroups(IVehicle vecInfo, IEcuKom ecuKom, List<IEcu> ecusToRemoveKMM)
        {
            IEcu eCUbyECU_GRUPPE = vecInfo.getECUbyECU_GRUPPE("D_RLS");
            if (eCUbyECU_GRUPPE == null && "e89x".Equals(vecInfo.MainSeriesSgbd, StringComparison.OrdinalIgnoreCase))
            {
                if (vecInfo.HasSA("521"))
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
                if ((eCUbyECU_GRUPPE3.IDENT_SUCCESSFULLY && string.Compare(eCUbyECU_GRUPPE3.VARIANTE, "RAD2", StringComparison.OrdinalIgnoreCase) == 0) || vecInfo.HasSA("6VC") || vecInfo.getECUbyECU_SGBD("CMEDIAR") != null)
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
                if (vehicle.BNType == BNType.BN2000_MOTORBIKE || vehicle.BNType == BNType.BNK01X_MOTORBIKE)
                {
                    Log.Warning(Log.CurrentMethod(), "Found BN2000 Motorbike. VehicleLifeStartdate cannot be read out of the vehicle");
                }
                else
                {
                    ExecuteVehicleLifeStartDateJobAndProcessResults("G_MRKOMB", "STATUS_LESEN", "ID;0x1701", 3, "STAT_SYSTEMZEIT_WERT", ecuKom, vehicle);
                }
            }
            else if (vehicle.Classification.IsNCar)
            {
                ExecuteVehicleLifeStartDateJobAndProcessResults("IPB_APP1", "STATUS_LESEN", "ARG;SYSTEM_TIME_SUPREME", 3, "SECONDS", ecuKom, vehicle, null, "MILLISECONDS");
            }
            else if (!ExecuteVehicleLifeStartDateJobAndProcessResults("G_ZGW", "STATUS_LESEN", "ID;0x1701", 3, "STAT_SYSTEMZEIT_WERT", ecuKom, vehicle))
            {
                ExecuteVehicleLifeStartDateJobAndProcessResults("BCP_SP21", "STATUS_LESEN", "ID;0x1769", 3, "STAT_SYSTIME_SECONDS_WERT", ecuKom, vehicle, "STAT_SYSTIME_SECONDS", "STAT_SYSTIME_MILLISECONDS_WERT", "STAT_SYSTIME_MILLISECONDS");
            }
        }

        public bool IsEPMEnabled(IVehicle vehicle)
        {
            if (vehicle == null || string.IsNullOrEmpty(vehicle.Produktlinie))
            {
                return false;
            }
            if (vehicle.BrandName != BrandName.BMWMOTORRAD)
            {
                return !ProductLinesEpmBlacklist.Contains(vehicle.Produktlinie);
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

        public List<string> RemoveFirstDigitOfSalapaIfLengthIs4(List<string> salapa)
        {
            string text = salapa.FirstOrDefault();
            if (text != null && text.Length == 4)
            {
                salapa = salapa.Select((string x) => x.Substring(1)).ToList();
            }
            return salapa;
        }

        public void Add14DigitFakeSerialNumberToFstdat(IVehicle vecInfo, IEnumerable<IEcuJob> jobList)
        {
            IEnumerable<IEcu> enumerable = vecInfo.ECU.Where((IEcu x) => varianteListFor14DigitSerialNumber.Contains(x.VARIANTE) && x.SERIENNUMMER != null && x.SERIENNUMMER.Length == 14);
            IEnumerable<IEcuJob> source = jobList.Where((IEcuJob x) => x.JobName.Equals("SERIENNUMMER_LESEN"));
            Dictionary<IEcu, ECUJob> dictionary = new Dictionary<IEcu, ECUJob>();
            foreach (IEcu ecu in enumerable)
            {
                IEnumerable<IEcuJob> source2 = source.Where((IEcuJob job) => job.EcuName.Equals(ecu.ECU_SGBD, StringComparison.OrdinalIgnoreCase) || job.EcuName.Equals(ecu.ECU_GRUPPE, StringComparison.OrdinalIgnoreCase));
                if (source2.Count() != 0)
                {
                    dictionary.Add(ecu, (ECUJob)source2.First());
                }
            }
            foreach (KeyValuePair<IEcu, ECUJob> item in dictionary)
            {
                IEcu key = item.Key;
                ECUJob value = item.Value;
                ECUResult eCUResult = value.JobResult.FirstOrDefault((ECUResult x) => x.Name == "SERIENNUMMER");
                if (eCUResult != null)
                {
                    ECUResult eCUResult2 = new ECUResult();
                    eCUResult2.Name = "SERIENNUMMER14";
                    eCUResult2.Value = key.SERIENNUMMER;
                    eCUResult2.Set = eCUResult.Set;
                    eCUResult2.Format = 6;
                    eCUResult2.Length = 0u;
                    eCUResult2.FASTARelevant = true;
                    value.JobResult.Add(eCUResult2);
                }
            }
        }

        public void CheckEcusFor14DigitSerialNumber(IEcuKom ecuKom, IEnumerable<IEcu> ecus)
        {
            foreach (IEcu ecu in ecus)
            {
                if (!(ecu is ECU eCU) || !varianteListFor14DigitSerialNumber.Contains(eCU.VARIANTE))
                {
                    continue;
                }
                IEcuJob ecuJob = ecuKom.ApiJob(eCU.VARIANTE, "STATUS_LESEN", "ID;0xD019", string.Empty, 3);
                if (ecuJob.IsOkay())
                {
                    string sERIENNUMMER = eCU.SERIENNUMMER;
                    string stringResult = ecuJob.getStringResult("STAT_SER_NR_DOM_WERT");
                    stringResult = ((stringResult == null) ? ecuJob.getStringResult("STAT_SER_NR_DOM_TEXT") : stringResult);
                    if (stringResult == null)
                    {
                        Log.Info("SerialNumberUtility.CheckEcusFor14DigitSerialNumber()", "The 14 digit serialnumber for the ecu with the variante '{0}' is null.", ecu.VARIANTE);
                    }
                    else if (stringResult.Length != 14)
                    {
                        Log.Warning("SerialNumberUtility.CheckEcusFor14DigitSerialNumber()", "The serialnumber for ecu variante '{0}' has '{1}' digits, expectet 14.", ecu.VARIANTE, stringResult.Length);
                    }
                    else
                    {
                        eCU.SERIENNUMMER = stringResult;
                        Log.Info("SerialNumberUtility.CheckEcusFor14DigitSerialNumber()", "The serialnumber for the ecu with the variante '{0}' was changed from '{1}' to '{2}'.", ecu.VARIANTE, sERIENNUMMER, stringResult);
                    }
                }
                else
                {
                    Log.Warning("SerialNumberUtility.CheckEcusFor14DigitSerialNumber()", "Changing the serialnumber to 14-digits for the ecu with the variante '{0}' fails.", ecu.VARIANTE);
                }
            }
        }
#if false
        public void ShowIsarPopup(IVehicle vecInfo, IFFMDynamicResolver fFMResolver, IInteractionService services)
        {
            InfoObject documentAndValidateAgainstRuleEvaluation = DocumentUtlitiy.GetDocumentAndValidateAgainstRuleEvaluation(vecInfo, fFMResolver, "POP-IDE%");
            if (documentAndValidateAgainstRuleEvaluation != null)
            {
                services.Register(new InteractionISARPopupModel
                {
                    Html = documentAndValidateAgainstRuleEvaluation.Content.TransformedDocument,
                    Title = documentAndValidateAgainstRuleEvaluation.Title,
                    DialogSize = 2
                });
            }
        }
#endif
        public bool ShouldNotValidateFAForOldCars(string ereihe, DateTime constructionDate)
        {
            if (ereiheWithoutFA.Contains(ereihe) || (ereihe == "E46" && constructionDate < new DateTime(2004, 4, 1)))
            {
                return true;
            }
            return false;
        }

        public bool IsPreDS2Vehicle(string ereihe, DateTime? c_DateTime)
        {
            if (!string.IsNullOrEmpty(ereihe))
            {
                if (Regex.Match(ereihe, "^E[0-3][0-5]$").Success)
                {
                    return true;
                }
                if ("E36".Equals(ereihe))
                {
                    return c_DateTime < LciDateE36;
                }
            }
            return false;
        }

        public bool IsPreE65Vehicle(string ereihe)
        {
            if (!string.IsNullOrEmpty(ereihe) && (Regex.Match(ereihe, "^E[0-5][0-9]$").Success || Regex.Match(ereihe, "^E6[0-4]$").Success))
            {
                return true;
            }
            return false;
        }

        public bool? HasMSAButton(FA fa, DateTime? c_DateTime, string productLine)
        {
            switch (productLine.ToUpper())
            {
                case "PL6-ALT":
                    if (fa != null && fa != null && c_DateTime > LciDateE60)
                    {
                        return true;
                    }
                    return false;
                case "PL5-ALT":
                case "PL3-ALT":
                    return false;
                case "PL2":
                case "PL3":
                case "PL4":
                case "PL5":
                case "PL6":
                case "PL7":
                case "35LG":
                case "PLLI":
                case "PLLU":
                    return true;
                default:
                    return null;
            }
        }

        public string GetServiceProgramName(TestModuleName testmoduleName)
        {
            switch (testmoduleName)
            {
                case TestModuleName.CheckVoltage:
                    return "ABL-LIF-FAHRZEUGDATEN__BATTERIE";
                case TestModuleName.RsuStart:
                    return "ABL-DIT-AG6510_RSU_START";
                case TestModuleName.RsuStop:
                    return "ABL-DIT-AG6510_RSU_STOP";
                case TestModuleName.RequestApplicationNumberAndUpgrade:
                    return "ABL-GEN-DETERMINE_REFURBISH_SWID";
                case TestModuleName.ServiceHistoryAction:
                    return "ABL-WAR-AS6100_SERVICEHISTORIE_AIR";
                default:
                    return null;
            }
        }

        private void MaskResultFASTARelevant(IEcuJob ecuJob, ushort startSet, int stopSet, IList<string> fsLesenExpertResultNames)
        {
            fsLesenExpertResultNames.ForEach(delegate (string x)
            {
                ecuJob.maskResultFASTARelevant(startSet, stopSet, x);
            });
        }

        private void SetVehicleLifeStartDateWithJobResult(IVehicle vehicle, long seconds, long? milliseconds)
        {
            DateTime vehicleLifeStartDate = DateTime.Now.AddSeconds(-seconds);
            if (milliseconds.HasValue)
            {
                vehicleLifeStartDate = vehicleLifeStartDate.AddMilliseconds(-milliseconds.Value);
            }
            vehicle.VehicleLifeStartDate = vehicleLifeStartDate;
            vehicle.VehicleSystemTime = (double)seconds + (milliseconds.HasValue ? ((double)milliseconds.Value / 1000.0) : 0.0);
        }

        private bool ExecuteVehicleLifeStartDateJobAndProcessResults(string sgbd, string jobName, string jobParams, int retryCount, string resultname, IEcuKom ecuKom, IVehicle vehicle, string alternativeResult = null, string supremeResultName = null, string alternativeSupremeResultName = null)
        {
            bool result = false;
            ECUJob eCUJob = ecuKom.ApiJobWithRetries(sgbd, jobName, jobParams, string.Empty, retryCount) as ECUJob;
            if (eCUJob.IsOkay())
            {
                long? num = eCUJob.getlongResult(1, resultname);
                if (!num.HasValue && !string.IsNullOrWhiteSpace(alternativeResult))
                {
                    num = eCUJob.getlongResult(1, alternativeResult);
                }
                long? milliseconds = eCUJob.getlongResult(supremeResultName);
                if (!milliseconds.HasValue && !string.IsNullOrWhiteSpace(alternativeSupremeResultName))
                {
                    milliseconds = eCUJob.getlongResult(alternativeSupremeResultName);
                }
                if (num.HasValue)
                {
                    SetVehicleLifeStartDateWithJobResult(vehicle, num.Value, milliseconds);
                    result = true;
                }
                else
                {
                    Log.Warning(Log.CurrentMethod(), "VehicleLifeStartdate could not be read out of the vehicle with the Job " + eCUJob.JobName + ", params " + eCUJob.JobParam + " and resultnames: " + resultname + ", " + alternativeResult + ", " + supremeResultName + ", " + alternativeSupremeResultName);
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
                    case BNType.BN2000:
                    case BNType.BEV2010:
                        clampJob = ecuKom.ApiJobWithRetries("D_CAS", "STEUERN_KL15_ABSCHALTUNG", i_geschw_schwelle.ToString(CultureInfo.InvariantCulture), string.Empty, retryCount);
                        break;
                    case BNType.BN2020:
                        {
                            string variante = null;
                            DetermineBn2020CentralEcuVariant(vecInfo, ecuKom, retryCount, ref clampJob, ref variante);
                            switch (variante)
                            {
                                case "CAS4_2":
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ID;0xAC51;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "FEM_20":
                                case "BDC":
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "BDC_G05":
                                case "BDC_G11":
                                    vecInfo.PADVehicle = true;
                                    ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ZUSTAND_FAHRZEUG", "PRUEFEN_ANALYSE_DIAGNOSE", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "BCP_SP21":
                                    vecInfo.PADVehicle = true;
                                    ecuKom.ApiJobWithRetries("G_ZGW", "STEUERN_ZUSTAND_FAHRZEUG", "PRUEFEN_ANALYSE_DIAGNOSE", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("G_ZGW", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                                case "IPB_APP1":
                                    vecInfo.PADVehicle = true;
                                    clampJob = ecuKom.ApiJobWithRetries("IPB_APP1", "STEUERN_ROUTINE", "ARG;Zustand_Fahrzeug;STR;7", string.Empty, retryCount);
                                    ecuKom.ApiJobWithRetries("IPB_APP1", "STATUS_LESEN", "ARG;Zustand_Fahrzeug", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("IPB_APP1", "STEUERN_ROUTINE", "ARG;STEUERN_KL15_ABSCHALTUNG;STR;3;", string.Empty, retryCount);
                                    break;
                                default:
                                    Log.Info(Log.CurrentMethod(), "Unexpected Variant for clamp shutdown management appeared: " + variante);
                                    ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ZUSTAND_FAHRZEUG", "PRUEFEN_ANALYSE_DIAGNOSE", string.Empty, retryCount);
                                    clampJob = ecuKom.ApiJobWithRetries("G_CAS", "STEUERN_ROUTINE", "ARG;STEUERN_KLEMME15_ABSCHALTUNG;STR;" + i_geschw_schwelle, string.Empty, retryCount);
                                    break;
                            }
                            break;
                        }
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
            if (ecu.VARIANTE == "IDCEVO25")
            {
                param = "ARG;BMW_CC_DATA_RECORD";
            }
            Log.Info(Log.CurrentMethod(), "CCM readout will use '" + ecu.VARIANTE + "' ecu.");
            return ecuKom.ApiJobWithRetries(ecu.VARIANTE, job, param, string.Empty, 1);
        }

        public IEcuJob SendStatusLesenNcarFallbackJobMmi(IEcuKom ecuKom, int retryCount)
        {
            return ecuKom.ApiJobWithRetries("IDCEVO25", "STATUS_LESEN", "ARG;VCM_DID_ISTUFE", string.Empty, retryCount);
        }

        public string SgbdNext(IEcuKom ecuKom)
        {
            Dictionary<string, EcuKomSamples> obj = new Dictionary<string, EcuKomSamples>
        {
            {
                "FIELD",
                new EcuKomSamples("MARS01", "STATUS_LESEN", "ID;0x1828", 1, "string[]", "STAT_LOG_CHANNEL_NAMES[].LOG_CHANNEL_NAME", -1)
            },
            {
                "FIELD2D",
                new EcuKomSamples("MARS01", "STEUERN_ROUTINE", "ID;0x1118;STR", 1, "double[,]", "IKE_ENTRIES[].CHILD_SA_ENTRIES[].PROTOCOL", -1)
            },
            {
                "FIELDxD",
                new EcuKomSamples("MARS01", "STATUS_LESEN", "ID;0x0000", 1, "int[,,,,,]", "A[].B[].C[].D[].E[].F[].V", -1)
            }
        };
            List<EcuKomSamples> list = new List<EcuKomSamples>();
            foreach (KeyValuePair<string, EcuKomSamples> item in obj)
            {
                list.Add(item.Value);
            }
            DoNewJob(ecuKom);
            DoNewBinaryJob(ecuKom);
            return DoSgbdNextJob(ecuKom, list);
        }

        internal string DoNewJob(IEcuKom ecukom)
        {
            try
            {
                string defaultRes = null;
                string defaultRes2 = null;
                byte[] defaultRes3 = null;
                byte[] defaultRes4 = null;
                long[] defaultRes5 = null;
                byte[,] defaultRes6 = new byte[3, 3];
                byte[,] defaultRes7 = null;
                byte[,] defaultRes8 = null;
                string[] defaultRes9 = null;
                long[,,,] defaultRes10 = null;
                long[,,,] defaultRes11 = new long[0, 0, 0, 0];
                long[,,,] defaultRes12 = new long[4, 2, 3, 5];
                long[,,,] defaultRes13 = new long[5, 5, 5, 5];
                long[,,] defaultRes14 = null;
                long[,,] defaultRes15 = new long[0, 0, 0];
                long[,,] defaultRes16 = new long[2, 3, 5];
                long[,,] defaultRes17 = new long[5, 5, 5];
                long[,,] defaultRes18 = null;
                long[,,] defaultRes19 = new long[0, 0, 0];
                long[,,] defaultRes20 = new long[2, 3, 5];
                long[,,] defaultRes21 = new long[5, 5, 5];
                long[,,] defaultRes22 = null;
                long[,,] defaultRes23 = new long[0, 0, 0];
                long[,,] defaultRes24 = new long[2, 3, 5];
                long[,,] defaultRes25 = new long[5, 5, 5];
                long[,,] defaultRes26 = null;
                long[,,] defaultRes27 = new long[0, 0, 0];
                long[,,] defaultRes28 = new long[2, 3, 5];
                long[,,] defaultRes29 = new long[5, 5, 5];
                long[,,] defaultRes30 = null;
                long[,,] defaultRes31 = new long[0, 0, 0];
                long[,,] defaultRes32 = new long[2, 3, 5];
                long[,,] defaultRes33 = new long[5, 5, 5];
                long[,,] defaultRes34 = null;
                long[,,] defaultRes35 = new long[0, 0, 0];
                long[,,] defaultRes36 = new long[2, 3, 5];
                long[,,] defaultRes37 = new long[5, 5, 5];
                long[,,] defaultRes38 = null;
                long[,,] defaultRes39 = new long[0, 0, 0];
                long[,,] defaultRes40 = new long[2, 3, 5];
                long[,,] defaultRes41 = new long[5, 5, 5];
                string text = "";
                text += 8405239;
                IEcuJob ecuJob = ecukom.ApiJob("BCP_SP21", "FS_LESEN_DETAIL", text, string.Empty);
                if (ecuJob != null && ecuJob.IsDone())
                {
                    defaultRes3 = ecuJob.getResultsAs("F_HEX_CODE", defaultRes3);
                    defaultRes4 = ecuJob.getResultsAs("F_HEX_CODE", defaultRes4);
                    defaultRes6 = ecuJob.getResultsAs("F_UW_BN", defaultRes6, -2);
                    defaultRes7 = ecuJob.getResultsAs("F_UW_BN", defaultRes7, -2);
                    defaultRes8 = ecuJob.getResultsAs("F_UW_BN_X", defaultRes8, -2);
                    defaultRes5 = ecuJob.getResultsAs("F_UW_KM", defaultRes5, -2);
                    defaultRes = ecuJob.getResultsAs("JOB_STATUS", defaultRes);
                    defaultRes2 = ecuJob.getResultsAs("VARIANTE", defaultRes2, 0);
                    defaultRes9 = ecuJob.getResultsAs("TEXTRESULT", defaultRes9, -2);
                    defaultRes10 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes10, -2);
                    defaultRes11 = ecuJob.getResultsAs("A[].B[].C[].W", defaultRes11, -2);
                    defaultRes12 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes12, -2);
                    defaultRes13 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes13, -2);
                    defaultRes14 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes14, 0);
                    defaultRes15 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes15, 0);
                    defaultRes16 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes16, 0);
                    defaultRes17 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes17, 0);
                    defaultRes18 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes18, 1);
                    defaultRes19 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes19, 1);
                    defaultRes20 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes20, 1);
                    defaultRes21 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes21, 1);
                    defaultRes22 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes22, 2);
                    defaultRes23 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes23, 2);
                    defaultRes24 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes24, 2);
                    defaultRes25 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes25, 2);
                    defaultRes26 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes26, 3);
                    defaultRes27 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes27, 3);
                    defaultRes28 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes28, 3);
                    defaultRes29 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes29, 3);
                    defaultRes30 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes30, 4);
                    defaultRes31 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes31, 4);
                    defaultRes32 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes32, 4);
                    defaultRes33 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes33, 4);
                    defaultRes34 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes34, 5);
                    defaultRes35 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes35, 5);
                    defaultRes36 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes36, 5);
                    defaultRes37 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes37, 5);
                    defaultRes38 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes38);
                    defaultRes39 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes39);
                    defaultRes40 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes40);
                    defaultRes41 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes41);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("VehicleIdent.GetVin17()", exception);
            }
            return null;
        }

        internal string DoNewBinaryJob(IEcuKom ecukom)
        {
            try
            {
                byte[,,,,] defaultRes = null;
                byte[,,,,] defaultRes2 = new byte[0, 0, 0, 0, 0];
                byte[,,,,] defaultRes3 = new byte[4, 2, 3, 5, 4];
                byte[,,,,] defaultRes4 = new byte[5, 5, 5, 5, 5];
                byte[,,,] defaultRes5 = null;
                byte[,,,] defaultRes6 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes7 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes8 = new byte[5, 5, 5, 5];
                byte[,,,] defaultRes9 = null;
                byte[,,,] defaultRes10 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes11 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes12 = new byte[5, 5, 5, 5];
                byte[,,,] defaultRes13 = null;
                byte[,,,] defaultRes14 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes15 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes16 = new byte[5, 5, 5, 5];
                byte[,,,] defaultRes17 = null;
                byte[,,,] defaultRes18 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes19 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes20 = new byte[5, 5, 5, 5];
                byte[,,,] defaultRes21 = null;
                byte[,,,] defaultRes22 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes23 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes24 = new byte[5, 5, 5, 5];
                byte[,,,] defaultRes25 = null;
                byte[,,,] defaultRes26 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes27 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes28 = new byte[5, 5, 5, 5];
                byte[,,,] defaultRes29 = null;
                byte[,,,] defaultRes30 = new byte[0, 0, 0, 0];
                byte[,,,] defaultRes31 = new byte[2, 3, 5, 4];
                byte[,,,] defaultRes32 = new byte[5, 5, 5, 5];
                string text = "";
                text += 8405239;
                IEcuJob ecuJob = ecukom.ApiJob("BCP_SP21", "FS_LESEN_DETAIL_BINARY", text, string.Empty);
                if (ecuJob != null && ecuJob.IsDone())
                {
                    defaultRes = ecuJob.getResultsAs("A[].B[].C[].V1", defaultRes, -2);
                    defaultRes2 = ecuJob.getResultsAs("A[].B[].C[].V1", defaultRes2, -2);
                    defaultRes3 = ecuJob.getResultsAs("A[].B[].C[].V1", defaultRes3, -2);
                    defaultRes4 = ecuJob.getResultsAs("A[].B[].C[].V1", defaultRes4, -2);
                    defaultRes5 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes5, 0);
                    defaultRes6 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes6, 0);
                    defaultRes7 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes7, 0);
                    defaultRes8 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes8, 0);
                    defaultRes9 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes9, 1);
                    defaultRes10 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes10, 1);
                    defaultRes11 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes11, 1);
                    defaultRes12 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes12, 1);
                    defaultRes13 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes13, 2);
                    defaultRes14 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes14, 2);
                    defaultRes15 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes15, 2);
                    defaultRes16 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes16, 2);
                    defaultRes17 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes17, 3);
                    defaultRes18 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes18, 3);
                    defaultRes19 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes19, 3);
                    defaultRes20 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes20, 3);
                    defaultRes21 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes21, 4);
                    defaultRes22 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes22, 4);
                    defaultRes23 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes23, 4);
                    defaultRes24 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes24, 4);
                    defaultRes25 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes25, 5);
                    defaultRes26 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes26, 5);
                    defaultRes27 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes27, 5);
                    defaultRes28 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes28, 5);
                    defaultRes29 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes29);
                    defaultRes30 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes30);
                    defaultRes31 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes31);
                    defaultRes32 = ecuJob.getResultsAs("A[].B[].C[].V", defaultRes32);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("VehicleIdent.GetVin17()", exception);
            }
            return null;
        }

        internal string DoExistingJob(IEcuKom ecukom)
        {
            try
            {
                string defaultRes = null;
                string defaultRes2 = null;
                byte[] defaultRes3 = null;
                long[] array = null;
                byte[,] array2 = new byte[3, 3];
                string text = "";
                text += 8405239;
                IEcuJob ecuJob = ecukom.ApiJob("BCP_SP21", "FS_LESEN_DETAIL", text, string.Empty);
                if (ecuJob != null && ecuJob.IsDone())
                {
                    defaultRes3 = ecuJob.getResultAs("F_HEX_CODE", defaultRes3, getLast: true);
                    for (int i = 0; i < array2.GetLength(0); i++)
                    {
                        byte[] resultAs = ecuJob.getResultAs<byte[]>((ushort)(i + 1), "F_UW_BN");
                        if (resultAs != null)
                        {
                            for (int j = 0; j < resultAs.Length; j++)
                            {
                                array2[i, j] = resultAs[j];
                            }
                        }
                    }
                    if (array == null || array.Length < ecuJob.JobResultSets)
                    {
                        array = (long[])__initArray<long>(new int[1] { ecuJob.JobResultSets }, long.MaxValue);
                    }
                    for (int k = 0; k < array.Length; k++)
                    {
                        array[k] = ecuJob.getResultAs((ushort)(k + 1), "F_UW_KM", array[k]);
                    }
                    defaultRes = ecuJob.getResultAs("JOB_STATUS", defaultRes, getLast: true);
                    defaultRes2 = ecuJob.getResultAs(0, "VARIANTE", defaultRes2);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("VehicleIdent.GetVin17()", exception);
            }
            return null;
        }

        internal string DoSgbdNextJob(IEcuKom ecuKom, List<EcuKomSamples> ecuKomSamples)
        {
            foreach (EcuKomSamples ecuKomSample in ecuKomSamples)
            {
                try
                {
                    IEcuJob ecuJob = ecuKom.ApiJob(ecuKomSample.Ecu, ecuKomSample.Job, ecuKomSample.Param, string.Empty);
                    if (ecuJob != null && ecuJob.IsDone())
                    {
                        switch (ecuKomSample.Type)
                        {
                            case "string[]":
                            {
                                string[] defaultRes3 = null;
                                defaultRes3 = ecuJob.getResultsAs(ecuKomSample.Result, defaultRes3);
                                break;
                            }
                            case "double[,]":
                            {
                                double[,] defaultRes2 = null;
                                defaultRes2 = ecuJob.getResultsAs(ecuKomSample.Result, defaultRes2);
                                break;
                            }
                            case "int[,,,,,]":
                            {
                                int[,,,,,] defaultRes = null;
                                defaultRes = ecuJob.getResultsAs(ecuKomSample.Result, defaultRes);
                                break;
                            }
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

        internal Array __initArray<T>(int[] sizes, object initValue)
        {
            Array array = null;
            try
            {
                Type type = typeof(T);
                if (type.BaseType == typeof(Array))
                {
                    type = type.GetElementType();
                }
                array = Array.CreateInstance(type, sizes);
                T val = default(T);
                if (initValue != null && typeof(T).IsAssignableFrom(initValue.GetType()))
                {
                    val = (T)initValue;
                }
                int num = sizes[0];
                for (int i = 1; i < sizes.Length; i++)
                {
                    num *= sizes[i];
                }
                int[] array2 = new int[sizes.Length];
                for (int j = 0; j < num; j++)
                {
                    array.SetValue(val, array2);
                    array2[sizes.Length - 1]++;
                    for (int num2 = sizes.Length - 1; num2 >= 0; num2--)
                    {
                        if (array2[num2] == sizes[num2])
                        {
                            array2[num2] = 0;
                            if (num2 > 0)
                            {
                                array2[num2 - 1]++;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return array;
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
            dictionary.Add("G_AIRBAG", new EcuKomConfig("G_AIRBAG", "STATUS_LESEN", "ARG;VIN", 1, "STAT_VIN_TEXT"));
            list.Add(dictionary["G_AIRBAG"]);
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
