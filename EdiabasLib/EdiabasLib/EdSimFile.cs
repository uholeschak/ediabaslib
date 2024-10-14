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
        private List<ResponseInfo> _keyBytesInfos;

        public string FileName => _fileName;

        public bool FileValid => _fileValid;

        public int UBatVolt { get; private set; } = DefaultBatteryVolt;

        public int UBatCurrent { get; private set; } = -1;

        public int UBatHistory { get; private set; } = -1;


        public int IgnitionVolt { get; private set; } = DefaultIgnitionVolt;

        public int IgnitionCurrent { get; private set; } = -1;

        public int IgnitionHistory { get; private set; } = -1;

        private class DataItem : IEquatable<DataItem>
        {
            public enum OperatorType
            {
                And,
                Or,
                Xor,
                Plus,
                Minus,
                Multiply,
                Divide,
            }

            public enum OperatorDataType
            {
                EcuAddrL,
                EcuAddrH,
            }

            public DataItem(byte dataValue, byte? dataMask, OperatorType? operatorType, byte? operatorValue, uint? operatorIndex, OperatorDataType? operatorDataType)
            {
                DataValue = dataValue;
                DataMask = dataMask;
                Operator = operatorType;
                OperatorValue = operatorValue;
                OperatorIndex = operatorIndex;
                OperatorData = operatorDataType;

                _hashCode = DataValue.GetHashCode() ^ (DataMask?.GetHashCode() ?? 0) ^ (Operator?.GetHashCode() ?? 0) ^
                            (OperatorValue?.GetHashCode() ?? 0) ^ (OperatorIndex?.GetHashCode() ?? 0);
            }

            public override bool Equals(object obj)
            {
                DataItem dataItem = obj as DataItem;
                if ((object)dataItem == null)
                {
                    return false;
                }

                return Equals(dataItem);
            }

            public bool Equals(DataItem other)
            {
                if (other == null)
                {
                    return false;
                }

                if (DataValue == other.DataValue && DataMask == other.DataMask && Operator == other.Operator &&
                    OperatorValue == other.OperatorValue && OperatorIndex == other.OperatorIndex)
                {
                    return true;
                }

                return false;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            private readonly int _hashCode;

            public byte DataValue { get; private set; }

            public byte? DataMask { get; private set; }

            public OperatorType? Operator { get; private set; }

            public byte? OperatorValue { get; private set; }

            public uint? OperatorIndex { get; private set; }

            public OperatorDataType? OperatorData { get; private set; }
        }

        private class ResponseInfo
        {
            public ResponseInfo(List<DataItem> requestData, List<DataItem> responseData, int? ecuAddr)
            {
                RequestData = requestData;
                ResponseDataList = new List<List<DataItem>> { responseData };
                EcuAddr = ecuAddr;
                ResponseIndex = 0;
            }

            public List<DataItem> RequestData { get; private set; }

            public List<List<DataItem>> ResponseDataList { get; private set; }

            public int? EcuAddr { get; private set; }

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

        public List<byte> GetResponse(List<byte> request, int? ecuAddr = null)
        {
            for (int iteration = 0; iteration < 2; ++iteration)
            {
                foreach (ResponseInfo responseInfo in _responseInfos)
                {
                    if ((responseInfo.EcuAddr ?? -1) != (ecuAddr ?? -1))
                    {
                        continue;
                    }

                    if (request.Count != responseInfo.RequestData.Count)
                    {
                        continue;
                    }

                    if (iteration == 0)
                    {
                        bool variableData = false;
                        foreach (DataItem dataItem in responseInfo.RequestData)
                        {
                            if (dataItem.DataMask != null || dataItem.Operator != null)
                            {
                                variableData = true;
                                break;
                            }
                        }

                        if (variableData)
                        {
                            continue;
                        }
                    }

                    List<byte> requestInfoList = ConvertData(responseInfo.RequestData, null, request, ecuAddr);
                    if (requestInfoList == null)
                    {
                        continue;
                    }

                    List<byte> requestDataList = ConvertData(responseInfo.RequestData, request, request, ecuAddr);
                    if (requestDataList == null)
                    {
                        continue;
                    }

                    if (requestInfoList.Count != requestDataList.Count)
                    {
                        continue;
                    }

                    bool matched = true;
                    for (int i = 0; i < requestDataList.Count; ++i)
                    {
                        if (requestInfoList[i] != (requestDataList[i]))
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

                        return ConvertData(response, null, request, ecuAddr);
                    }
                }
            }

            return null;
        }

        public List<byte> GetKeyBytes(int? ecuAddr = null)
        {
            foreach (ResponseInfo responseInfo in _keyBytesInfos)
            {
                if ((responseInfo.EcuAddr ?? -1) != (ecuAddr ?? -1))
                {
                    continue;
                }

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

                return ConvertData(response, null, null, ecuAddr);
            }

            return null;
        }

        private static List<byte> ConvertData(List<DataItem> dataItems, List<byte> inputData, List<byte> requestData, int? ecuAddr)
        {
            if (dataItems == null)
            {
                return null;
            }

            List<byte> dataBytesList = new List<byte>();
            int index = 0;
            foreach (DataItem dataItem in dataItems)
            {
                byte dataValue = dataItem.DataValue;
                if (inputData != null)
                {
                    if (index < inputData.Count)
                    {
                        dataValue = inputData[index];
                    }
                }

                if (dataItem.DataMask != null)
                {
                    dataValue &= dataItem.DataMask.Value;
                }

                if (dataItem.Operator != null)
                {
                    byte operatorValue;
                    if (dataItem.OperatorIndex != null)
                    {
                        uint operatorIndex = dataItem.OperatorIndex.Value;
                        if (requestData == null || requestData.Count < operatorIndex)
                        {
                            return null;
                        }

                        operatorValue = requestData[(int) operatorIndex];
                    }
                    else if (dataItem.OperatorData != null)
                    {
                        switch (dataItem.OperatorData.Value)
                        {
                            case DataItem.OperatorDataType.EcuAddrL:
                                if (ecuAddr == null)
                                {
                                    return null;
                                }

                                operatorValue = (byte) ecuAddr.Value;
                                break;

                            case DataItem.OperatorDataType.EcuAddrH:
                                if (ecuAddr == null)
                                {
                                    return null;
                                }

                                operatorValue = (byte)(ecuAddr.Value >> 8);
                                break;

                            default:
                                return null;
                        }
                    }
                    else if (dataItem.OperatorValue != null)
                    {
                        operatorValue = dataItem.OperatorValue.Value;
                    }
                    else
                    {
                        return null;
                    }

                    switch (dataItem.Operator)
                    {
                        case DataItem.OperatorType.And:
                            dataValue &= operatorValue;
                            break;

                        case DataItem.OperatorType.Or:
                            dataValue |= operatorValue;
                            break;

                        case DataItem.OperatorType.Xor:
                            dataValue ^= operatorValue;
                            break;

                        case DataItem.OperatorType.Plus:
                            dataValue += operatorValue;
                            break;

                        case DataItem.OperatorType.Minus:
                            dataValue -= operatorValue;
                            break;

                        case DataItem.OperatorType.Multiply:
                            dataValue *= operatorValue;
                            break;

                        case DataItem.OperatorType.Divide:
                            if (operatorValue == 0)
                            {
                                return null;
                            }

                            dataValue /= operatorValue;
                            break;
                    }
                }

                dataBytesList.Add(dataValue);
                index++;
            }

            return dataBytesList;
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
            _keyBytesInfos = new List<ResponseInfo>();
            List<string> sections =  _iniFile.GetSections();
            if (sections == null)
            {
                return false;
            }

            foreach (string section in sections)
            {
                if (!section.EndsWith(SectionRequest, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string sectionPrefix = section.Substring(0, section.Length - SectionRequest.Length);
                int? ecuAddr = null;

                if (sectionPrefix.Length > 0)
                {
                    ecuAddr = ParseSectionEcuAddr(sectionPrefix);
                }

                List<string> requestKeys = _iniFile.GetKeys(section);
                if (requestKeys != null)
                {
                    foreach (string requestKey in requestKeys)
                    {
                        string requestValue = _iniFile.GetValue(section, requestKey, string.Empty).Trim();
                        if (string.IsNullOrEmpty(requestValue))
                        {
                            return false;
                        }

                        string sectionResponse = sectionPrefix + SectionResponse;
                        string responseValue = _iniFile.GetValue(sectionResponse, requestKey, string.Empty).Trim();
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
                            if ((responseInfo.EcuAddr ?? -1) != (ecuAddr ?? -1))
                            {
                                continue;
                            }

                            if (requestBytes.Count != responseInfo.RequestData.Count)
                            {
                                continue;
                            }

                            bool identical = true;
                            for (int i = 0; i < requestBytes.Count; ++i)
                            {
                                if (!requestBytes[i].Equals(responseInfo.RequestData[i]))
                                {
                                    identical = false;
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
                            _responseInfos.Add(new ResponseInfo(requestBytes, responseBytes, ecuAddr));
                        }
                    }
                }
            }

            foreach (string section in sections)
            {
                if (!section.EndsWith(SectionKeybytes, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string sectionPrefix = section.Substring(0, section.Length - SectionKeybytes.Length);
                int? ecuAddr = null;

                if (sectionPrefix.Length > 0)
                {
                    ecuAddr = ParseSectionEcuAddr(sectionPrefix);
                }

                List<string> keyBytesKeys = _iniFile.GetKeys(section);
                if (keyBytesKeys != null)
                {
                    foreach (string keyBytesKey in keyBytesKeys)
                    {
                        string keyBytesValue = _iniFile.GetValue(section, keyBytesKey, string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(keyBytesValue))
                        {
                            continue;
                        }

                        List<DataItem> keyBytesBytes = ParseHexString(keyBytesValue, true);
                        if (keyBytesBytes == null)
                        {
                            return false;
                        }

                        ResponseInfo keyBytesInfoMatch = null;
                        foreach (ResponseInfo keyBytesInfo in _keyBytesInfos)
                        {
                            if ((keyBytesInfo.EcuAddr ?? -1) != (ecuAddr ?? -1))
                            {
                                continue;
                            }

                            keyBytesInfoMatch = keyBytesInfo;
                            break;
                        }

                        if (keyBytesInfoMatch != null)
                        {
                            keyBytesInfoMatch.ResponseDataList.Add(keyBytesBytes);
                        }
                        else
                        {
                            _keyBytesInfos.Add(new ResponseInfo(null, keyBytesBytes, ecuAddr));
                        }
                    }
                }
            }

            return true;
        }

        private int? ParseSectionEcuAddr(string sectionPrefix)
        {
            if (sectionPrefix.Length > 1 && sectionPrefix.EndsWith("."))
            {
                string sectionNumber = sectionPrefix.Substring(0, sectionPrefix.Length - 1);
                if (!int.TryParse(sectionNumber, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int sectionValue))
                {
                    return null;
                }

                return sectionValue;
            }

            return null;
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
                    sbValue.Append(partTrim.Substring(0, 2));
                }

                if (!byte.TryParse(sbValue.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte dataValue))
                {
                    return null;
                }

                byte? dataMask = null;
                if (mask != 0xFF)
                {
                    dataMask = mask;
                }

                DataItem.OperatorType? operatorType = null;
                DataItem.OperatorDataType? operatorDataType = null;
                byte? operatorValue = null;
                uint? operatorIndex = null;
                if (partTrim.Length >= 4)
                {
                    char operatorSymbol = partTrim[2];
                    switch (operatorSymbol)
                    {
                        case '|':
                            operatorType = DataItem.OperatorType.Or;
                            break;

                        case '&':
                            operatorType = DataItem.OperatorType.And;
                            break;

                        case '^':
                            operatorType = DataItem.OperatorType.Xor;
                            break;

                        case '+':
                            operatorType = DataItem.OperatorType.Plus;
                            break;

                        case '-':
                            operatorType = DataItem.OperatorType.Minus;
                            break;

                        case '*':
                            operatorType = DataItem.OperatorType.Multiply;
                            break;

                        case '/':
                            operatorType = DataItem.OperatorType.Divide;
                            break;

                        default:
                            return null;
                    }

                    string operatorString = partTrim.Substring(3);
                    if (operatorString.StartsWith("[") && operatorString.EndsWith("]"))
                    {
                        operatorString = operatorString.Substring(1, operatorString.Length - 2);

                        if (!uint.TryParse(operatorString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint opDataValue))
                        {
                            return null;
                        }

                        operatorIndex = opDataValue;
                    }
                    else if (operatorString.StartsWith("#"))
                    {
                        operatorString = operatorString.Substring(1, operatorString.Length - 1);
                        if (string.Compare(operatorString, "L", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            operatorDataType = DataItem.OperatorDataType.EcuAddrH;
                        }
                        else if (string.Compare(operatorString, "H", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            operatorDataType = DataItem.OperatorDataType.EcuAddrH;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (!byte.TryParse(operatorString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte opDataValue))
                        {
                            return null;
                        }

                        operatorValue = opDataValue;
                    }
                }

                result.Add(new DataItem(dataValue, dataMask, operatorType, operatorValue, operatorIndex, operatorDataType));
            }

            return result;
        }
    }
}
