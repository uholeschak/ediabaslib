using System;
using System.Collections.Generic;
using System.Globalization;

namespace EdiabasLib
{
    public class EdSimFile
    {
        private const string SectionRequest = "REQUEST";
        private const string SectionResponse = "RESPONSE";
        private readonly string _fileName;
        private readonly IniFile _iniFile;
        private List<ResponseInfo> _responseInfos;

        public string FileName => _fileName;

        public class ResponseInfo
        {
            public ResponseInfo(List<byte> requestData, List<byte> responseData)
            {
                RequestData = requestData;
                ResponseData = responseData;
            }

            public List<byte> RequestData { get; private set; }

            public List<byte> ResponseData { get; private set; }
        }

        public EdSimFile(string fileName)
        {
            _fileName = fileName;
            _iniFile = new IniFile(fileName);
            ParseIniFile();
        }

        private bool ParseIniFile()
        {
            _responseInfos = new List<ResponseInfo>();
            List<string> requestKeys = _iniFile.GetKeys(SectionRequest);
            if (requestKeys != null)
            {
                foreach (string requestKey in requestKeys)
                {
                    string requestValue = _iniFile.GetValue(SectionRequest, requestKey, string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(requestValue))
                    {
                        continue;
                    }

                    string responseValue = _iniFile.GetValue(SectionResponse, requestKey, string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(responseValue))
                    {
                        continue;
                    }

                    List<byte> requestBytes = ParseHexString(requestValue);
                    if (requestBytes == null)
                    {
                        return false;
                    }

                    List<byte> responseBytes = ParseHexString(responseValue);
                    if (responseBytes == null)
                    {
                        return false;
                    }

                    _responseInfos.Add(new ResponseInfo(requestBytes, responseBytes));
                }
            }
            return true;
        }

        private List<byte> ParseHexString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (string.Compare(text, "_", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new List<byte>();
            }

            string[] parts = text.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            List<byte> result = new List<byte>();
            foreach (string part in parts)
            {
                string partTrim = part.Trim();
                if (partTrim.Length != 2)
                {
                    return null;
                }

                if (!UInt32.TryParse(partTrim, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt32 value))
                {
                    return null;
                }

                if (value > 0xFF)
                {
                    return null;
                }

                result.Add((byte)value);
            }

            return result;
        }
    }
}
