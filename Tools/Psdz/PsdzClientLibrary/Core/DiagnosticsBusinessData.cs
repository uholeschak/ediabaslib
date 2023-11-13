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
	public class DiagnosticsBusinessData : IDiagnosticsBusinessData
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

        //private IFasta2Service fastaService = ServiceLocator.Current.GetService<IFasta2Service>();

        //private string ServiceCodeName = ServiceCodes.DBD01_ObseleteCode_nu_LF;

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

        public List<string> ProductLinesEpmBlacklist => new List<string> { "PL0", "PL2", "PL3", "PL3-alt", "PL4", "PL5", "PL5-alt", "PL6", "PL6-alt", "PL7" };

        public DateTime DTimeF01Lci => DateTime.ParseExact("01.07.2013", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTimeRR_S2 => DateTime.ParseExact("01.06.2012", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTimeF25Lci => DateTime.ParseExact("01.04.2014", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTimeF01BN2020MostDomain => DateTime.ParseExact("30.06.2010", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTime2022_07 => DateTime.ParseExact("01.07.2022", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTime2023_03 => DateTime.ParseExact("01.03.2023", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public DateTime DTime2023_07 => DateTime.ParseExact("01.07.2023", "dd.MM.yyyy", new CultureInfo("de-DE"));

        public void SetSp2021Enabled(IVehicle vecInfo)
        {
            if (string.IsNullOrEmpty(vecInfo.Produktlinie) && ClientContext.GetBrand((Vehicle) vecInfo) == CharacteristicExpression.EnumBrand.BMWMotorrad)
            {
                vecInfo.Produktlinie = "-";
            }
            vecInfo.Sp2021Enabled = vecInfo.Produktlinie.StartsWith("21");
        }

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

        // ToDo: Check on update
        public string GetMainSeriesSgbd(IVehicle vecInfo)
        {
            switch (vecInfo.BNType)
            {
                case BNType.BEV2010:
                    return "E89X";
                default:
                    if (vecInfo.Prodart == "P")
                    {
                        if (!string.IsNullOrEmpty(vecInfo.Produktlinie))
                        {
                            switch (vecInfo.Produktlinie.ToUpper())
                            {
                                case "PL2":
                                    return "E89X";
                                case "PL3":
                                    return "R56";
                                case "PL0":
                                    break;
                                case "PL6-ALT":
                                    return "E60";
                                case "PL4":
                                    return "E70";
                                case "PL3-ALT":
                                    return "ZCS_ALL";
                                default:
                                    return "F01";
                                case "PL5-ALT":
                                    return "RR1";
                            }
                            if (vecInfo.Ereihe == "E38" || vecInfo.Ereihe == "E46" || vecInfo.Ereihe == "E83" || vecInfo.Ereihe == "E85" || vecInfo.Ereihe == "E86" || vecInfo.Ereihe == "E36" || vecInfo.Ereihe == "E39" || vecInfo.Ereihe == "E52" || vecInfo.Ereihe == "E53")
                            {
                                return "ZCS_ALL";
                            }
                            if (vecInfo.Ereihe == "E65" || vecInfo.Ereihe == "E66" || vecInfo.Ereihe == "E67" || vecInfo.Ereihe == "E68")
                            {
                                return "E65";
                            }
                        }
                        AddServiceCode(string.Empty, 1);
                        return "-";
                    }
                    if (vecInfo.Prodart == "M")
                    {
                        switch (vecInfo.BNType)
                        {
                            default:
                                AddServiceCode(string.Empty, 2);
                                return "-";
                            case BNType.BN2000_MOTORBIKE:
                                return "MRK24";
                            case BNType.BN2020_MOTORBIKE:
                                return "X_K001";
                            case BNType.BNK01X_MOTORBIKE:
                                return "MRK24";
                        }
                    }
                    AddServiceCode(string.Empty, 3);
                    return "";
                case BNType.IBUS:
                    return "-";
            }
        }

        // ToDo: Check on update
        public string GetMainSeriesSgbdAdditional(IVehicle vecInfo)
        {
            Log.Info(Log.CurrentMethod(), "Entering GetMainSeriesSgbdAdditional");
            if (vecInfo.Prodart == "P")
            {
                if (!string.IsNullOrEmpty(vecInfo.Produktlinie))
                {
                    string text = vecInfo.Produktlinie.ToUpper();
                    if (!(text == "PL5-ALT"))
                    {
                        if (!(text == "PL6"))
                        {
                            AddServiceCode(string.Empty, 3);
                            Log.Info(Log.CurrentMethod(), "Reached default block, produck line: " + vecInfo.Produktlinie);
                        }
                        else if (!vecInfo.C_DATETIME.HasValue)
                        {
                            AddServiceCode(string.Empty, 2);
                            Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                            if (vecInfo.Ereihe == "F01" || vecInfo.Ereihe == "F02" || vecInfo.Ereihe == "F03" || vecInfo.Ereihe == "F04" || vecInfo.Ereihe == "F06" || vecInfo.Ereihe == "F07" || vecInfo.Ereihe == "F10" || vecInfo.Ereihe == "F11" || vecInfo.Ereihe == "F12" || vecInfo.Ereihe == "F13" || vecInfo.Ereihe == "F18")
                            {
                                Log.Info(Log.CurrentMethod(), "Ereihe: " + vecInfo.Ereihe + ", returning F01BN2K");
                                return "F01BN2K";
                            }
                        }
                        else if (vecInfo.C_DATETIME < DTimeF01Lci)
                        {
                            Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is earlier than DTimeF01Lci");
                            return "F01BN2K";
                        }
                    }
                    else
                    {
                        if (!vecInfo.C_DATETIME.HasValue)
                        {
                            AddServiceCode(string.Empty, 1);
                            Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                            return "RR1_2020";
                        }
                        if (vecInfo.C_DATETIME >= DTimeRR_S2)
                        {
                            Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is later than DTimeRR_S2");
                            return "RR1_2020";
                        }
                    }
                }
            }
            else
            {
                _ = vecInfo.Prodart == "M";
            }
            Log.Info(Log.CurrentMethod(), "Returning null for product line: " + vecInfo?.Produktlinie + ", ereihe: " + vecInfo.Ereihe);
            return null;
        }

        // ToDo: Check on update
        public void SpecialTreatmentBasedOnEreihe(string typsnr, IVehicle vecInfo)
        {
            if ((string.Compare(vecInfo.Ereihe, "M12", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(vecInfo.Ereihe, "M2_", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(vecInfo.Ereihe, "UNBEK", StringComparison.OrdinalIgnoreCase) == 0) && string.Compare(typsnr, "CZ31", StringComparison.OrdinalIgnoreCase) == 0)
            {
                AddServiceCode(string.Empty, 1);
                vecInfo.VerkaufsBezeichnung = "ACTIVEE";
                vecInfo.Motor = "IB1";
                vecInfo.Leistung = "23";
                vecInfo.Hubraum = "0";
                vecInfo.Karosserie = "SAV";
                vecInfo.Antrieb = "RWD";
                GearboxUtility.SetGearboxType(vecInfo, "MECH", "SpecialTreatmentBasedOnEreihe");
                vecInfo.Baureihe = "X'";
                vecInfo.Lenkung = "LL";
                vecInfo.Land = "CHN";
                vecInfo.Ereihe = "M12";
                vecInfo.BNType = BNType.BEV2010;
                vecInfo.BNMixed = BNMixed.HETEROGENEOUS;
                vecInfo.Ueberarbeitung = "0";
                vecInfo.MOTBezeichnung = "IB1P23M0";
                vecInfo.Baureihenverbund = "M012";
                vecInfo.Abgas = "KAT";
            }
            if ((string.Compare(vecInfo.Ereihe, "E82", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(vecInfo.Ereihe, "UNBEK", StringComparison.OrdinalIgnoreCase) == 0) && (string.Compare(typsnr, "UP31", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(typsnr, "UP33", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(typsnr, "UP3C", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(typsnr, "UP9C", StringComparison.OrdinalIgnoreCase) == 0))
            {
                AddServiceCode(string.Empty, 2);
                vecInfo.VerkaufsBezeichnung = "ActiveE";
                vecInfo.Motor = "IB1";
                vecInfo.Leistung = "23";
                vecInfo.Hubraum = "0";
                vecInfo.Karosserie = "COU";
                vecInfo.Antrieb = "RWD";
                GearboxUtility.SetGearboxType(vecInfo, "MECH", "SpecialTreatmentBasedOnEreihe");
                vecInfo.Baureihe = "1'";
                vecInfo.Lenkung = "LL";
                if (string.Compare(typsnr, "UP31", StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(typsnr, "UP31", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    vecInfo.Land = "USA";
                }
                else
                {
                    vecInfo.Land = "EUR";
                }
            }
            if (string.Compare(vecInfo.Ereihe, "R56", StringComparison.OrdinalIgnoreCase) == 0 && (string.Compare(typsnr, "MF74", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(typsnr, "MF84", StringComparison.OrdinalIgnoreCase) == 0))
            {
                AddServiceCode(string.Empty, 3);
                vecInfo.VerkaufsBezeichnung = "MINI E";
                vecInfo.Motor = "I15";
                vecInfo.Leistung = "140";
                vecInfo.Hubraum = "0";
            }
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
                                    if (item.ID_SG_ADR == 128L)
                                    {
                                        list.Add(128);
                                    }
                                    else if (item.ID_SG_ADR == 13L)
                                    {
                                        list.Add(13);
                                    }
                                }
                            }
                            else if (!(vecInfo.Ereihe == "E65") && !(vecInfo.Ereihe == "E66") && !(vecInfo.Ereihe == "E67") && !(vecInfo.Ereihe == "E68"))
                            {
                                list.Add(128);
                            }
                            else
                            {
                                list.Add(0);
                                list.Add(1);
                            }
                            break;
                        case "PL4":
                            list.Add(0);
                            list.Add(100);
                            break;
                        case "PL7":
                            if (!(vecInfo.Ereihe == "F25") && !(vecInfo.Ereihe == "F26"))
                            {
                                list.Add(16);
                                list.Add(64);
                            }
                            else
                            {
                                list.Add(16);
                            }
                            break;
                        case "PL3":
                        case "PL2":
                        case "PL6-ALT":
                            list.Add(0);
                            break;
                        case "PL6":
                            if (!(vecInfo.Ereihe == "F15") && !(vecInfo.Ereihe == "F16") && !(vecInfo.Ereihe == "F85") && !(vecInfo.Ereihe == "F86"))
                            {
                                list.Add(16);
                                break;
                            }
                            list.Add(16);
                            list.Add(64);
                            break;
                        case "21LI":
                        case "21LG":
                        case "21LU":
                            list.Add(16);
                            list.Add(50);
                            break;
                        case "35LR":
                        case "35LG":
                            if (vecInfo.Ereihe == "G09")
                            {
                                list.Add(16);
                                list.Add(50);
                            }
                            else if (vecInfo.Ereihe == "G07")
                            {
                                if (!vecInfo.C_DATETIME.HasValue)
                                {
                                    AddServiceCode(string.Empty, 1);
                                    Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                                    list.Add(16);
                                    list.Add(50);
                                    list.Add(64);
                                }
                                else if (vecInfo.C_DATETIME >= DTime2022_07)
                                {
                                    list.Add(16);
                                    list.Add(50);
                                }
                                else
                                {
                                    list.Add(16);
                                    list.Add(64);
                                }
                            }
                            else if (!(vecInfo.Ereihe == "F95") && !(vecInfo.Ereihe == "F96") && !(vecInfo.Ereihe == "G05") && !(vecInfo.Ereihe == "G06"))
                            {
                                if (vecInfo.Ereihe == "G18")
                                {
                                    if (!vecInfo.C_DATETIME.HasValue)
                                    {
                                        AddServiceCode(string.Empty, 3);
                                        Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                                        list.Add(16);
                                        list.Add(50);
                                        list.Add(64);
                                    }
                                    else if (vecInfo.C_DATETIME >= DTime2023_07)
                                    {
                                        list.Add(16);
                                        list.Add(50);
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
                                    list.Add(50);
                                }
                                else
                                {
                                    list.Add(16);
                                    list.Add(64);
                                }
                            }
                            else if (!vecInfo.C_DATETIME.HasValue)
                            {
                                AddServiceCode(string.Empty, 2);
                                Log.Info(Log.CurrentMethod(), "Product line: " + vecInfo.Produktlinie + ", C_DATETIME is null");
                                list.Add(16);
                                list.Add(50);
                                list.Add(64);
                            }
                            else if (vecInfo.C_DATETIME >= DTime2023_03)
                            {
                                list.Add(16);
                                list.Add(50);
                            }
                            else
                            {
                                list.Add(16);
                                list.Add(64);
                            }
                            break;
                        case "PL3-ALT":
                            list.Add(128);
                            list.Add(0);
                            break;
                        case "35LK":
                        case "35LU":
                        case "PLLI":
                        case "PLLU":
                            list.Add(16);
                            list.Add(64);
                            break;
                        default:
                            list.Add(16);
                            break;
                        case "PL5-ALT":
                            list.Add(0);
                            list.Add(1);
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
                        default:
                            list.Add(16);
                            break;
                        case "K01X":
                        case "K024":
                        case "KH24":
                        case "K001":
                        case "KS01":
                        case "KE01":
                            list.Add(18);
                            break;
                    }
                }
                return list;
            }
            Log.Info(Log.CurrentMethod(), "Returning null for product line: " + vecInfo?.Produktlinie + ", ereihe: " + vecInfo.Ereihe);
            return null;
        }

        // ToDo: Check on update
        public BNType GetBNType(IVehicle vecInfo)
        {
            if (vecInfo == null)
            {
                return BNType.UNKNOWN;
            }
            if (vecInfo.Prodart == "P")
            {
                if (!string.IsNullOrEmpty(vecInfo.Baureihenverbund))
                {
                    switch (vecInfo.Baureihenverbund.ToUpper())
                    {
                        case "M012":
                            return BNType.BEV2010;
                        case "R050":
                        case "E083":
                        case "E085":
                            return BNType.IBUS;
                        case "R056":
                        case "E065":
                        case "E060":
                        case "E070":
                        case "E89X":
                        case "RR01":
                            return BNType.BN2000;
                        default:
                            return BNType.BN2020;
                        case "U006":
                        case "M013":
                        case "F010":
                        case "F025":
                        case "F020":
                        case "J001":
                        case "S18A":
                        case "I001":
                        case "F001":
                        case "G070":
                        case "S15A":
                        case "S18T":
                        case "S15C":
                        case "F056":
                        case "I020":
                        case "RR21":
                            return BNType.BN2020;
                    }
                }
                Log.Warning(Log.CurrentMethod(), "Baureihenverbund is null or empty. BNType will be determined by Ereihe!");
                switch (vecInfo.Ereihe)
                {
                    default:
                        Log.Warning(Log.CurrentMethod(), "Ereihe is null or empty. No BNType can be determined!");
                        return BNType.UNKNOWN;
                    case "E38":
                    case "E39":
                    case "E32":
                    case "E30":
                    case "E31":
                    case "E46":
                    case "E34":
                    case "E52":
                    case "E53":
                    case "E36":
                        return BNType.IBUS;
                }
            }
            if (vecInfo.Prodart == "M")
            {
                if (!string.IsNullOrEmpty(vecInfo.Baureihenverbund))
                {
                    switch (vecInfo.Baureihenverbund.ToUpper())
                    {
                        case "XS01":
                        case "K001":
                        case "KE01":
                        case "X001":
                        case "KS01":
                            return BNType.BN2020_MOTORBIKE;
                        case "K024":
                        case "KH24":
                            return BNType.BN2000_MOTORBIKE;
                        default:
                            return BNType.BN2020_MOTORBIKE;
                        case "K01X":
                            return BNType.BNK01X_MOTORBIKE;
                    }
                }
                Log.Info(Log.CurrentMethod(), "Baureihenverbund was empty, returning default value.");
                return BNType.BN2020_MOTORBIKE;
            }
            Log.Info(Log.CurrentMethod(), "Returning BNType.UNKNOWN for Prodart: " + vecInfo?.Prodart);
            return BNType.UNKNOWN;
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

        public void AddServiceCode(string methodName, int identifier)
        {
            //fastaService.AddServiceCode(ServiceCodeName, string.Format(ServiceCodeValuePattern, methodName, identifier), layoutGroup);
        }

        public void ReadILevelBn2020(IVehicle vecInfo, IEcuKom ecuKom, int retryCount)
        {
            // [UH] get reactor from vehicle
            Reactor reactor = (vecInfo as Vehicle)?.Reactor;
            if (reactor == null)
            {
                return;
            }

            IEcuJob ecuJob = new ECUJob();
            if (IsSp2021Gateway(vecInfo, ecuKom, retryCount))
            {
                ecuJob = ecuKom.ApiJobWithRetries("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR", string.Empty, string.Empty, retryCount);
                if (!ecuJob.IsOkay())
                {
                    HandleReadILevelForSp2021Fallback(reactor, vecInfo, ecuKom, retryCount);
                }
                else if (!ProcessILevelJobResults(reactor, vecInfo, ecuJob))
                {
                    HandleReadILevelForSp2021Fallback(reactor, vecInfo, ecuKom, retryCount);
                }
            }
            else
            {
                ecuJob = ecuKom.ApiJobWithRetries("g_zgw", "STATUS_VCM_I_STUFE_LESEN", string.Empty, string.Empty, retryCount);
                if (!ecuJob.IsOkay())
                {
                    HandleReadIlevelBackup(reactor, vecInfo, ecuKom, retryCount);
                }
                else if (!ProcessILevelJobResults(reactor, vecInfo, ecuJob))
                {
                    HandleReadIlevelBackup(reactor, vecInfo, ecuKom, retryCount);
                }
            }
            if (!string.IsNullOrEmpty(vecInfo.ILevel))
            {
                if (vecInfo.ILevel.Length < 4 || vecInfo.ILevel.Contains("UNBEK"))
                {
                    vecInfo.ILevel = vecInfo.ILevelWerk;
                }
            }
            else
            {
                vecInfo.ILevel = vecInfo.ILevelWerk;
            }
        }

        public bool ProcessILevelJobResults(Reactor reactor, IVehicle vecInfo, IEcuJob iJob)
        {
            string stringResult = iJob.getStringResult(1, "STAT_I_STUFE_WERK");
            string stringResult2 = iJob.getStringResult(1, "STAT_I_STUFE_HO");
            string stringResult3 = iJob.getStringResult(1, "STAT_I_STUFE_HO_BACKUP");
            if (!IsExcludedFromILevelValidation(iJob) && (!ValidateILevelWithRegexPattern(stringResult, "ILevelWerk") || !ValidateILevelWithRegexPattern(stringResult2, "ILevelHO") || !ValidateILevelWithRegexPattern(stringResult3, "ILevelHOBackup")))
            {
                return false;
            }
            reactor.SetILevelWerk(stringResult, DataSource.Vehicle);
            reactor.SetILevel(stringResult2, DataSource.Vehicle);
            vecInfo.ILevelBackup = stringResult3;
            return true;
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
            IEcuJob ecuJob = ecuKom.ApiJobWithRetries("G_KOMBI", "STATUS_I_STUFE_LESEN_OHNE_SIGNATUR", string.Empty, string.Empty, retryCount);
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
            if (!Regex.IsMatch(ilevelInput, ILevelBN2020RegexPattern))
            {
                Log.Warning(Log.CurrentMethod(), "Validation of ILevel " + ilevelInput + " for type " + iLevelDescription + " failed");
                return false;
            }
            return true;
        }
    }
}
