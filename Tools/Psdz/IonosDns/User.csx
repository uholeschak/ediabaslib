// Requires CodegenCS VS extension
// https://marketplace.visualstudio.com/items?itemName=Drizin.CodegenCS
using CodegenCS.Runtime;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    async Task<int> Main(ICodegenContext context, ILogger logger, VSExecutionContext vsContext)
    {
        string templatePath = vsContext?.TemplatePath;
        if (string.IsNullOrEmpty(templatePath))
        {
            templatePath = "User.csx";
            await logger.WriteLineAsync($"Template path is empty using: {templatePath}");
        }

        string templateName = Path.GetFileNameWithoutExtension(templatePath);
        await logger.WriteLineAsync($"Template name: {templatePath}");
        bool result = await GenerateConfig(context[templateName + ".config"], logger);
        if (!result)
        {
            await logger.WriteLineAsync("GenerateConfig failed");
            return 1;
        }

        return 0;
    }

    async Task<bool> GenerateConfig(ICodegenTextWriter writer, ILogger logger)
    {
        string prefix = string.Empty;
        string key = string.Empty;

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

        writer.WriteLine(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<appSettings>
    <add key=""Prefix"" value=""{prefix}""/>
    <add key=""Key"" value=""{key}""/>
</appSettings>");

        return true;
    }
}
