using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using Newtonsoft.Json;
using PsdzClient.Core;


namespace PsdzClient.Programming
{
    internal class JsonHelper
    {
        public ResponseJson ReadResponseJson(string jsonContent)
        {
            if (string.IsNullOrEmpty(jsonContent))
            {
                return null;
            }
            ResponseJson responseJson = JsonConvert.DeserializeObject<ResponseJson>(jsonContent);
            Log.Info(Log.CurrentMethod(), $"Json file Content: {responseJson}");
            return responseJson;
        }

        public RequestJson ReadRequestJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            string text = File.ReadAllText(filePath);
            RequestJson result = JsonConvert.DeserializeObject<RequestJson>(text);
            Log.Info(Log.CurrentMethod(), "File: " + filePath + ": Content: " + text);
            return result;
        }

        public string NormalizeJsonContent(string jsonContent)
        {
            return jsonContent.Replace("{", "{\"").Replace(":", "\":\"").Replace(",", "\",\"")
                .Replace("}", "\"}")
                .Replace(":\"[", ":[")
                .Replace(":\"{", ":{")
                .Replace("https\":\"", "https:")
                .Replace("http\":\"", "http:")
                .Replace("\":\"9", ":9")
                .Replace("}\",", "},")
                .Replace("]\",", "],")
                .Replace("}\"}", "}}");
        }
    }
}
