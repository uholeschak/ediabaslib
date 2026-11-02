using System;
using System.Xml;
using System.Xml.Linq;

namespace BmwDeepObd
{
    public static class MailInfoUtils
    {
        public static string GetMailErrorMessage(string mailXml, out string adapterBlacklist)
        {
            adapterBlacklist = null;

            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return null;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);
                if (xmlDoc.Root == null)
                {
                    return null;
                }

                foreach (XElement blacklistNode in xmlDoc.Root.Elements("blacklists"))
                {
                    XAttribute adaptersAttr = blacklistNode.Attribute("adapters");
                    if (adaptersAttr != null && !string.IsNullOrEmpty(adaptersAttr.Value))
                    {
                        adapterBlacklist = adaptersAttr.Value;
                    }
                }

                XElement errorNode = xmlDoc.Root.Element("error");
                // ReSharper disable once UseNullPropagation
                if (errorNode != null)
                {
                    XAttribute messageAttr = errorNode.Attribute("message");
                    if (messageAttr != null && !string.IsNullOrEmpty(messageAttr.Value))
                    {
                        return messageAttr.Value;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public static bool GetMailInfo(string mailXml, out string dbId, out string commitId, out string host, out int port, out bool ssl, out string from, out string to, out string name, out string password)
        {
            dbId = null;
            commitId = null;
            host = null;
            port = 0;
            ssl = false;
            from = null;
            to = null;
            name = null;
            password = null;
            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);

                XElement dbNode = xmlDoc.Root?.Element("db_info");
                if (dbNode != null)
                {
                    XAttribute dbIdAttr = dbNode.Attribute("db_id");
                    if (dbIdAttr == null)
                    {
                        return false;
                    }
                    dbId = dbIdAttr.Value;

                    XAttribute commitIdAttr = dbNode.Attribute("commit_id");
                    if (commitIdAttr == null)
                    {
                      return false;
                    }
                    commitId = commitIdAttr.Value;
                    return true;
                }

                XElement emailNode = xmlDoc.Root?.Element("email");
                XAttribute hostAttr = emailNode?.Attribute("host");
                if (hostAttr == null)
                {
                    return false;
                }
                host = hostAttr.Value;
                XAttribute portAttr = emailNode.Attribute("port");
                if (portAttr == null)
                {
                    return false;
                }
                port = XmlConvert.ToInt32(portAttr.Value);
                XAttribute sslAttr = emailNode.Attribute("ssl");
                if (sslAttr == null)
                {
                    return false;
                }
                ssl = XmlConvert.ToBoolean(sslAttr.Value);
                XAttribute fromAttr = emailNode.Attribute("from");
                if (fromAttr == null)
                {
                    return false;
                }
                from = fromAttr.Value;
                XAttribute toAttr = emailNode.Attribute("to");
                if (toAttr == null)
                {
                    return false;
                }
                to = toAttr.Value;
                XAttribute usernameAttr = emailNode.Attribute("username");
                if (usernameAttr != null)
                {
                    name = usernameAttr.Value;
                }
                XAttribute passwordAttr = emailNode.Attribute("password");
                if (passwordAttr != null)
                {
                    password = passwordAttr.Value;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool GetMailKeyWordsInfo(string mailXml, out string regEx, out int maxWords)
        {
            regEx = null;
            maxWords = 0;
            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);
                XElement keyWordsNode = xmlDoc.Root?.Element("keywords");
                XAttribute regexAttr = keyWordsNode?.Attribute("regex");
                if (regexAttr == null)
                {
                    return false;
                }
                regEx = regexAttr.Value;
                XAttribute maxWordsAttr = keyWordsNode.Attribute("max_words");
                if (maxWordsAttr != null)
                {
                    maxWords = XmlConvert.ToInt32(maxWordsAttr.Value);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool GetMailLinesInfo(string mailXml, out string regEx)
        {
            regEx = null;
            try
            {
                if (string.IsNullOrEmpty(mailXml))
                {
                    return false;
                }
                XDocument xmlDoc = XDocument.Parse(mailXml);
                XElement keyWordsNode = xmlDoc.Root?.Element("lineinfo");
                XAttribute regexAttr = keyWordsNode?.Attribute("regex");
                if (regexAttr == null)
                {
                    return false;
                }
                regEx = regexAttr.Value;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
