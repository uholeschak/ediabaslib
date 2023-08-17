using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using BmwDeepObd;
using EdiabasLib;

namespace BmwFileReader
{
    public class DetectVehicleBmw
    {
        [XmlType("VehicleDataBmw")]
        public class VehicleDataBmw
        {
            public const string DataVersion = "4";

            public string GetVersionString(DetectVehicleBmw detectVehicleBmw)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(DataVersion);
                if (!string.IsNullOrEmpty(detectVehicleBmw._fileTimeStamp))
                {
                    sb.Append("/");
                    sb.Append(detectVehicleBmw._fileTimeStamp);
                }

                string seriesTimeStamp = VehicleInfoBmw.GetVehicleSeriesInfoTimeStamp();
                if (!string.IsNullOrEmpty(seriesTimeStamp))
                {
                    sb.Append("/");
                    sb.Append(seriesTimeStamp);
                }

                sb.Append(DataVersion);
                return sb.ToString();
            }

            public VehicleDataBmw()
            {
                Salapa = new List<string>();
                HoWords = new List<string>();
                EWords = new List<string>();
                ZbWords = new List<string>();
            }

            public VehicleDataBmw(DetectVehicleBmw detectVehicleBmw)
            {
                Version = GetVersionString(detectVehicleBmw);
                Ds2Vehicle = detectVehicleBmw.Ds2Vehicle;
                Vin = detectVehicleBmw.Vin;
                TypeKey = detectVehicleBmw.TypeKey;
                GroupSgdb = detectVehicleBmw.GroupSgdb;
                ModelSeries = detectVehicleBmw.ModelSeries;
                Series = detectVehicleBmw.Series;
                BrandList = detectVehicleBmw.BrandList;
                Ds2GroupFiles = detectVehicleBmw.Ds2GroupFiles;
                ConstructYear = detectVehicleBmw.ConstructYear;
                ConstructMonth = detectVehicleBmw.ConstructMonth;
                Paint = detectVehicleBmw.Paint;
                Upholstery = detectVehicleBmw.Upholstery;
                StandardFa = detectVehicleBmw.StandardFa;
                Salapa = detectVehicleBmw.Salapa;
                HoWords = detectVehicleBmw.HoWords;
                EWords = detectVehicleBmw.EWords;
                ZbWords = detectVehicleBmw.ZbWords;
                ILevelShip = detectVehicleBmw.ILevelShip;
                ILevelCurrent = detectVehicleBmw.ILevelCurrent;
                ILevelBackup = detectVehicleBmw.ILevelBackup;
            }

            public bool Restore(DetectVehicleBmw detectVehicleBmw)
            {
                string versionString = GetVersionString(detectVehicleBmw);
                if (string.Compare(Version, versionString, StringComparison.InvariantCulture) != 0)
                {
                    return false;
                }

                detectVehicleBmw.Ds2Vehicle = Ds2Vehicle;
                detectVehicleBmw.Vin = Vin;
                detectVehicleBmw.TypeKey = TypeKey;
                detectVehicleBmw.GroupSgdb = GroupSgdb;
                detectVehicleBmw.ModelSeries = ModelSeries;
                detectVehicleBmw.Series = Series;
                detectVehicleBmw.BrandList = BrandList;
                detectVehicleBmw.Ds2GroupFiles = Ds2GroupFiles;
                detectVehicleBmw.ConstructYear = ConstructYear;
                detectVehicleBmw.ConstructMonth = ConstructMonth;
                detectVehicleBmw.Paint = Paint;
                detectVehicleBmw.Upholstery = Upholstery;
                detectVehicleBmw.StandardFa = StandardFa;
                detectVehicleBmw.Salapa = Salapa ?? new List<string>();
                detectVehicleBmw.HoWords = HoWords ?? new List<string>();
                detectVehicleBmw.EWords = EWords ?? new List<string>();
                detectVehicleBmw.ZbWords = ZbWords ?? new List<string>();
                detectVehicleBmw.ILevelShip = ILevelShip;
                detectVehicleBmw.ILevelCurrent = ILevelCurrent;
                detectVehicleBmw.ILevelBackup = ILevelBackup;

                return true;
            }

            [XmlElement("Version")] public string Version { get; set; }
            [XmlElement("Ds2Vehicle"), DefaultValue(false)] public bool Ds2Vehicle { get; set; }
            [XmlElement("Vin"), DefaultValue(null)] public string Vin { get; set; }
            [XmlElement("TypeKey"), DefaultValue(null)] public string TypeKey { get; set; }
            [XmlElement("GroupSgdb"), DefaultValue(null)] public string GroupSgdb { get; set; }
            [XmlElement("ModelSeries"), DefaultValue(null)] public string ModelSeries { get; set; }
            [XmlElement("Series"), DefaultValue(null)] public string Series { get; set; }
            [XmlElement("BrandList"), DefaultValue(null)] public List<string> BrandList { get; set; }
            [XmlElement("Ds2GroupFiles"), DefaultValue(null)] public string Ds2GroupFiles { get; set; }
            [XmlElement("ConstructYear"), DefaultValue(null)] public string ConstructYear { get; set; }
            [XmlElement("ConstructMonth"), DefaultValue(null)] public string ConstructMonth { get; set; }
            [XmlElement("Paint"), DefaultValue(null)] public string Paint { get; private set; }
            [XmlElement("Upholstery"), DefaultValue(null)] public string Upholstery { get; private set; }
            [XmlElement("StandardFa"), DefaultValue(null)] public string StandardFa { get; private set; }
            [XmlElement("Salapa"), DefaultValue(null)] public List<string> Salapa { get; private set; }
            [XmlElement("HoWords"), DefaultValue(null)] public List<string> HoWords { get; private set; }
            [XmlElement("EWords"), DefaultValue(null)] public List<string> EWords { get; private set; }
            [XmlElement("ZbWords"), DefaultValue(null)] public List<string> ZbWords { get; private set; }
            [XmlElement("ILevelShip"), DefaultValue(null)] public string ILevelShip { get; set; }
            [XmlElement("ILevelCurrent"), DefaultValue(null)] public string ILevelCurrent { get; set; }
            [XmlElement("ILevelBackup"), DefaultValue(null)] public string ILevelBackup { get; set; }
        }

        private class JobInfo
        {
            public JobInfo(string sgdbName, string jobName, string jobArgs = null, string jobResult = null, bool motorbike = false)
            {
                SgdbName = sgdbName;
                JobName = jobName;
                JobArgs = jobArgs;
                JobResult = jobResult;
                Motorbike = motorbike;
            }

            public string SgdbName { get; }
            public string JobName { get; }
            public string JobArgs { get; }
            public string JobResult { get; }
            public bool Motorbike { get; }
        }

        public delegate bool AbortDelegate();
        public delegate void ProgressDelegate(int percent);

        public AbortDelegate AbortFunc { get; set; }
        public ProgressDelegate ProgressFunc { get; set; }

        public bool Valid { get; private set; }
        public bool Ds2Vehicle { get; private set; }
        public string Vin { get; private set; }
        public string TypeKey { get; private set; }
        public Dictionary<string, string> TypeKeyProperties;
        public string GroupSgdb { get; private set; }
        public string ModelSeries { get; private set; }
        public string Series { get; private set; }
        public List<string> BrandList { get; private set; }
        public string Ds2GroupFiles { get; private set; }
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
        public bool Pin78ConnectRequire { get; private set; }

        private EdiabasNet _ediabas;
        private string _bmwDir;
        private string _fileTimeStamp;

        public const string DataFileExtension = "_VehicleDataBmw.xml";

        public const string AllDs2GroupFiles = "d_0000,d_0008,d_000d,d_0010,d_0011,d_0012,d_motor,d_0013,d_0014,d_0015,d_0016,d_0020,d_0021,d_0022,d_0024,d_0028,d_002c,d_002e,d_0030,d_0032,d_0035,d_0036,d_003b,d_0040,d_0044,d_0045,d_0050,d_0056,d_0057,d_0059,d_005a,d_005b,d_0060,d_0068,d_0069,d_006a,d_006c,d_0070,d_0071,d_0072,d_007f,d_0080,d_0086,d_0099,d_009a,d_009b,d_009c,d_009d,d_009e,d_00a0,d_00a4,d_00a6,d_00a7,d_00ac,d_00b0,d_00b9,d_00bb,d_00c0,d_00c8,d_00cd,d_00d0,d_00da,d_00e0,d_00e8,d_00ed,d_00f0,d_00f5,d_00ff,d_b8_d0,,d_m60_10,d_m60_12,d_spmbt,d_spmft,d_szm,d_zke3bt,d_zke3ft,d_zke3pm,d_zke3sb,d_zke3sd,d_zke_gm,d_zuheiz,d_sitz_f,d_sitz_b,d_0047,d_0048,d_00ce,d_00ea,d_abskwp,d_0031,d_0019,d_smac,d_0081,d_xen_l,d_xen_r";

        public static Regex VinRegex = new Regex(@"^(?!0{7,})([a-zA-Z0-9]{7,})$");

        private static readonly List<JobInfo> ReadVinJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_VIN_LESEN", null, "STAT_VIN"),
            new JobInfo("ZGW_01", "STATUS_VIN_LESEN", null, "STAT_VIN"),
            new JobInfo("G_CAS", "STATUS_FAHRGESTELLNUMMER", null, "STAT_FGNR17_WERT"),
            new JobInfo("D_CAS", "STATUS_FAHRGESTELLNUMMER", null, "FGNUMMER"),
            // motorbikes BN2000
            new JobInfo("D_MRMOT", "STATUS_FAHRGESTELLNUMMER", null, "STAT_FGNUMMER", true),
            new JobInfo("D_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT", true),
            // motorbikes BN2020
            new JobInfo("G_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT", true),
            new JobInfo("X_K001", "PROG_FG_NR_LESEN_FUNKTIONAL", "18", "FG_NR_LANG", true),
            new JobInfo("X_KS01", "PROG_FG_NR_LESEN_FUNKTIONAL", "18", "FG_NR_LANG", true),
        };

        private static readonly List<JobInfo> ReadIdentJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_VCM_GET_FA", null, "STAT_BAUREIHE"),
            new JobInfo("ZGW_01", "STATUS_VCM_GET_FA", null, "STAT_BAUREIHE"),
            new JobInfo("G_CAS", "STATUS_FAHRZEUGAUFTRAG", null, "STAT_FAHRZEUGAUFTRAG_KOMPLETT_WERT"),
            new JobInfo("D_CAS", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG"),
            new JobInfo("D_LM", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG"),
            new JobInfo("D_KBM", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG"),
            // motorbikes BN2000
            new JobInfo("D_MRMOT", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG", true),
            new JobInfo("D_MRKOMB", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG", true),
            new JobInfo("D_MRZFE", "C_FA_LESEN", null, "FAHRZEUGAUFTRAG", true),
            // motorbikes BN2020
            new JobInfo("X_K001", "FA_LESEN", null, "FAHRZEUGAUFTRAG", true),
            new JobInfo("X_KS01", "FA_LESEN", null, "FAHRZEUGAUFTRAG", true),
        };

        private static readonly List<JobInfo> ReadILevelJobsBmwFast = new List<JobInfo>
        {
            new JobInfo("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR"),
            new JobInfo("G_ZGW", "STATUS_VCM_I_STUFE_LESEN"),
            new JobInfo("G_FRM", "STATUS_VCM_I_STUFE_LESEN"),
        };

        private static readonly List<JobInfo> ReadVinJobsDs2 = new List<JobInfo>
        {
            new JobInfo("ZCS_ALL", "FGNR_LESEN", null, "FG_NR"),
            new JobInfo("D_0080", "AIF_FG_NR_LESEN", null, "AIF_FG_NR"),
            new JobInfo("D_0010", "AIF_LESEN", null, "AIF_FG_NR"),
        };

        private static readonly List<JobInfo> ReadIdentJobsDs2 = new List<JobInfo>
        {
            new JobInfo("FZGIDENT", "GRUNDMERKMALE_LESEN", null, "BR_TXT"),
            new JobInfo("FZGIDENT", "STRINGS_LESEN", null, "BR_TXT"),
        };

        private static readonly string[] ReadMotorJobsDs2 =
        {
            "D_0012", "D_MOTOR", "D_0010", "D_0013", "D_0014"
        };

        public DetectVehicleBmw(EdiabasNet ediabas, string bmwDir)
        {
            _ediabas = ediabas;
            _bmwDir = bmwDir;
        }

        public bool DetectVehicleBmwFast(bool detectMotorbikes = false)
        {
            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Try to detect vehicle BMW fast, Motorbikes: {0}", detectMotorbikes);
            ResetValues();
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                ProgressFunc?.Invoke(0);

                string detectedVin = null;
                int jobCount = ReadVinJobsBmwFast.Count + ReadIdentJobsBmwFast.Count + ReadILevelJobsBmwFast.Count;
                int index = 0;
                foreach (JobInfo jobInfo in ReadVinJobsBmwFast)
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read VIN job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                    try
                    {
                        if (AbortFunc != null && AbortFunc())
                        {
                            return false;
                        }

                        ProgressFunc?.Invoke(100 * index / jobCount);

                        if (!detectMotorbikes && jobInfo.Motorbike)
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Motorbike ignored: {0}", jobInfo.SgdbName);
                            index++;
                            continue;
                        }

                        ActivityCommon.ResolveSgbdFile(_ediabas, jobInfo.SgdbName);

                        _ediabas.ArgString = string.Empty;
                        if (!string.IsNullOrEmpty(jobInfo.JobArgs))
                        {
                            _ediabas.ArgString = jobInfo.JobArgs;
                        }

                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(jobInfo.JobName);

                        invalidSgbdSet.Remove(jobInfo.SgdbName.ToUpperInvariant());
                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            if (detectedVin == null)
                            {
                                detectedVin = string.Empty;
                            }
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(jobInfo.JobResult, out EdiabasNet.ResultData resultData))
                            {
                                string vin = resultData.OpData as string;
                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!string.IsNullOrEmpty(vin) && VinRegex.IsMatch(vin))
                                {
                                    detectedVin = vin;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected VIN: {0}", detectedVin);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        invalidSgbdSet.Add(jobInfo.SgdbName.ToUpperInvariant());
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "No VIN response");
                        // ignored
                    }
                    index++;
                }

                if (string.IsNullOrEmpty(detectedVin))
                {
                    return false;
                }

                Vin = detectedVin;
                TypeKeyProperties = VehicleInfoBmw.GetVehiclePropertiesFromVin(detectedVin, _ediabas, _bmwDir, out VehicleInfoBmw.VinRangeInfo vinRangeInfo);
                if (vinRangeInfo != null)
                {
                    TypeKey = vinRangeInfo.TypeKey;
                    ConstructYear = vinRangeInfo.ProdYear;
                    ConstructMonth = vinRangeInfo.ProdMonth;
                }

                foreach (JobInfo jobInfo in ReadIdentJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read BR job: {0} {1} {2}", jobInfo.SgdbName, jobInfo.JobName, jobInfo.JobArgs ?? string.Empty);

                    try
                    {
                        ProgressFunc?.Invoke(100 * index / jobCount);

                        if (!detectMotorbikes && jobInfo.Motorbike)
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Motorbike ignored: {0}", jobInfo.SgdbName);
                            index++;
                            continue;
                        }

                        if (invalidSgbdSet.Contains(jobInfo.SgdbName.ToUpperInvariant()))
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", jobInfo.SgdbName);
                            index++;
                            continue;
                        }

                        bool statVcm = string.Compare(jobInfo.JobName, "STATUS_VCM_GET_FA", StringComparison.OrdinalIgnoreCase) == 0;
                        ActivityCommon.ResolveSgbdFile(_ediabas, jobInfo.SgdbName);

                        _ediabas.ArgString = string.Empty;
                        if (!string.IsNullOrEmpty(jobInfo.JobArgs))
                        {
                            _ediabas.ArgString = jobInfo.JobArgs;
                        }

                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(jobInfo.JobName);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(jobInfo.JobResult, out EdiabasNet.ResultData resultData))
                            {
                                if (!statVcm)
                                {
                                    string fa = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(fa))
                                    {
                                        ActivityCommon.ResolveSgbdFile(_ediabas, "FA");

                                        _ediabas.ArgString = "1;" + fa;
                                        _ediabas.ArgBinaryStd = null;
                                        _ediabas.ResultsRequests = string.Empty;
                                        _ediabas.ExecuteJob("FA_STREAM2STRUCT");

                                        List<Dictionary<string, EdiabasNet.ResultData>> resultSetsFa = _ediabas.ResultSets;
                                        if (resultSetsFa != null && resultSetsFa.Count >= 2)
                                        {
                                            Dictionary<string, EdiabasNet.ResultData> resultDictFa = resultSetsFa[1];
                                            if (resultDictFa.TryGetValue("STANDARD_FA", out EdiabasNet.ResultData resultStdFa))
                                            {
                                                string stdFaStr = resultStdFa.OpData as string;
                                                if (!string.IsNullOrEmpty(stdFaStr))
                                                {
                                                    StandardFa = stdFaStr;
                                                    SetInfoFromStdFa(stdFaStr);
                                                }
                                            }

                                            if (resultDictFa.TryGetValue("BR", out EdiabasNet.ResultData resultDataBa))
                                            {
                                                string br = resultDataBa.OpData as string;
                                                if (!string.IsNullOrEmpty(br))
                                                {
                                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected BR: {0}", br);
                                                    string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                                                    if (!string.IsNullOrEmpty(vSeries))
                                                    {
                                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle series: {0}", vSeries);
                                                        ModelSeries = br;
                                                        Series = vSeries;
                                                    }
                                                }

                                                if (resultDictFa.TryGetValue("C_DATE", out EdiabasNet.ResultData resultDataCDate))
                                                {
                                                    string cDateStr = resultDataCDate.OpData as string;
                                                    DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                                    if (dateTime != null)
                                                    {
                                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected construction date: {0}",
                                                            dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                        SetConstructDate(dateTime);
                                                    }
                                                }

                                                if (resultDictFa.TryGetValue("LACK", out EdiabasNet.ResultData resultPaint))
                                                {
                                                    string paintStr = resultPaint.OpData as string;
                                                    if (!string.IsNullOrEmpty(paintStr))
                                                    {
                                                        Paint = paintStr;
                                                    }
                                                }

                                                if (resultDictFa.TryGetValue("POLSTER", out EdiabasNet.ResultData resultUpholstery))
                                                {
                                                    string upholsteryStr = resultUpholstery.OpData as string;
                                                    if (!string.IsNullOrEmpty(upholsteryStr))
                                                    {
                                                        Upholstery = upholsteryStr;
                                                    }
                                                }
                                            }

                                            if (Series != null)
                                            {
                                                SetFaSalpaInfo(resultDictFa);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string br = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(br))
                                    {
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected BR: {0}", br);
                                        string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                                        if (!string.IsNullOrEmpty(vSeries))
                                        {
                                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected vehicle series: {0}", vSeries);
                                            ModelSeries = br;
                                            Series = vSeries;
                                        }

                                        if (resultDict.TryGetValue("STAT_ZEIT_KRITERIUM", out EdiabasNet.ResultData resultDataCDate))
                                        {
                                            string cDateStr = resultDataCDate.OpData as string;
                                            DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                            if (dateTime != null)
                                            {
                                                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected construction date: {0}",
                                                    dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                SetConstructDate(dateTime);
                                            }
                                        }

                                        if (resultDict.TryGetValue("STAT_LACKCODE", out EdiabasNet.ResultData resultPaint))
                                        {
                                            string paintStr = resultPaint.OpData as string;
                                            if (!string.IsNullOrEmpty(paintStr))
                                            {
                                                Paint = paintStr;
                                            }
                                        }

                                        if (resultDict.TryGetValue("STAT_POLSTERCODE", out EdiabasNet.ResultData resultUpholstery))
                                        {
                                            string upholsteryStr = resultUpholstery.OpData as string;
                                            if (!string.IsNullOrEmpty(upholsteryStr))
                                            {
                                                Upholstery = upholsteryStr;
                                            }
                                        }

                                        if (resultDict.TryGetValue("STAT_TYP_SCHLUESSEL", out EdiabasNet.ResultData resultType))
                                        {
                                            string typeStr = resultType.OpData as string;
                                            if (!string.IsNullOrEmpty(typeStr))
                                            {
                                                TypeKey = typeStr;
                                            }
                                        }
                                    }

                                    if (Series != null)
                                    {
                                        SetStatVcmSalpaInfo(resultSets);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No BR response");
                        // ignored
                    }
                    index++;
                }

                ProgressFunc?.Invoke(100);

                if (TypeKeyProperties != null)
                {
                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.VehicleTypeName, out string vehicleSeriesProp))
                    {
                        if (!string.IsNullOrEmpty(vehicleSeriesProp))
                        {
                            Series = vehicleSeriesProp;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ConstructYear) && !string.IsNullOrEmpty(ConstructMonth))
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Construct date: {0}.{1}", ConstructYear, ConstructMonth);
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(Series, ConstructYear, ConstructMonth, _ediabas);
                if (vehicleSeriesInfo == null)
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Vehicle series info not found");
                    return false;
                }
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Group SGBD: {0}", vehicleSeriesInfo.BrSgbd);
                GroupSgdb = vehicleSeriesInfo.BrSgbd;
                BrandList = vehicleSeriesInfo.BrandList;

                string iLevelShip = null;
                string iLevelCurrent = null;
                string iLevelBackup = null;
                foreach (JobInfo jobInfo in ReadILevelJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    ProgressFunc?.Invoke(100 * index / jobCount);

                    if (!detectMotorbikes && jobInfo.Motorbike)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Motorbike ignored: {0}", jobInfo.SgdbName);
                        index++;
                        continue;
                    }

                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read ILevel job: {0},{1}", jobInfo.SgdbName, jobInfo.JobName);
                    if (invalidSgbdSet.Contains(jobInfo.SgdbName.ToUpperInvariant()))
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Job ignored: {0}", jobInfo.SgdbName);
                        index++;
                        continue;
                    }

                    try
                    {
                        _ediabas.ResolveSgbdFile(jobInfo.SgdbName);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(jobInfo.JobName);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue("STAT_I_STUFE_WERK", out EdiabasNet.ResultData resultData))
                            {
                                string iLevel = resultData.OpData as string;
                                if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                    string.Compare(iLevel, VehicleInfoBmw.ResultUnknown,
                                        StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    iLevelShip = iLevel;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel ship: {0}", iLevelShip);
                                }
                            }

                            if (!string.IsNullOrEmpty(iLevelShip))
                            {
                                if (resultDict.TryGetValue("STAT_I_STUFE_HO", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelCurrent = iLevel;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel current: {0}", iLevelCurrent);
                                    }
                                }

                                if (string.IsNullOrEmpty(iLevelCurrent))
                                {
                                    iLevelCurrent = iLevelShip;
                                }

                                if (resultDict.TryGetValue("STAT_I_STUFE_HO_BACKUP", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelBackup = iLevel;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected ILevel backup: {0}",
                                            iLevelBackup);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No ILevel response");
                        // ignored
                    }

                    index++;
                }

                if (string.IsNullOrEmpty(iLevelShip))
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ILevel not found");
                }
                else
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "ILevel: Ship={0}, Current={1}, Backup={2}",
                        iLevelShip, iLevelCurrent, iLevelBackup);

                    ILevelShip = iLevelShip;
                    ILevelCurrent = iLevelCurrent;
                    ILevelBackup = iLevelBackup;
                }

                Ds2Vehicle = false;
                Valid = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DetectVehicleDs2()
        {
            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Try to detect DS2 vehicle");
            ResetValues();

            try
            {
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                ProgressFunc?.Invoke(0);

                string groupFiles = null;
                try
                {
                    ActivityCommon.ResolveSgbdFile(_ediabas, "d_0044");

                    _ediabas.ArgString = "6";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("KD_DATEN_LESEN");

                    string kdData1 = null;
                    resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                        if (resultDict.TryGetValue("KD_DATEN_TEXT", out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                kdData1 = (string)resultData.OpData;
                            }
                        }
                    }

                    _ediabas.ArgString = "7";
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("KD_DATEN_LESEN");

                    string kdData2 = null;
                    resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                        if (resultDict.TryGetValue("KD_DATEN_TEXT", out EdiabasNet.ResultData resultData))
                        {
                            if (resultData.OpData is string)
                            {
                                kdData2 = (string)resultData.OpData;
                            }
                        }
                    }

                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(kdData1) && !string.IsNullOrEmpty(kdData2))
                    {
                        ActivityCommon.ResolveSgbdFile(_ediabas, "grpliste");

                        _ediabas.ArgString = kdData1 + kdData2 + ";ja";
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob("GRUPPENDATEI_ERZEUGE_LISTE_AUS_DATEN");

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue("GRUPPENDATEI", out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    groupFiles = (string)resultData.OpData;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "KD group files: {0}", groupFiles);
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(groupFiles))
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "KD data empty, using fallback");
                        groupFiles = AllDs2GroupFiles;
                    }
                }
                catch (Exception)
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "Read KD data failed");
                    // ignored
                }

                string detectedVin = null;

                if (!string.IsNullOrEmpty(groupFiles))
                {
                    int index = 0;
                    foreach (JobInfo jobInfo in ReadVinJobsDs2)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read VIN job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                        try
                        {
                            ProgressFunc?.Invoke(100 * index / ReadVinJobsDs2.Count);
                            ActivityCommon.ResolveSgbdFile(_ediabas, jobInfo.SgdbName);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(jobInfo.JobName);

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (resultDict.TryGetValue(jobInfo.JobResult, out EdiabasNet.ResultData resultData))
                                {
                                    string vin = resultData.OpData as string;
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    if (!string.IsNullOrEmpty(vin) && VinRegex.IsMatch(vin))
                                    {
                                        detectedVin = vin;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected VIN: {0}", detectedVin);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No VIN response");
                            // ignored
                        }
                        index++;
                    }
                }
                else
                {
                    int index = 0;
                    foreach (string fileName in ReadMotorJobsDs2)
                    {
                        try
                        {
                            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read motor job: {0}", fileName);

                            ProgressFunc?.Invoke(100 * index / ReadMotorJobsDs2.Length);
                            ActivityCommon.ResolveSgbdFile(_ediabas, fileName);

                            _ediabas.ArgString = string.Empty;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob("IDENT");

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (EdiabasThread.IsJobStatusOk(resultDict))
                                {
                                    groupFiles = fileName;
                                    Pin78ConnectRequire = true;
                                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Motor ECUs detected: {0}", groupFiles);
                                    break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        index++;
                    }
                }

                Vin = detectedVin;
                TypeKeyProperties = VehicleInfoBmw.GetVehiclePropertiesFromVin(detectedVin, _ediabas, _bmwDir, out VehicleInfoBmw.VinRangeInfo vinRangeInfo);
                if (vinRangeInfo != null)
                {
                    TypeKey = vinRangeInfo.TypeKey;
                    ConstructYear = vinRangeInfo.ProdYear;
                    ConstructMonth = vinRangeInfo.ProdMonth;
                }

                if (string.IsNullOrEmpty(ConstructYear) || string.IsNullOrEmpty(ConstructMonth))
                {
                    int modelYear = VehicleInfoBmw.GetModelYearFromVin(detectedVin);
                    if (modelYear >= 0)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Model year: {0}", modelYear);
                        ConstructYear = string.Format(CultureInfo.InvariantCulture, "{0:0000}", modelYear);
                        ConstructMonth = "01";
                    }
                }

                if (!string.IsNullOrEmpty(ConstructYear) && !string.IsNullOrEmpty(ConstructMonth))
                {
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Construct date: {0}.{1}", ConstructYear, ConstructMonth);
                }

                string series = null;
                if (!string.IsNullOrEmpty(detectedVin) && detectedVin.Length == 17)
                {
                    string typeSnr = detectedVin.Substring(3, 4);
                    _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Type SNR: {0}", typeSnr);
                    foreach (JobInfo jobInfo in ReadIdentJobsDs2)
                    {
                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Read vehicle type job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                        try
                        {
                            ActivityCommon.ResolveSgbdFile(_ediabas, jobInfo.SgdbName);

                            _ediabas.ArgString = typeSnr;
                            _ediabas.ArgBinaryStd = null;
                            _ediabas.ResultsRequests = string.Empty;
                            _ediabas.ExecuteJob(jobInfo.JobName);

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (resultDict.TryGetValue(jobInfo.JobResult, out EdiabasNet.ResultData resultData))
                                {
                                    string detectedSeries = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(detectedSeries) &&
                                        string.Compare(detectedSeries, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        series = detectedSeries;
                                        _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected Vehicle type: {0}", series);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No vehicle type response");
                            // ignored
                        }
                    }
                }

                if (TypeKeyProperties != null)
                {
                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.VehicleTypeName, out string vehicleSeriesProp))
                    {
                        if (!string.IsNullOrEmpty(vehicleSeriesProp))
                        {
                            series = vehicleSeriesProp;
                        }
                    }
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(series, ConstructYear, ConstructMonth, _ediabas);
                if (vehicleSeriesInfo != null)
                {
                    BrandList = vehicleSeriesInfo.BrandList;
                }

                Series = series;
                Ds2GroupFiles = groupFiles;

                if (string.IsNullOrEmpty(groupFiles))
                {
                    return false;
                }

                Ds2Vehicle = true;
                Valid = true;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SaveDataToFile(string fileName, string fileTimeStamp)
        {
            try
            {
                if (!Valid)
                {
                    return false;
                }

                _fileTimeStamp = fileTimeStamp;
                VehicleDataBmw vehicleDataBmw = new VehicleDataBmw(this);
                XmlSerializer serializer = new XmlSerializer(typeof(VehicleDataBmw));
                using (FileStream fileStream = File.Create(fileName))
                {
                    serializer.Serialize(fileStream, vehicleDataBmw);
                }
            }
            catch (Exception ex)
            {
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "SaveDataToFile Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return true;
        }

        public bool LoadDataFromFile(string fileName, string fileTimeStamp)
        {
            try
            {
                ResetValues();

                if (!File.Exists(fileName))
                {
                    return false;
                }

                _fileTimeStamp = fileTimeStamp;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(VehicleDataBmw));
                using (StreamReader sr = new StreamReader(fileName))
                {
                    VehicleDataBmw vehicleDataBmw = xmlSerializer.Deserialize(sr) as VehicleDataBmw;
                    if (vehicleDataBmw == null)
                    {
                        return false;
                    }

                    if (vehicleDataBmw.Restore(this))
                    {
                        Valid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "LoadDataFromFile Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return Valid;
        }

        public bool IsDs2GroupSgbd(string name)
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

        private void ResetValues()
        {
            Valid = false;
            Ds2Vehicle = false;
            Vin = null;
            TypeKey = null;
            TypeKeyProperties = null;
            GroupSgdb = null;
            ModelSeries = null;
            Series = null;
            BrandList = null;
            Ds2GroupFiles = null;
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
            Pin78ConnectRequire = false;
        }

        private void SetConstructDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return;
            }

            ConstructYear = dateTime.Value.ToString("yyyy", CultureInfo.InvariantCulture);
            ConstructMonth = dateTime.Value.ToString("MM", CultureInfo.InvariantCulture);
        }

        private void SetStatVcmSalpaInfo(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
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
                    if (!string.IsNullOrEmpty(saStr))
                    {
                        if (!Salapa.Contains(saStr))
                        {
                            Salapa.Add(saStr);
                        }
                    }
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

            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected FA: {0}", GetFaInfo());
        }

        private void SetFaSalpaInfo(Dictionary<string, EdiabasNet.ResultData> resultDict)
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
                            if (!string.IsNullOrEmpty(saStr))
                            {
                                if (!Salapa.Contains(saStr))
                                {
                                    Salapa.Add(saStr);
                                }
                            }
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

            _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected FA: {0}", GetFaInfo());
        }

        private bool SetInfoFromStdFa(string stdFaStr)
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
                        if (!string.IsNullOrEmpty(saStr))
                        {
                            if (!Salapa.Contains(saStr))
                            {
                                Salapa.Add(saStr);
                            }
                        }
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

                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "Detected FA: {0}", GetFaInfo());
                return true;
            }
            catch (Exception ex)
            {
                _ediabas.LogFormat(EdiabasNet.EdLogLevel.Ifh, "SetInfoFromStdFa Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }
    }
}
