using System.IO;
using System.Xml;

Output.SetExtension(".config");
Output.BuildAction = BuildAction.Content;

string prefix = string.Empty;
string key = string.Empty;

try
{
    string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".apk", "ionos_dns.xml");
    if (File.Exists(fileName))
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        XmlNode nodeDns = doc.SelectSingleNode("/dns_info/api");
        if (nodeDns != null)
        {
            XmlAttribute attribPrefix = nodeDns.Attributes["prefix"];
            if (attribPrefix != null)
            {
                prefix = attribPrefix.Value;
            }
            XmlAttribute attribKey = nodeDns.Attributes["key"];
            if (attribKey != null)
            {
                key = attribKey.Value;
            }
        }
    }
}
catch (Exception ex)
{
    Output.WriteLine("Exception: {0}", ex.Message);
    return;
}

Output.WriteLine(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<appSettings>
    <add key=""Prefix"" value=""{prefix}""/>
    <add key=""Key"" value=""{key}""/>
</appSettings>");
