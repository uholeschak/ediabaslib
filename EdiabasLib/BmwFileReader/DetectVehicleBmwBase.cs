using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using BmwDeepObd;
using EdiabasLib;

namespace BmwFileReader
{
    public class DetectVehicleBmwBase
    {
        protected class JobInfo
        {
            public JobInfo(string sgdbName, string jobName, string jobArgs = null, string jobResult = null, bool motorbike = false, string ecuListJob = null)
            {
                SgdbName = sgdbName;
                JobName = jobName;
                JobArgs = jobArgs;
                JobResult = jobResult;
                Motorbike = motorbike;
                EcuListJob = ecuListJob;
            }

            public string SgdbName { get; }
            public string JobName { get; }
            public string JobArgs { get; }
            public string JobResult { get; }
            public bool Motorbike { get; }
            public string EcuListJob { get; }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, Int64 address, string grp)
            {
                Name = name;
                Address = address;
                Grp = grp;
            }

            public string Name { get; set; }

            public Int64 Address { get; set; }

            public string Grp { get; set; }
        }

        public string Vin { get; private set; }
        public string TypeKey { get; private set; }
        public string GroupSgdb { get; private set; }
        public string ModelSeries { get; private set; }
        public string Series { get; private set; }
        public string ProductType { get; private set; }
        public string BnType { get; private set; }
        public List<string> BrandList { get; private set; }
        public List<EcuInfo> EcuList { get; private set; }
        public string ConstructYear { get; private set; }
        public string ConstructMonth { get; private set; }
        public string Paint { get; private set; }
        public string Upholstery { get; private set; }
        public string StandardFa { get; private set; }
        public List<string> Salapa { get; private set; }
        public List<string> HoWords { get; private set; }
        public List<string> EWords { get; private set; }
        public List<string> ZbWords { get; private set; }
        public string ILevelShip { get; private set; }
        public string ILevelCurrent { get; private set; }
        public string ILevelBackup { get; private set; }

        protected EdiabasNet _ediabas;

        public static Regex VinRegex = new Regex(@"^(?!0{7,})([a-zA-Z0-9]{7,})$");

        protected static readonly List<JobInfo> ReadVinJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_VIN_LESEN", string.Empty, "STAT_VIN", false, "STATUS_VCM_GET_ECU_LIST_ALL"),
            new JobInfo("ZGW_01", "STATUS_VIN_LESEN", string.Empty, "STAT_VIN", false, "STATUS_VCM_GET_ECU_LIST_ALL"),
            new JobInfo("G_CAS", "STATUS_FAHRGESTELLNUMMER", string.Empty, "STAT_FGNR17_WERT"),
            new JobInfo("D_CAS", "STATUS_FAHRGESTELLNUMMER", string.Empty, "FGNUMMER"),
            // motorbikes BN2000
            new JobInfo("D_MRMOT", "STATUS_FAHRGESTELLNUMMER", string.Empty, "STAT_FGNUMMER", true),
            new JobInfo("D_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT", true),
            // motorbikes BN2020
            new JobInfo("G_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT", true),
            new JobInfo("X_K001", "PROG_FG_NR_LESEN_FUNKTIONAL", "18", "FG_NR_LANG", true),
            new JobInfo("X_KS01", "PROG_FG_NR_LESEN_FUNKTIONAL", "18", "FG_NR_LANG", true),
        };

        protected static readonly List<JobInfo> ReadIdentJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_VCM_GET_FA", string.Empty, "STAT_BAUREIHE"),
            new JobInfo("ZGW_01", "STATUS_VCM_GET_FA", string.Empty, "STAT_BAUREIHE"),
            new JobInfo("G_CAS", "STATUS_FAHRZEUGAUFTRAG", string.Empty, "STAT_FAHRZEUGAUFTRAG_KOMPLETT_WERT"),
            new JobInfo("D_CAS", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG"),
            new JobInfo("D_LM", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG"),
            new JobInfo("D_KBM", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG"),
            // motorbikes BN2000
            new JobInfo("D_MRMOT", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", true),
            new JobInfo("D_MRKOMB", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", true),
            new JobInfo("D_MRZFE", "C_FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", true),
            // motorbikes BN2020
            new JobInfo("X_K001", "FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", true),
            new JobInfo("X_KS01", "FA_LESEN", string.Empty, "FAHRZEUGAUFTRAG", true),
        };

        protected static readonly List<JobInfo> ReadILevelJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR"),
            new JobInfo("G_ZGW", "STATUS_VCM_I_STUFE_LESEN"),
            new JobInfo("G_FRM", "STATUS_VCM_I_STUFE_LESEN"),
        };

        protected static readonly List<JobInfo> ReadVinJobsDs2 = new List<JobInfo>
        {
            new JobInfo("ZCS_ALL", "FGNR_LESEN", null, "FG_NR"),
            new JobInfo("D_0080", "AIF_FG_NR_LESEN", null, "AIF_FG_NR"),
            new JobInfo("D_0010", "AIF_LESEN", null, "AIF_FG_NR"),
        };

        protected static readonly List<JobInfo> ReadIdentJobsDs2 = new List<JobInfo>
        {
            new JobInfo("FZGIDENT", "GRUNDMERKMALE_LESEN", null, "BR_TXT"),
            new JobInfo("FZGIDENT", "STRINGS_LESEN", null, "BR_TXT"),
        };

        public DetectVehicleBmwBase(EdiabasNet ediabas)
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

        protected virtual void ResetValues()
        {
            Vin = null;
            TypeKey = null;
            GroupSgdb = null;
            ModelSeries = null;
            Series = null;
            ProductType = null;
            BnType = null;
            BrandList = null;
            EcuList = new List<EcuInfo>();
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

        protected void SetConstructDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return;
            }

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

            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected FA: {0}", GetFaInfo());
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

            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected FA: {0}", GetFaInfo());
        }

        protected bool SetInfoFromStdFa(string stdFaStr)
        {
            if (string.IsNullOrEmpty(stdFaStr))
            {
                return false;
            }

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
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle series: {0}", vSeries);
                            ModelSeries = br;
                            Series = vSeries;
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

                _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected FA: {0}", GetFaInfo());
                return true;
            }
            catch (Exception ex)
            {
                _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, "SetInfoFromStdFa Exception: {0}", EdiabasNet.GetExceptionText(ex));
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
                EcuInfo ecuInfoAdd = new EcuInfo("RLS", 86, "D_RLS");
                EcuInfo ecuInfoRls = GetEcuByEcuGroup(ecuInfoAdd.Grp);
                if (ecuInfoRls == null)
                {
                    if (EcuList.All(ecuInfo => ecuInfo.Address != ecuInfoAdd.Address))
                    {
                        bool addEcu = HasSa("521") || !string.IsNullOrEmpty(GetEcuNameByIdent(ecuInfoAdd.Grp));
                        if (addEcu)
                        {
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

            if (!string.IsNullOrEmpty(BnType) && string.Compare(BnType, "BEV2010", StringComparison.OrdinalIgnoreCase) == 0)
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

            foreach (EcuInfo ecuInfoRemove in ecuRemoveList)
            {
                ecuRemoveList.Remove(ecuInfoRemove);
            }
        }

        protected string GetEcuNameByIdent(string sgbd)
        {
            try
            {
                ActivityCommon.ResolveSgbdFile(_ediabas, sgbd);

                _ediabas.ArgString = string.Empty;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.ExecuteJob("IDENT");

                string ecuName = Path.GetFileNameWithoutExtension(_ediabas.SgbdFileName);
                return ecuName.ToUpperInvariant();
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
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
    }
}
