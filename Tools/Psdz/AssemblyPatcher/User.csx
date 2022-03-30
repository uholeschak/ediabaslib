using System.IO;
using System.Xml;

Output.SetExtension(".config");
Output.BuildAction = BuildAction.Content;

string patchCtorNamespace = string.Empty;
string patchCtorClass = string.Empty;
string patchMethodNamespace = string.Empty;
string patchMethodClass = string.Empty;
string patchMethodName = string.Empty;
string licFileName = string.Empty;

try
{
    string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".apk", "psdz_patcher.xml");
    if (File.Exists(fileName))
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(fileName);
        XmlNode nodeCtor = doc.SelectSingleNode("/patch_info/ctor");
        if (nodeCtor != null)
        {
            XmlAttribute attribNamespace = nodeCtor.Attributes["namespace"];
            if (attribNamespace != null)
            {
                patchCtorNamespace = attribNamespace.Value;
            }

            XmlAttribute attribClass = nodeCtor.Attributes["class"];
            if (attribClass != null)
            {
                patchCtorClass = attribClass.Value;
            }
        }

        XmlNode nodeMethod = doc.SelectSingleNode("/patch_info/method");
        if (nodeMethod != null)
        {
            XmlAttribute attribNamespace = nodeMethod.Attributes["namespace"];
            if (attribNamespace != null)
            {
                patchMethodNamespace = attribNamespace.Value;
            }

            XmlAttribute attribClass = nodeMethod.Attributes["class"];
            if (attribClass != null)
            {
                patchMethodClass = attribClass.Value;
            }

            XmlAttribute attribName = nodeMethod.Attributes["name"];
            if (attribName != null)
            {
                patchMethodName = attribName.Value;
            }
        }

        XmlNode nodeLic = doc.SelectSingleNode("/patch_info/license");
        if (nodeLic != null)
        {
            XmlAttribute attribFileName = nodeLic.Attributes["file_name"];
            if (attribFileName != null)
            {
                licFileName = attribFileName.Value;
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
    <add key=""PatchCtorNamespace"" value=""BMW.Rheingold.SecurityAndLicense""/>
    <add key=""PatchCtorClass"" value=""IntegrityManager""/>
    <add key=""PatchMethodNamespace"" value=""BMW.Rheingold.CoreFramework.LicenseManagement""/>
    <add key=""PatchMethodClass"" value=""LicenseStatusChecker""/>
    <add key=""PatchMethodName"" value=""IsLicenseValid""/>
    <add key=""LicFileName"" value=""License.xml""/>
</appSettings>");
