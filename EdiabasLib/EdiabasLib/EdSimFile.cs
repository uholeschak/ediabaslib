using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace EdiabasLib
{
    public class EdSimFile
    {
        public const int DefaultBatteryVolt = 12000;
        public const int DefaultIgnitionVolt = 12000;
        private const string SectionPowerSupply = "POWERSUPPLY";
        private const string SectionIgnition = "IGNITION";
        private const string SectionRequest = "REQUEST";
        private const string SectionResponse = "RESPONSE";
        private const string SectionKeybytes = "KEYBYTES";
        private readonly string _fileName;
        private readonly bool _fileValid;
        private readonly IniFile _iniFile;
        private List<ResponseInfo> _responseInfos;

        public string FileName => _fileName;

        public bool FileValid => _fileValid;

        public int UBatVolt { get; private set; } = DefaultBatteryVolt;

        public int UBatCurrent { get; private set; } = -1;

        public int UBatHistory { get; private set; } = -1;


        public int IgnitionVolt { get; private set; } = DefaultIgnitionVolt;

        public int IgnitionCurrent { get; private set; } = -1;

        public int IgnitionHistory { get; private set; } = -1;

        public List<DataItem> KeyBytes { get; private set; }

        public class DataItem
        {
            public DataItem(byte? dataValue, byte? dataMask = null, uint? index = null)
            {
                DataValue = dataValue;
                DataMask = dataMask;
                Index = index;
            }

            public byte? DataValue { get; private set; }

            public byte? DataMask { get; private set; }

            public uint? Index { get; private set; }
        }

        public class ResponseInfo
        {
            public ResponseInfo(List<DataItem> requestData, List<DataItem> responseData)
            {
                RequestData = requestData;
                ResponseDataList = new List<List<DataItem>> { responseData };
                ResponseIndex = 0;
            }

            public List<DataItem> RequestData { get; private set; }

            public List<DataItem> RequestMask { get; private set; }

            public List<List<DataItem>> ResponseDataList { get; private set; }

            public int ResponseIndex { get; set; }
        }

        public EdSimFile(string fileName)
        {
            _fileName = fileName;
            _iniFile = new IniFile(fileName);

            _fileValid = ParseIniFile();
            ResetResponses();
        }

        public void ResetResponses()
        {
            foreach (ResponseInfo responseInfo in _responseInfos)
            {
                responseInfo.ResponseIndex = 0;
            }
        }

        public List<DataItem> GetResponse(List<byte> request)
        {
            for (int iteration = 0; iteration < 2; ++iteration)
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
                        byte? dataMask = responseInfo.RequestData[i].DataMask;
                        if (iteration == 0 && dataMask != null)
                        {   // try to match without mask first
                            continue;
                        }

                        byte maskValue = 0xFF;
                        if (dataMask != null)
                        {
                            maskValue = dataMask.Value;
                        }

                        byte? dataValue = responseInfo.RequestData[i].DataValue;
                        if (dataValue != null && (request[i] & maskValue) != (dataValue & maskValue))
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        int responseIndex = responseInfo.ResponseIndex;
                        List<DataItem> response = null;
                        if (responseIndex < responseInfo.ResponseDataList.Count)
                        {
                            response = responseInfo.ResponseDataList[responseIndex++];
                            if (responseIndex >= responseInfo.ResponseDataList.Count)
                            {
                                responseIndex = 0;
                            }
                            responseInfo.ResponseIndex = responseIndex;
                        }

                        return response;
                    }
                }
            }

            return null;
        }

        private bool ParseIniFile()
        {
            UBatVolt = _iniFile.GetValue(SectionPowerSupply, "UBatt", DefaultBatteryVolt);
            UBatCurrent = _iniFile.GetValue(SectionPowerSupply, "UBATTCURRENT", -1);
            UBatHistory = _iniFile.GetValue(SectionPowerSupply, "UBATTHISTORY", -1);

            IgnitionVolt = _iniFile.GetValue(SectionIgnition, "Ignition", DefaultIgnitionVolt);
            IgnitionCurrent = _iniFile.GetValue(SectionIgnition, "IGNITIONCURRENT", -1);
            IgnitionHistory = _iniFile.GetValue(SectionIgnition, "IGNITIONHISTORY", -1);

            _responseInfos = new List<ResponseInfo>();
            List<string> requestKeys = _iniFile.GetKeys(SectionRequest);
            if (requestKeys != null)
            {
                foreach (string requestKey in requestKeys)
                {
                    string requestValue = _iniFile.GetValue(SectionRequest, requestKey, string.Empty).Trim();
                    if (string.IsNullOrEmpty(requestValue))
                    {
                        return false;
                    }

                    string responseValue = _iniFile.GetValue(SectionResponse, requestKey, string.Empty).Trim();
                    if (string.IsNullOrEmpty(responseValue))
                    {
                        return false;
                    }

                    List<DataItem> requestBytes = ParseHexString(requestValue, true);
                    if (requestBytes == null)
                    {
                        return false;
                    }

                    List<DataItem> responseBytes = ParseHexString(responseValue, false);
                    if (responseBytes == null)
                    {
                        return false;
                    }

                    ResponseInfo responseInfoMatch = null;
                    foreach (ResponseInfo responseInfo in _responseInfos)
                    {
                        if (requestBytes.Count != responseInfo.RequestData.Count)
                        {
                            continue;
                        }

                        bool identical = true;
                        for (int i = 0; i < requestBytes.Count; ++i)
                        {
                            if (requestBytes[i].DataValue != responseInfo.RequestData[i].DataValue ||
                                requestBytes[i].DataMask != responseInfo.RequestData[i].DataMask)
                            {
                                identical = false;
                                break;
                            }
                        }

                        if (!identical)
                        {
                            continue;
                        }

                        responseInfoMatch = responseInfo;
                        break;
                    }

                    if (responseInfoMatch != null)
                    {
                        responseInfoMatch.ResponseDataList.Add(responseBytes);
                    }
                    else
                    {
                        _responseInfos.Add(new ResponseInfo(requestBytes, responseBytes));
                    }
                }
            }

            KeyBytes = null;
            List<string> keyBytesKeys = _iniFile.GetKeys(SectionKeybytes);
            if (keyBytesKeys != null)
            {
                foreach (string keyBytesKey in keyBytesKeys)
                {
                    string keyBytesValue = _iniFile.GetValue(SectionKeybytes, keyBytesKey, string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(keyBytesValue))
                    {
                        continue;
                    }

                    List<DataItem> keyBytesBytes = ParseHexString(keyBytesValue, true);
                    if (keyBytesBytes == null)
                    {
                        return false;
                    }

                    KeyBytes = keyBytesBytes;
                    break;
                }
            }

            return true;
        }

        private List<DataItem> ParseHexString(string text, bool request)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            string textTrim = text.Trim();
            if (string.IsNullOrWhiteSpace(textTrim))
            {
                return new List<DataItem>();
            }

            if (string.Compare(textTrim, "_", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new List<DataItem>();
            }

            string[] parts = textTrim.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            List<DataItem> result = new List<DataItem>();
            foreach (string part in parts)
            {
                string partTrim = part.ToUpperInvariant().Trim();
                if (partTrim.Length < 2)
                {
                    return null;
                }

                if (!request)
                {
                    if (partTrim[0] == 'I')
                    {
                        string indexText = partTrim.Substring(1);
                        if (!uint.TryParse(indexText, out uint indexValue))
                        {
                            return null;
                        }

                        result.Add(new DataItem(null, null, indexValue));
                        continue;
                    }
                }

                StringBuilder sbValue = new StringBuilder();
                byte mask = 0xFF;
                if (request)
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

                if (!uint.TryParse(sbValue.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint itemValue))
                {
                    return null;
                }

                if (itemValue > 0xFF)
                {
                    return null;
                }

                byte? dataValue = (byte)itemValue;
                byte? dataMask = null;
                if (mask != 0xFF)
                {
                    dataMask = mask;
                }

                result.Add(new DataItem(dataValue, dataMask));
            }

            return result;
        }
    }
}
