using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Programming.Controller.SecureCoding.Model;
using Newtonsoft.Json;
using PsdzClientLibrary.Core;

namespace PsdzClient.Programming
{
    public class JsonHelper
    {
        public ResponseJson ReadResponseJson(string jsonContent)
        {
            ResponseJson responseJson;
            try
            {
                if (string.IsNullOrEmpty(jsonContent))
                {
                    return null;
                }
                responseJson = JsonConvert.DeserializeObject<ResponseJson>(jsonContent);
                Log.Info(Log.CurrentMethod(), string.Format("Json file Content: {0}", responseJson), Array.Empty<object>());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return responseJson;
        }

        public RequestJson ReadRequestJson(string filePath)
        {
            RequestJson result;
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }
                string text = File.ReadAllText(filePath);
                result = JsonConvert.DeserializeObject<RequestJson>(text);
                Log.Info(Log.CurrentMethod(), "File: " + filePath + ": Content: " + text, Array.Empty<object>());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public string NormalizeJsonContent(string jsonContent)
        {
            return jsonContent.Replace("{", "{\"").Replace(":", "\":\"").Replace(",", "\",\"").Replace("}", "\"}").Replace(":\"[", ":[").Replace(":\"{", ":{").Replace("https\":\"", "https:").Replace("http\":\"", "http:").Replace("\":\"9", ":9").Replace("}\",", "},").Replace("]\",", "],").Replace("}\"}", "}}");
        }
    }
}
