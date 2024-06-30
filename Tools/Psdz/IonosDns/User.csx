#r "System.Xml.dll"
// Requires CodegenCS VS extension
// https://marketplace.visualstudio.com/items?itemName=Drizin.CodegenCS
// Additionally install: dotnet tool install --global dotnet-codegencs
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
        public string Prefix { set; get; }
        public string Key { set; get; }
    }

    public class InfoDict
    {
        public Dictionary<string, Info> DnsInfo { set; get; }
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

            return 0;
        }
        catch (Exception ex)
        {
            await logger.WriteLineAsync($"Exception: {ex.Message}");
            return 1;
        }
    }

    async Task<bool> GenerateConfig(ICodegenTextWriter writer, ILogger logger)
    {
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
                        await logger.WriteLineAsync($"Api: Prefix={prefix}");
                    }
                    XmlAttribute attribKey = nodeDns.Attributes["key"];
                    if (attribKey != null)
                    {
                        key = attribKey.Value;
                        await logger.WriteLineAsync($"Api: Key={key}");
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

        bool xmlOk = !string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(key);
        if (!xmlOk)
        {
            await logger.WriteLineAsync($"XML data invalid, using json");

            try
            {
                string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".apk", "ionos_dns.json");
                if (File.Exists(fileName))
                {
                    InfoDict infoDict = JsonConvert.DeserializeObject<InfoDict>(File.ReadAllText(fileName));
                    if (infoDict != null)
                    {
                        if (infoDict.DnsInfo.TryGetValue("Api", out Info apiInfo))
                        {
                            prefix = apiInfo.Prefix;
                            key = apiInfo.Key;
                            await logger.WriteLineAsync($"Api: Prefix={prefix}, Key={key}");
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
    <add key=""Prefix"" value=""{prefix}""/>
    <add key=""Key"" value=""{key}""/>
</appSettings>");

        return true;
    }
}
