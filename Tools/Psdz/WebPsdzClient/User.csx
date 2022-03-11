using System.IO;
using System.Xml;

Output.BuildAction = BuildAction.Content;
Output.SetExtension(".config");

string istaLocation = "C:\\ISTA-D";
string sqlUrl = "url";
string sqlUser = "user";
string sqlPassword = "password";
string accessPassword = string.Empty;
string testLic = string.Empty;

try
{
    string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".apk", "psdz_credentials.xml");
    if (File.Exists(fileName))
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        XmlNode nodeSqlServer = doc.SelectSingleNode("/credentials_info/sqlserver");
        XmlNode nodeIsta = doc.SelectSingleNode("/credentials_info/ista");
        if (nodeIsta != null)
        {
            XmlAttribute attribLocation = nodeIsta.Attributes["location"];
            if (attribLocation != null)
            {
                istaLocation = attribLocation.Value;
            }
        }

        if (nodeSqlServer != null)
        {
            XmlAttribute attribUrl = nodeSqlServer.Attributes["url"];
            if (attribUrl != null)
            {
                sqlUrl = attribUrl.Value;
            }

            XmlAttribute attribName = nodeSqlServer.Attributes["name"];
            if (attribName != null)
            {
                sqlUser = attribName.Value;
            }

            XmlAttribute attribPassword = nodeSqlServer.Attributes["password"];
            if (attribPassword != null)
            {
                sqlPassword = attribPassword.Value;
            }
        }

        XmlNode nodeAuth = doc.SelectSingleNode("/credentials_info/authentication");
        if (nodeAuth != null)
        {
            XmlAttribute attribPassword = nodeAuth.Attributes["password"];
            if (attribPassword != null)
            {
                accessPassword = attribPassword.Value;
            }
        }

        XmlNode nodeLic = doc.SelectSingleNode("/credentials_info/licenses");
        if (nodeLic != null)
        {
            XmlAttribute attribTest = nodeLic.Attributes["test"];
            if (attribTest != null)
            {
                testLic = attribTest.Value;
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
    <add key=""DealerId"" value=""32395""/>
    <add key=""IstaFolder"" value=""{istaLocation}""/>
    <add key=""SqlServer"" value=""Server={sqlUrl};User ID={sqlUser};Password={sqlPassword}""/>
    <add key=""AccessPwd"" value=""{accessPassword}""/>
    <add key=""TestLicenses"" value=""{testLic}""/>
</appSettings>");
