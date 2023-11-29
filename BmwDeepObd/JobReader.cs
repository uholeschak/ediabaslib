using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using EdiabasLib;

namespace BmwDeepObd
{
    public class JobReader
    {
        public class DisplayInfo
        {
            public DisplayInfo(int originalPosition, string name, string result, string ecuJobId, string ecuJobResultId, string format, UInt32 displayOrder, GridModeType gridType, double minValue, double maxValue, string logTag)
            {
                OriginalPosition = originalPosition;
                Name = name;
                Result = result;
                EcuJobId = ecuJobId;
                EcuJobResultId = ecuJobResultId;
                Format = format;
                DisplayOrder = displayOrder;
                GridType = gridType;
                MinValue = minValue;
                MaxValue = maxValue;
                LogTag = logTag;
            }

            public enum GridModeType
            {
                Hidden,
                Text,
                // ReSharper disable InconsistentNaming
                Simple_Gauge_Square,
                Simple_Gauge_Round,
                Simple_Gauge_Dot,
                // ReSharper restore InconsistentNaming
            }

            public int OriginalPosition { get; }

            public string Name { get; }

            public string Result { get; }

            public string EcuJobId { get; }

            public string EcuJobResultId { get; }

            public string Format { get; }

            public UInt32 DisplayOrder { get; }

            public GridModeType GridType { get; }

            public double MinValue { get; }

            public double MaxValue { get; }

            public string LogTag { get; }
        }

        public class StringInfo
        {
            public StringInfo(string lang, Dictionary<string, string> stringDict)
            {
                Lang = lang;
                StringDict = stringDict;
            }

            public string Lang { get; }

            public Dictionary<string, string> StringDict { get; }
        }

        public class JobInfo
        {
            public JobInfo(string id, string sgbd, string name, string rawTelegrams, string fixedFuncStructId, string argsFirst, string args, string results, int argLimit)
            {
                Id = id;
                Sgbd = sgbd;
                Name = name;
                RawTelegrams = rawTelegrams;
                FixedFuncStructId = fixedFuncStructId;
                ArgsFirst = argsFirst;
                Args = args;
                Results = results;
                ArgLimit = argLimit;
                UseCompatIds = false;
            }

            public string Id { get; }

            public string Sgbd { get; }

            public string Name { get; }

            public string RawTelegrams { get; }

            public string FixedFuncStructId { get; }

            public string Args { get; }

            public string ArgsFirst { get; }

            public string Results { get; }

            public int ArgLimit { get; set; }

            public bool UseCompatIds { get; set; }
        }

        public class JobsInfo
        {
            public JobsInfo(string sgbd, string vagDataFileName, string vagUdsFileName, List<JobInfo> jobList)
            {
                Sgbd = sgbd;
                VagDataFileName = vagDataFileName;
                VagUdsFileName = vagUdsFileName;
                JobList = jobList;
            }

            public string Sgbd { get; }

            public string VagDataFileName { get; }

            public string VagUdsFileName { get; }

            public List<JobInfo> JobList { get; }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, string sgbd, string vagDataFileName, string vagUdsFileName, string results)
            {
                Name = name;
                Sgbd = sgbd;
                VagDataFileName = vagDataFileName;
                VagUdsFileName = vagUdsFileName;
                Results = results;
            }

            public string Name { get; }

            public string Sgbd { get; }

            public string VagDataFileName { get; }

            public string VagUdsFileName { get; }

            public string Results { get; }
        }

        public class ErrorsInfo
        {
            public ErrorsInfo(string sgbdFunctional, string vehicleSeries, string bnType, string brandName, List<EcuInfo> ecuList)
            {
                SgbdFunctional = sgbdFunctional;
                VehicleSeries = vehicleSeries;
                BnType = bnType;
                BrandName = brandName;
                EcuList = ecuList;
            }

            public string SgbdFunctional { get; }

            public string VehicleSeries { get; }

            public string BnType { get; }

            public string BrandName { get; }

            public List<EcuInfo> EcuList { get; }
        }

        public class PageInfo
        {
            public PageInfo(string xmlFileName, string name, float weight, DisplayModeType displayMode, int? textResId, int? gaugesPortrait, int? gaugesLandscape, string logFile, string dbName, bool jobActivate, string classCode, bool codeShowWarnings, JobsInfo jobsInfo, ErrorsInfo errorsInfo, List<DisplayInfo> displayList, List<StringInfo> stringList)
            {
                XmlFileName = xmlFileName;
                Name = name;
                Weight = weight;
                DisplayMode = displayMode;
                TextResId = textResId;
                GaugesPortrait = gaugesPortrait;
                GaugesLandscape = gaugesLandscape;
                LogFile = logFile;
                DbName = dbName;
                JobActivate = jobActivate;
                ClassCode = classCode;
                CodeShowWarnings = codeShowWarnings;
                JobsInfo = jobsInfo;
                ErrorsInfo = errorsInfo;
                DisplayList = displayList;
                StringList = stringList;

                string assetFileName = Path.GetFileName(ActivityCommon.AssetFileName) ?? string.Empty;
                UseCompatIds = string.Compare(dbName, assetFileName, StringComparison.OrdinalIgnoreCase) != 0;

                CompatIdsUsed = false;
                if (JobsInfo != null)
                {
                    foreach (JobInfo jobInfo in JobsInfo.JobList)
                    {
                        jobInfo.UseCompatIds = UseCompatIds;
                        if (UseCompatIds && !string.IsNullOrWhiteSpace(jobInfo.FixedFuncStructId))
                        {
                            CompatIdsUsed = true;
                            break;
                        }
                    }
                }

                InfoObject = null;
                ClassObject = null;
            }

            public enum DisplayModeType
            {
                List,
                Grid,
            }

            public string XmlFileName { get; }

            public string Name { get; }

            public float Weight { get; }

            public DisplayModeType DisplayMode { get; }

            public int? TextResId { get; }

            public int? GaugesPortrait { get; }

            public int GaugesPortraitValue => GaugesPortrait ?? GaugesPortraitDefault;

            public int? GaugesLandscape { get; }

            public int GaugesLandscapeValue => GaugesLandscape ?? GaugesLandscapeDefault;

            public string LogFile { get; }

            public string DbName { get; }

            public bool JobActivate { get; }

            public string ClassCode { get; }

            public bool CodeShowWarnings { get; }

            public JobsInfo JobsInfo { get; }

            public ErrorsInfo ErrorsInfo { get; }

            public List<DisplayInfo> DisplayList { get; }

            public List<StringInfo> StringList { get; }

            public bool UseCompatIds { get; }

            public bool CompatIdsUsed { get; }

            public object InfoObject { get; set; }

            public object ClassObject { get; set; }
        }

        class DisplayInfoComparer : IComparer<DisplayInfo>
        {
            public int Compare(DisplayInfo x, DisplayInfo y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                if (x.DisplayOrder > y.DisplayOrder)
                {
                    return 1;
                }

                if (x.DisplayOrder < y.DisplayOrder)
                {
                    return -1;
                }

                if (x.OriginalPosition > y.OriginalPosition)
                {
                    return 1;
                }

                if (x.OriginalPosition < y.OriginalPosition)
                {
                    return -1;
                }

                return 0;
            }
        }

        public const int GaugesPortraitDefault = 2;
        public const int GaugesLandscapeDefault = 4;
        private readonly List<PageInfo> _pageList = new List<PageInfo>();
        private PageInfo _errorPage = null;
        private string _ecuPath = string.Empty;
        private string _logPath = string.Empty;
        private bool _appendLog;
        private bool _logTagsPresent;
        private bool _compatIdsUsed;
        private string _sgbdFunctional = string.Empty;
        private string _interfaceName = string.Empty;
        private string _vehicleSeries = string.Empty;
        private string _bnType = string.Empty;
        private string _brandName = string.Empty;
        private string _manufacturerName = string.Empty;
        private string _xmlFileNamePages = string.Empty;
        private string _xmlFileName = string.Empty;
        private ActivityCommon.ManufacturerType _manufacturerType = ActivityCommon.ManufacturerType.Bmw;
        private ActivityCommon.InterfaceType _interfaceType = ActivityCommon.InterfaceType.None;

        public List<PageInfo> PageList => _pageList;

        public PageInfo ErrorPage => _errorPage;

        public string EcuPath => _ecuPath;

        public string LogPath => _logPath;

        public bool AppendLog => _appendLog;

        public bool LogTagsPresent => _logTagsPresent;

        public bool CompatIdsUsed => _compatIdsUsed;

        public string SgbdFunctional => _sgbdFunctional;

        public string ManufacturerName => _manufacturerName;

        public string InterfaceName => _interfaceName;

        public string VehicleSeries => _vehicleSeries;

        public string BnType => _bnType;

        public string BrandName => _brandName;

        public string XmlFileNamePages => _xmlFileNamePages;

        public string XmlFileName => _xmlFileName;

        public ActivityCommon.ManufacturerType Manufacturer => _manufacturerType;

        public ActivityCommon.InterfaceType Interface => _interfaceType;

        public bool IsMotorbike {
            get
            {
                if (!string.IsNullOrEmpty(_bnType))
                {
                    if (_bnType.Contains("MOTORBIKE", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(_brandName))
                {
                    if (_brandName.Contains("MOTORRAD", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public JobReader()
        {
        }

        public JobReader(string xmlName)
        {
            ReadXml(xmlName, out string _);
        }

        public void Clear()
        {
            _pageList.Clear();
            _errorPage = null;
            _ecuPath = string.Empty;
            _logPath = string.Empty;
            _logTagsPresent = false;
            _compatIdsUsed = false;
            _sgbdFunctional = string.Empty;
            _manufacturerName = string.Empty;
            _interfaceName = string.Empty;
            _vehicleSeries = string.Empty;
            _bnType = string.Empty;
            _brandName = string.Empty;
            _xmlFileNamePages = string.Empty;
            _xmlFileName = null;
        }

        public bool ReadXml(string xmlName, out string errorMessage)
        {
            errorMessage = null;
            Clear();
            if (string.IsNullOrEmpty(xmlName))
            {
                return false;
            }
            if (!File.Exists(xmlName))
            {
                return false;
            }
            string xmlDir = Path.GetDirectoryName(xmlName);

            try
            {
                string prefix = string.Empty;
                XmlDocument xdocConfig = XmlDocumentLoader.LoadWithIncludes(xmlName);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xdocConfig.NameTable);
                XPathNavigator xNav = xdocConfig.CreateNavigator();
                if (xNav.MoveToFollowing(XPathNodeType.Element))
                {
                    IDictionary<string, string> localNamespaces = xNav.GetNamespacesInScope(XmlNamespaceScope.Local);
                    if (localNamespaces.TryGetValue("", out string nameSpace))
                    {
                        namespaceManager.AddNamespace("carcontrol", nameSpace);
                        prefix = "carcontrol:";
                    }
                }

                XmlAttribute attrib;
                XmlNode xnodeGlobal = xdocConfig.SelectSingleNode(string.Format("/{0}configuration/{0}global", prefix), namespaceManager);
                if (xnodeGlobal?.Attributes != null)
                {
                    attrib = xnodeGlobal.Attributes["ecu_path"];
                    if (attrib != null)
                    {
                        if (Path.IsPathRooted(attrib.Value))
                        {
                            _ecuPath = attrib.Value;
                        }
                        else
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            _ecuPath = string.IsNullOrEmpty(xmlDir) ? attrib.Value : Path.Combine(xmlDir, attrib.Value);
                        }
                    }

                    attrib = xnodeGlobal.Attributes["log_path"];
                    if (attrib != null)
                    {
                        _logPath = attrib.Value;
                    }

                    attrib = xnodeGlobal.Attributes["append_log"];
                    if (attrib != null)
                    {
                        _appendLog = XmlConvert.ToBoolean(attrib.Value);
                    }

                    attrib = xnodeGlobal.Attributes["manufacturer"];
                    if (attrib != null)
                    {
                        _manufacturerName = attrib.Value;
                    }

                    attrib = xnodeGlobal.Attributes["interface"];
                    if (attrib != null)
                    {
                        _interfaceName = attrib.Value;
                    }

                    attrib = xnodeGlobal.Attributes["vehicle_series"];
                    if (attrib != null)
                    {
                        _vehicleSeries = attrib.Value;
                    }

                    attrib = xnodeGlobal.Attributes["bn_type"];
                    if (attrib != null)
                    {
                        _bnType = attrib.Value;
                    }

                    attrib = xnodeGlobal.Attributes["brand_name"];
                    if (attrib != null)
                    {
                        _brandName = attrib.Value;
                    }
                }

                if (string.Compare(_manufacturerName, "Audi", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _manufacturerType = ActivityCommon.ManufacturerType.Audi;
                }
                else if (string.Compare(_manufacturerName, "Seat", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _manufacturerType = ActivityCommon.ManufacturerType.Seat;
                }
                else if (string.Compare(_manufacturerName, "Skoda", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _manufacturerType = ActivityCommon.ManufacturerType.Skoda;
                }
                else if (string.Compare(_manufacturerName, "VW", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _manufacturerType = ActivityCommon.ManufacturerType.Vw;
                }
                else
                {
                    _manufacturerType = ActivityCommon.ManufacturerType.Bmw;
                }

                bool isBmw = _manufacturerType == ActivityCommon.ManufacturerType.Bmw;
                if (isBmw && string.Compare(_interfaceName, "ENET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _interfaceType = ActivityCommon.InterfaceType.Enet;
                }
                else if (isBmw && string.Compare(_interfaceName, "ELMWIFI", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _interfaceType = ActivityCommon.InterfaceType.ElmWifi;
                }
                else if (string.Compare(_interfaceName, "DEEPOBDWIFI", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _interfaceType = ActivityCommon.InterfaceType.DeepObdWifi;
                }
                else if (isBmw && string.Compare(_interfaceName, "FTDI", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _interfaceType = ActivityCommon.InterfaceType.Ftdi;
                }
                else
                {
                    _interfaceType = ActivityCommon.InterfaceType.Bluetooth;
                }

                XmlNode xnodePages = xdocConfig.SelectSingleNode(string.Format("/{0}configuration/{0}pages", prefix), namespaceManager);
                if (xnodePages?.Attributes != null)
                {
                    attrib = xnodePages.Attributes["include_filename"];
                    if (attrib != null) _xmlFileNamePages = attrib.Value;
                }

                XmlNodeList xnodesPage = xdocConfig.SelectNodes(string.Format("/{0}configuration/{0}pages/{0}page", prefix), namespaceManager);
                if (xnodesPage != null)
                {
                    foreach (XmlNode xnodePage in xnodesPage)
                    {
                        string pageName = string.Empty;
                        string xmlFileName = string.Empty;
                        float pageWeight = -1;
                        int? textResId = null;
                        int? gaugesPortrait = null;
                        int? gaugesLandscape = null;
                        PageInfo.DisplayModeType displayMode = PageInfo.DisplayModeType.List;
                        string logFile = string.Empty;
                        string dbName = string.Empty;
                        bool jobActivate = false;
                        if (xnodePage.Attributes != null)
                        {
                            attrib = xnodePage.Attributes["include_filename"];
                            if (attrib != null) xmlFileName = attrib.Value;

                            attrib = xnodePage.Attributes["name"];
                            if (attrib != null) pageName = attrib.Value;

                            attrib = xnodePage.Attributes["weight"];
                            if (attrib != null)
                            {
                                try
                                {
                                    pageWeight = XmlConvert.ToSingle(attrib.Value);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            attrib = xnodePage.Attributes["display-mode"];
                            if (attrib != null)
                            {
                                if (!Enum.TryParse(attrib.Value, true, out displayMode))
                                {
                                    displayMode = PageInfo.DisplayModeType.List;
                                }
                            }

                            attrib = xnodePage.Attributes["fontsize"];
                            if (attrib != null)
                            {
                                string size = attrib.Value.ToLowerInvariant();
                                switch (size)
                                {
                                    case "small":
                                        textResId = Android.Resource.Style.TextAppearanceSmall;
                                        break;

                                    case "medium":
                                        textResId = Android.Resource.Style.TextAppearanceMedium;
                                        break;

                                    case "large":
                                        textResId = Android.Resource.Style.TextAppearanceLarge;
                                        break;
                                }
                            }

                            attrib = xnodePage.Attributes["gauges-portrait"];
                            if (attrib != null)
                            {
                                try
                                {
                                    int gauges = XmlConvert.ToInt32(attrib.Value);
                                    if (gauges >= 1)
                                    {
                                        gaugesPortrait = gauges;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            attrib = xnodePage.Attributes["gauges-landscape"];
                            if (attrib != null)
                            {
                                try
                                {
                                    int gauges = XmlConvert.ToInt32(attrib.Value);
                                    if (gauges >= 1)
                                    {
                                        gaugesLandscape = gauges;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            attrib = xnodePage.Attributes["logfile"];
                            if (attrib != null) logFile = attrib.Value;

                            attrib = xnodePage.Attributes["db_name"];
                            if (attrib != null) dbName = attrib.Value;

                            attrib = xnodePage.Attributes["activate"];
                            if (attrib != null)
                            {
                                try
                                {
                                    jobActivate = XmlConvert.ToBoolean(attrib.Value);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }
                        }

                        JobsInfo jobsInfo = null;
                        ErrorsInfo errorsInfo = null;
                        List<DisplayInfo> displayList = new List<DisplayInfo>();
                        List<StringInfo> stringList = new List<StringInfo>();
                        bool logEnabled = false;
                        string classCode = null;
                        bool codeShowWarnings = false;
                        foreach (XmlNode xnodePageChild in xnodePage.ChildNodes)
                        {
                            ReadDisplayNode(xnodePageChild, displayList, null, isBmw, ref logEnabled);
                            if (string.Compare(xnodePageChild.Name, "strings", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string lang = null;
                                if (xnodePageChild.Attributes != null)
                                {
                                    attrib = xnodePageChild.Attributes["lang"];
                                    if (attrib != null) lang = attrib.Value;
                                }

                                Dictionary<string, string> stringDict = new Dictionary<string, string>();
                                foreach (XmlNode xnodeString in xnodePageChild.ChildNodes)
                                {
                                    string text = xnodeString.InnerText;
                                    string name = string.Empty;
                                    if (xnodeString.Attributes != null)
                                    {
                                        attrib = xnodeString.Attributes["name"];
                                        if (attrib != null) name = attrib.Value;
                                    }
                                    if (string.IsNullOrEmpty(name)) continue;
                                    if (!stringDict.ContainsKey(name))
                                    {
                                        stringDict.Add(name, text);
                                    }
                                }
                                stringList.Add(new StringInfo(lang, stringDict));
                            }
                            if (string.Compare(xnodePageChild.Name, "jobs", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string sgbd = null;
                                string vagDataFileName = null;
                                string vagUdsFileName = null;
                                List<JobInfo> jobList = new List<JobInfo>();
                                if (xnodePageChild.Attributes != null)
                                {
                                    attrib = xnodePageChild.Attributes["sgbd"];
                                    if (attrib != null) sgbd = attrib.Value;

                                    attrib = xnodePageChild.Attributes["vag_data_file"];
                                    if (attrib != null) vagDataFileName = attrib.Value;

                                    attrib = xnodePageChild.Attributes["vag_uds_file"];
                                    if (attrib != null) vagUdsFileName = attrib.Value;
                                }
                                foreach (XmlNode xnodeJobsChild in xnodePageChild.ChildNodes)
                                {
                                    if (string.Compare(xnodeJobsChild.Name, "job", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        string jobId = string.Empty;
                                        string jobSgbd = string.Empty;
                                        string jobName = string.Empty;
                                        string jobRawTelegrams = string.Empty;
                                        string jobFixedFuncStructId = string.Empty;
                                        string jobArgsFirst = string.Empty;
                                        string jobArgs = string.Empty;
                                        string jobResults = string.Empty;
                                        int argLimit = 0;
                                        if (xnodeJobsChild.Attributes != null)
                                        {
                                            attrib = xnodeJobsChild.Attributes["id"];
                                            if (attrib != null) jobId = attrib.Value;

                                            attrib = xnodeJobsChild.Attributes["sgbd"];
                                            if (attrib != null) jobSgbd = attrib.Value;

                                            attrib = xnodeJobsChild.Attributes["name"];
                                            if (attrib != null) jobName = attrib.Value;

                                            attrib = xnodeJobsChild.Attributes["raw_telegrams"];
                                            if (attrib != null) jobRawTelegrams = attrib.Value;

                                            if (isBmw)
                                            {
                                                attrib = xnodeJobsChild.Attributes["fixed_func_struct_id"];
                                                if (attrib != null) jobFixedFuncStructId = attrib.Value;
                                            }

                                            attrib = xnodeJobsChild.Attributes["args_first"];
                                            if (attrib != null) jobArgsFirst = attrib.Value;

                                            attrib = xnodeJobsChild.Attributes["args"];
                                            if (attrib != null) jobArgs = attrib.Value;

                                            attrib = xnodeJobsChild.Attributes["results"];
                                            if (attrib != null) jobResults = attrib.Value;

                                            attrib = xnodeJobsChild.Attributes["arg_limit"];
                                            if (attrib != null)
                                            {
                                                try
                                                {
                                                    argLimit = XmlConvert.ToInt32(attrib.Value);
                                                }
                                                catch
                                                {
                                                    // ignored
                                                }
                                            }
                                        }

                                        jobList.Add(new JobInfo(jobId, jobSgbd, jobName, jobRawTelegrams, jobFixedFuncStructId, jobArgsFirst, jobArgs, jobResults, argLimit));
                                        foreach (XmlNode xnodeJobChild in xnodeJobsChild.ChildNodes)
                                        {
                                            string nodePrefix = (string.IsNullOrEmpty(jobId) ? jobName : jobId);
                                            if (!string.IsNullOrWhiteSpace(jobFixedFuncStructId))
                                            {
                                                nodePrefix = jobFixedFuncStructId;
                                            }
                                            ReadDisplayNode(xnodeJobChild, displayList, nodePrefix + "#", isBmw, ref logEnabled);
                                        }
                                    }
                                }
                                jobsInfo = new JobsInfo(sgbd, vagDataFileName, vagUdsFileName, jobList);
                            }
                            if (string.Compare(xnodePageChild.Name, "read_errors", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string sgbdFunctional = string.Empty;
                                string vehicleSeries = string.Empty;
                                string bnType = string.Empty;
                                string brandName = string.Empty;

                                attrib = xnodePageChild.Attributes["sgbd_functional"];
                                if (attrib != null) sgbdFunctional = attrib.Value;

                                attrib = xnodePageChild.Attributes["vehicle_series"];
                                if (attrib != null) vehicleSeries = attrib.Value;

                                attrib = xnodePageChild.Attributes["bn_type"];
                                if (attrib != null) bnType = attrib.Value;

                                attrib = xnodePageChild.Attributes["brand_name"];
                                if (attrib != null) brandName = attrib.Value;

                                List<EcuInfo> ecuList = new List<EcuInfo>();
                                foreach (XmlNode xnodeErrorsChild in xnodePageChild.ChildNodes)
                                {
                                    if (string.Compare(xnodeErrorsChild.Name, "ecu", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        string ecuName = string.Empty;
                                        string sgbd = string.Empty;
                                        string vagDataFileName = null;
                                        string vagUdsFileName = null;
                                        string results = "F_UW_KM";
                                        if (xnodeErrorsChild.Attributes != null)
                                        {
                                            attrib = xnodeErrorsChild.Attributes["name"];
                                            if (attrib != null) ecuName = attrib.Value;
                                            attrib = xnodeErrorsChild.Attributes["sgbd"];
                                            if (attrib != null) sgbd = attrib.Value;
                                            attrib = xnodeErrorsChild.Attributes["vag_data_file"];
                                            if (attrib != null) vagDataFileName = attrib.Value;
                                            attrib = xnodeErrorsChild.Attributes["vag_uds_file"];
                                            if (attrib != null) vagUdsFileName = attrib.Value;
                                            attrib = xnodeErrorsChild.Attributes["results"];
                                            if (attrib != null) results = attrib.Value;
                                        }
                                        ecuList.Add(new EcuInfo(ecuName, sgbd, vagDataFileName, vagUdsFileName, results));
                                    }
                                }
                                errorsInfo = new ErrorsInfo(sgbdFunctional, vehicleSeries, bnType, brandName, ecuList);
                            }
                            if (string.Compare(xnodePageChild.Name, "code", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                classCode = xnodePageChild.InnerText;
                                attrib = xnodePageChild.Attributes["show_warnings"];
                                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                                if (attrib == null)
                                {
                                    // for backward compatibility
                                    attrib = xnodePageChild.Attributes["show_warnigs"];
                                }
                                if (attrib != null)
                                {
                                    try
                                    {
                                        codeShowWarnings = XmlConvert.ToBoolean(attrib.Value);
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                            }
                        }

                        if (!logEnabled)
                        {
                            logFile = string.Empty;
                        }

                        if (logEnabled)
                        {
                            _logTagsPresent = true;
                        }

                        if (string.IsNullOrEmpty(pageName))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(classCode))
                        {
                            classCode = null;
                        }

                        _pageList.Add(new PageInfo(xmlFileName, pageName, pageWeight, displayMode, textResId, gaugesPortrait, gaugesLandscape, logFile, dbName, jobActivate, classCode, codeShowWarnings, jobsInfo, errorsInfo, displayList, stringList));
                    }
                }

                foreach (PageInfo pageInfo in _pageList)
                {
                    if (pageInfo.CompatIdsUsed)
                    {
                        _compatIdsUsed = true;
                    }

                    if (pageInfo.ErrorsInfo != null)
                    {
                        _errorPage = pageInfo;

                        if (string.IsNullOrEmpty(_vehicleSeries))
                        {
                            _vehicleSeries = pageInfo.ErrorsInfo.VehicleSeries;
                        }

                        if (string.IsNullOrEmpty(_sgbdFunctional))
                        {
                            _sgbdFunctional = pageInfo.ErrorsInfo.SgbdFunctional;
                        }

                        if (string.IsNullOrEmpty(_bnType))
                        {
                            _bnType = pageInfo.ErrorsInfo.BnType;
                        }

                        if (string.IsNullOrEmpty(_brandName))
                        {
                            _brandName = pageInfo.ErrorsInfo.BrandName;
                        }
                    }
                }

                _xmlFileName = xmlName;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = string.Empty;
                string fileName = Path.GetFileName(xmlName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    errorMessage = fileName + ":\r\n";
                }
                errorMessage += EdiabasNet.GetExceptionText(ex) ?? string.Empty;
                return false;
            }
        }

        private void ReadDisplayNode(XmlNode xmlNode, List<DisplayInfo> displayList, string prefix, bool isBmw, ref bool logEnabled)
        {
            if (string.Compare(xmlNode.Name, "display", StringComparison.OrdinalIgnoreCase) == 0)
            {
                string name = string.Empty;
                string result = string.Empty;
                string ecuJobId = string.Empty;
                string ecuJobResultId = string.Empty;
                string format = null;
                UInt32 displayOrder = 0;
                DisplayInfo.GridModeType gridType = DisplayInfo.GridModeType.Hidden;
                double minValue = 0;
                double maxValue = 100;
                string logTag = string.Empty;
                if (xmlNode.Attributes != null)
                {
                    XmlAttribute attrib = xmlNode.Attributes["name"];
                    if (attrib != null) name = attrib.Value;

                    attrib = xmlNode.Attributes["result"];
                    if (attrib != null) result = attrib.Value;

                    if (isBmw)
                    {
                        attrib = xmlNode.Attributes["ecu_job_id"];
                        if (attrib != null) ecuJobId = attrib.Value;

                        attrib = xmlNode.Attributes["ecu_job_result_id"];
                        if (attrib != null) ecuJobResultId = attrib.Value;
                    }

                    attrib = xmlNode.Attributes["format"];
                    if (attrib != null) format = attrib.Value;

                    attrib = xmlNode.Attributes["display-order"];
                    if (attrib != null)
                    {
                        try
                        {
                            displayOrder = XmlConvert.ToUInt32(attrib.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    attrib = xmlNode.Attributes["grid-type"];
                    if (attrib != null)
                    {
                        if (!Enum.TryParse(attrib.Value.Replace("-", "_"), true, out gridType))
                        {
                            gridType = DisplayInfo.GridModeType.Hidden;
                        }
                    }

                    attrib = xmlNode.Attributes["min-value"];
                    if (attrib != null)
                    {
                        try
                        {
                            minValue = XmlConvert.ToDouble(attrib.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    attrib = xmlNode.Attributes["max-value"];
                    if (attrib != null)
                    {
                        try
                        {
                            maxValue = XmlConvert.ToDouble(attrib.Value);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                    attrib = xmlNode.Attributes["log_tag"];
                    if (attrib != null) logTag = attrib.Value;
                    if (!string.IsNullOrEmpty(logTag)) logEnabled = true;

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(result)) return;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        if (!string.IsNullOrWhiteSpace(ecuJobId) && !string.IsNullOrWhiteSpace(ecuJobResultId))
                        {
                            result = prefix + ecuJobId + "#" + ecuJobResultId;
                        }
                        else
                        {
                            result = prefix + result;
                        }
                    }
                    displayList.Add(new DisplayInfo(displayList.Count, name, result, ecuJobId, ecuJobResultId, format, displayOrder, gridType, minValue, maxValue, logTag));
                }

                DisplayInfoComparer dic = new DisplayInfoComparer();
                displayList.Sort(dic);
            }
        }
    }
}
