using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using XmlGenSharp.Lib.Utility;

namespace CarControl
{
    public class JobReader
    {
        public class DisplayInfo
        {
            public DisplayInfo(string name, string result, string format)
            {
                this.name = name;
                this.result = result;
                this.format = format;
            }

            private string name;
            private string result;
            private string format;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public string Result
            {
                get
                {
                    return result;
                }
            }

            public string Format
            {
                get
                {
                    return format;
                }
            }
        }

        public class StringInfo
        {
            public StringInfo(string lang, Dictionary<string, string> stringDict)
            {
                this.lang = lang;
                this.stringDict = stringDict;
            }

            private string lang;
            private Dictionary<string, string> stringDict;

            public string Lang
            {
                get
                {
                    return lang;
                }
            }

            public Dictionary<string, string> StringDict
            {
                get
                {
                    return stringDict;
                }
            }
        }

        public class JobInfo
        {
            public JobInfo(string sgbd, bool activate, bool showWarnings, string name, string args, string results, string classCode)
            {
                this.sgbd = sgbd;
                this.activate = activate;
                this.showWarnings = showWarnings;
                this.name = name;
                this.args = args;
                this.results = results;
                this.classCode = classCode;
            }

            private string sgbd;
            private bool activate;
            private bool showWarnings;
            private string name;
            private string args;
            private string results;
            private string classCode;

            public string Sgbd
            {
                get
                {
                    return sgbd;
                }
            }

            public bool Activate
            {
                get
                {
                    return activate;
                }
            }

            public bool ShowWarnings
            {
                get
                {
                    return showWarnings;
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public string Args
            {
                get
                {
                    return args;
                }
            }

            public string Results
            {
                get
                {
                    return results;
                }
            }

            public string ClassCode
            {
                get
                {
                    return classCode;
                }
            }
        }

        public class PageInfo
        {
            public PageInfo(string name, float weight, JobInfo jobInfo, List<DisplayInfo> displayList, List<StringInfo> stringList)
            {
                this.name = name;
                this.weight = weight;
                this.jobInfo = jobInfo;
                this.displayList = displayList;
                this.stringList = stringList;
                this.infoObject = null;
                this.classObject = null;
            }

            private string name;
            private float weight;
            private JobInfo jobInfo;
            private List<DisplayInfo> displayList;
            private List<StringInfo> stringList;
            private object infoObject;
            private Evaluator eval;
            private dynamic classObject;

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public float Weight
            {
                get
                {
                    return weight;
                }
            }

            public JobInfo JobInfo
            {
                get
                {
                    return jobInfo;
                }
            }

            public List<DisplayInfo> DisplayList
            {
                get
                {
                    return displayList;
                }
            }

            public List<StringInfo> StringList
            {
                get
                {
                    return stringList;
                }
            }

            public object InfoObject
            {
                get
                {
                    return infoObject;
                }
                set
                {
                    infoObject = value;
                }
            }

            public Evaluator Eval
            {
                get
                {
                    return eval;
                }
                set
                {
                    eval = value;
                }
            }

            public dynamic ClassObject
            {
                get
                {
                    return classObject;
                }
                set
                {
                    classObject = value;
                }
            }
        }

        private List<PageInfo> pageList = new List<PageInfo>();
        private string ecuPath = string.Empty;

        public List<PageInfo> PageList
        {
            get
            {
                return pageList;
            }
        }

        public string EcuPath
        {
            get
            {
                return ecuPath;
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
            pageList.Clear();
            if (string.IsNullOrEmpty(xmlName))
            {
                return false;
            }
            if (!File.Exists(xmlName))
            {
                return false;
            }
            ecuPath = Path.GetDirectoryName(xmlName);

            try
            {
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
                    }
                }

                XmlAttribute attrib;
                XmlNode xnodeGlobal = xdocConfig.SelectSingleNode("/carcontrol:configuration/carcontrol:global", namespaceManager);
                if (xnodeGlobal != null)
                {
                    if (xnodeGlobal.Attributes != null)
                    {
                        attrib = xnodeGlobal.Attributes["ecu_path"];
                        if (attrib != null)
                        {
                            if (Path.IsPathRooted(attrib.Value))
                            {
                                ecuPath = attrib.Value;
                            }
                            else
                            {
                                ecuPath = Path.Combine(ecuPath, attrib.Value);
                            }
                        }
                    }
                }

                XmlNodeList xnodePages = xdocConfig.SelectNodes("/carcontrol:configuration/carcontrol:pages/carcontrol:page", namespaceManager);
                if (xnodePages != null)
                {
                    foreach (XmlNode xnodePage in xnodePages)
                    {
                        string pageName = string.Empty;
                        float pageWeight = -1;
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
                                catch { }
                            }
                        }

                        JobInfo jobInfo = null;
                        List<DisplayInfo> displayList = new List<DisplayInfo>();
                        List<StringInfo> stringList = new List<StringInfo>();
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
                                if (xnodePageChild.Attributes != null)
                                {
                                    attrib = xnodePageChild.Attributes["name"];
                                    if (attrib != null) name = attrib.Value;
                                    attrib = xnodePageChild.Attributes["result"];
                                    if (attrib != null) result = attrib.Value;
                                    attrib = xnodePageChild.Attributes["format"];
                                    if (attrib != null) format = attrib.Value;

                                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(result)) continue;
                                    displayList.Add (new DisplayInfo (name, result, format));
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
                        if (string.IsNullOrEmpty(pageName) || (jobInfo == null)) continue;

                        pageList.Add(new PageInfo(pageName, pageWeight, jobInfo, displayList, stringList));
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
