#r "System.Xml.dll"
// Requires CodegenCS.SourceGenerator NuGet package
using CodegenCS.Runtime;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

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

    async Task<int> Main(ICodegenContext context, ILogger logger)
    {
        try
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            await logger.WriteLineAsync($"Assembly path: {assemblyPath}");
            string templateName = Path.GetFileNameWithoutExtension(assemblyPath);
            await logger.WriteLineAsync($"Template name: {templateName}");
            bool result = await GenerateConfig(context[templateName + ".config"], logger);
            if (!result)
            {
                await logger.WriteLineAsync("GenerateConfig failed");
                return 1;
            }
        }
        catch (Exception ex)
        {
            await logger.WriteLineAsync($"Exception: {ex.Message}");
            return 1;
        }

        return 0;
    }

    async Task<bool> GenerateConfig(ICodegenTextWriter writer, ILogger logger)
    {
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
                        await logger.WriteLineAsync($"Ctor: Namespace={patchCtorNamespace}");
                    }

                    XmlAttribute attribClass = nodeCtor.Attributes["class"];
                    if (attribClass != null)
                    {
                        patchCtorClass = attribClass.Value;
                        await logger.WriteLineAsync($"Ctor: Class={patchCtorClass}");
                    }
                }

                XmlNode nodeMethod = doc.SelectSingleNode("/patch_info/method");
                if (nodeMethod != null)
                {
                    XmlAttribute attribNamespace = nodeMethod.Attributes["namespace"];
                    if (attribNamespace != null)
                    {
                        patchMethodNamespace = attribNamespace.Value;
                        await logger.WriteLineAsync($"Method: Namespace={patchMethodNamespace}");
                    }

                    XmlAttribute attribClass = nodeMethod.Attributes["class"];
                    if (attribClass != null)
                    {
                        patchMethodClass = attribClass.Value;
                        await logger.WriteLineAsync($"Method: Class={patchMethodClass}");
                    }

                    XmlAttribute attribName = nodeMethod.Attributes["name"];
                    if (attribName != null)
                    {
                        patchMethodName = attribName.Value;
                        await logger.WriteLineAsync($"Method: Name={patchMethodName}");
                    }
                }

                XmlNode nodeLic = doc.SelectSingleNode("/patch_info/license");
                if (nodeLic != null)
                {
                    XmlAttribute attribFileName = nodeLic.Attributes["file_name"];
                    if (attribFileName != null)
                    {
                        licFileName = attribFileName.Value;
                        await logger.WriteLineAsync($"License: Filename={licFileName}");
                    }
                }
            }
            else
            {
                await logger.WriteLineAsync($"Configuration file not found: {fileName}");
            }
        }
        catch (Exception ex)
        {
            await logger.WriteLineAsync($"Exception: {ex.Message}");
            return false;
        }

        bool xmlOk = !string.IsNullOrEmpty(patchCtorNamespace) && !string.IsNullOrEmpty(patchCtorClass) &&
                     !string.IsNullOrEmpty(patchMethodNamespace) && !string.IsNullOrEmpty(patchMethodClass) &&
                     !string.IsNullOrEmpty(patchMethodName) &&
                     !string.IsNullOrEmpty(licFileName);

        if (!xmlOk)
        {
            await logger.WriteLineAsync($"XML data invalid, using json");

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
                            await logger.WriteLineAsync($"Ctor: Namespace={patchCtorNamespace}, Class={patchCtorClass}");
                        }

                        if (infoDict.PatchInfo.TryGetValue("Method", out Info methodInfo))
                        {
                            patchMethodNamespace = methodInfo.Namespace;
                            patchMethodClass = methodInfo.Class;
                            patchMethodName = methodInfo.Name;
                            await logger.WriteLineAsync($"Method: Namespace={patchMethodNamespace}, Class={patchMethodClass}, Name={patchMethodName}");
                        }

                        if (infoDict.PatchInfo.TryGetValue("License", out Info licInfo))
                        {
                            licFileName = licInfo.Filename;
                            await logger.WriteLineAsync($"License: Filename={licFileName}");
                        }
                    }
                }
                else
                {
                    await logger.WriteLineAsync($"Configuration file not found: {fileName}");
                }
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Exception: {ex.Message}");
                return false;
            }
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
