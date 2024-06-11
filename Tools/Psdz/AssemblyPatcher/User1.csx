using Newtonsoft.Json;
using System.IO;
using System;
using System.Reflection;
using System.Collections.Generic;

public class UserTemplate
{
    public class Info
    {
        public string Namespace { set; get; }
        public string Class { set; get; }
        public string Name { set; get; }
        public string Filename { set; get; }
    }

    public class InfoDict
    {
        public Dictionary<string, Info> PatchInfo { set; get; }
    }

    int Main(ICodegenContext context)
    {
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

        try
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".apk", "psdz_patcher.json");
            if (File.Exists(fileName))
            {
                InfoDict infoDict = JsonConvert.DeserializeObject<InfoDict>(File.ReadAllText(fileName));
                if (infoDict != null)
                {
                    if (infoDict.PatchInfo.TryGetValue("Ctor", out Info ctorInfo))
                    {
                        patchCtorNamespace = ctorInfo.Namespace;
                        patchCtorClass = ctorInfo.Class;
                    }

                    if (infoDict.PatchInfo.TryGetValue("Method", out Info methodInfo))
                    {
                        patchMethodNamespace = methodInfo.Namespace;
                        patchMethodClass = methodInfo.Class;
                        patchMethodName = methodInfo.Name;
                    }

                    if (infoDict.PatchInfo.TryGetValue("License", out Info licInfo))
                    {
                        licFileName = licInfo.Filename;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }

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
