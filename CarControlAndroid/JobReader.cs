using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace CarControlAndroid
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
                get
                {
                    return _name;
                }
            }

            public string Result
            {
                get
                {
                    return _result;
                }
            }

            public string Format
            {
                get
                {
                    return _format;
                }
            }

            public string LogTag
            {
                get
                {
                    return _logTag;
                }
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
                get
                {
                    return _lang;
                }
            }

            public Dictionary<string, string> StringDict
            {
                get
                {
                    return _stringDict;
                }
            }
        }

        public class JobInfo
        {
            public JobInfo(string sgbd, bool activate, bool showWarnings, string name, string args, string results, string classCode)
            {
                _sgbd = sgbd;
                _activate = activate;
                _showWarnings = showWarnings;
                _name = name;
                _args = args;
                _results = results;
                _classCode = classCode;
            }

            private readonly string _sgbd;
            private readonly bool _activate;
            private readonly bool _showWarnings;
            private readonly string _name;
            private readonly string _args;
            private readonly string _results;
            private readonly string _classCode;

            public string Sgbd
            {
                get
                {
                    return _sgbd;
                }
            }

            public bool Activate
            {
                get
                {
                    return _activate;
                }
            }

            public bool ShowWarnings
            {
                get
                {
                    return _showWarnings;
                }
            }

            public string Name
            {
                get
                {
                    return _name;
                }
            }

            public string Args
            {
                get
                {
                    return _args;
                }
            }

            public string Results
            {
                get
                {
                    return _results;
                }
            }

            public string ClassCode
            {
                get
                {
                    return _classCode;
                }
            }
        }

        public class PageInfo
        {
            public PageInfo(string name, float weight, string logFile, JobInfo jobInfo, List<DisplayInfo> displayList, List<StringInfo> stringList)
            {
                _name = name;
                _weight = weight;
                _logFile = logFile;
                _jobInfo = jobInfo;
                _displayList = displayList;
                _stringList = stringList;
                InfoObject = null;
                ClassObject = null;
            }

            private readonly string _name;
            private readonly float _weight;
            private readonly string _logFile;
            private readonly JobInfo _jobInfo;
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

            public JobInfo JobInfo
            {
                get
                {
                    return _jobInfo;
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
            _ecuPath = Path.GetDirectoryName(xmlName);
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
                                _ecuPath = string.IsNullOrEmpty(_ecuPath) ? attrib.Value : Path.Combine(_ecuPath, attrib.Value);
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

                _interfaceType = ActivityCommon.InterfaceType.Bluetooth;
                if (string.Compare(_interfaceName, "ENET", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _interfaceType = ActivityCommon.InterfaceType.Enet;
                }

                XmlNodeList xnodePages = xdocConfig.SelectNodes(string.Format("/{0}configuration/{0}pages/{0}page", prefix), namespaceManager);
                if (xnodePages != null)
                {
                    foreach (XmlNode xnodePage in xnodePages)
                    {
                        string pageName = string.Empty;
                        float pageWeight = -1;
                        string logFile = string.Empty;
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
                        }

                        JobInfo jobInfo = null;
                        List<DisplayInfo> displayList = new List<DisplayInfo>();
                        List<StringInfo> stringList = new List<StringInfo>();
                        bool logEnabled = false;
                        foreach (XmlNode xnodePageChild in xnodePage.ChildNodes)
                        {
                            if (string.Compare(xnodePageChild.Name, "job", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string sgbd = null;
                                bool jobActivate = false;
                                bool jobShowWarnings = false;
                                string jobName = null;
                                string jobArgs = string.Empty;
                                string jobResults = string.Empty;
                                string classCode = xnodePageChild.InnerText;
                                if (string.IsNullOrWhiteSpace(classCode)) classCode = null;
                                if (xnodePageChild.Attributes != null)
                                {
                                    attrib = xnodePageChild.Attributes["sgbd"];
                                    if (attrib != null) sgbd = attrib.Value;
                                    attrib = xnodePageChild.Attributes["activate"];
                                    if (attrib != null) jobActivate = XmlConvert.ToBoolean(attrib.Value);
                                    attrib = xnodePageChild.Attributes["show_warnigs"];
                                    if (attrib != null) jobShowWarnings = XmlConvert.ToBoolean(attrib.Value);
                                    attrib = xnodePageChild.Attributes["name"];
                                    if (attrib != null) jobName = attrib.Value;
                                    attrib = xnodePageChild.Attributes["args"];
                                    if (attrib != null) jobArgs = attrib.Value;
                                    attrib = xnodePageChild.Attributes["results"];
                                    if (attrib != null) jobResults = attrib.Value;
                                }
                                if (classCode == null && string.IsNullOrEmpty(jobName)) continue;
                                jobInfo = new JobInfo(sgbd, jobActivate, jobShowWarnings, jobName, jobArgs, jobResults, classCode);
                            }
                            if (string.Compare(xnodePageChild.Name, "display", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string name = string.Empty;
                                string result = string.Empty;
                                string format = null;
                                string logTag = string.Empty;
                                if (xnodePageChild.Attributes != null)
                                {
                                    attrib = xnodePageChild.Attributes["name"];
                                    if (attrib != null) name = attrib.Value;
                                    attrib = xnodePageChild.Attributes["result"];
                                    if (attrib != null) result = attrib.Value;
                                    attrib = xnodePageChild.Attributes["format"];
                                    if (attrib != null) format = attrib.Value;
                                    attrib = xnodePageChild.Attributes["log_tag"];
                                    if (attrib != null) logTag = attrib.Value;
                                    if (!string.IsNullOrEmpty(logTag)) logEnabled = true;

                                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(result)) continue;
                                    displayList.Add(new DisplayInfo(name, result, format, logTag));
                                }
                            }
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
                        }
                        if (!logEnabled) logFile = string.Empty;
                        if (string.IsNullOrEmpty(pageName) || (jobInfo == null)) continue;

                        _pageList.Add(new PageInfo(pageName, pageWeight, logFile, jobInfo, displayList, stringList));
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
