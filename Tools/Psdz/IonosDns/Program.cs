using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using Newtonsoft.Json.Linq;

namespace IonosDns
{
    internal class Program
    {
        private const string BaseUrl = @"https://api.hosting.ionos.com/dns/v1/";
        private static HttpClient _httpClient;

        static int Main(string[] args)
        {
            try
            {
                string logFileName = Path.Combine(AssemblyDirectory, "IonosDns.txt");
                bool append = false;
                FileInfo fi = new FileInfo(logFileName);
                if (fi.Exists && fi.Length < 10000)
                {
                    append = true;
                }

                using (StreamWriter logFile = new StreamWriter(logFileName, append))
                {
                    try
                    {
                        logFile.WriteLine("------------------------------------");
                        logFile.WriteLine("Date: {0}", DateTime.Now.ToString());

                        string prefix = ConfigurationManager.AppSettings["Prefix"];
                        if (string.IsNullOrEmpty(prefix))
                        {
                            logFile.WriteLine("Prefix not configured");
                            return 1;
                        }

                        string key = ConfigurationManager.AppSettings["Key"];
                        if (string.IsNullOrEmpty(key))
                        {
                            logFile.WriteLine("Key not configured");
                            return 1;
                        }

                        logFile.Write("Arguments:");
                        foreach (string arg in args)
                        {
                            logFile.Write(" '{0}'", arg);
                        }
                        logFile.WriteLine();

                        string apiKey = prefix + "." + key;
                        if (args.Length < 1)
                        {
                            logFile.WriteLine("No operation specified");
                            return 1;
                        }

                        string operation = args[0];
                        bool create = string.Compare(operation, "create", StringComparison.OrdinalIgnoreCase) == 0;
                        bool delete = string.Compare(operation, "delete", StringComparison.OrdinalIgnoreCase) == 0;

                        if (!create && !delete)
                        {
                            logFile.WriteLine("Operation must be create or delete");
                            return 1;
                        }

                        if (create)
                        {
                            if (args.Length < 4)
                            {
                                logFile.WriteLine("Create argument: create {Identifier} {RecordName} {Token}");
                                return 1;
                            }
                        }

                        if (delete)
                        {
                            if (args.Length < 3)
                            {
                                logFile.WriteLine("Create argument: create {Identifier} {RecordName}");
                                return 1;
                            }
                        }

                        string identifier = args[1];
                        string recordName = args[2];
                        string recordToken = string.Empty;
                        if (create)
                        {
                            recordToken = args[3];
                        }

                        string domain = GetDomainNameOfIdentifier(identifier);
                        _httpClient = new HttpClient(new HttpClientHandler()
                        {
                            SslProtocols = SslProtocols.None,
                            ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                        });

                        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                        _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                        _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("curl", "7.79.1"));

                        logFile.WriteLine("Get zones ID for domain: {0}", domain);
                        string zonesId = GetZonesId(domain);
                        if (string.IsNullOrEmpty(zonesId))
                        {
                            logFile.WriteLine("No zones ID");
                            return 1;
                        }

                        logFile.WriteLine("Zones ID: {0}", zonesId);
                        logFile.WriteLine("Get record: '{0}'", recordName);
                        string recordId = GetRecordId(zonesId, recordName);
                        if (!string.IsNullOrEmpty(recordId))
                        {
                            logFile.WriteLine("Delete Record ID: {0}", recordId);
                            if (!DeleteRecord(zonesId, recordId))
                            {
                                logFile.WriteLine("Delete record failed");
                                return 1;
                            }

                            logFile.WriteLine("Deleted Record ID: {0}", recordId);
                        }
                        else
                        {
                            logFile.WriteLine("Record not found: '{0}'", recordName);
                        }

                        if (create)
                        {
                            logFile.WriteLine("Create Record: '{0}' '{1}'", recordName, recordToken);
                            if (!CreateRecord(zonesId, recordName, recordToken))
                            {
                                logFile.WriteLine("Create record failed");
                                return 1;
                            }

                            logFile.WriteLine("Created Record: '{0}' '{1}'", recordName, recordToken);
                        }
                    }
                    catch (Exception e)
                    {
                        logFile.WriteLine("Exception: {0}", e.Message);
                        return 1;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }

            return 0;
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static string GetDomainNameOfIdentifier(string urlString)
        {
            return urlString.Substring(urlString.LastIndexOf('.', urlString.LastIndexOf('.') - 1) + 1);
        }

        private static string GetZonesId(string domain)
        {
            try
            {
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append(BaseUrl);
                sbUrl.Append(@"zones");

                HttpResponseMessage response = _httpClient.GetAsync(new Uri(sbUrl.ToString())).Result;
                bool success = response.IsSuccessStatusCode;
                if (success)
                {
                    string responseZonesResult = response.Content.ReadAsStringAsync().Result;
                    JToken resultJson = JToken.Parse(responseZonesResult);
                    foreach (JToken token in resultJson)
                    {
                        string typeName = token["type"]?.ToString() ?? string.Empty;
                        if (string.Compare(typeName, "NATIVE", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            string name = token["name"]?.ToString() ?? string.Empty;
                            if (string.Compare(name, domain, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string idName = token["id"]?.ToString() ?? string.Empty;
                                if (!string.IsNullOrEmpty(idName))
                                {
                                    return idName;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        private static string GetRecordId(string zoneId, string recordName)
        {
            try
            {
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append(BaseUrl);
                sbUrl.Append(@"zones/");
                sbUrl.Append(Uri.EscapeDataString(zoneId));
                sbUrl.Append("?");
                sbUrl.Append("recordName=");
                sbUrl.Append(Uri.EscapeDataString(recordName));
                sbUrl.Append("&recordType=TXT");

                HttpResponseMessage response = _httpClient.GetAsync(new Uri(sbUrl.ToString())).Result;
                bool success = response.IsSuccessStatusCode;
                if (success)
                {
                    string responseZonesResult = response.Content.ReadAsStringAsync().Result;
                    JToken resultJson = JToken.Parse(responseZonesResult);
                    JToken recordsToken = resultJson["records"];
                    if (recordsToken != null)
                    {
                        foreach (JToken token in recordsToken)
                        {
                            string typeName = token["type"]?.ToString() ?? string.Empty;
                            if (string.Compare(typeName, "TXT", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                string name = token["name"]?.ToString() ?? string.Empty;
                                if (string.Compare(name, recordName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    string idName = token["id"]?.ToString() ?? string.Empty;
                                    if (!string.IsNullOrEmpty(idName))
                                    {
                                        return idName;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

        private static bool DeleteRecord(string zoneId, string recordId)
        {
            try
            {
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append(BaseUrl);
                sbUrl.Append(@"zones/");
                sbUrl.Append(Uri.EscapeDataString(zoneId));
                sbUrl.Append(@"/records/");
                sbUrl.Append(Uri.EscapeDataString(recordId));

                HttpResponseMessage response = _httpClient.DeleteAsync(new Uri(sbUrl.ToString())).Result;
                bool success = response.IsSuccessStatusCode;
                if (success)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        private static bool CreateRecord(string zoneId, string recordName, string recordToken)
        {
            try
            {
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append(BaseUrl);
                sbUrl.Append(@"zones/");
                sbUrl.Append(Uri.EscapeDataString(zoneId));
                sbUrl.Append(@"/records");

                string jsonText = "[{\"name\":\"" + recordName + "\", \"type\":\"TXT\", \"content\":\"" + recordToken + "\", \"ttl\":60, \"prio\":100, \"disabled\":false}]";
                StringContent content = new StringContent(jsonText, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = _httpClient.PostAsync(new Uri(sbUrl.ToString()), content).Result;
                bool success = response.IsSuccessStatusCode;
                if (success)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

    }
}
