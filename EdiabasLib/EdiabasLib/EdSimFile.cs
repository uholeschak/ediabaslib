using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EdiabasLib
{
    public class EdSimFile
    {
        private const string SectionPowerSupply = "POWERSUPPLY";
        private const string SectionIgnition = "IGNITION";
        private const string SectionRequest = "REQUEST";
        private const string SectionResponse = "RESPONSE";
        private readonly string _fileName;
        private readonly IniFile _iniFile;
        private List<ResponseInfo> _responseInfos;

        public string FileName => _fileName;

        public int UBatVolt { get; private set; }

        public int IgnitionVolt { get; private set; }

        public class ResponseInfo
        {
            public ResponseInfo(List<byte> requestData, List<byte> requestMask, List<byte> responseData)
            {
                RequestData = requestData;
                RequestMask = requestMask;
                ResponseData = responseData;
            }

            public List<byte> RequestData { get; private set; }

            public List<byte> RequestMask { get; private set; }

            public List<byte> ResponseData { get; private set; }
        }

        public EdSimFile(string fileName)
        {
            _fileName = fileName;
            _iniFile = new IniFile(fileName);

            UBatVolt = -1;
            IgnitionVolt = -1;
            ParseIniFile();
        }

        public List<byte> GetResponse(List<byte> request)
        {
            foreach (ResponseInfo responseInfo in _responseInfos)
            {
                if (request.Count != responseInfo.RequestData.Count)
                {
                    continue;
                }

                bool matched = true;
                for (int i = 0; i < request.Count; ++i)
                {
                    byte mask = responseInfo.RequestMask[i];
                    if ((request[i] & mask) != (responseInfo.RequestData[i] & mask))
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return responseInfo.ResponseData;
                }
            }

            return null;
        }

        private void ParseIniFile()
        {
            UBatVolt = _iniFile.GetValue(SectionPowerSupply, "UBatt", -1);
            IgnitionVolt = _iniFile.GetValue(SectionIgnition, "Ignition", -1);

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

                    List<byte> requestMask = new List<byte>();
                    List<byte> requestBytes = ParseHexString(requestValue, ref requestMask);
                    if (requestBytes == null)
                    {
                        continue;
                    }

                    List<byte> nullList = null;
                    List<byte> responseBytes = ParseHexString(responseValue, ref nullList);
                    if (responseBytes == null)
                    {
                        continue;
                    }

                    _responseInfos.Add(new ResponseInfo(requestBytes, requestMask, responseBytes));
                }
            }
        }

        private List<byte> ParseHexString(string text, ref List<byte> maskList)
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
                string partTrim = part.ToUpperInvariant().Trim();
                if (partTrim.Length != 2)
                {
                    return null;
                }

                StringBuilder sbValue = new StringBuilder();
                byte mask = 0xFF;
                if (maskList != null)
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

                if (maskList != null)
                {
                    maskList.Add(mask);
                }
            }

            return result;
        }
    }
}
