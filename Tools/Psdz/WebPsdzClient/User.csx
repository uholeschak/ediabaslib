#r "System.Xml.dll"
#r "System.Xml.ReaderWriter.dll"
// // Requires CodegenCS VS extension
// https://marketplace.visualstudio.com/items?itemName=Drizin.CodegenCS
// Additionally install: dotnet tool install --global dotnet-codegencs
using CodegenCS.Runtime;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;

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

    async Task<int> Main(ICodegenContext context, ILogger logger)
    {
        try
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            await logger.WriteLineAsync($"Assembly path: {assemblyPath}").ConfigureAwait(false);
            string templateName = Path.GetFileNameWithoutExtension(assemblyPath);
            await logger.WriteLineAsync($"Template name: {templateName}").ConfigureAwait(false);
            bool result = await GenerateConfig(context[templateName + ".config"], logger).ConfigureAwait(false);
            if (!result)
            {
                await logger.WriteLineAsync("GenerateConfig failed").ConfigureAwait(false);
                return 1;
            }
        }
        catch (Exception ex)
        {
            await logger.WriteLineAsync($"Exception: {ex.Message}").ConfigureAwait(false);
            return 1;
        }

        return 0;
    }

    async Task<bool> GenerateConfig(ICodegenTextWriter writer, ILogger logger)
    {
        string istaLocation = string.Empty; // auto detect
        string rpcServerLocation = string.Empty; // auto detect
        string rpcServerUser = string.Empty;
        string rpcServerPassword = string.Empty;
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
                XmlNode nodeIsta = doc.SelectSingleNode("/credentials_info/ista");
                if (nodeIsta != null)
                {
                    XmlAttribute attribLocation = nodeIsta.Attributes["location"];
                    if (attribLocation != null)
                    {
                        istaLocation = attribLocation.Value;
                        await logger.WriteLineAsync($"Ista: Location={istaLocation}").ConfigureAwait(false);
                    }
                }

                XmlNode nodeRpcServer = doc.SelectSingleNode("/credentials_info/rpcserver");
                if (nodeRpcServer != null)
                {
                    XmlAttribute attribLocation = nodeRpcServer.Attributes["location"];
                    if (attribLocation != null)
                    {
                        rpcServerLocation = attribLocation.Value;
                        await logger.WriteLineAsync($"RpcServer: Location={rpcServerLocation}").ConfigureAwait(false);
                    }

                    XmlAttribute attribUser = nodeRpcServer.Attributes["name"];
                    if (attribUser != null)
                    {
                        rpcServerUser = attribUser.Value;
                        await logger.WriteLineAsync($"RpcServer: User={rpcServerUser}").ConfigureAwait(false);
                    }

                    XmlAttribute attribPassword = nodeRpcServer.Attributes["password"];
                    if (attribPassword != null)
                    {
                        rpcServerPassword = attribPassword.Value;
                        await logger.WriteLineAsync($"RpcServer: Password Length={rpcServerPassword.Length}").ConfigureAwait(false);
                    }
                }

                XmlNode nodeSqlServer = doc.SelectSingleNode("/credentials_info/sqlserver");
                if (nodeSqlServer != null)
                {
                    XmlAttribute attribUrl = nodeSqlServer.Attributes["url"];
                    if (attribUrl != null)
                    {
                        sqlUrl = attribUrl.Value;
                        await logger.WriteLineAsync($"SqlServer: Url={sqlUrl}").ConfigureAwait(false);
                    }

                    XmlAttribute attribName = nodeSqlServer.Attributes["name"];
                    if (attribName != null)
                    {
                        sqlUser = attribName.Value;
                        await logger.WriteLineAsync($"SqlServer: Name={sqlUser}").ConfigureAwait(false);
                    }

                    XmlAttribute attribPassword = nodeSqlServer.Attributes["password"];
                    if (attribPassword != null)
                    {
                        sqlPassword = attribPassword.Value;
                        await logger.WriteLineAsync($"SqlServer: Password={sqlPassword}").ConfigureAwait(false);
                    }
                }

                XmlNode nodeAuth = doc.SelectSingleNode("/credentials_info/authentication");
                if (nodeAuth != null)
                {
                    XmlAttribute attribPassword = nodeAuth.Attributes["password"];
                    if (attribPassword != null)
                    {
                        accessPassword = attribPassword.Value;
                        await logger.WriteLineAsync($"Authentication: Password={accessPassword}").ConfigureAwait(false);
                    }
                }

                XmlNode nodeLic = doc.SelectSingleNode("/credentials_info/licenses");
                if (nodeLic != null)
                {
                    XmlAttribute attribTest = nodeLic.Attributes["test"];
                    if (attribTest != null)
                    {
                        testLic = attribTest.Value;
                        await logger.WriteLineAsync($"Licenses: Test={testLic}").ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await logger.WriteLineAsync($"Configuration file not found: {fileName}").ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            await logger.WriteLineAsync($"Exception: {ex.Message}").ConfigureAwait(false);
            return false;
        }

        bool xmlOk = !string.IsNullOrEmpty(sqlUrl) &&
                     !string.IsNullOrEmpty(sqlUser) && !string.IsNullOrEmpty(sqlPassword) &&
                     !string.IsNullOrEmpty(accessPassword) && !string.IsNullOrEmpty(testLic);

        if (!xmlOk)
        {
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
                            await logger.WriteLineAsync($"Ista: Location={istaLocation}").ConfigureAwait(false);
                        }

                        if (infoDict.CredentialsInfo.TryGetValue("RpcServer", out Info rpcServerInfo))
                        {
                            rpcServerLocation = rpcServerInfo.Location;
                            await logger.WriteLineAsync($"RpcServer: Location={rpcServerLocation}").ConfigureAwait(false);
                            rpcServerUser = rpcServerInfo.Name;
                            await logger.WriteLineAsync($"RpcServer: User={rpcServerUser}").ConfigureAwait(false);
                            rpcServerPassword = rpcServerInfo.Password;
                            await logger.WriteLineAsync($"RpcServer: Password Length={rpcServerPassword.Length}").ConfigureAwait(false);
                        }

                        if (infoDict.CredentialsInfo.TryGetValue("SqlServer", out Info sqlInfo))
                        {
                            sqlUrl = sqlInfo.Url;
                            sqlUser = sqlInfo.Name;
                            sqlPassword = sqlInfo.Password;
                            await logger.WriteLineAsync($"SqlServer: Url={sqlUrl}, Name={sqlUser}, Password={sqlPassword}").ConfigureAwait(false);
                        }

                        if (infoDict.CredentialsInfo.TryGetValue("Authentication", out Info authInfo))
                        {
                            accessPassword = authInfo.Password;
                            await logger.WriteLineAsync($"Authentication: Password={accessPassword}").ConfigureAwait(false);
                        }

                        if (infoDict.CredentialsInfo.TryGetValue("Licenses", out Info licensesInfo))
                        {
                            testLic = licensesInfo.Test;
                            await logger.WriteLineAsync($"Licenses: Test={testLic}").ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await logger.WriteLineAsync($"Configuration file not found: {fileName}").ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await logger.WriteLineAsync($"Exception: {ex.Message}").ConfigureAwait(false);
                return false;
            }
        }

        writer.WriteLine(
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<appSettings>
    <add key=""DealerId"" value=""40626""/>
    <add key=""IstaFolder"" value=""{istaLocation}""/>
    <add key=""RpcServer"" value=""{rpcServerLocation}""/>
    <add key=""RpcServerUser"" value=""{rpcServerUser}""/>
    <add key=""RpcServerPassword"" value=""{rpcServerPassword}""/>
    <add key=""SqlServer"" value=""Server={sqlUrl};User ID={sqlUser};Password={sqlPassword}""/>
    <add key=""AccessPwd"" value=""{accessPassword}""/>
    <add key=""TestLicenses"" value=""{testLic}""/>
    <add key=""DisplayOptions"" value=""Hardware""/>
</appSettings>");

        return true;
    }
}
