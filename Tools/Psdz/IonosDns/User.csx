// Requires CodegenCS VS extension
// https://marketplace.visualstudio.com/items?itemName=Drizin.CodegenCS
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;

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

    int Main(ICodegenContext context, VSExecutionContext vsContext, CodegenCS.Runtime.ILogger logger)
    {
        string templatePath = vsContext?.TemplatePath;
        if (string.IsNullOrEmpty(templatePath))
        {
            templatePath = "User.csx";
            logger.WriteLineAsync($"Template path is empty using: {templatePath}");
        }

        string templateName = Path.GetFileNameWithoutExtension(templatePath);
        logger.WriteLineAsync($"Template name: {templatePath}");
        if (!GenerateConfig(context[templateName + ".config"]))
        {
            return 1;
        }

        return 0;
    }

    bool GenerateConfig(ICodegenTextWriter writer)
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
    <add key=""Prefix"" value=""{prefix}""/>
    <add key=""Key"" value=""{key}""/>
</appSettings>");

        return true;
    }
}
