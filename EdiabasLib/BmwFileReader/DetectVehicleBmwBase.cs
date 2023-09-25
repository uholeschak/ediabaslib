using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EdiabasLib;

namespace BmwFileReader
{
    public class DetectVehicleBmwBase
    {
        protected class JobInfo
        {
            public JobInfo(string sgdbName, string jobName, string jobArgs = null, string jobResult = null, string bnType = null, string ecuListJob = null)
            {
                SgdbName = sgdbName;
                JobName = jobName;
                JobArgs = jobArgs;
                JobResult = jobResult;
                BnType = bnType;
                EcuListJob = ecuListJob;
                Motorbike = !string.IsNullOrEmpty(bnType) && bnType.ToUpperInvariant().Contains("MOTORBIKE");
            }

            public string SgdbName { get; }
            public string JobName { get; }
            public string JobArgs { get; }
            public string JobResult { get; }
            public string BnType { get; }
            public string EcuListJob { get; }
            public bool Motorbike { get; }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string grp)
            {
                Name = name;
                Address = address;
                Grp = grp;
                Sgbd = null;
                Description = null;
            }

            public string Name { get; set; }
            public Int64 Address { get; set; }
            public string Grp { get; set; }
            public string Sgbd { get; set; }
            public string Description { get; set; }
        }

        public string Vin { get; protected set; }
        public string TypeKey { get; protected set; }
        public string GroupSgdb { get; protected set; }
        public List<string> SgdbAddList { get; protected set; }
        public string ModelSeries { get; protected set; }
        public string Series { get; protected set; }
        public string ProductType { get; protected set; }
        public string BnType { get; protected set; }
        public List<string> BrandList { get; protected set; }
        public string TransmissionType { get; protected set; }
        public string Motor { get; protected set; }
        public List<EcuInfo> EcuList { get; protected set; }
        public DateTime? ConstructDate { get; protected set; }
        public string ConstructYear { get; protected set; }
        public string ConstructMonth { get; protected set; }
        public string Paint { get; protected set; }
        public string Upholstery { get; protected set; }
        public string StandardFa { get; protected set; }
        public List<string> Salapa { get; protected set; }
        public List<string> HoWords { get; protected set; }
        public List<string> EWords { get; protected set; }
        public List<string> ZbWords { get; protected set; }
        public string ILevelShip { get; protected set; }
        public string ILevelCurrent { get; protected set; }
        public string ILevelBackup { get; protected set; }

        protected EdiabasNet _ediabas;

        public static Regex VinRegex = new Regex(@"^(?!0{7,})([a-zA-Z0-9]{7,})$");

        // from ReadVinForGroupCars, ReadVinForMotorcycles
        protected static readonly List<JobInfo> ReadVinJobsBmwFast = new List<JobInfo>
        {
            // BN2020
            new JobInfo("G_ZGW", "STATUS_VIN_LESEN", string.Empty, "STAT_VIN", "BN2020", "STATUS_VCM_GET_ECU_LIST_ALL"),
            new JobInfo("G_CAS", "STATUS_FAHRGESTELLNUMMER", string.Empty, "STAT_FGNR17_WERT", "BN2020"),
            new JobInfo("G_FRM", "STATUS_VCM_VIN", string.Empty, "STAT_VIN_EINH", "BN2020"),
            // BN2000
            new JobInfo("D_CAS", "STATUS_FAHRGESTELLNUMMER", string.Empty, "FGNUMMER", "BN2000"),
            new JobInfo("D_LM", "READ_FVIN", string.Empty, "FVIN", "BN2000"),
            new JobInfo("FRM_87", "READ_FVIN", string.Empty, "FVIN", "BN2000"),
            new JobInfo("D_ZGM", "C_FG_LESEN", string.Empty, "FG_NR", "BN2000"),
            // motorbikes BN2000
            new JobInfo("D_MRMOT", "STATUS_FAHRGESTELLNUMMER", string.Empty, "STAT_FGNUMMER", "BN2000_MOTORBIKE"),
            new JobInfo("D_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT", "BN2000_MOTORBIKE"),
            // motorbikes BN2020
            new JobInfo("G_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT", "BN2020_MOTORBIKE"),
            new JobInfo("X_K001", "PROG_FG_NR_LESEN_FUNKTIONAL", "18", "FG_NR_LANG", "BN2020_MOTORBIKE"),
            new JobInfo("X_KS01", "PROG_FG_NR_LESEN_FUNKTIONAL", "18", "FG_NR_LANG", "BN2020_MOTORBIKE"),
        };

        protected static readonly List<JobInfo> ReadFaJobsBmwFast = new List<JobInfo>
        {
            // BN2020
            new JobInfo("G_ZGW", "STATUS_VCM_GET_FA", string.Empty, "STAT_BAUREIHE", "BN2020"),
            new JobInfo("ZGW_01", "STATUS_VCM_GET_FA", string.Empty, "STAT_BAUREIHE", "BN2020"),
            new JobInfo("G_CAS", "STATUS_FAHRZEUGAUFTRAG", string.Empty, "STAT_FAHRZEUGAUFTRAG_KOMPLETT_WERT", "BN2020"),
            // BN2000
            new JobInfo("D_CAS", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2000"),
            new JobInfo("D_LM", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2000"),
            new JobInfo("D_KBM", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2000"),
            // motorbikes BN2000
            new JobInfo("D_MRKOMB", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2000_MOTORBIKE"),
            new JobInfo("D_MRZFE", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2000_MOTORBIKE"),
            new JobInfo("D_MRMOT", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2000_MOTORBIKE"),
            // motorbikes BN2020
            new JobInfo("X_K001", "FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2020_MOTORBIKE"),
            new JobInfo("X_KS01", "FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", "BN2020_MOTORBIKE"),
        };

        protected static readonly List<JobInfo> ReadILevelJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR"),
            new JobInfo("G_ZGW", "STATUS_VCM_I_STUFE_LESEN"),
            new JobInfo("G_FRM", "STATUS_VCM_I_STUFE_LESEN"),
        };

        // from ReadVinForGroupCars
        protected static readonly List<JobInfo> ReadVinJobsDs2 = new List<JobInfo>
        {
            new JobInfo("ZCS_ALL", "FGNR_LESEN", null, "FG_NR", "IBUS"),
            new JobInfo("D_0080", "AIF_FG_NR_LESEN", null, "AIF_FG_NR", "IBUS"),
            new JobInfo("D_0010", "AIF_LESEN", null, "AIF_FG_NR", "IBUS"),
            new JobInfo("EWS3", "FGNR_LESEN", null, "FG_NR", "IBUS"),
        };

        protected static readonly List<JobInfo> ReadIdentJobsDs2 = new List<JobInfo>
        {
            new JobInfo("FZGIDENT", "GRUNDMERKMALE_LESEN", null, "BR_TXT", "IBUS"),
            new JobInfo("FZGIDENT", "STRINGS_LESEN", null, "BR_TXT", "IBUS"),
        };

        protected static readonly List<JobInfo> ReadFaJobsDs2 = new List<JobInfo>
        {
            new JobInfo("D_0080", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG", "IBUS"),
            new JobInfo("D_00D0", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG", "IBUS"),
        };

        protected static readonly string[] ReadMotorJobsDs2 =
        {
            "D_0012", "D_MOTOR", "D_0010", "D_0013", "D_0014"
        };

        public const string AllDs2GroupFiles = "d_0000,d_0008,d_000d,d_0010,d_0011,d_0012,d_motor,d_0013,d_0014,d_0015,d_0016,d_0020,d_0021,d_0022,d_0024,d_0028,d_002c,d_002e,d_0030,d_0032,d_0035,d_0036,d_003b,d_0040,d_0044,d_0045,d_0050,d_0056,d_0057,d_0059,d_005a,d_005b,d_0060,d_0068,d_0069,d_006a,d_006c,d_0070,d_0071,d_0072,d_007f,d_0080,d_0086,d_0099,d_009a,d_009b,d_009c,d_009d,d_009e,d_00a0,d_00a4,d_00a6,d_00a7,d_00ac,d_00b0,d_00b9,d_00bb,d_00c0,d_00c8,d_00cd,d_00d0,d_00da,d_00e0,d_00e8,d_00ed,d_00f0,d_00f5,d_00ff,d_b8_d0,,d_m60_10,d_m60_12,d_spmbt,d_spmft,d_szm,d_zke3bt,d_zke3ft,d_zke3pm,d_zke3sb,d_zke3sd,d_zke_gm,d_zuheiz,d_sitz_f,d_sitz_b,d_0047,d_0048,d_00ce,d_00ea,d_abskwp,d_0031,d_0019,d_smac,d_0081,d_xen_l,d_xen_r";

        public DetectVehicleBmwBase(EdiabasNet ediabas = null)
        {
            _ediabas = ediabas;
        }

        public string GetFaInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in Salapa)
            {
                sb.Append("$");
                sb.Append(item);
            }

            foreach (string item in EWords)
            {
                sb.Append("-");
                sb.Append(item);
            }

            foreach (string item in HoWords)
            {
                sb.Append("+");
                sb.Append(item);
            }

            foreach (string item in ZbWords)
            {
                sb.Append(";");
                sb.Append(item);
            }

            return sb.ToString();
        }

        public static bool IsDs2GroupSgbd(string name)
        {
            string nameTrim = name.Trim();
            string[] groupArray = AllDs2GroupFiles.Split(',');
            foreach (string group in groupArray)
            {
                if (string.Compare(group, nameTrim, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetEcuName(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            string ecuName = string.Empty;
            if (resultSets != null && resultSets.Count >= 2)
            {
                int dictIndex = 0;
                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    if (dictIndex == 0)
                    {
                        dictIndex++;
                        continue;
                    }

                    if (resultDict.TryGetValue("ECU", out EdiabasNet.ResultData resultData))
                    {
                        if (resultData.OpData is string)
                        {
                            ecuName = (string)resultData.OpData;
                        }
                    }

                    dictIndex++;
                }
            }

            return ecuName;
        }

        public static string GetVin7(string vin17)
        {
            if (string.IsNullOrEmpty(vin17) || vin17.Length < 17)
            {
                return null;
            }
            return vin17.Substring(10, 7);
        }

        public static string GetVinType(string vin17)
        {
            if (string.IsNullOrEmpty(vin17) || vin17.Length < 17)
            {
                return null;
            }
            return vin17.Substring(3, 4);
        }

        public static string GetEcuComment(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (resultSets != null && resultSets.Count >= 2)
            {
                int dictIndex = 0;
                foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                {
                    if (dictIndex == 0)
                    {
                        dictIndex++;
                        continue;
                    }
                    for (int i = 0; ; i++)
                    {
                        if (resultDict.TryGetValue("ECUCOMMENT" + i.ToString(CultureInfo.InvariantCulture), out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                if (stringBuilder.Length > 0)
                                {
                                    stringBuilder.Append(";");
                                }
                                stringBuilder.Append((string)resultData.OpData);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    dictIndex++;
                }
            }
            return stringBuilder.ToString();
        }

        protected void SetConstructDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return;
            }

            ConstructDate = dateTime;
            ConstructYear = dateTime.Value.ToString("yyyy", CultureInfo.InvariantCulture);
            ConstructMonth = dateTime.Value.ToString("MM", CultureInfo.InvariantCulture);
        }

        protected void SetStatVcmSalpaInfo(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            int dictIndex = 0;
            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
            {
                if (dictIndex == 0)
                {
                    dictIndex++;
                    continue;
                }

                if (resultDict.TryGetValue("STAT_SALAPA", out EdiabasNet.ResultData resultDataSa))
                {
                    string saStr = resultDataSa.OpData as string;
                    AddSalapa(saStr);
                }

                if (resultDict.TryGetValue("STAT_HO_WORTE", out EdiabasNet.ResultData resultDataHo))
                {
                    string hoStr = resultDataHo.OpData as string;
                    if (!string.IsNullOrEmpty(hoStr))
                    {
                        if (!HoWords.Contains(hoStr))
                        {
                            HoWords.Add(hoStr);
                        }
                    }
                }

                if (resultDict.TryGetValue("STAT_E_WORTE", out EdiabasNet.ResultData resultDataEw))
                {
                    string ewStr = resultDataEw.OpData as string;
                    if (!string.IsNullOrEmpty(ewStr))
                    {
                        if (!EWords.Contains(ewStr))
                        {
                            EWords.Add(ewStr);
                        }
                    }
                }

                dictIndex++;
            }

            LogInfoFormat("Detected FA: {0}", GetFaInfo());
        }

        protected void SetFaSalpaInfo(Dictionary<string, EdiabasNet.ResultData> resultDict)
        {
            if (resultDict.TryGetValue("SA_ANZ", out EdiabasNet.ResultData resultDataSaCount))
            {
                Int64? saCount = resultDataSaCount.OpData as Int64?;
                if (saCount != null)
                {
                    for (int index = 0; index < saCount.Value; index++)
                    {
                        string saName = string.Format(CultureInfo.InvariantCulture, "SA_{0}", index + 1);
                        if (resultDict.TryGetValue(saName, out EdiabasNet.ResultData resultDataSa))
                        {
                            string saStr = resultDataSa.OpData as string;
                            AddSalapa(saStr);
                        }
                    }
                }
            }

            if (resultDict.TryGetValue("HO_WORT_ANZ", out EdiabasNet.ResultData resultDataHoCount))
            {
                Int64? haCount = resultDataHoCount.OpData as Int64?;
                if (haCount != null)
                {
                    for (int index = 0; index < haCount.Value; index++)
                    {
                        string hoName = string.Format(CultureInfo.InvariantCulture, "HO_WORT_{0}", index + 1);
                        if (resultDict.TryGetValue(hoName, out EdiabasNet.ResultData resultDataHo))
                        {
                            string hoStr = resultDataHo.OpData as string;
                            if (!string.IsNullOrEmpty(hoStr))
                            {
                                if (!HoWords.Contains(hoStr))
                                {
                                    HoWords.Add(hoStr);
                                }
                            }
                        }
                    }
                }
            }

            if (resultDict.TryGetValue("E_WORT_ANZ", out EdiabasNet.ResultData resultDataEwCount))
            {
                Int64? ewCount = resultDataEwCount.OpData as Int64?;
                if (ewCount != null)
                {
                    for (int index = 0; index < ewCount.Value; index++)
                    {
                        string ewName = string.Format(CultureInfo.InvariantCulture, "E_WORT_{0}", index + 1);
                        if (resultDict.TryGetValue(ewName, out EdiabasNet.ResultData resultDataEw))
                        {
                            string ewStr = resultDataEw.OpData as string;
                            if (!string.IsNullOrEmpty(ewStr))
                            {
                                if (!EWords.Contains(ewStr))
                                {
                                    EWords.Add(ewStr);
                                }
                            }
                        }
                    }
                }
            }

            if (resultDict.TryGetValue("ZUSBAU_ANZ", out EdiabasNet.ResultData resultDataZbCount))
            {
                Int64? zbCount = resultDataZbCount.OpData as Int64?;
                if (zbCount != null)
                {
                    for (int index = 0; index < zbCount.Value; index++)
                    {
                        string zbName = string.Format(CultureInfo.InvariantCulture, "ZUSBAU_{0}", index + 1);
                        if (resultDict.TryGetValue(zbName, out EdiabasNet.ResultData resultDataEw))
                        {
                            string zbStr = resultDataEw.OpData as string;
                            if (!string.IsNullOrEmpty(zbStr))
                            {
                                if (!ZbWords.Contains(zbStr))
                                {
                                    ZbWords.Add(zbStr);
                                }
                            }
                        }
                    }
                }
            }

            LogInfoFormat("Detected FA: {0}", GetFaInfo());
        }

        protected bool SetStreamToStructInfo(Dictionary<string, EdiabasNet.ResultData> resultDict)
        {
            bool dataValid = false;
            if (resultDict.TryGetValue("STANDARD_FA", out EdiabasNet.ResultData resultStdFa))
            {
                string stdFaStr = resultStdFa.OpData as string;
                if (!string.IsNullOrEmpty(stdFaStr))
                {
                    StandardFa = stdFaStr;
                    if (SetInfoFromStdFa(stdFaStr))
                    {
                        dataValid = true;
                    }
                }
            }

            if (resultDict.TryGetValue("BR", out EdiabasNet.ResultData resultDataBa))
            {
                string br = resultDataBa.OpData as string;
                if (!string.IsNullOrEmpty(br))
                {
                    LogInfoFormat("Detected BR: {0}", br);
                    string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                    if (!string.IsNullOrEmpty(vSeries))
                    {
                        LogInfoFormat("Detected vehicle series: {0}", vSeries);
                        ModelSeries = br;
                        Series = vSeries;
                        dataValid = true;
                    }
                }

                if (resultDict.TryGetValue("C_DATE", out EdiabasNet.ResultData resultDataCDate))
                {
                    string cDateStr = resultDataCDate.OpData as string;
                    DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                    if (dateTime != null)
                    {
                        LogInfoFormat("Detected construction date: {0}",
                            dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                        SetConstructDate(dateTime);
                    }
                }

                if (resultDict.TryGetValue("LACK", out EdiabasNet.ResultData resultPaint))
                {
                    string paintStr = resultPaint.OpData as string;
                    if (!string.IsNullOrEmpty(paintStr))
                    {
                        Paint = paintStr;
                    }
                }

                if (resultDict.TryGetValue("POLSTER", out EdiabasNet.ResultData resultUpholstery))
                {
                    string upholsteryStr = resultUpholstery.OpData as string;
                    if (!string.IsNullOrEmpty(upholsteryStr))
                    {
                        Upholstery = upholsteryStr;
                    }
                }
            }

            if (dataValid)
            {
                SetFaSalpaInfo(resultDict);
            }

            return dataValid;
        }

        protected bool SetInfoFromStdFa(string stdFaStr)
        {
            if (string.IsNullOrEmpty(stdFaStr))
            {
                return false;
            }

            bool dataValid = false;
            try
            {
                string vehicleType = VehicleInfoBmw.GetVehicleTypeFromStdFa(stdFaStr, _ediabas);
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    TypeKey = vehicleType;
                }

                Match matchBr = Regex.Match(stdFaStr, "^(?<BR>\\w+)[^\\w]");
                if (!matchBr.Success)
                {
                    matchBr = Regex.Match(stdFaStr, "(?<BR>\\w{4})[^\\w]");
                }

                if (matchBr.Success)
                {
                    string br = matchBr.Groups["BR"]?.Value;
                    if (!string.IsNullOrEmpty(br))
                    {
                        string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                        if (!string.IsNullOrEmpty(vSeries))
                        {
                            LogInfoFormat("Detected vehicle series: {0}", vSeries);
                            ModelSeries = br;
                            Series = vSeries;
                            dataValid = true;
                        }
                    }
                }

                Match matchDate = Regex.Match(stdFaStr, "#(?<C_DATE>\\d{4})");
                if (matchDate.Success)
                {
                    string dateStr = matchDate.Groups["C_DATE"]?.Value;
                    if (!string.IsNullOrEmpty(dateStr))
                    {
                        DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(dateStr);
                        SetConstructDate(dateTime);
                    }
                }

                Match matchPaint = Regex.Match(stdFaStr, "%(?<LACK>\\w{4})");
                if (matchPaint.Success)
                {
                    string paintStr = matchPaint.Groups["LACK"]?.Value;
                    if (!string.IsNullOrEmpty(paintStr))
                    {
                        Paint = paintStr;
                    }
                }

                Match matchUpholstery = Regex.Match(stdFaStr, "&(?<POLSTER>\\w{4})");
                if (matchUpholstery.Success)
                {
                    string upholsteryStr = matchUpholstery.Groups["POLSTER"]?.Value;
                    if (!string.IsNullOrEmpty(upholsteryStr))
                    {
                        Upholstery = upholsteryStr;
                    }
                }

                foreach (Match match in Regex.Matches(stdFaStr, "\\$(?<SA>\\w{3})"))
                {
                    if (match.Success)
                    {
                        string saStr = match.Groups["SA"]?.Value;
                        AddSalapa(saStr);
                    }
                }

                foreach (Match match in Regex.Matches(stdFaStr, "\\+(?<HOWORT>\\w{4})"))
                {
                    if (match.Success)
                    {
                        string hoStr = match.Groups["HOWORT"]?.Value;
                        if (!string.IsNullOrEmpty(hoStr))
                        {
                            if (!HoWords.Contains(hoStr))
                            {
                                HoWords.Add(hoStr);
                            }
                        }
                    }
                }

                foreach (Match match in Regex.Matches(stdFaStr, "\\-(?<EWORT>\\w{4})"))
                {
                    if (match.Success)
                    {
                        string ewStr = match.Groups["EWORT"]?.Value;
                        if (!string.IsNullOrEmpty(ewStr))
                        {
                            if (!EWords.Contains(ewStr))
                            {
                                EWords.Add(ewStr);
                            }
                        }
                    }
                }

                if (dataValid)
                {
                    LogInfoFormat("Detected FA: {0}", GetFaInfo());
                }

                return dataValid;
            }
            catch (Exception ex)
            {
                LogErrorFormat("SetInfoFromStdFa Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        // from: Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.HandleECUGroups
        protected void HandleSpecialEcus(List<EcuInfo> ecusToRemove = null)
        {
            List<EcuInfo> ecuRemoveList = new List<EcuInfo>();
            if (ecusToRemove != null)
            {
                ecuRemoveList.AddRange(ecusToRemove);
            }

            if (string.Compare(GroupSgdb, "E89X", StringComparison.OrdinalIgnoreCase) == 0)
            {
                const string groupRls = "D_RLS";
                EcuInfo ecuInfoRls = GetEcuByEcuGroup(groupRls);
                if (ecuInfoRls == null)
                {
                    const int addressRls = 86;
                    if (EcuList.All(ecuInfo => ecuInfo.Address != addressRls))
                    {
                        bool addEcu = HasSa("521") || !string.IsNullOrEmpty(GetEcuNameByIdent(groupRls));
                        if (addEcu)
                        {
                            EcuInfo ecuInfoAdd = new EcuInfo("RLS", addressRls, groupRls);
                            LogInfoFormat("HandleSpecialEcus Adding ECU: Name={0}, Address={1}, Group={2}", ecuInfoAdd.Name, ecuInfoAdd.Address, ecuInfoAdd.Grp);
                            EcuList.Add(ecuInfoAdd);
                        }
                    }
                }
            }

            const string groupIspd = "D_ISPB";
            EcuInfo ecuInfoIspd = GetEcuByEcuGroup(groupIspd);
            if (ecuInfoIspd != null)
            {
                if (string.IsNullOrEmpty(GetEcuNameByIdent(groupIspd)))
                {
                    bool removeEcu = false;
                    if (HasSa("6VC") || GetEcuByEcuGroup("CMEDIAR") != null)
                    {
                        removeEcu = true;
                    }
                    else
                    {
                        const string groupMmi = "D_MMI";
                        EcuInfo ecuInfoMmi = GetEcuByEcuGroup(groupMmi);
                        if (ecuInfoMmi != null)
                        {
                            string ecuMmiName = GetEcuNameByIdent(groupMmi);
                            if (!string.IsNullOrEmpty(ecuMmiName) && string.Compare(ecuMmiName, "RAD2", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                removeEcu = true;
                            }
                        }
                    }

                    if (removeEcu)
                    {
                        // found RAD2 with built-in USB/audio (SA 6FL/6ND/6NE)
                        if (!ecuRemoveList.Contains(ecuInfoIspd))
                        {
                            ecuRemoveList.Add(ecuInfoIspd);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(BnType))
            {
                // from HandleECUGroups
                if (string.Compare(BnType, "BEV2010", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    const string groupFdi = "D_FBI";
                    EcuInfo ecuInfoFdi = GetEcuByEcuGroup(groupFdi);
                    if (ecuInfoFdi != null)
                    {
                        if (HasSa("8AA") && string.IsNullOrEmpty(GetEcuNameByIdent(groupFdi)))
                        {
                            if (!ecuRemoveList.Contains(ecuInfoFdi))
                            {
                                ecuRemoveList.Add(ecuInfoFdi);
                            }
                        }
                    }
                }

                // from KMMFix
                if (string.Compare(BnType, "BN2000", StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare(BnType, "BEV2010", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!string.IsNullOrEmpty(TypeKey) &&
                        (string.Compare(TypeKey, "VZ91", StringComparison.OrdinalIgnoreCase) == 0 ||
                         string.Compare(TypeKey, "VN91", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        EcuInfo ecuInfoEgs = GetEcuByAddr(24);
                        if (ecuInfoEgs != null)
                        {
                            if (!HasGearBoxEcu() && string.IsNullOrEmpty(GetEcuNameByIdent(ecuInfoEgs.Grp)))
                            {   // EGS in MECH gear E84 found
                                if (!ecuRemoveList.Contains(ecuInfoEgs))
                                {
                                    ecuRemoveList.Add(ecuInfoEgs);
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(Series) && string.Compare(Series, "R59", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        EcuInfo ecuInfoCvm = GetEcuByAddr(36);
                        if (ecuInfoCvm != null)
                        {
                            if (string.IsNullOrEmpty(GetEcuNameByIdent(ecuInfoCvm.Grp)))
                            {   // CVM in R59 found
                                if (!ecuRemoveList.Contains(ecuInfoCvm))
                                {
                                    ecuRemoveList.Add(ecuInfoCvm);
                                }
                            }
                        }
                    }
                }
            }

            foreach (EcuInfo ecuInfoRemove in ecuRemoveList)
            {
                LogInfoFormat("HandleSpecialEcus Removing ECU: Name={0}, Address={1}, Group={2}", ecuInfoRemove.Name, ecuInfoRemove.Address, ecuInfoRemove.Grp);
                ecuRemoveList.Remove(ecuInfoRemove);
            }
        }

        protected EcuInfo GetEcuByEcuGroup(string groups)
        {
            if (string.IsNullOrEmpty(groups))
            {
                return null;
            }

            string[] groupArray = groups.Split('|');
            foreach (string group in groupArray)
            {
                foreach (EcuInfo ecuInfo in EcuList)
                {
                    if (!string.IsNullOrEmpty(ecuInfo.Grp))
                    {
                        string[] ecuGroups = ecuInfo.Grp.Split('|');
                        foreach (string ecuGroup in ecuGroups)
                        {
                            if (string.Compare(ecuGroup, group, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                return ecuInfo; 
                            }
                        }
                    }
                }
            }

            return null;
        }

        protected EcuInfo GetEcuByAddr(long sgAddr)
        {
            foreach (EcuInfo ecuInfo in EcuList)
            {
                if (ecuInfo.Address == sgAddr)
                {
                    return ecuInfo; 
                }
            }

            return null;
        }

        protected bool AddSalapa(string salapa)
        {
            if (string.IsNullOrEmpty(salapa))
            {
                return false;
            }

            string saStr = salapa;
            if (saStr.Length == 4)
            {
                saStr = saStr.Substring(1);
            }

            if (!Salapa.Contains(saStr))
            {
                Salapa.Add(saStr);
                return true;
            }

            return false;
        }

        protected bool HasSa(string checkSA)
        {
            if (string.IsNullOrEmpty(checkSA))
            {
                return false;
            }

            if (Salapa != null)
            {
                foreach (string item in Salapa)
                {
                    if (string.Compare(item, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            if (EWords != null)
            {
                foreach (string item in EWords)
                {
                    if (string.Compare(item, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            if (HoWords != null)
            {
                foreach (string item in HoWords)
                {
                    if (string.Compare(item, checkSA, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected bool HasGearBoxEcu()
        {
            if (!string.IsNullOrEmpty(Motor))
            {
                if (string.Compare(Motor, "W10", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(TransmissionType))
            {
                if (string.Compare(TransmissionType, "AUT", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return HasSa("205") || HasSa("206") || HasSa("2TB") || HasSa("2TC") || HasSa("2MK");
        }

        protected virtual void ResetValues()
        {
            Vin = null;
            TypeKey = null;
            GroupSgdb = null;
            SgdbAddList = null;
            ModelSeries = null;
            Series = null;
            ProductType = null;
            BnType = null;
            BrandList = null;
            TransmissionType = null;
            Motor = null;
            EcuList = new List<EcuInfo>();
            ConstructDate = null;
            ConstructYear = null;
            ConstructMonth = null;
            Paint = null;
            Upholstery = null;
            StandardFa = null;
            Salapa = new List<string>();
            HoWords = new List<string>();
            EWords = new List<string>();
            ZbWords = new List<string>();
            ILevelShip = null;
            ILevelCurrent = null;
            ILevelBackup = null;
        }

        protected virtual string GetEcuNameByIdent(string sgbd)
        {
            return null;
        }

        protected virtual void LogInfoFormat(string format, params object[] args)
        {
        }
        protected virtual void LogErrorFormat(string format, params object[] args)
        {
        }
    }
}
