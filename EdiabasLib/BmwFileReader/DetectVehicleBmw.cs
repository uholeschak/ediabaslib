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
    public class DetectVehicleBmw : DetectVehicleBmwBase
    {
        [XmlType("VehicleDataBmw")]
        public class VehicleDataBmw
        {
            public const string DataVersion = "6";

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
                SgdbAddList = detectVehicleBmw.SgdbAddList;
                ModelSeries = detectVehicleBmw.ModelSeries;
                Series = detectVehicleBmw.Series;
                ProductType = detectVehicleBmw.ProductType;
                BnType = detectVehicleBmw.BnType;
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
                detectVehicleBmw.SgdbAddList = SgdbAddList;
                detectVehicleBmw.ModelSeries = ModelSeries;
                detectVehicleBmw.Series = Series;
                detectVehicleBmw.ProductType = ProductType;
                detectVehicleBmw.BnType = BnType;
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
            [XmlElement("SgdbAddList"), DefaultValue(null)] public List<string> SgdbAddList { get; set; }
            [XmlElement("ModelSeries"), DefaultValue(null)] public string ModelSeries { get; set; }
            [XmlElement("Series"), DefaultValue(null)] public string Series { get; set; }
            [XmlElement("ProductType"), DefaultValue(null)] public string ProductType { get; set; }
            [XmlElement("BnType")] public string BnType { get; private set; }
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

        public delegate bool AbortDelegate();
        public delegate void ProgressDelegate(int percent);

        public AbortDelegate AbortFunc { get; set; }
        public ProgressDelegate ProgressFunc { get; set; }

        public bool Valid { get; private set; }
        public bool Ds2Vehicle { get; private set; }
        public Dictionary<string, string> TypeKeyProperties { get; private set; }
        public VehicleStructsBmw.VehicleSeriesInfo VehicleSeriesInfo { get; private set; }
        public List<string> SgdbAddList { get; private set; }
        public string Ds2GroupFiles { get; private set; }
        public bool Pin78ConnectRequire { get; private set; }

        private string _bmwDir;
        private string _fileTimeStamp;

        public const string DataFileExtension = "_VehicleDataBmw.xml";

        public DetectVehicleBmw(EdiabasNet ediabas, string bmwDir) : base(ediabas)
        {
            _bmwDir = bmwDir;
        }

        public bool DetectVehicleBmwFast(bool detectMotorbikes = false)
        {
            LogFormat("Try to detect vehicle BMW fast, Motorbikes: {0}", detectMotorbikes);
            ResetValues();
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                List<JobInfo> readVinJobsBmwFast = new List<JobInfo>(ReadVinJobsBmwFast);
                List<JobInfo> readIdentJobsBmwFast = new List<JobInfo>(ReadIdentJobsBmwFast);
                List<JobInfo> readILevelJobsBmwFast = new List<JobInfo>(ReadILevelJobsBmwFast);

                if (!detectMotorbikes)
                {
                    readVinJobsBmwFast.RemoveAll(x => x.Motorbike);
                    readIdentJobsBmwFast.RemoveAll(x => x.Motorbike);
                    readILevelJobsBmwFast.RemoveAll(x => x.Motorbike);
                }

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                ProgressFunc?.Invoke(0);

                string detectedVin = null;
                JobInfo jobInfoEcuList = null;
                int jobCount = readVinJobsBmwFast.Count + readIdentJobsBmwFast.Count + readILevelJobsBmwFast.Count;
                int indexOffset = 0;
                int index = 0;
                foreach (JobInfo jobInfo in readVinJobsBmwFast)
                {
                    LogFormat("Read VIN job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                    try
                    {
                        if (AbortFunc != null && AbortFunc())
                        {
                            return false;
                        }

                        ProgressFunc?.Invoke(100 * index / jobCount);

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
                        if (!string.IsNullOrEmpty(jobInfo.EcuListJob))
                        {
                            jobInfoEcuList = jobInfo;
                        }

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
                                    LogFormat("Detected VIN: {0}", detectedVin);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        invalidSgbdSet.Add(jobInfo.SgdbName.ToUpperInvariant());
                        LogFormat("No VIN response");
                        // ignored
                    }
                    index++;
                }

                indexOffset += readVinJobsBmwFast.Count;
                index = indexOffset;
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

                foreach (JobInfo jobInfo in readIdentJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    LogFormat("Read BR job: {0} {1} {2}", jobInfo.SgdbName, jobInfo.JobName, jobInfo.JobArgs ?? string.Empty);

                    try
                    {
                        ProgressFunc?.Invoke(100 * index / jobCount);

                        if (invalidSgbdSet.Contains(jobInfo.SgdbName.ToUpperInvariant()))
                        {
                            LogFormat("Job ignored: {0}", jobInfo.SgdbName);
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
                                                    LogFormat("Detected BR: {0}", br);
                                                    string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                                                    if (!string.IsNullOrEmpty(vSeries))
                                                    {
                                                        LogFormat("Detected vehicle series: {0}", vSeries);
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
                                                        LogFormat("Detected construction date: {0}",
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
                                        LogFormat("Detected BR: {0}", br);
                                        string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                                        if (!string.IsNullOrEmpty(vSeries))
                                        {
                                            LogFormat("Detected vehicle series: {0}", vSeries);
                                            ModelSeries = br;
                                            Series = vSeries;
                                        }

                                        if (resultDict.TryGetValue("STAT_ZEIT_KRITERIUM", out EdiabasNet.ResultData resultDataCDate))
                                        {
                                            string cDateStr = resultDataCDate.OpData as string;
                                            DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                            if (dateTime != null)
                                            {
                                                LogFormat("Detected construction date: {0}",
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

                indexOffset += readIdentJobsBmwFast.Count;
                index = indexOffset;
                if (TypeKeyProperties != null)
                {
                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.VehicleSeriesName, out string vehicleSeriesProp))
                    {
                        if (!string.IsNullOrEmpty(vehicleSeriesProp))
                        {
                            Series = vehicleSeriesProp;
                        }
                    }

                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.ProductTypeName, out string productTypeProp))
                    {
                        if (!string.IsNullOrEmpty(productTypeProp))
                        {
                            ProductType = productTypeProp;
                            LogFormat("Product type: {0}", ProductType);
                        }
                    }

                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.BrandName, out string brandProp))
                    {
                        if (!string.IsNullOrEmpty(brandProp))
                        {
                            BrandList = new List<string> { brandProp };
                            LogFormat("Brand: {0}", brandProp);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ConstructYear) && !string.IsNullOrEmpty(ConstructMonth))
                {
                    LogFormat("Construct date: {0}.{1}", ConstructYear, ConstructMonth);
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(Series, ConstructYear, ConstructMonth, _ediabas);
                if (vehicleSeriesInfo == null)
                {
                    LogFormat("Vehicle series info not found");
                    return false;
                }

                VehicleSeriesInfo = vehicleSeriesInfo;
                GroupSgdb = vehicleSeriesInfo.BrSgbd;
                SgdbAddList = vehicleSeriesInfo.SgdbAdd;
                BnType = vehicleSeriesInfo.BnType;
                if (BrandList == null || BrandList.Count == 0)
                {
                    BrandList = vehicleSeriesInfo.BrandList;
                }
                LogFormat("Group SGBD: {0}, BnType: {1}", GroupSgdb ?? string.Empty, BnType ?? string.Empty);

                EcuList.Clear();
                if (jobInfoEcuList != null)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    try
                    {
                        ProgressFunc?.Invoke(100 * index / jobCount);
                        _ediabas.ResolveSgbdFile(jobInfoEcuList.SgdbName);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(jobInfoEcuList.EcuListJob);

                        resultSets = _ediabas.ResultSets;
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

                                string ecuName = string.Empty;
                                Int64 ecuAdr = -1;
                                // ReSharper disable once InlineOutVariableDeclaration
                                EdiabasNet.ResultData resultData;
                                if (resultDict.TryGetValue("STAT_SG_NAME_TEXT", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        ecuName = (string)resultData.OpData;
                                    }
                                }

                                if (resultDict.TryGetValue("STAT_SG_DIAG_ADRESSE", out resultData))
                                {
                                    if (resultData.OpData is string)
                                    {
                                        string ecuAdrStr = (string)resultData.OpData;
                                        if (!string.IsNullOrEmpty(ecuAdrStr) && ecuAdrStr.Length > 1)
                                        {
                                            string hexString = ecuAdrStr.Trim().Substring(2);
                                            if (Int32.TryParse(hexString, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out Int32 addrValue))
                                            {
                                                ecuAdr = addrValue;
                                            }
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(ecuName) && ecuAdr >= 0 && ecuAdr <= VehicleStructsBmw.MaxEcuAddr)
                                {
                                    if (EcuList.All(ecuInfo => ecuInfo.Address != ecuAdr))
                                    {
                                        string groupSgbd = null;
                                        if (vehicleSeriesInfo.EcuList != null)
                                        {
                                            foreach (VehicleStructsBmw.VehicleEcuInfo vehicleEcuInfo in vehicleSeriesInfo.EcuList)
                                            {
                                                if (vehicleEcuInfo.DiagAddr == ecuAdr)
                                                {
                                                    groupSgbd = vehicleEcuInfo.GroupSgbd;
                                                    break;
                                                }
                                            }
                                        }

                                        if (!string.IsNullOrEmpty(groupSgbd))
                                        {
                                            EcuInfo ecuInfo = new EcuInfo(ecuName, ecuAdr, groupSgbd);
                                            EcuList.Add(ecuInfo);
                                        }
                                    }
                                }

                                dictIndex++;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "No ecu list response");
                        // ignored
                    }

                    indexOffset++;
                    jobCount++;
                    index++;
                }

                string iLevelShip = null;
                string iLevelCurrent = null;
                string iLevelBackup = null;
                foreach (JobInfo jobInfo in readILevelJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    ProgressFunc?.Invoke(100 * index / jobCount);

                    LogFormat("Read ILevel job: {0},{1}", jobInfo.SgdbName, jobInfo.JobName);
                    if (invalidSgbdSet.Contains(jobInfo.SgdbName.ToUpperInvariant()))
                    {
                        LogFormat("Job ignored: {0}", jobInfo.SgdbName);
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
                                if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 && string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    iLevelShip = iLevel;
                                    LogFormat("Detected ILevel ship: {0}", iLevelShip);
                                }
                            }

                            if (!string.IsNullOrEmpty(iLevelShip))
                            {
                                if (resultDict.TryGetValue("STAT_I_STUFE_HO", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 && string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelCurrent = iLevel;
                                        LogFormat("Detected ILevel current: {0}", iLevelCurrent);
                                    }
                                }

                                if (string.IsNullOrEmpty(iLevelCurrent))
                                {
                                    iLevelCurrent = iLevelShip;
                                }

                                if (resultDict.TryGetValue("STAT_I_STUFE_HO_BACKUP", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 && string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelBackup = iLevel;
                                        LogFormat("Detected ILevel backup: {0}", iLevelBackup);
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

                indexOffset += readILevelJobsBmwFast.Count;
                index = indexOffset;
                ProgressFunc?.Invoke(100 * index / jobCount);

                if (string.IsNullOrEmpty(iLevelShip))
                {
                    _ediabas.LogString(EdiabasNet.EdLogLevel.Ifh, "ILevel not found");
                }
                else
                {
                    LogFormat("ILevel: Ship={0}, Current={1}, Backup={2}",
                        iLevelShip, iLevelCurrent, iLevelBackup);

                    ILevelShip = iLevelShip;
                    ILevelCurrent = iLevelCurrent;
                    ILevelBackup = iLevelBackup;
                }

                HandleSpecialEcus();

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
                                    LogFormat("KD group files: {0}", groupFiles);
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
                        LogFormat("Read VIN job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
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
                                        LogFormat("Detected VIN: {0}", detectedVin);
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
                            LogFormat("Read motor job: {0}", fileName);

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
                                    LogFormat("Motor ECUs detected: {0}", groupFiles);
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
                        LogFormat("Model year: {0}", modelYear);
                        ConstructYear = string.Format(CultureInfo.InvariantCulture, "{0:0000}", modelYear);
                        ConstructMonth = "01";
                    }
                }

                if (!string.IsNullOrEmpty(ConstructYear) && !string.IsNullOrEmpty(ConstructMonth))
                {
                    LogFormat("Construct date: {0}.{1}", ConstructYear, ConstructMonth);
                }

                if (!string.IsNullOrEmpty(detectedVin) && detectedVin.Length == 17)
                {
                    string typeSnr = detectedVin.Substring(3, 4);
                    LogFormat("Type SNR: {0}", typeSnr);
                    foreach (JobInfo jobInfo in ReadIdentJobsDs2)
                    {
                        LogFormat("Read vehicle type job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
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
                                        Series = detectedSeries;
                                        LogFormat("Detected Vehicle series: {0}", Series);
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
                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.VehicleSeriesName, out string vehicleSeriesProp))
                    {
                        if (!string.IsNullOrEmpty(vehicleSeriesProp))
                        {
                            Series = vehicleSeriesProp;
                        }
                    }

                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.ProductTypeName, out string productTypeProp))
                    {
                        if (!string.IsNullOrEmpty(productTypeProp))
                        {
                            ProductType = productTypeProp;
                            LogFormat("Product type: {0}", ProductType);
                        }
                    }

                    if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.BrandName, out string brandProp))
                    {
                        if (!string.IsNullOrEmpty(brandProp))
                        {
                            BrandList = new List<string> { brandProp };
                            LogFormat("Brand: {0}", brandProp);
                        }
                    }
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(Series, ConstructYear, ConstructMonth, _ediabas);
                if (vehicleSeriesInfo != null)
                {
                    VehicleSeriesInfo = vehicleSeriesInfo;
                    SgdbAddList = vehicleSeriesInfo.SgdbAdd;
                    BnType = vehicleSeriesInfo.BnType;
                    if (BrandList == null || BrandList.Count == 0)
                    {
                        BrandList = vehicleSeriesInfo.BrandList;
                    }
                }

                LogFormat("BnType: {0}", BnType ?? string.Empty);
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
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "\t"
                    };
                    using (XmlWriter writer = XmlWriter.Create(fileStream, settings))
                    {
                        serializer.Serialize(writer, vehicleDataBmw);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFormat("SaveDataToFile Exception: {0}", EdiabasNet.GetExceptionText(ex));
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
                LogFormat("LoadDataFromFile Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return Valid;
        }

        protected override void ResetValues()
        {
            base.ResetValues();

            Valid = false;
            Ds2Vehicle = false;
            TypeKeyProperties = null;
            VehicleSeriesInfo = null;
            SgdbAddList = null;
            Ds2GroupFiles = null;
            Pin78ConnectRequire = false;
        }

        protected override string GetEcuNameByIdent(string sgbd)
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

        protected override void LogFormat(string format, params object[] args)
        {
            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, format, args);
        }
    }
}
