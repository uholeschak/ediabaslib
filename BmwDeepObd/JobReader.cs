using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace BmwDeepObd
{
    public class JobReader
    {
        public class DisplayInfo
        {
            public DisplayInfo(string name, string result, string format, GridModeType gridType, double minValue, double maxValue, string logTag)
            {
                Name = name;
                Result = result;
                Format = format;
                GridType = gridType;
                MinValue = minValue;
                MaxValue = maxValue;
                LogTag = logTag;
            }

            public enum GridModeType
            {
                Hidden,
                // ReSharper disable InconsistentNaming
                Simple_Gauge_Square,
                Simple_Gauge_Round,
                Simple_Gauge_Dot,
                // ReSharper restore InconsistentNaming
            }

            public string Name { get; }

            public string Result { get; }

            public string Format { get; }

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
            public JobInfo(string id, string sgbd, string name, string argsFirst, string args, string results)
            {
                Id = id;
                Sgbd = sgbd;
                Name = name;
                ArgsFirst = argsFirst;
                Args = args;
                Results = results;
            }

            public string Id { get; }

            public string Sgbd { get; }

            public string Name { get; }

            public string Args { get; }

            public string ArgsFirst { get; }

            public string Results { get; }
        }

        public class JobsInfo
        {
            public JobsInfo(string sgbd, List<JobInfo> jobList)
            {
                Sgbd = sgbd;
                JobList = jobList;
            }

            public string Sgbd { get; }

            public List<JobInfo> JobList { get; }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, string sgbd, string results)
            {
                Name = name;
                Sgbd = sgbd;
                Results = results;
            }

            public string Name { get; }

            public string Sgbd { get; }

            public string Results { get; }
        }

        public class ErrorsInfo
        {
            public ErrorsInfo(List<EcuInfo> ecuList)
            {
                EcuList = ecuList;
            }

            public List<EcuInfo> EcuList { get; }
        }

        public class PageInfo
        {
            public PageInfo(string name, float weight, DisplayModeType displayMode, int textResId, int gaugesPortrait, int gaugesLandscape, string logFile, bool jobActivate, string classCode, bool codeShowWarnings, JobsInfo jobsInfo, ErrorsInfo errorsInfo, List<DisplayInfo> displayList, List<StringInfo> stringList)
            {
                Name = name;
                Weight = weight;
                DisplayMode = displayMode;
                TextResId = textResId;
                GaugesPortrait = gaugesPortrait;
                GaugesLandscape = gaugesLandscape;
                LogFile = logFile;
                JobActivate = jobActivate;
                ClassCode = classCode;
                CodeShowWarnings = codeShowWarnings;
                JobsInfo = jobsInfo;
                ErrorsInfo = errorsInfo;
                DisplayList = displayList;
                StringList = stringList;
                InfoObject = null;
                ClassObject = null;
            }

            public enum DisplayModeType
            {
                List,
                Grid,
            }

            public string Name { get; }

            public float Weight { get; }

            public DisplayModeType DisplayMode { get; }

            public int TextResId { get; }

            public int GaugesPortrait { get; }

            public int GaugesLandscape { get; }

            public string LogFile { get; }

            public bool JobActivate { get; }

            public string ClassCode { get; }

            public bool CodeShowWarnings { get; }

            public JobsInfo JobsInfo { get; }

            public ErrorsInfo ErrorsInfo { get; }

            public List<DisplayInfo> DisplayList { get; }

            public List<StringInfo> StringList { get; }

            public object InfoObject { get; set; }

            public object ClassObject { get; set; }
        }

        private readonly List<PageInfo> _pageList = new List<PageInfo>();
        private string _ecuPath = string.Empty;
        private string _logPath = string.Empty;
        private bool _appendLog;
        private string _interfaceName = string.Empty;
        private string _manufacturerName = string.Empty;
        private ActivityCommon.ManufacturerType _manufacturerType = ActivityCommon.ManufacturerType.Bmw;
        private ActivityCommon.InterfaceType _interfaceType = ActivityCommon.InterfaceType.None;

        public List<PageInfo> PageList => _pageList;

        public string EcuPath => _ecuPath;

        public string LogPath => _logPath;

        public bool AppendLog => _appendLog;

        public string ManufacturerName => _manufacturerName;

        public string InterfaceName => _interfaceName;

        public ActivityCommon.ManufacturerType Manufacturer => _manufacturerType;

        public ActivityCommon.InterfaceType Interface => _interfaceType;

        public JobReader()
        {
        }

        public JobReader(string xmlName)
        {
            ReadXml(xmlName);
        }

        public void Clear()
        {
            _pageList.Clear();
        }

        public bool ReadXml(string xmlName)
        {
            _pageList.Clear();
            if (string.IsNullOrEmpty(xmlName))
            {
                return false;
            }
            if (!File.Exists(xmlName))
            {
                return false;
            }
            string xmlDir = Path.GetDirectoryName(xmlName);
            _ecuPath = string.Empty;
            _logPath = string.Empty;
            _manufacturerName = string.Empty;
            _interfaceName = string.Empty;

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

                XmlNodeList xnodePages = xdocConfig.SelectNodes(string.Format("/{0}configuration/{0}pages/{0}page", prefix), namespaceManager);
                if (xnodePages != null)
                {
                    foreach (XmlNode xnodePage in xnodePages)
                    {
                        string pageName = string.Empty;
                        float pageWeight = -1;
                        int textResId = 0;
                        int gaugesLandscape = 4;
                        int gaugesPortrait = 2;
                        PageInfo.DisplayModeType displayMode = PageInfo.DisplayModeType.List;
                        string logFile = string.Empty;
                        bool jobActivate = false;
                        if (xnodePage.Attributes != null)
                        {
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
                            attrib = xnodePage.Attributes["gauges_portrait"];
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
                            attrib = xnodePage.Attributes["gauges_landscape"];
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
                            ReadDisplayNode(xnodePageChild, displayList, null, ref logEnabled);
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
                                List<JobInfo> jobList = new List<JobInfo>();
                                if (xnodePageChild.Attributes != null)
                                {
                                    attrib = xnodePageChild.Attributes["sgbd"];
                                    if (attrib != null) sgbd = attrib.Value;
                                }
                                foreach (XmlNode xnodeJobsChild in xnodePageChild.ChildNodes)
                                {
                                    if (string.Compare(xnodeJobsChild.Name, "job", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        string jobId = string.Empty;
                                        string jobSgbd = string.Empty;
                                        string jobName = string.Empty;
                                        string jobArgsFirst = string.Empty;
                                        string jobArgs = string.Empty;
                                        string jobResults = string.Empty;
                                        if (xnodeJobsChild.Attributes != null)
                                        {
                                            attrib = xnodeJobsChild.Attributes["id"];
                                            if (attrib != null) jobId = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["sgbd"];
                                            if (attrib != null) jobSgbd = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["name"];
                                            if (attrib != null) jobName = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["args_first"];
                                            if (attrib != null) jobArgsFirst = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["args"];
                                            if (attrib != null) jobArgs = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["results"];
                                            if (attrib != null) jobResults = attrib.Value;
                                        }
                                        jobList.Add(new JobInfo(jobId, jobSgbd, jobName, jobArgsFirst, jobArgs, jobResults));
                                        foreach (XmlNode xnodeJobChild in xnodeJobsChild.ChildNodes)
                                        {
                                            ReadDisplayNode(xnodeJobChild, displayList, (string.IsNullOrEmpty(jobId) ? jobName : jobId) + "#", ref logEnabled);
                                        }
                                    }
                                }
                                jobsInfo = new JobsInfo(sgbd, jobList);
                            }
                            if (string.Compare(xnodePageChild.Name, "read_errors", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                List<EcuInfo> ecuList = new List<EcuInfo>();
                                foreach (XmlNode xnodeErrorsChild in xnodePageChild.ChildNodes)
                                {
                                    if (string.Compare(xnodeErrorsChild.Name, "ecu", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        string ecuName = string.Empty;
                                        string sgbd = string.Empty;
                                        string results = "F_UW_KM";
                                        if (xnodeErrorsChild.Attributes != null)
                                        {
                                            attrib = xnodeErrorsChild.Attributes["name"];
                                            if (attrib != null) ecuName = attrib.Value;
                                            attrib = xnodeErrorsChild.Attributes["sgbd"];
                                            if (attrib != null) sgbd = attrib.Value;
                                            attrib = xnodeErrorsChild.Attributes["results"];
                                            if (attrib != null) results = attrib.Value;
                                        }
                                        ecuList.Add(new EcuInfo(ecuName, sgbd, results));
                                    }
                                }
                                errorsInfo = new ErrorsInfo(ecuList);
                            }
                            if (string.Compare(xnodePageChild.Name, "code", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                classCode = xnodePageChild.InnerText;
                                attrib = xnodePageChild.Attributes["show_warnigs"];
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
                        if (!logEnabled) logFile = string.Empty;
                        if (string.IsNullOrEmpty(pageName)) continue;
                        if (string.IsNullOrWhiteSpace(classCode)) classCode = null;

                        _pageList.Add(new PageInfo(pageName, pageWeight, displayMode, textResId, gaugesPortrait, gaugesLandscape, logFile, jobActivate, classCode, codeShowWarnings, jobsInfo, errorsInfo, displayList, stringList));
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReadDisplayNode(XmlNode xmlNode, List<DisplayInfo> displayList, string prefix, ref bool logEnabled)
        {
            if (string.Compare(xmlNode.Name, "display", StringComparison.OrdinalIgnoreCase) == 0)
            {
                string name = string.Empty;
                string result = string.Empty;
                string format = null;
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
                    attrib = xmlNode.Attributes["format"];
                    if (attrib != null) format = attrib.Value;
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
                        result = prefix + result;
                    }
                    displayList.Add(new DisplayInfo(name, result, format, gridType, minValue, maxValue, logTag));
                }
            }
        }
    }
}
