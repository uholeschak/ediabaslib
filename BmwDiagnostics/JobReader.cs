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
            public DisplayInfo(string name, string result, string format, string logTag)
            {
                _name = name;
                _result = result;
                _format = format;
                _logTag = logTag;
            }

            private readonly string _name;
            private readonly string _result;
            private readonly string _format;
            private readonly string _logTag;

            public string Name
            {
                get { return _name; }
            }

            public string Result
            {
                get { return _result; }
            }

            public string Format
            {
                get { return _format; }
            }

            public string LogTag
            {
                get { return _logTag; }
            }
        }

        public class StringInfo
        {
            public StringInfo(string lang, Dictionary<string, string> stringDict)
            {
                _lang = lang;
                _stringDict = stringDict;
            }

            private readonly string _lang;
            private readonly Dictionary<string, string> _stringDict;

            public string Lang
            {
                get { return _lang; }
            }

            public Dictionary<string, string> StringDict
            {
                get { return _stringDict; }
            }
        }

        public class JobInfo
        {
            public JobInfo(string name, string argsFirst, string args, string results)
            {
                _name = name;
                _argsFirst = argsFirst;
                _args = args;
                _results = results;
            }

            private readonly string _name;
            private readonly string _argsFirst;
            private readonly string _args;
            private readonly string _results;

            public string Name
            {
                get { return _name; }
            }

            public string Args
            {
                get { return _args; }
            }

            public string ArgsFirst
            {
                get { return _argsFirst; }
            }

            public string Results
            {
                get { return _results; }
            }
        }

        public class JobsInfo
        {
            public JobsInfo(string sgbd, List<JobInfo> jobList)
            {
                _sgbd = sgbd;
                _jobList = jobList;
            }

            private readonly string _sgbd;
            private readonly List<JobInfo> _jobList;

            public string Sgbd
            {
                get { return _sgbd; }
            }

            public List<JobInfo> JobList
            {
                get { return _jobList; }
            }
        }

        public class EcuInfo
        {
            public EcuInfo(string name, string sgbd, string results)
            {
                _name = name;
                _sgbd = sgbd;
                _results = results;
            }

            private readonly string _name;
            private readonly string _sgbd;
            private readonly string _results;

            public string Name
            {
                get { return _name; }
            }

            public string Sgbd
            {
                get { return _sgbd; }
            }

            public string Results
            {
                get { return _results; }
            }
        }

        public class ErrorsInfo
        {
            public ErrorsInfo(List<EcuInfo> ecuList)
            {
                _ecuList = ecuList;
            }

            private readonly List<EcuInfo> _ecuList;

            public List<EcuInfo> EcuList
            {
                get { return _ecuList; }
            }
        }

        public class PageInfo
        {
            public PageInfo(string name, float weight, string logFile, bool jobActivate, string classCode, bool codeShowWarnings, JobsInfo jobsInfo, ErrorsInfo errorsInfo, List<DisplayInfo> displayList, List<StringInfo> stringList)
            {
                _name = name;
                _weight = weight;
                _logFile = logFile;
                _jobActivate = jobActivate;
                _classCode = classCode;
                _codeShowWarnings = codeShowWarnings;
                _jobsInfo = jobsInfo;
                _errorsInfo = errorsInfo;
                _displayList = displayList;
                _stringList = stringList;
                InfoObject = null;
                ClassObject = null;
            }

            private readonly string _name;
            private readonly float _weight;
            private readonly string _logFile;
            private readonly bool _jobActivate;
            private readonly string _classCode;
            private readonly bool _codeShowWarnings;
            private readonly JobsInfo _jobsInfo;
            private readonly ErrorsInfo _errorsInfo;
            private readonly List<DisplayInfo> _displayList;
            private readonly List<StringInfo> _stringList;

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public float Weight
            {
                get
                {
                    return _weight;
                }
            }

            public string LogFile
            {
                get
                {
                    return _logFile;
                }
            }

            public bool JobActivate
            {
                get
                {
                    return _jobActivate;
                }
            }

            public string ClassCode
            {
                get
                {
                    return _classCode;
                }
            }

            public bool CodeShowWarnings
            {
                get
                {
                    return _codeShowWarnings;
                }
            }

            public JobsInfo JobsInfo
            {
                get
                {
                    return _jobsInfo;
                }
            }

            public ErrorsInfo ErrorsInfo
            {
                get
                {
                    return _errorsInfo;
                }
            }

            public List<DisplayInfo> DisplayList
            {
                get
                {
                    return _displayList;
                }
            }

            public List<StringInfo> StringList
            {
                get
                {
                    return _stringList;
                }
            }

            public object InfoObject { get; set; }

            public dynamic ClassObject { get; set; }
        }

        private readonly List<PageInfo> _pageList = new List<PageInfo>();
        private string _ecuPath = string.Empty;
        private string _logPath = string.Empty;
        private bool _appendLog;
        private string _interfaceName = string.Empty;
        private ActivityCommon.InterfaceType _interfaceType = ActivityCommon.InterfaceType.None;

        public List<PageInfo> PageList
        {
            get
            {
                return _pageList;
            }
        }

        public string EcuPath
        {
            get
            {
                return _ecuPath;
            }
        }

        public string LogPath
        {
            get
            {
                return _logPath;
            }
        }

        public bool AppendLog
        {
            get
            {
                return _appendLog;
            }
        }

        public string InterfaceName
        {
            get
            {
                return _interfaceName;
            }
        }

        public ActivityCommon.InterfaceType Interface
        {
            get
            {
                return _interfaceType;
            }
        }

        public JobReader()
        {
        }

        public JobReader(string xmlName)
        {
            ReadXml(xmlName);
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
                    string nameSpace;
                    if (localNamespaces.TryGetValue("", out nameSpace))
                    {
                        namespaceManager.AddNamespace("carcontrol", nameSpace);
                        prefix = "carcontrol:";
                    }
                }

                XmlAttribute attrib;
                XmlNode xnodeGlobal = xdocConfig.SelectSingleNode(string.Format("/{0}configuration/{0}global", prefix), namespaceManager);
                if (xnodeGlobal != null)
                {
                    if (xnodeGlobal.Attributes != null)
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

                        attrib = xnodeGlobal.Attributes["interface"];
                        if (attrib != null)
                        {
                            _interfaceName = attrib.Value;
                        }
                    }
                }

                if (string.Compare(_interfaceName, "ENET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _interfaceType = ActivityCommon.InterfaceType.Enet;
                }
                else if (string.Compare(_interfaceName, "FTDI", StringComparison.OrdinalIgnoreCase) == 0)
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
                            attrib = xnodePage.Attributes["logfile"];
                            if (attrib != null) logFile = attrib.Value;
                            attrib = xnodePage.Attributes["activate"];
                            if (attrib != null) jobActivate = XmlConvert.ToBoolean(attrib.Value);
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
                                        string jobName = string.Empty;
                                        string jobArgsFirst = string.Empty;
                                        string jobArgs = string.Empty;
                                        string jobResults = string.Empty;
                                        if (xnodeJobsChild.Attributes != null)
                                        {
                                            attrib = xnodeJobsChild.Attributes["name"];
                                            if (attrib != null) jobName = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["args_first"];
                                            if (attrib != null) jobArgsFirst = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["args"];
                                            if (attrib != null) jobArgs = attrib.Value;
                                            attrib = xnodeJobsChild.Attributes["results"];
                                            if (attrib != null) jobResults = attrib.Value;
                                        }
                                        jobList.Add(new JobInfo(jobName, jobArgsFirst, jobArgs, jobResults));
                                        foreach (XmlNode xnodeJobChild in xnodeJobsChild.ChildNodes)
                                        {
                                            ReadDisplayNode(xnodeJobChild, displayList, jobName + "#", ref logEnabled);
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
                                if (attrib != null) codeShowWarnings = XmlConvert.ToBoolean(attrib.Value);
                            }
                        }
                        if (!logEnabled) logFile = string.Empty;
                        if (string.IsNullOrEmpty(pageName)) continue;
                        if (string.IsNullOrWhiteSpace(classCode)) classCode = null;

                        _pageList.Add(new PageInfo(pageName, pageWeight, logFile, jobActivate, classCode, codeShowWarnings, jobsInfo, errorsInfo, displayList, stringList));
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
                string logTag = string.Empty;
                if (xmlNode.Attributes != null)
                {
                    XmlAttribute attrib = xmlNode.Attributes["name"];
                    if (attrib != null) name = attrib.Value;
                    attrib = xmlNode.Attributes["result"];
                    if (attrib != null) result = attrib.Value;
                    attrib = xmlNode.Attributes["format"];
                    if (attrib != null) format = attrib.Value;
                    attrib = xmlNode.Attributes["log_tag"];
                    if (attrib != null) logTag = attrib.Value;
                    if (!string.IsNullOrEmpty(logTag)) logEnabled = true;

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(result)) return;
                    if (!string.IsNullOrEmpty(prefix))
                    {
                        result = prefix + result;
                    }
                    displayList.Add(new DisplayInfo(name, result, format, logTag));
                }
            }
        }
    }
}
