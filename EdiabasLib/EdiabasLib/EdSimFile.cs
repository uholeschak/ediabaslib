using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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

                    List<byte> requestBytes = ParseHexString(requestValue, true);
                    if (requestBytes == null)
                    {
                        continue;
                    }

                    List<byte> responseBytes = ParseHexString(responseValue, false);
                    if (responseBytes == null)
                    {
                        continue;
                    }

                    _responseInfos.Add(new ResponseInfo(requestBytes, responseBytes));
                }
            }
            return true;
        }

        private List<byte> ParseHexString(string text, bool request)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            if (string.Compare(text, "_", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (request)
                {
                    return null;
                }

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

                StringBuilder sbValue = new StringBuilder();
                byte mask = 0xFF;
                if (!request)
                {
                    if (partTrim[0] == 'X')
                    {
                        mask &= 0x0F;
                        sbValue.Append('0');
                    }
                    else
                    {
                        sbValue.Append(partTrim[0]);
                    }

                    if (partTrim[1] == 'X')
                    {
                        mask &= 0xF0;
                        sbValue.Append('0');
                    }
                    else
                    {
                        sbValue.Append(partTrim[1]);
                    }
                }
                else
                {
                    sbValue.Append(partTrim);
                }

                if (!UInt32.TryParse(sbValue.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt32 value))
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
