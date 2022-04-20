using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
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
                if (args.Length < 2)
                {
                    Console.WriteLine("No operation specified");
                    return 1;
                }

                string operation = args[0];
                bool create = string.Compare(operation, "create", StringComparison.OrdinalIgnoreCase) == 0;
                bool delete = string.Compare(operation, "delete", StringComparison.OrdinalIgnoreCase) == 0;

                if (!create && !delete)
                {
                    Console.WriteLine("Operation must be create or delete");
                    return 1;
                }

                string apiKey = args[1];
                _httpClient = new HttpClient(new HttpClientHandler()
                {
                    SslProtocols = SslProtocols.None,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                });

                _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("curl", "7.79.1"));

                string zonesId = GetZonesId();
                if (string.IsNullOrEmpty(zonesId))
                {
                    Console.WriteLine("No zones ID");
                    return 1;
                }

                GetRecord(zonesId, "_acme-challenge.holeschak.de");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                return 1;
            }

            return 0;
        }

        private static string GetZonesId()
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
                            string idName = token["id"]?.ToString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(idName))
                            {
                                return idName;
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

        private static string GetRecord(string zoneId, string recordName)
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
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }

    }
}
