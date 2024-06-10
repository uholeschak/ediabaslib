using System.IO;
using System.Xml;
using System;
using System.Reflection;

public class UserTemplate
{
    int Main(ICodegenContext context)
    {
        Assembly.LoadFrom("C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETFramework\\v4.7.2\\System.Xml.dll");

        if (!GenerateConfig(context["Users1.config"]))
        {
            return 1;
        }
        return 0;
    }

    bool GenerateConfig(ICodegenTextWriter writer)
    {
        string patchCtorNamespace = string.Empty;
        string patchCtorClass = string.Empty;
        string patchMethodNamespace = string.Empty;
        string patchMethodClass = string.Empty;
        string patchMethodName = string.Empty;
        string licFileName = string.Empty;

#if false
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
            return false;
        }
#endif
        writer.WriteLine(
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
<appSettings>
    <add key=""PatchCtorNamespace"" value=""{patchCtorNamespace}""/>
    <add key=""PatchCtorClass"" value=""{patchCtorClass}""/>
    <add key=""PatchMethodNamespace"" value=""{patchMethodNamespace}""/>
    <add key=""PatchMethodClass"" value=""{patchMethodClass}""/>
    <add key=""PatchMethodName"" value=""{patchMethodName}""/>
    <add key=""LicFileName"" value=""{licFileName}""/>
</appSettings>");

        return true;
    }
}
