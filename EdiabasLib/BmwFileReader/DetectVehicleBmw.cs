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
        [XmlInclude(typeof(EcuInfo)), XmlInclude(typeof(VehicleStructsBmw.VehicleSeriesInfo))]
        [XmlType("VehicleDataBmw")]
        public class VehicleDataBmw
        {
            public const string DataVersion = "8";

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
                Vin = detectVehicleBmw.Vin;
                TypeKey = detectVehicleBmw.TypeKey;
                GroupSgdb = detectVehicleBmw.GroupSgbd;
                SgdbAdd = detectVehicleBmw.SgbdAdd;
                ModelSeries = detectVehicleBmw.ModelSeries;
                Series = detectVehicleBmw.Series;
                ProductType = detectVehicleBmw.ProductType;
                BnType = detectVehicleBmw.BnType;
                Brand = detectVehicleBmw.Brand;
                TransmissionType = detectVehicleBmw.TransmissionType;
                Motor = detectVehicleBmw.Motor;
                VehicleSeriesInfo = detectVehicleBmw.VehicleSeriesInfo?.Clone();
                EcuList = detectVehicleBmw.EcuList?.ConvertAll(x => x.Clone()).ToList();
                // ConstructDate is not stored
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
                Ds2Vehicle = detectVehicleBmw.Ds2Vehicle;
                Ds2GroupFiles = detectVehicleBmw.Ds2GroupFiles;
                Pin78ConnectRequire = detectVehicleBmw.Pin78ConnectRequire;
                TypeKeyProperties = detectVehicleBmw.TypeKeyProperties != null ? new SerializableDictionary<string, string>(detectVehicleBmw.TypeKeyProperties).Clone() : null;
            }

            public bool Restore(DetectVehicleBmw detectVehicleBmw)
            {
                string versionString = GetVersionString(detectVehicleBmw);
                if (!string.IsNullOrEmpty(Version) && string.Compare(Version, versionString, StringComparison.InvariantCulture) != 0)
                {
                    return false;
                }

                detectVehicleBmw.Vin = Vin;
                detectVehicleBmw.TypeKey = TypeKey;
                detectVehicleBmw.GroupSgbd = GroupSgdb;
                detectVehicleBmw.SgbdAdd = SgdbAdd;
                detectVehicleBmw.ModelSeries = ModelSeries;
                detectVehicleBmw.Series = Series;
                detectVehicleBmw.ProductType = ProductType;
                detectVehicleBmw.BnType = BnType;
                detectVehicleBmw.Brand = Brand;
                detectVehicleBmw.TransmissionType = TransmissionType;
                detectVehicleBmw.Motor = Motor;
                detectVehicleBmw.VehicleSeriesInfo = VehicleSeriesInfo?.Clone();
                detectVehicleBmw.EcuList = EcuList?.ConvertAll(x => x.Clone()).ToList();
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
                detectVehicleBmw.Ds2Vehicle = Ds2Vehicle;
                detectVehicleBmw.Ds2GroupFiles = Ds2GroupFiles;
                detectVehicleBmw.Pin78ConnectRequire = Pin78ConnectRequire;
                detectVehicleBmw.TypeKeyProperties = TypeKeyProperties?.Clone();

                return true;
            }

            [XmlElement("Version")] public string Version { get; set; }
            [XmlElement("Vin"), DefaultValue(null)] public string Vin { get; set; }
            [XmlElement("TypeKey"), DefaultValue(null)] public string TypeKey { get; set; }
            [XmlElement("GroupSgdb"), DefaultValue(null)] public string GroupSgdb { get; set; }
            [XmlElement("SgdbAdd"), DefaultValue(null)] string SgdbAdd { get; set; }
            [XmlElement("ModelSeries"), DefaultValue(null)] public string ModelSeries { get; set; }
            [XmlElement("Series"), DefaultValue(null)] public string Series { get; set; }
            [XmlElement("ProductType"), DefaultValue(null)] public string ProductType { get; set; }
            [XmlElement("BnType"), DefaultValue(null)] public string BnType { get; private set; }
            [XmlElement("Brand"), DefaultValue(null)] public string Brand { get; set; }
            [XmlElement("TransmissionType"), DefaultValue(null)] public string TransmissionType { get; set; }
            [XmlElement("Motor"), DefaultValue(null)] public string Motor { get; set; }
            [XmlElement("VehicleSeriesInfo"), DefaultValue(null)] public VehicleStructsBmw.VehicleSeriesInfo VehicleSeriesInfo { get; protected set; }
            [XmlElement("EcuList"), DefaultValue(null)] public List<EcuInfo> EcuList { get; protected set; }
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
            [XmlElement("Ds2Vehicle"), DefaultValue(false)] public bool Ds2Vehicle { get; set; }
            [XmlElement("Ds2GroupFiles"), DefaultValue(null)] public string Ds2GroupFiles { get; set; }
            [XmlElement("Pin78ConnectRequire")] public bool Pin78ConnectRequire { get; set; }
            [XmlElement("TypeKeyProperties")] public SerializableDictionary<string, string> TypeKeyProperties { get; set; }
        }

        public delegate bool AbortDelegate();
        public delegate void ProgressDelegate(int percent);

        public AbortDelegate AbortFunc { get; set; }
        public ProgressDelegate ProgressFunc { get; set; }

        public bool Valid { get; private set; }
        public bool Ds2Vehicle { get; private set; }
        public string Ds2GroupFiles { get; private set; }
        public bool Pin78ConnectRequire { get; private set; }
        public Dictionary<string, string> TypeKeyProperties { get; private set; }

        private string _bmwDir;
        private string _fileTimeStamp;

        public const string DataFileExtension = "_VehicleDataBmw.xml";

        public DetectVehicleBmw(EdiabasNet ediabas, string bmwDir) : base(ediabas)
        {
            _bmwDir = bmwDir;
        }

        public bool DetectVehicleBmwFast(bool detectMotorbikes = false)
        {
            LogInfoFormat("Try to detect vehicle BMW fast, Motorbikes: {0}", detectMotorbikes);
            ResetValues();
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                List<JobInfo> readVinJobsBmwFast = new List<JobInfo>(ReadVinJobsBmwFast);
                List<JobInfo> readFaJobsBmwFast = new List<JobInfo>(ReadFaJobsBmwFast);
                List<JobInfo> readILevelJobsBmwFast = new List<JobInfo>(ReadILevelJobsBmwFast);

                if (!detectMotorbikes)
                {
                    readVinJobsBmwFast.RemoveAll(x => x.Motorbike);
                    readFaJobsBmwFast.RemoveAll(x => x.Motorbike);
                    readILevelJobsBmwFast.RemoveAll(x => x.Motorbike);
                }

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                ProgressFunc?.Invoke(0);

                JobInfo jobInfoVin = null;
                JobInfo jobInfoEcuList = null;
                int jobCount = readVinJobsBmwFast.Count + readFaJobsBmwFast.Count + readILevelJobsBmwFast.Count;
                int indexOffset = 0;
                int index = 0;
                foreach (JobInfo jobInfo in readVinJobsBmwFast)
                {
                    LogInfoFormat("Read VIN job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
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
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(jobInfo.JobResult, out EdiabasNet.ResultData resultData))
                            {
                                string vin = resultData.OpData as string;
                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!string.IsNullOrEmpty(vin) && VinRegex.IsMatch(vin))
                                {
                                    jobInfoVin = jobInfo;
                                    Vin = vin;
                                    BnType = jobInfo.BnType;
                                    LogInfoFormat("Detected VIN: {0}, BnType={1}", Vin, BnType);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        invalidSgbdSet.Add(jobInfo.SgdbName.ToUpperInvariant());
                        LogErrorFormat("No VIN response");
                        // ignored
                    }
                    index++;
                }

                indexOffset += readVinJobsBmwFast.Count;
                index = indexOffset;
                if (string.IsNullOrEmpty(Vin))
                {
                    LogErrorFormat("VIN detection failed");
                    return false;
                }

                TypeKeyProperties = VehicleInfoBmw.GetVehiclePropertiesFromVin(Vin, _ediabas, _bmwDir, out VehicleInfoBmw.VinRangeInfo vinRangeInfo);
                if (vinRangeInfo != null)
                {
                    TypeKey = vinRangeInfo.TypeKey;
                    ConstructYear = vinRangeInfo.ProdYear;
                    ConstructMonth = vinRangeInfo.ProdMonth;
                }

                foreach (JobInfo jobInfo in readFaJobsBmwFast)
                {
                    if (AbortFunc != null && AbortFunc())
                    {
                        return false;
                    }

                    LogInfoFormat("Read FA job: {0} {1} {2}", jobInfo.SgdbName, jobInfo.JobName, jobInfo.JobArgs ?? string.Empty);

                    try
                    {
                        ProgressFunc?.Invoke(100 * index / jobCount);

                        if (string.Compare(BnType, jobInfo.BnType, StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            LogInfoFormat("Invalid BnType job ignored: {0}, BnType={1}", jobInfo.SgdbName, jobInfo.BnType);
                            index++;
                            continue;
                        }

                        if (invalidSgbdSet.Contains(jobInfo.SgdbName.ToUpperInvariant()))
                        {
                            LogInfoFormat("Invalid SGBD job ignored: {0}, BnType={1}", jobInfo.SgdbName, jobInfo.BnType);
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
                                            if (SetStreamToStructInfo(resultDictFa))
                                            {
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
                                        SetBrInfo(br);
                                        SetStatVcmInfo(resultDict);
                                        SetStatVcmSalpaInfo(resultSets);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        LogErrorFormat("No BR response");
                        // ignored
                    }
                    index++;
                }

                indexOffset += readFaJobsBmwFast.Count;
                index = indexOffset;
                UpdateTypeKeyProperties();

                if (!string.IsNullOrEmpty(ConstructYear) && !string.IsNullOrEmpty(ConstructMonth))
                {
                    LogInfoFormat("Construct date: {0}.{1}", ConstructYear, ConstructMonth);
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(this);
                if (vehicleSeriesInfo == null)
                {
                    if (!jobInfoVin.Motorbike)
                    {
                        LogInfoFormat("Vehicle series info not found, aborting");
                        return false;
                    }

                    GroupSgbd = jobInfoVin.SgdbName;
                    LogInfoFormat("Vehicle series info not found, using motorbike group SGBD: {0}", GroupSgbd);
                }
                else
                {
                    VehicleSeriesInfo = vehicleSeriesInfo;
                    GroupSgbd = vehicleSeriesInfo.BrSgbd;
                    SgbdAdd = vehicleSeriesInfo.SgbdAdd;
                    if (!string.IsNullOrEmpty(vehicleSeriesInfo.BnType))
                    {
                        BnType = vehicleSeriesInfo.BnType;
                    }

                    Brand = vehicleSeriesInfo.Brand;
                }

                LogInfoFormat("Group SGBD: {0}, BnType: {1}", GroupSgbd ?? string.Empty, BnType ?? string.Empty);

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
                                        if (vehicleSeriesInfo != null && vehicleSeriesInfo.EcuList != null)
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
                        LogErrorFormat("No ecu list response");
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

                    LogInfoFormat("Read ILevel job: {0},{1}", jobInfo.SgdbName, jobInfo.JobName);
                    if (invalidSgbdSet.Contains(jobInfo.SgdbName.ToUpperInvariant()))
                    {
                        LogInfoFormat("Job ignored: {0}", jobInfo.SgdbName);
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
                                    LogInfoFormat("Detected ILevel ship: {0}", iLevelShip);
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
                                        LogInfoFormat("Detected ILevel current: {0}", iLevelCurrent);
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
                                        LogInfoFormat("Detected ILevel backup: {0}", iLevelBackup);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        LogErrorFormat("No ILevel response");
                        // ignored
                    }

                    index++;
                }

                indexOffset += readILevelJobsBmwFast.Count;
                index = indexOffset;
                ProgressFunc?.Invoke(100 * index / jobCount);

                if (string.IsNullOrEmpty(iLevelShip))
                {
                    LogErrorFormat("ILevel not found");
                }
                else
                {
                    LogInfoFormat("ILevel: Ship={0}, Current={1}, Backup={2}",
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
            LogInfoFormat("Try to detect DS2 vehicle");
            ResetValues();

            int jobCount = 1 + ReadVinJobsDs2.Count + ReadIdentJobsDs2.Count + ReadFaJobsDs2.Count;
            int indexOffset = 0;
            int index = 0;

            try
            {
                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;

                ProgressFunc?.Invoke(0);

                string groupFiles = null;
                try
                {
                    ProgressFunc?.Invoke(100 * index / jobCount);
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
                                    LogInfoFormat("KD group files: {0}", groupFiles);
                                }
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(groupFiles))
                    {
                        LogErrorFormat("KD data empty, using fallback");
                        groupFiles = AllDs2GroupFiles;
                    }
                }
                catch (Exception)
                {
                    LogErrorFormat("Read KD data failed");
                    // ignored
                }

                indexOffset += 1;
                index = indexOffset;

                string detectedVin = null;
                if (!string.IsNullOrEmpty(groupFiles))
                {
                    foreach (JobInfo jobInfo in ReadVinJobsDs2)
                    {
                        LogInfoFormat("Read VIN job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                        try
                        {
                            ProgressFunc?.Invoke(100 * index / jobCount);
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
                                        LogInfoFormat("Detected VIN: {0}", detectedVin);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            LogErrorFormat("No VIN response");
                            // ignored
                        }
                        index++;
                    }
                }
                else
                {
                    foreach (string fileName in ReadMotorJobsDs2)
                    {
                        try
                        {
                            LogInfoFormat("Read motor job: {0}", fileName);

                            ProgressFunc?.Invoke(100 * index / jobCount);
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
                                    LogInfoFormat("Motor ECUs detected: {0}", groupFiles);
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

                indexOffset += ReadVinJobsDs2.Count;
                index = indexOffset;

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
                        LogInfoFormat("Model year: {0}", modelYear);
                        ConstructYear = string.Format(CultureInfo.InvariantCulture, "{0:0000}", modelYear);
                        ConstructMonth = "01";
                    }
                }

                if (!string.IsNullOrEmpty(ConstructYear) && !string.IsNullOrEmpty(ConstructMonth))
                {
                    LogInfoFormat("Construct date: {0}.{1}", ConstructYear, ConstructMonth);
                }

                if (!string.IsNullOrEmpty(detectedVin) && detectedVin.Length == 17)
                {
                    string typeSnr = detectedVin.Substring(3, 4);
                    LogInfoFormat("Type SNR: {0}", typeSnr);
                    foreach (JobInfo jobInfo in ReadIdentJobsDs2)
                    {
                        LogInfoFormat("Read vehicle type job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                        try
                        {
                            ProgressFunc?.Invoke(100 * index / jobCount);
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
                                        LogInfoFormat("Detected Vehicle series: {0}", Series);
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            LogErrorFormat("No vehicle type response");
                            // ignored
                        }

                        index++;
                    }
                }

                indexOffset += ReadIdentJobsDs2.Count;
                index = indexOffset;

                if (!string.IsNullOrEmpty(groupFiles))
                {
                    string[] groupArray = groupFiles.Split(',');
                    foreach (JobInfo jobInfo in ReadFaJobsDs2)
                    {
                        LogInfoFormat("Read vehicle FA job: {0} {1}", jobInfo.SgdbName, jobInfo.JobName);
                        if (groupArray.All(x => string.Compare(x, jobInfo.SgdbName, StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            LogInfoFormat("Missing FA SGDB ignored: {0}", jobInfo.SgdbName);
                            index++;
                            continue;
                        }

                        try
                        {
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

                            resultSets = _ediabas.ResultSets;
                            if (resultSets != null && resultSets.Count >= 2)
                            {
                                Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                                if (resultDict.TryGetValue(jobInfo.JobResult, out EdiabasNet.ResultData resultData))
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
                                            if (SetStreamToStructInfo(resultDictFa))
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                            LogErrorFormat("No read FA response");
                            // ignored
                        }

                        index++;
                    }
                }

                indexOffset += ReadFaJobsDs2.Count;
                index = indexOffset;

                UpdateTypeKeyProperties();
                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(this);
                if (vehicleSeriesInfo != null)
                {
                    VehicleSeriesInfo = vehicleSeriesInfo;
                    SgbdAdd = vehicleSeriesInfo.SgbdAdd;
                    if (!string.IsNullOrEmpty(vehicleSeriesInfo.BnType))
                    {
                        BnType = vehicleSeriesInfo.BnType;
                    }
                    Brand = vehicleSeriesInfo.Brand;
                }

                ProgressFunc?.Invoke(100 * index / jobCount);
                LogInfoFormat("BnType: {0}", BnType ?? string.Empty);
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

        public bool SaveDataToFile(string fileName, string fileTimeStamp = null)
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
                LogInfoFormat("SaveDataToFile Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return true;
        }

        public bool LoadDataFromFile(string fileName, string fileTimeStamp = null)
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
                LogInfoFormat("LoadDataFromFile Exception: {0}", EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return Valid;
        }

        private void UpdateTypeKeyProperties()
        {
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
                        LogInfoFormat("Product type: {0}", ProductType);
                    }
                }

                if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.BrandName, out string brandProp))
                {
                    if (!string.IsNullOrEmpty(brandProp))
                    {
                        Brand = brandProp;
                        LogInfoFormat("Brand: {0}", brandProp);
                    }
                }

                if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.TransmisionName, out string transmissionProp))
                {
                    if (!string.IsNullOrEmpty(transmissionProp))
                    {
                        TransmissionType = transmissionProp;
                        LogInfoFormat("Transmission: {0}", brandProp);
                    }
                }

                if (TypeKeyProperties.TryGetValue(VehicleInfoBmw.MotorName, out string motorProp))
                {
                    if (!string.IsNullOrEmpty(motorProp))
                    {
                        Motor = motorProp;
                        LogInfoFormat("Motor: {0}", brandProp);
                    }
                }
            }
        }

        protected override void ResetValues()
        {
            base.ResetValues();

            Valid = false;
            Ds2Vehicle = false;
            Ds2GroupFiles = null;
            Pin78ConnectRequire = false;
            TypeKeyProperties = null;
        }

        public override string GetEcuNameByIdent(string sgbd)
        {
            try
            {
                if (string.IsNullOrEmpty(sgbd))
                {
                    return null;
                }

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

        protected override void LogInfoFormat(string format, params object[] args)
        {
            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, format, args);
        }
        protected override void LogErrorFormat(string format, params object[] args)
        {
            _ediabas?.LogFormat(EdiabasNet.EdLogLevel.Ifh, format, args);
        }
    }
}
