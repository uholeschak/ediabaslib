#r "System.Xml.dll"
#r "System.Xml.ReaderWriter.dll"
// Requires CodegenCS.SourceGenerator NuGet package
using CodegenCS.Runtime;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;

public class UserTemplate
{
    public class Info
    {
        public string Namespace { set; get; }
        public string Class { set; get; }
        public string Name { set; get; }
        public string Name2 { set; get; }
        public string Filename { set; get; }
    }

    public class InfoDict
    {
        public Dictionary<string, Info> PatchInfo { set; get; }
    }

    async Task<int> Main(ICodegenContext context, ILogger logger)
    {
        ICodegenTextWriter logWriter = null;
        try
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            await logger.WriteLineAsync($"Assembly path: {assemblyPath}");
            string templateName = Path.GetFileNameWithoutExtension(assemblyPath);
            await logger.WriteLineAsync($"Template name: {templateName}");

            logWriter = context[templateName + ".log"];
            logWriter.WriteLine($"Template name: {templateName}");

            bool result = await GenerateConfig(context[templateName + ".config"], logWriter);
            if (!result)
            {
                logWriter.WriteLine("GenerateConfig failed");
                return 1;
            }
        }
        catch (Exception ex)
        {
            logWriter?.WriteLine($"Exception: {ex.Message}");
            return 1;
        }

        return 0;
    }

    async Task<bool> GenerateConfig(ICodegenTextWriter writer, ICodegenTextWriter logWriter)
    {
        string patchCtorNamespace = string.Empty;
        string patchCtorClass = string.Empty;
        string patchMethod1Namespace = string.Empty;
        string patchMethod1Class = string.Empty;
        string patchMethod1Name = string.Empty;
        string patchMethod1Name2 = string.Empty;
        string patchMethod2Namespace = string.Empty;
        string patchMethod2Class = string.Empty;
        string patchMethod2Name = string.Empty;
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
                        logWriter.WriteLine($"Ctor: Namespace={patchCtorNamespace}");
                    }

                    XmlAttribute attribClass = nodeCtor.Attributes["class"];
                    if (attribClass != null)
                    {
                        patchCtorClass = attribClass.Value;
                        logWriter.WriteLine($"Ctor: Class={patchCtorClass}");
                    }
                }

                XmlNode nodeMethod1 = doc.SelectSingleNode("/patch_info/method1");
                if (nodeMethod1 != null)
                {
                    XmlAttribute attribNamespace = nodeMethod1.Attributes["namespace"];
                    if (attribNamespace != null)
                    {
                        patchMethod1Namespace = attribNamespace.Value;
                        logWriter.WriteLine($"Method1: Namespace={patchMethod1Namespace}");
                    }

                    XmlAttribute attribClass = nodeMethod1.Attributes["class"];
                    if (attribClass != null)
                    {
                        patchMethod1Class = attribClass.Value;
                        logWriter.WriteLine($"Method1: Class={patchMethod1Class}");
                    }

                    XmlAttribute attribName = nodeMethod1.Attributes["name"];
                    if (attribName != null)
                    {
                        patchMethod1Name = attribName.Value;
                        logWriter.WriteLine($"Method1: Name={patchMethod1Name}");
                    }

                    XmlAttribute attribName2 = nodeMethod1.Attributes["name2"];
                    if (attribName2 != null)
                    {
                        patchMethod1Name2 = attribName2.Value;
                        logWriter.WriteLine($"Method1_2: Name={patchMethod1Name2}");
                    }
                }

                XmlNode nodeMethod2 = doc.SelectSingleNode("/patch_info/method2");
                if (nodeMethod2 != null)
                {
                    XmlAttribute attribNamespace = nodeMethod2.Attributes["namespace"];
                    if (attribNamespace != null)
                    {
                        patchMethod2Namespace = attribNamespace.Value;
                        logWriter.WriteLine($"Method2: Namespace={patchMethod2Namespace}");
                    }

                    XmlAttribute attribClass = nodeMethod2.Attributes["class"];
                    if (attribClass != null)
                    {
                        patchMethod2Class = attribClass.Value;
                        logWriter.WriteLine($"Method2: Class={patchMethod2Class}");
                    }

                    XmlAttribute attribName = nodeMethod2.Attributes["name"];
                    if (attribName != null)
                    {
                        patchMethod2Name = attribName.Value;
                        logWriter.WriteLine($"Method2: Name={patchMethod2Name}");
                    }
                }

                XmlNode nodeLic = doc.SelectSingleNode("/patch_info/license");
                if (nodeLic != null)
                {
                    XmlAttribute attribFileName = nodeLic.Attributes["file_name"];
                    if (attribFileName != null)
                    {
                        licFileName = attribFileName.Value;
                        logWriter.WriteLine($"License: Filename={licFileName}");
                    }
                }
            }
            else
            {
                logWriter.WriteLine($"Configuration file not found: {fileName}");
            }
        }
        catch (Exception ex)
        {
            logWriter.WriteLine($"Exception: {ex.Message}");
            return false;
        }

        bool xmlOk = !string.IsNullOrEmpty(patchCtorNamespace) && !string.IsNullOrEmpty(patchCtorClass) &&
                     !string.IsNullOrEmpty(patchMethod1Namespace) && !string.IsNullOrEmpty(patchMethod1Class) &&
                     !string.IsNullOrEmpty(patchMethod1Name) &&
                     !string.IsNullOrEmpty(patchMethod2Namespace) && !string.IsNullOrEmpty(patchMethod2Class) &&
                     !string.IsNullOrEmpty(patchMethod2Name) &&
                     !string.IsNullOrEmpty(licFileName);

        if (!xmlOk)
        {
            logWriter.WriteLine($"XML data invalid, using json");

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
                            logWriter.WriteLine($"Ctor: Namespace={patchCtorNamespace}, Class={patchCtorClass}");
                        }

                        if (infoDict.PatchInfo.TryGetValue("Method1", out Info methodInfo1))
                        {
                            patchMethod1Namespace = methodInfo1.Namespace;
                            patchMethod1Class = methodInfo1.Class;
                            patchMethod1Name = methodInfo1.Name;
                            patchMethod1Name2 = methodInfo1.Name2;
                            logWriter.WriteLine($"Method: Namespace={patchMethod1Namespace}, Class={patchMethod1Class}, Name={patchMethod1Name}, Name2={patchMethod1Name2}");
                        }

                        if (infoDict.PatchInfo.TryGetValue("Method2", out Info methodInfo2))
                        {
                            patchMethod2Namespace = methodInfo2.Namespace;
                            patchMethod2Class = methodInfo2.Class;
                            patchMethod2Name = methodInfo2.Name;
                            logWriter.WriteLine($"Method: Namespace={patchMethod2Namespace}, Class={patchMethod2Class}, Name={patchMethod2Name}");
                        }

                        if (infoDict.PatchInfo.TryGetValue("License", out Info licInfo))
                        {
                            licFileName = licInfo.Filename;
                            logWriter.WriteLine($"License: Filename={licFileName}");
                        }
                    }
                }
                else
                {
                    logWriter.WriteLine($"Configuration file not found: {fileName}");
                }
            }
            catch (Exception ex)
            {
                logWriter.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }

        writer.WriteLine(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<appSettings>
    <add key=""PatchCtorNamespace"" value=""{patchCtorNamespace}""/>
    <add key=""PatchCtorClass"" value=""{patchCtorClass}""/>
    <add key=""PatchMethod1Namespace"" value=""{patchMethod1Namespace}""/>
    <add key=""PatchMethod1Class"" value=""{patchMethod1Class}""/>
    <add key=""PatchMethod1Name"" value=""{patchMethod1Name}""/>
    <add key=""PatchMethod1Name2"" value=""{patchMethod1Name2}""/>
    <add key=""PatchMethod2Namespace"" value=""{patchMethod2Namespace}""/>
    <add key=""PatchMethod2Class"" value=""{patchMethod2Class}""/>
    <add key=""PatchMethod2Name"" value=""{patchMethod2Name}""/>
    <add key=""LicFileName"" value=""{licFileName}""/>
</appSettings>");

        return true;
    }
}
