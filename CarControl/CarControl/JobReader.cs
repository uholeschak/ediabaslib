using Mono.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

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

        public class PageInfo
        {
            public PageInfo(string name, string sgbd, string classCode, List<DisplayInfo> displayList)
            {
                this.name = name;
                this.sgbd = sgbd;
                this.classCode = classCode;
                this.displayList = displayList;
                this.infoObject = null;
                this.classObject = null;
            }

            private string name;
            private string sgbd;
            private string classCode;
            private List<DisplayInfo> displayList;
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

            public string Sgbd
            {
                get
                {
                    return sgbd;
                }
            }

            public string ClassCode
            {
                get
                {
                    return classCode;
                }
            }

            public List<DisplayInfo> DisplayList
            {
                get
                {
                    return displayList;
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

        public List<PageInfo> pageList;

        public List<PageInfo> PageList
        {
            get
            {
                return pageList;
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
            pageList = null;
            if (!File.Exists (xmlName))
            {
                return false;
            }

            XmlDocument xdocConfig = new XmlDocument();
            try
            {
                xdocConfig.Load(xmlName);
                XmlNodeList xnodePages = xdocConfig.SelectNodes("/configuration/pages/page");
                if (xnodePages != null)
                {
                    pageList = new List<PageInfo>();
                    foreach (XmlNode xnodePage in xnodePages)
                    {
                        XmlAttribute attrib;
                        string pageName = string.Empty;
                        string sgbdName = string.Empty;
                        string classCode = xnodePage.InnerText;
                        if (xnodePage.Attributes != null)
                        {
                            attrib = xnodePage.Attributes["name"];
                            if (attrib != null) pageName = attrib.Value;
                            attrib = xnodePage.Attributes["sgbd"];
                            if (attrib != null) sgbdName = attrib.Value;
                        }
                        if (string.IsNullOrEmpty(pageName) || string.IsNullOrEmpty(classCode)) continue;

                        List<DisplayInfo> displayList = new List<DisplayInfo> ();
                        foreach (XmlNode xnodeDisplay in xnodePage.ChildNodes)
                        {
                            if (string.Compare(xnodeDisplay.Name, "display", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string name = string.Empty;
                                string result = string.Empty;
                                string format = null;
                                if (xnodeDisplay.Attributes != null)
                                {
                                    attrib = xnodeDisplay.Attributes["name"];
                                    if (attrib != null) name = attrib.Value;
                                    attrib = xnodeDisplay.Attributes["result"];
                                    if (attrib != null) result = attrib.Value;
                                    attrib = xnodeDisplay.Attributes["format"];
                                    if (attrib != null) format = attrib.Value;

                                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(result)) continue;
                                    displayList.Add (new DisplayInfo (name, result, format));
                                }
                            }
                        }
                        pageList.Add(new PageInfo(pageName, sgbdName, classCode, displayList));
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
