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
        public string Location { set; get; }
        public string Url { set; get; }
        public string Name { set; get; }
        public string Password { set; get; }
        public string Test { set; get; }
    }

    public class InfoDict
    {
        public Dictionary<string, Info> CredentialsInfo { set; get; }
    }

    int Main(ICodegenContext context)
    {
        if (!GenerateConfig(context["User.config"]))
        {
            return 1;
        }

        return 0;
    }

    bool GenerateConfig(ICodegenTextWriter writer)
    {
        string istaLocation = "C:\\ISTA-D";
        string sqlUrl = "url";
        string sqlUser = "user";
        string sqlPassword = "password";
        string accessPassword = string.Empty;
        string testLic = string.Empty;

        try
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".apk", "psdz_credentials.json");
            if (File.Exists(fileName))
            {
                InfoDict infoDict = JsonConvert.DeserializeObject<InfoDict>(File.ReadAllText(fileName));
                if (infoDict != null)
                {
                    if (infoDict.CredentialsInfo.TryGetValue("Ista", out Info istaInfo))
                    {
                        istaLocation = istaInfo.Location;
                    }

                    if (infoDict.CredentialsInfo.TryGetValue("SqlServer", out Info sqlInfo))
                    {
                        sqlUrl = sqlInfo.Url;
                        sqlUser = sqlInfo.Name;
                        sqlPassword = sqlInfo.Password;
                    }

                    if (infoDict.CredentialsInfo.TryGetValue("Authentication", out Info authInfo))
                    {
                        accessPassword = authInfo.Password;
                    }

                    if (infoDict.CredentialsInfo.TryGetValue("Licenses", out Info licensesInfo))
                    {
                        testLic = licensesInfo.Test;
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
    <add key=""DealerId"" value=""32395""/>
    <add key=""IstaFolder"" value=""{istaLocation}""/>
    <add key=""SqlServer"" value=""Server={sqlUrl};User ID={sqlUser};Password={sqlPassword}""/>
    <add key=""AccessPwd"" value=""{accessPassword}""/>
    <add key=""TestLicenses"" value=""{testLic}""/>
    <add key=""DisplayOptions"" value=""Hardware""/>
</appSettings>");

        return true;
    }
}
