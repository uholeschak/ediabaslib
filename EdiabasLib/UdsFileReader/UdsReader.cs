using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace UdsFileReader
{
    public class UdsReader
    {
        public const string FileExtension = ".uds";
        public const string UdsDir = "uds_ev";

        public enum SegmentType
        {
            Adp,
            Dtc,
            Ffmux,
            Ges,
            Mwb,
            Slv,
            Sot,
            Xpl,
        }

        public enum DataType
        {
            FloatScaled = 0,
            Binary1 = 1,
            Integer1 = 2,
            ValueName = 3,
            FixedEncoding = 4,
            Binary2 = 5,
            MuxTable = 6,
            HexBytes = 7,
            String = 8,
            HexScaled = 9,
            Integer2 = 10,
        }

        public const int DataTypeMaskSwapped = 0x40;
        public const int DataTypeMaskSigned = 0x80;
        public const int DataTypeMaskEnum = 0x3F;

        private readonly Dictionary<string, List<ParseInfoAdp>> _adpParseInfoDict = new Dictionary<string, List<ParseInfoAdp>>();
        private readonly Dictionary<string, Dictionary<string, ParseInfoMwb>> _mwbParseInfoDict = new Dictionary<string, Dictionary<string, ParseInfoMwb>>();
        private readonly Dictionary<string, List<ParseInfoBase>> _dtcMwbParseInfoDict = new Dictionary<string, List<ParseInfoBase>>();
        private readonly Dictionary<string, Dictionary<uint, ParseInfoDtc>> _dtcParseInfoDict = new Dictionary<string, Dictionary<uint, ParseInfoDtc>>();
        private readonly Dictionary<string, List<ParseInfoSlv>> _slvParseInfoDict = new Dictionary<string, List<ParseInfoSlv>>();

        public class FileNameResolver
        {
            public FileNameResolver(UdsReader udsReader, string vin, string asamData, string asamRev, string partNumber, string didHarwareNumber)
            {
                UdsReader = udsReader;
                Vin = vin;
                AsamData = asamData;
                AsamRev = asamRev;
                PartNumber = partNumber;
                DidHarwareNumber = didHarwareNumber;
                Manufacturer = string.Empty;
                AssemblyPlant = ' ';
                SerialNumber = -1;
                if (!string.IsNullOrEmpty(Vin) && Vin.Length >= 17)
                {
                    Manufacturer = Vin.Substring(0, 3);
                    AssemblyPlant = Vin[10];
                    string serial = Vin.Substring(11, 6);
                    if (Int64.TryParse(serial, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int64 serValue))
                    {
                        SerialNumber = serValue;
                    }
                }
                ModelYear = DataReader.GetModelYear(Vin);
            }

            public string GetChassisType(string modelCode)
            {
                if (string.IsNullOrEmpty(modelCode) || modelCode.Length < 2)
                {
                    return string.Empty;
                }
                string key = modelCode.Substring(0, 2).ToUpperInvariant();
                if (UdsReader._chassisMap.TryGetValue(key, out ChassisInfo chassisInfo))
                {
                    if (chassisInfo.ConverterFunc != null)
                    {
                        return chassisInfo.ConverterFunc(UdsReader, this);
                    }
                    return chassisInfo.ChassisName;
                }

                return string.Empty;
            }

            public string GetFileName(string rootDir)
            {
                string udsDir = Path.Combine(rootDir, UdsDir);
                List<string> fileList = GetFileList(udsDir);
                if (fileList == null || fileList.Count < 1)
                {
                    return null;
                }

                return fileList[0];
            }

            public static List<string> GetAllFiles(string fileName)
            {
                List<string> includeFiles = new List<string> { fileName };
                if (!GetIncludeFiles(fileName, includeFiles))
                {
                    return null;
                }

                return includeFiles;
            }

            public List<string> GetFileList(string dir)
            {
                try
                {
                    string chassisType = null;
                    if (ModelYear >= 0 && !string.IsNullOrEmpty(Vin) && Vin.Length >= 10)
                    {
                        chassisType = GetChassisType(Vin.Substring(6, 2));
                    }

                    if (string.IsNullOrEmpty(chassisType))
                    {
                        chassisType = GetChassisType(PartNumber);
                    }

                    if (string.IsNullOrEmpty(chassisType))
                    {
                        chassisType = GetChassisType(DidHarwareNumber);
                    }

                    if (string.IsNullOrEmpty(chassisType))
                    {
                        return null;
                    }

                    if (ChassisInvalid.Contains(chassisType))
                    {
                        return null;
                    }

                    if (string.IsNullOrEmpty(AsamRev) || AsamRev.Length < 3)
                    {
                        return null;
                    }

                    string baseRev = AsamRev.Substring(0, 3);

                    string fileName = Path.Combine(dir, AsamData + "_" + baseRev + "_" + chassisType + FileExtension);
                    List<string> fileList = UdsReader.GetFileList(fileName);
                    if (fileList != null)
                    {
                        return fileList;
                    }

                    fileName = Path.Combine(dir, AsamData + "_" + baseRev + FileExtension);
                    fileList = UdsReader.GetFileList(fileName);
                    if (fileList != null)
                    {
                        return fileList;
                    }

                    string chassisType2 = null;
                    if (string.Compare(chassisType, "VW32", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        chassisType2 = "VW36";
                    }
                    else if (string.Compare(chassisType, "VW36", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        chassisType2 = "VW32";
                    }

                    if (!string.IsNullOrEmpty(chassisType2))
                    {
                        fileName = Path.Combine(dir, AsamData + "_" + baseRev + "_" + chassisType2 + FileExtension);
                        fileList = UdsReader.GetFileList(fileName);
                        if (fileList != null)
                        {
                            return fileList;
                        }
                    }

                    string revNumberString = baseRev.Substring(1, 2);
                    if (!Int32.TryParse(revNumberString, NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 revNumber))
                    {
                        revNumber = -1;
                    }

                    int revNumberIndex = revNumber;
                    while (revNumberIndex >= 0)
                    {
                        string baseRevSub = baseRev.Substring(0, 1) + string.Format(CultureInfo.InvariantCulture, "{0:00}", revNumberIndex);
                        fileName = Path.Combine(dir, AsamData + "_" + baseRevSub + "_" + chassisType + FileExtension);
                        fileList = UdsReader.GetFileList(fileName);
                        if (fileList != null)
                        {
                            return fileList;
                        }
                        revNumberIndex--;
                    }

                    fileName = Path.Combine(dir, AsamData + "_" + chassisType + FileExtension);
                    fileList = UdsReader.GetFileList(fileName);
                    if (fileList != null)
                    {
                        return fileList;
                    }

                    revNumberIndex = revNumber;
                    while (revNumberIndex >= 0)
                    {
                        string baseRevSub = baseRev.Substring(0, 1) + string.Format(CultureInfo.InvariantCulture, "{0:00}", revNumberIndex);
                        fileName = Path.Combine(dir, AsamData + "_" + baseRevSub + FileExtension);
                        fileList = UdsReader.GetFileList(fileName);
                        if (fileList != null)
                        {
                            return fileList;
                        }
                        revNumberIndex--;
                    }

                    fileName = Path.Combine(dir, AsamData + FileExtension);
                    fileList = UdsReader.GetFileList(fileName);
                    if (fileList != null)
                    {
                        return fileList;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                return null;
            }

            public UdsReader UdsReader { get; }
            public string Vin { get; }
            public string AsamData { get; }
            public string AsamRev { get; }
            public string PartNumber { get; }
            public string DidHarwareNumber { get; }
            public string Manufacturer { get; }
            public char AssemblyPlant { get; }
            public long SerialNumber { get; }
            public int ModelYear { get; }
        }

        public class ValueName
        {
            public ValueName(UdsReader udsReader, string[] lineArray)
            {
                LineArray = lineArray;

                if (lineArray.Length >= 5)
                {
                    try
                    {
                        string textMin = lineArray[1];
                        if (textMin.Length >= 2 && textMin.Length % 2 == 0 && !textMin.StartsWith("0x") && textMin.StartsWith("0"))
                        {
                            if (Int64.TryParse(textMin, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Int64 minValue))
                            {
                                MinValue = minValue;
                            }
                        }
                        else
                        {
                            if (textMin.Length < 34)
                            {
                                object valueObjMin = new Int64Converter().ConvertFromInvariantString(textMin);
                                if (valueObjMin != null)
                                {
                                    MinValue = (Int64)valueObjMin;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    try
                    {
                        string textMax = lineArray[2];
                        if (textMax.Length >= 2 && textMax.Length % 2 == 0 && !textMax.StartsWith("0x") && textMax.StartsWith("0"))
                        {
                            if (Int64.TryParse(textMax, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out Int64 maxValue))
                            {
                                MaxValue = maxValue;
                            }
                        }
                        else
                        {
                            if (textMax.Length < 34)
                            {
                                object valueObjMax = new Int64Converter().ConvertFromInvariantString(textMax);
                                if (valueObjMax != null)
                                {
                                    MaxValue = (Int64) valueObjMax;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (UInt32.TryParse(lineArray[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 valueNameKey))
                    {
                        if (udsReader._textMap.TryGetValue(valueNameKey, out string[] nameValueArray))
                        {
                            NameArray = nameValueArray;
                        }
                    }
                }
            }

            public string[] LineArray { get; }
            public string[] NameArray { get; }
            public Int64? MinValue { get; }
            public Int64? MaxValue { get; }
        }

        public class MuxEntry
        {
            public MuxEntry(UdsReader udsReader, string[] lineArray)
            {
                LineArray = lineArray;

                if (lineArray.Length >= 8)
                {
                    if (string.Compare(lineArray[5], "D", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Default = true;
                    }
                    else
                    {
                        if (Int32.TryParse(lineArray[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 minValue))
                        {
                            MinValue = minValue;
                        }
                    }

                    if (string.Compare(lineArray[6], "D", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        Default = true;
                    }
                    else
                    {
                        if (Int32.TryParse(lineArray[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out Int32 maxValue))
                        {
                            MaxValue = maxValue;
                        }
                    }
                    DataTypeEntry = new DataTypeEntry(udsReader, lineArray, 7);
                }
            }

            public string[] LineArray { get; }
            public bool Default { get; }
            public Int32? MinValue { get; }
            public Int32? MaxValue { get; }
            public DataTypeEntry DataTypeEntry { get; }
        }

        public class FixedEncodingEntry
        {
            public delegate string ConvertDelegate(UdsReader udsReader, byte[] data);

            public FixedEncodingEntry(UInt32[] keyArray, UInt32 dataLength, UInt32? unitKey = null, UInt32? numberOfDigits = null, double? scaleOffset = null, double? scaleMult = null, string unitExtra = null)
            {
                KeyArray = keyArray;
                DataLength = dataLength;
                UnitKey = unitKey;
                NumberOfDigits = numberOfDigits;
                ScaleOffset = scaleOffset;
                ScaleMult = scaleMult;
                UnitExtra = unitExtra;
            }

            public FixedEncodingEntry(UInt32[] keyArray, ConvertDelegate convertFunc)
            {
                KeyArray = keyArray;
                ConvertFunc = convertFunc;
            }

            public string ToString(UdsReader udsReader, byte[] data)
            {
                if (ConvertFunc != null)
                {
                    return ConvertFunc(udsReader, data);
                }

                if (DataLength == 0)
                {
                    return string.Empty;
                }

                if (data.Length < DataLength)
                {
                    return string.Empty;
                }

                UInt32 value;
                switch (DataLength)
                {
                    case 1:
                        value = data[0];
                        break;

                    case 2:
                        value = (UInt32) (data[0] << 8) | data[1];
                        break;

                    default:
                        return string.Empty;
                }

                double displayValue = value;
                if (ScaleOffset.HasValue)
                {
                    displayValue += ScaleOffset.Value;
                }
                if (ScaleMult.HasValue)
                {
                    displayValue *= ScaleMult.Value;
                }

                StringBuilder sb = new StringBuilder();
                UInt32 numberOfDigits = NumberOfDigits ?? 0;
                sb.Append(displayValue.ToString($"F{numberOfDigits}"));

                if (UnitKey.HasValue)
                {
                    sb.Append(" ");
                    sb.Append(GetUnitMapText(udsReader, UnitKey.Value) ?? string.Empty);
                }
                if (UnitExtra != null)
                {
                    sb.Append(" ");
                    sb.Append(UnitExtra);
                }

                return sb.ToString();
            }

            public UInt32[] KeyArray { get; }
            public UInt32 DataLength { get; }
            public UInt32? UnitKey { get; }
            public UInt32? NumberOfDigits { get; }
            public double? ScaleOffset { get; }
            public double? ScaleMult { get; }
            public string UnitExtra { get; }
            public ConvertDelegate ConvertFunc { get; }
        }

        public class DataTypeEntry
        {
            public DataTypeEntry(UdsReader udsReader, string[] lineArray, int offset)
            {
                UdsReader = udsReader;
                LineArray = lineArray;

                if (lineArray.Length >= offset + 10)
                {
                    UInt32 dataTypeId;
                    if (lineArray[offset + 1].Length > 0)
                    {
                        if (!UInt32.TryParse(lineArray[offset + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out dataTypeId))
                        {
                            throw new Exception("No data type id");
                        }
                    }
                    else
                    {
                        dataTypeId = (UInt32)DataType.HexBytes;
                    }

                    DataTypeId = dataTypeId;
                    DataType dataType = (DataType)(dataTypeId & DataTypeMaskEnum);

                    Int64? dataTypeExtra = null;
                    try
                    {
                        if (lineArray[offset].Length > 0)
                        {
                            object valueObjExtra = new Int64Converter().ConvertFromInvariantString(lineArray[offset]);
                            if (valueObjExtra != null)
                            {
                                dataTypeExtra = (Int64)valueObjExtra;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (UInt32.TryParse(lineArray[offset + 6], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 byteOffset))
                    {
                        ByteOffset = byteOffset;
                    }

                    if (UInt32.TryParse(lineArray[offset + 7], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 bitOffset))
                    {
                        BitOffset = bitOffset;
                    }

                    if (UInt32.TryParse(lineArray[offset + 8], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 bitLength))
                    {
                        BitLength = bitLength;
                    }

                    if (BitLength.HasValue)
                    {
                        MinTelLength = (ByteOffset ?? 0) + ((BitLength ?? 0) + (BitOffset ?? 0) + 7) / 8;
                    }

                    if (UInt32.TryParse(lineArray[offset + 9], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 nameDetailKey))
                    {
                        if (udsReader._textMap.TryGetValue(nameDetailKey, out string[] nameDetailArray))
                        {
                            NameDetailKey = nameDetailKey;
                            NameDetailArray = nameDetailArray;
                            if (nameDetailArray != null)
                            {
                                if (nameDetailArray.Length >= 1)
                                {
                                    NameDetail = nameDetailArray[0];
                                }
                                if (nameDetailArray.Length >= 3)
                                {
                                    if (UInt32.TryParse(nameDetailArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 valueType))
                                    {
                                        DataDetailIdType = valueType;
                                    }
                                    if (UInt32.TryParse(nameDetailArray[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 valueId))
                                    {
                                        DataDetailId = valueId;
                                    }
                                }
                            }
                        }
                    }

                    if (UInt32.TryParse(lineArray[offset + 5], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 unitKey) && unitKey > 0)
                    {
                        if (!udsReader._unitMap.TryGetValue(unitKey, out string[] unitArray))
                        {
                            throw new Exception("No unit text found");
                        }
                        if (unitArray.Length < 1)
                        {
                            throw new Exception("No unit array too short");
                        }
                        UnitText = unitArray[0];
                    }

                    switch (dataType)
                    {
                        case DataType.FloatScaled:
                        case DataType.Integer1:
                        {
                            if (double.TryParse(lineArray[offset + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out double scaleOffset))
                            {
                                ScaleOffset = scaleOffset;
                            }

                            if (double.TryParse(lineArray[offset + 3], NumberStyles.Float, CultureInfo.InvariantCulture, out double scaleMult))
                            {
                                ScaleMult = scaleMult;
                            }

                            if (double.TryParse(lineArray[offset + 4], NumberStyles.Float, CultureInfo.InvariantCulture, out double scaleDiv))
                            {
                                ScaleDiv = scaleDiv;
                            }

                            NumberOfDigits = dataTypeExtra;
                            break;
                        }

                        case DataType.ValueName:
                        {
                            if (dataTypeExtra == null)
                            {
                                break;
                            }

                            NameValueList = new List<ValueName>();
                            IEnumerable<string[]> bitList = udsReader._ttdopLookup[(uint)dataTypeExtra.Value];
                            foreach (string[] ttdopArray in bitList)
                            {
                                if (ttdopArray.Length >= 5)
                                {
                                    NameValueList.Add(new ValueName(udsReader, ttdopArray));
                                }
                            }
                            break;
                        }

                        case DataType.FixedEncoding:
                        {
                            if (!UInt32.TryParse(lineArray[offset + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 fixedEncodingId))
                            {
                                throw new Exception("No fixed data type id");
                            }
                            FixedEncodingId = fixedEncodingId;

                            if (!udsReader._fixedEncodingMap.TryGetValue(fixedEncodingId, out FixedEncodingEntry fixedEncodingEntry))
                            {
                                break;
                            }

                            FixedEncoding = fixedEncodingEntry;
                            NumberOfDigits = fixedEncodingEntry.NumberOfDigits;
                            ScaleOffset = fixedEncodingEntry.ScaleOffset;
                            ScaleMult = fixedEncodingEntry.ScaleMult;

                            if (fixedEncodingEntry.UnitKey != null)
                            {
                                if (!udsReader._unitMap.TryGetValue(fixedEncodingEntry.UnitKey.Value, out string[] unitArray))
                                {
                                    throw new Exception("No unit text found");
                                }
                                if (unitArray.Length < 1)
                                {
                                    throw new Exception("No unit array too short");
                                }
                                UnitText = unitArray[0];
                            }
                            break;
                        }

                        case DataType.MuxTable:
                        {
                            if (dataTypeExtra == null)
                            {   // possible for lines with data length 0
                                break;
                            }

                            MuxEntryList = new List<MuxEntry>();
                            IEnumerable<string[]> muxList = udsReader._muxLookup[(uint)dataTypeExtra.Value];
                            foreach (string[] muxArray in muxList)
                            {
                                if (muxArray.Length >= 17)
                                {
                                    MuxEntryList.Add(new MuxEntry(udsReader, muxArray));
                                }
                            }
                            break;
                        }
                    }
                }
            }

            public UdsReader UdsReader { get; }
            public string[] LineArray { get; }
            public UInt32 DataTypeId { get; }
            public UInt32? NameDetailKey { get; }
            public string[] NameDetailArray { get; }
            public string NameDetail { get; }
            public UInt32? DataDetailId { get; }
            public UInt32? DataDetailIdType { get; }
            public Int64? NumberOfDigits { get; }
            public UInt32? FixedEncodingId { get; }
            public double? ScaleOffset { get; }
            public double? ScaleMult { get; }
            public double? ScaleDiv { get; }
            public string UnitText { get; }
            public UInt32? ByteOffset { get; }
            public UInt32? BitOffset { get; }
            public UInt32? BitLength { get; }
            public UInt32? MinTelLength { get; }
            public List<ValueName> NameValueList { get; }
            public List<MuxEntry> MuxEntryList { get; }
            public FixedEncodingEntry FixedEncoding { get; }

            public static string DataTypeIdToString(UInt32 dataTypeId)
            {
                UInt32 dataTypeEnum = dataTypeId & DataTypeMaskEnum;
                string dataTypeName = Enum.GetName(typeof(DataType), dataTypeEnum);
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (dataTypeName == null)
                {
                    dataTypeName = string.Format(CultureInfo.InvariantCulture, "{0}", dataTypeEnum);
                }

                if ((dataTypeId & DataTypeMaskSwapped) != 0x00)
                {
                    dataTypeName += " (Swapped)";
                }
                if ((dataTypeId & DataTypeMaskSigned) != 0x00)
                {
                    dataTypeName += " (Signed)";
                }

                return dataTypeName;
            }

            public bool HasDataValue()
            {
                DataType dataType = (DataType)(DataTypeId & DataTypeMaskEnum);
                switch (dataType)
                {
                    case DataType.FloatScaled:
                    case DataType.HexScaled:
                    case DataType.Integer1:
                    case DataType.Integer2:
                        return true;
                }
                return false;
            }

            public string ToString(byte[] data)
            {
                return ToString(null, data, out double? _);
            }

            public string ToString(CultureInfo cultureInfo, byte[] data, out double? stringDataValue)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ToString(cultureInfo, data, out string unitText, out stringDataValue));
                if (!string.IsNullOrEmpty(unitText))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append(unitText);
                }

                return sb.ToString();
            }

            public string ToString(CultureInfo cultureInfo, byte[] data, out string unitText, out double? stringDataValue)
            {
                stringDataValue = null;
                string result = ToString(cultureInfo, data, null, out unitText, out object dataValue, out UInt32? _, out byte[] _);

                DataType dataType = (DataType)(DataTypeId & DataTypeMaskEnum);
                if (dataType != DataType.ValueName)
                {
                    if (dataValue is double dataValueDouble)
                    {
                        stringDataValue = dataValueDouble;
                    }
                    else if (dataValue is UInt64 dataValueUint)
                    {
                        stringDataValue = dataValueUint;
                    }
                    else if (dataValue is Int64 dataValueInt)
                    {
                        stringDataValue = dataValueInt;
                    }
                }

                return result;
            }

            public string ToString(CultureInfo cultureInfo, byte[] data, object newValueObject, out string unitText, out object stringDataValue, out UInt32? usedBitLength, out byte[] dataNew, bool hideInvalid = false)
            {
                string result = DataToString(cultureInfo, data, newValueObject, out unitText, out stringDataValue, out usedBitLength, out dataNew, hideInvalid);
                if (dataNew != null)
                {
                    result = DataToString(cultureInfo, dataNew, null, out unitText, out stringDataValue, out usedBitLength, out byte[] _, hideInvalid);
                }
                return result;
            }

            private string DataToString(CultureInfo cultureInfo, byte[] data, object newValueObject, out string unitText, out object stringDataValue, out UInt32? usedBitLength, out byte[] dataNew, bool hideInvalid)
            {
                unitText = null;
                stringDataValue = null;
                usedBitLength = null;
                dataNew = null;
                if (data.Length == 0)
                {
                    return string.Empty;
                }

                string newValueString = newValueObject as string;
                UInt64? newValue = null;
                double? newScaledValue = null;
                byte[] newDataBytes = null;
                UInt32 bitOffset = BitOffset ?? 0;
                UInt32 byteOffset = ByteOffset ?? 0;
                if (byteOffset > data.Length)
                {
                    return string.Empty;
                }
                int maxLength = data.Length - (int)byteOffset;
                int bitLength = maxLength * 8;
                int byteLength = maxLength;
                if (BitLength.HasValue)
                {
                    bitLength = (int)BitLength.Value;
                    byteLength = (int) ((bitLength + bitOffset + 7) / 8);
                }
                if ((bitLength < 1) || (data.Length < byteOffset + byteLength))
                {
                    return string.Empty;
                }

                byte[] subData = new byte[byteLength];
                Array.Copy(data, byteOffset, subData, 0, byteLength);
                if (bitOffset > 0 || (bitLength & 0x7) != 0)
                {
                    BitArray bitArray = new BitArray(subData);
                    if (bitOffset > bitArray.Length)
                    {
                        return string.Empty;
                    }
                    // shift bits to the left
                    for (int i = 0; i < bitArray.Length - bitOffset; i++)
                    {
                        bitArray[i] = bitArray[(int)(i + bitOffset)];
                    }
                    // clear unused bits
                    for (int i = bitLength; i < bitArray.Length; i++)
                    {
                        bitArray[i] = false;
                    }
                    bitArray.CopyTo(subData, 0);
                }

                usedBitLength = (UInt32)bitLength;

                CultureInfo oldCulture = null;
                try
                {
                    if (cultureInfo != null)
                    {
                        oldCulture = CultureInfo.CurrentCulture;
                        CultureInfo.CurrentCulture = cultureInfo;
                    }
                    StringBuilder sb = new StringBuilder();
                    DataType dataType = (DataType) (DataTypeId & DataTypeMaskEnum);
                    switch (dataType)
                    {
                        case DataType.FloatScaled:
                        case DataType.HexScaled:
                        case DataType.Integer1:
                        case DataType.Integer2:
                        case DataType.ValueName:
                        case DataType.MuxTable:
                        {
                            if (usedBitLength.Value > sizeof(UInt64) * 8)
                            {
                                usedBitLength = sizeof(UInt64) * 8;
                            }

                            UInt64 value = 0;
                            if ((DataTypeId & DataTypeMaskSwapped) != 0)
                            {
                                for (int i = 0; i < byteLength; i++)
                                {
                                    value <<= 8;
                                    value |= subData[byteLength - i - 1];
                                }
                            }
                            else
                            {
                                for (int i = 0; i < byteLength; i++)
                                {
                                    value <<= 8;
                                    value |= subData[i];
                                }
                            }

                            if (dataType == DataType.ValueName)
                            {
                                if (NameValueList == null)
                                {
                                    return string.Empty;
                                }

                                if (newValueObject is UInt64 newValueUlong)
                                {
                                    newValue = newValueUlong;
                                }

                                foreach (ValueName valueName in NameValueList)
                                {
                                    // ReSharper disable once ReplaceWithSingleAssignment.True
                                    bool match = true;
                                    if (valueName.MinValue.HasValue && (Int64)value < valueName.MinValue.Value)
                                    {
                                        match = false;
                                    }
                                    if (valueName.MaxValue.HasValue && (Int64)value > valueName.MaxValue.Value)
                                    {
                                        match = false;
                                    }
                                    if (match)
                                    {
                                        if (valueName.NameArray != null && valueName.NameArray.Length > 0)
                                        {
                                            stringDataValue = value;
                                            return valueName.NameArray[0];
                                        }
                                        return string.Empty;
                                    }
                                }

                                if (hideInvalid)
                                {
                                    return string.Empty;
                                }
                                return $"{GetTextMapText(UdsReader, 3455) ?? string.Empty}: {value}"; // Unbekannt
                            }

                            if (dataType == DataType.MuxTable)
                            {
                                if (MuxEntryList == null)
                                {
                                    return string.Empty;
                                }

                                MuxEntry muxEntryDefault = null;
                                foreach (MuxEntry muxEntry in MuxEntryList)
                                {
                                    if (muxEntry.Default)
                                    {
                                        muxEntryDefault = muxEntry;
                                        continue;
                                    }
                                    // ReSharper disable once ReplaceWithSingleAssignment.True
                                    bool match = true;
                                    if (muxEntry.MinValue.HasValue && (Int64)value < muxEntry.MinValue.Value)
                                    {
                                        match = false;
                                    }
                                    if (muxEntry.MaxValue.HasValue && (Int64)value > muxEntry.MaxValue.Value)
                                    {
                                        match = false;
                                    }
                                    if (match)
                                    {
                                        return muxEntry.DataTypeEntry.ToString(subData);
                                    }
                                }

                                if (muxEntryDefault != null)
                                {
                                    return muxEntryDefault.DataTypeEntry.ToString(subData);
                                }
                                return $"{GetTextMapText(UdsReader, 3455) ?? string.Empty}: {value}"; // Unbekannt
                            }

                            double scaledValue;
                            if ((DataTypeId & DataTypeMaskSigned) != 0)
                            {
                                UInt64 valueConv = value;
                                UInt64 signMask = (UInt64)1 << (bitLength - 1);
                                if ((signMask & value) != 0)
                                {
                                    valueConv = (value ^ signMask) - signMask;  // sign extend
                                }
                                Int64 valueSigned = (Int64)valueConv;

                                if (dataType == DataType.Integer1 || dataType == DataType.Integer2)
                                {
                                    if (newValueString != null)
                                    {
                                        try
                                        {
                                            newValue = (UInt64) Convert.ToInt64(newValueString);
                                        }
                                        catch (Exception)
                                        {
                                            // ignored
                                        }
                                    }
                                    sb.Append($"{valueSigned}");
                                    stringDataValue = valueSigned;
                                    break;
                                }
                                scaledValue = valueSigned;
                            }
                            else
                            {
                                if (dataType == DataType.Integer1 || dataType == DataType.Integer2)
                                {
                                    if (newValueString != null)
                                    {
                                        try
                                        {
                                            newValue = Convert.ToUInt64(newValueString);
                                        }
                                        catch (Exception)
                                        {
                                            // ignored
                                        }
                                    }
                                    sb.Append($"{value}");
                                    stringDataValue = value;
                                    break;
                                }
                                scaledValue = value;
                            }

                            try
                            {
                                if (ScaleMult.HasValue)
                                {
                                    scaledValue *= ScaleMult.Value;
                                }
                                if (ScaleOffset.HasValue)
                                {
                                    scaledValue += ScaleOffset.Value;
                                }
                                if (ScaleDiv.HasValue)
                                {
                                    scaledValue /= ScaleDiv.Value;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            if (dataType == DataType.HexScaled)
                            {
                                if (newValueString != null)
                                {
                                    try
                                    {
                                        newScaledValue = Convert.ToUInt64(newValueString, 16);
                                    }
                                    catch (Exception)
                                    {
                                        // ignored
                                    }
                                }
                                sb.Append($"{(UInt64)scaledValue:X}");
                                stringDataValue = scaledValue;
                                break;
                            }

                            if (newValueString != null)
                            {
                                try
                                {
                                    newScaledValue = Convert.ToDouble(newValueString);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            sb.Append(scaledValue.ToString($"F{NumberOfDigits ?? 0}"));
                            stringDataValue = scaledValue;
                            break;
                        }

                        case DataType.Binary1:
                        case DataType.Binary2:
                        {
                            if (newValueString != null)
                            {
                                try
                                {
                                    string[] newValueArray = newValueString.Trim().Split(' ', ';', ',');
                                    List<byte> binList = new List<byte>();
                                    foreach (string arg in newValueArray)
                                    {
                                        if (!string.IsNullOrEmpty(arg))
                                        {
                                            binList.Add(Convert.ToByte(arg, 2));
                                        }
                                    }

                                    if (binList.Count == subData.Length)
                                    {
                                        newDataBytes = binList.ToArray();
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            foreach (byte value in subData)
                            {
                                if (sb.Length > 0)
                                {
                                    sb.Append(" ");
                                }
                                sb.Append(Convert.ToString(value, 2).PadLeft(8, '0'));
                            }
                            break;
                        }

                        case DataType.HexBytes:
                            if (newValueString != null)
                            {
                                string[] newValueArray = newValueString.Trim().Split(' ', ';', ',');
                                try
                                {
                                    List<byte> binList = new List<byte>();
                                    foreach (string arg in newValueArray)
                                    {
                                        if (!string.IsNullOrEmpty(arg))
                                        {
                                            binList.Add(Convert.ToByte(arg, 16));
                                        }
                                    }

                                    if (binList.Count == subData.Length)
                                    {
                                        newDataBytes = binList.ToArray();
                                    }
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            sb.Append(BitConverter.ToString(subData).Replace("-", " "));
                            break;

                        case DataType.FixedEncoding:
                            return FixedEncoding.ToString(UdsReader, subData);

                        case DataType.String:
                            if (newValueString != null)
                            {
                                try
                                {
                                    newDataBytes = new byte[subData.Length];
                                    int dataLength = newValueString.Length > newDataBytes.Length ? newDataBytes.Length : newValueString.Length;
                                    DataReader.EncodingLatin1.GetBytes(newValueString, 0, dataLength, newDataBytes, 0);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            sb.Append(DataReader.EncodingLatin1.GetString(subData).TrimEnd('\0', ' '));
                            break;

                        default:
                            return string.Empty;
                    }

                    unitText = UnitText;
                    return sb.ToString();
                }
                finally
                {
                    UInt64 maxValueUnsigned = UInt64.MaxValue;
                    Int64 maxValueSigned = Int64.MaxValue;
                    Int64 minValueSigned = Int64.MinValue;

                    if (newScaledValue.HasValue || newValue.HasValue)
                    {
                        maxValueUnsigned = 0;
                        for (int i = 0; i < bitLength; i++)
                        {
                            maxValueUnsigned <<= 1;
                            maxValueUnsigned |= 0x01;
                        }

                        maxValueSigned = (Int64)(maxValueUnsigned >> 1);
                        minValueSigned = ~maxValueSigned;
                    }

                    if (newScaledValue.HasValue)
                    {
                        try
                        {
                            double tempValue = newScaledValue.Value;
                            if (ScaleDiv.HasValue)
                            {
                                tempValue *= ScaleDiv.Value;
                            }
                            if (ScaleOffset.HasValue)
                            {
                                tempValue -= ScaleOffset.Value;
                            }
                            if (ScaleMult.HasValue)
                            {
                                tempValue /= ScaleMult.Value;
                            }

                            if ((DataTypeId & DataTypeMaskSigned) != 0)
                            {
                                if (tempValue > maxValueSigned)
                                {
                                    newValue = (UInt64)maxValueSigned;
                                }
                                else if (tempValue < minValueSigned)
                                {
                                    newValue = (UInt64) minValueSigned;
                                }
                                else
                                {
                                    Int64 valueSigned = (Int64)tempValue;
                                    newValue = (UInt64)valueSigned;
                                }
                            }
                            else
                            {
                                if (tempValue > maxValueUnsigned)
                                {
                                    newValue = maxValueUnsigned;
                                }
                                else if (tempValue < 0)
                                {
                                    newValue = 0;
                                }
                                else
                                {
                                    newValue = (UInt64)tempValue;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }

                    if (newValue.HasValue)
                    {
                        if ((DataTypeId & DataTypeMaskSigned) != 0)
                        {
                            Int64 valueSigned = (Int64)newValue.Value;
                            if (valueSigned > maxValueSigned)
                            {
                                newValue = (UInt64)maxValueSigned;
                            }
                            else if (valueSigned < minValueSigned)
                            {
                                newValue = (UInt64)minValueSigned;
                            }
                        }
                        else
                        {
                            if (newValue.Value > maxValueUnsigned)
                            {
                                newValue = maxValueUnsigned;
                            }
                        }

                        newDataBytes = new byte[byteLength];
                        UInt64 tempValue = newValue.Value;
                        if ((DataTypeId & DataTypeMaskSwapped) != 0)
                        {
                            for (int i = 0; i < byteLength; i++)
                            {
                                newDataBytes[i] = (byte)tempValue;
                                tempValue >>= 8;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < byteLength; i++)
                            {
                                newDataBytes[byteLength - i - 1] = (byte)tempValue;
                                tempValue >>= 8;
                            }
                        }
                    }

                    if (newDataBytes != null && newDataBytes.Length == byteLength)
                    {
                        if (bitOffset > 0 || (bitLength & 0x7) != 0)
                        {
                            byte[] subDataOld = new byte[byteLength];
                            Array.Copy(data, byteOffset, subDataOld, 0, byteLength);
                            BitArray bitArrayOld = new BitArray(subDataOld);
                            BitArray bitArrayNew = new BitArray(newDataBytes);
                            if (bitOffset + bitLength <= bitArrayOld.Length)
                            {
                                // insert new bit in the old data
                                for (int i = 0; i < bitLength; i++)
                                {
                                    bitArrayOld[(int)(i + bitOffset)] = bitArrayNew[i];
                                }
                                bitArrayOld.CopyTo(newDataBytes, 0);
                            }
                        }

                        if (data.Length >= byteOffset + newDataBytes.Length)
                        {
                            dataNew = new byte[data.Length];
                            Array.Copy(data, dataNew, data.Length);
                            Array.Copy(newDataBytes, 0, dataNew, byteOffset, newDataBytes.Length);
                        }
                    }
                    if (oldCulture != null)
                    {
                        CultureInfo.CurrentCulture = oldCulture;
                    }
                }
            }
        }

        public class ParseInfoBase
        {
            public ParseInfoBase(string[] lineArray)
            {
                LineArray = lineArray;
            }

            public string[] LineArray { get; }
        }

        public class ParseInfoMwb : ParseInfoBase
        {
            public ParseInfoMwb(UInt32 serviceId, string[] lineArray, UInt32 nameKey, string[] nameArray, DataTypeEntry dataTypeEntry) : base(lineArray)
            {
                ServiceId = serviceId;
                NameKey = nameKey;
                NameArray = nameArray;
                DataTypeEntry = dataTypeEntry;
                if (nameArray != null)
                {
                    if (nameArray.Length >= 1)
                    {
                        Name = nameArray[0];
                    }
                    if (nameArray.Length >= 3)
                    {
                        if (UInt32.TryParse(nameArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 valueType))
                        {
                            DataIdType = valueType;
                        }
                        if (UInt32.TryParse(nameArray[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 valueId))
                        {
                            DataId = valueId;
                        }
                    }
                }

                StringBuilder sbIdName = new StringBuilder();
                string namePart1 = GetDataIdName(NameKey, DataId, DataIdType);
                if (!string.IsNullOrEmpty(namePart1))
                {
                    sbIdName.Append(namePart1);
                    string namePart2 = GetDataIdName(DataTypeEntry.NameDetailKey, DataTypeEntry.DataDetailId, DataTypeEntry.DataDetailIdType);
                    if (!string.IsNullOrEmpty(namePart2))
                    {
                        sbIdName.Append("-");
                        sbIdName.Append(namePart2);
                    }
                }
                DataIdString = sbIdName.ToString();

                StringBuilder sbId = new StringBuilder();
                sbId.Append(string.Format(CultureInfo.InvariantCulture, "{0}", ServiceId));
                sbId.Append("-");
                sbId.Append(DataIdString);
                UniqueIdString = sbId.ToString();

                StringBuilder sbIdOld = new StringBuilder();
                sbIdOld.Append(string.Format(CultureInfo.InvariantCulture, "{0}", ServiceId));
                if (DataId.HasValue)
                {
                    sbIdOld.Append("-");
                    sbIdOld.Append(string.Format(CultureInfo.InvariantCulture, "{0}", DataId.Value));
                    if (DataTypeEntry.DataDetailId.HasValue)
                    {
                        sbIdOld.Append("-");
                        sbIdOld.Append(string.Format(CultureInfo.InvariantCulture, "{0}", DataTypeEntry.DataDetailId.Value));
                    }
                }
                UniqueIdStringOld = sbIdOld.ToString();
            }

            public UInt32 ServiceId { get; }
            public UInt32 NameKey { get; }
            public string[] NameArray { get; }
            public string Name { get; }
            public UInt32? DataId { get; }
            public UInt32? DataIdType { get; }
            public DataTypeEntry DataTypeEntry { get; }
            public string DataIdString { get; }
            public string UniqueIdString { get; }
            public string UniqueIdStringOld { get; }

            static string GetDataIdName(UInt32? nameKey, UInt32? dataId, UInt32? dataIdType)
            {
                if (!nameKey.HasValue)
                {
                    return string.Empty;
                }

                UInt32 displayValue;
                string prefix;
                if (dataId.HasValue && dataIdType.HasValue)
                {
                    displayValue = dataId.Value;
                    switch (dataIdType.Value)
                    {
                        case 1:
                            prefix = "FSS";
                            break;

                        case 2:
                            prefix = "IDE";
                            break;

                        case 3:
                            prefix = "LTD";
                            break;

                        case 4:
                            prefix = "LTE";
                            break;

                        case 5:
                            prefix = "LTF";
                            break;

                        case 6:
                            prefix = "LTG";
                            break;

                        case 7:
                            prefix = "MAS";
                            break;

                        case 8:
                            prefix = "SER";
                            break;

                        case 9:
                            prefix = "SFT";
                            break;

                        case 10:
                        case 11:
                        case 12:
                        case 13:
                            prefix = string.Format(CultureInfo.InvariantCulture, "LR{0}", dataIdType.Value - 10);
                            break;

                        default:
                            prefix = "UNK";
                            break;
                    }
                }
                else
                {
                    displayValue = nameKey.Value;
                    prefix = "ENG";
                }

                return string.Format(CultureInfo.InvariantCulture, "{0}{1:00000}", prefix, displayValue & 0x1FFFF);
            }
        }

        public class ParseInfoAdp : ParseInfoMwb
        {
            public ParseInfoAdp(UInt32 serviceId, UInt32? subItem, string[] lineArray, UInt32 nameKey, string[] nameArray, DataTypeEntry dataTypeEntry) :
                base(serviceId, lineArray, nameKey, nameArray, dataTypeEntry)
            {
                SubItem = subItem;
            }

            public UInt32? SubItem { get; }
        }

        public class ParseInfoDtc : ParseInfoBase
        {
            public ParseInfoDtc(string[] lineArray, UInt32 errorCode, string pcodeText, string errorText, UInt32? detailCode, string errorDetail) : base(lineArray)
            {
                ErrorCode = errorCode;
                PcodeText = pcodeText;
                ErrorText = errorText;
                DetailCode = detailCode;
                ErrorDetail = errorDetail;
            }

            public UInt32 ErrorCode { get; }
            public string PcodeText { get; }
            public string ErrorText { get; }
            public UInt32? DetailCode { get; }
            public string ErrorDetail { get; }
        }

        public class ParseInfoSlv : ParseInfoBase
        {
            public ParseInfoSlv(string[] lineArray, UInt32? tableKey, List<SlaveInfo> slaveList) : base(lineArray)
            {
                TableKey = tableKey;
                SlaveList = slaveList;
            }

            public UInt32? TableKey { get; }
            public List<SlaveInfo> SlaveList { get; }

            public class SlaveInfo
            {
                public SlaveInfo(UInt32 minAddr, UInt32 maxAddr, string name)
                {
                    MinAddr = minAddr;
                    MaxAddr = maxAddr;
                    Name = name;
                }

                public UInt32 MinAddr { get; }
                public UInt32 MaxAddr { get; }
                public string Name { get; }
            }

        }

        public class SegmentInfo
        {
            public SegmentInfo(SegmentType segmentType, string segmentName, string fileName = null)
            {
                SegmentType = segmentType;
                SegmentName = segmentName;
                FileName = fileName;
            }

            public SegmentType SegmentType { get; }
            public string SegmentName { get; }
            public string FileName { get; }
            public bool Ignored { set; get; }
            public List<string[]> LineList { set; get; }
        }

        private static readonly SegmentInfo[] SegmentInfos =
        {
            new SegmentInfo(SegmentType.Adp, "ADP", "RA"),
            new SegmentInfo(SegmentType.Dtc, "DTC", "RD"),
            new SegmentInfo(SegmentType.Ffmux, "FFMUX", "RF"),
            new SegmentInfo(SegmentType.Ges, "GES", "RG"),
            new SegmentInfo(SegmentType.Mwb, "MWB", "RM"),
            new SegmentInfo(SegmentType.Sot, "SOT", "RS"),
            new SegmentInfo(SegmentType.Xpl, "XPL", "RX"),
            new SegmentInfo(SegmentType.Slv, "SLV"),
        };

        public class ChassisInfo
        {
            public delegate string NameConverterDelegate(UdsReader udsReader, FileNameResolver fileNameResolver);

            public ChassisInfo(string chassisName)
            {
                ChassisName = chassisName;
            }

            public ChassisInfo(NameConverterDelegate converterFunc)
            {
                ConverterFunc = converterFunc;
            }

            public string ChassisName { get; }
            public NameConverterDelegate ConverterFunc { get; }
        }

        static string ChassisName1K(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2009)
                {
                    oldVer = true;
                }
                else if (fileNameResolver.ModelYear == 2009)
                {
                    long serNum = fileNameResolver.SerialNumber;
                    if (serNum >= 0)
                    {
                        if (serNum < 199999)
                        {
                            oldVer = true;
                        }
                        else if (serNum > 400000 && serNum <= 700000)
                        {
                            oldVer = true;
                        }
                        else if (serNum > 800000 && serNum <= 900000)
                        {
                            oldVer = true;
                        }
                    }
                }
            }

            return oldVer ? "VW35" : "VW36";
        }

        static string ChassisName6R(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2015)
                {
                    oldVer = true;
                }
                else if (fileNameResolver.ModelYear == 2015)
                {
                    if (string.Compare(fileNameResolver.Manufacturer, "LSV", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        oldVer = true;
                    }
                }
            }
            else
            {
                oldVer = true;
            }

            return oldVer ? "VW25" : "VW26";
        }

        static string ChassisName3C(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2015)
                {
                    oldVer = true;
                }
                else if (fileNameResolver.ModelYear == 2016 &&
                        string.Compare(fileNameResolver.Manufacturer, "LSV", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    oldVer = true;
                }
                else if (fileNameResolver.ModelYear == 2015)
                {
                    if (fileNameResolver.SerialNumber >= 0 && fileNameResolver.SerialNumber < 200000)
                    {
                        oldVer = true;
                    }
                }
            }
            else
            {
                oldVer = true;
            }

            return oldVer ? "VW46" : "VW48";
        }

        static string ChassisName1T(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2016)
                {
                    oldVer = true;
                }
                else
                {
                    if (string.Compare(fileNameResolver.Manufacturer, "LSV", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        oldVer = true;
                    }
                }
            }
            else
            {
                oldVer = true;
            }

            return oldVer ? "VW36" : "VW37";
        }

        static string ChassisName6J(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2016)
                {
                    oldVer = true;
                }
            }
            else
            {
                oldVer = true;
            }

            return oldVer ? "SE26" : "SE27";
        }

        static string ChassisName5N(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2016)
                {
                    oldVer = true;
                }
                else if (fileNameResolver.ModelYear == 2016)
                {
                    if (string.Compare(fileNameResolver.Manufacturer, "WVG", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        oldVer = true;
                    }
                    else
                    {
                        char plant = char.ToUpperInvariant(fileNameResolver.AssemblyPlant);
                        long serNum = fileNameResolver.SerialNumber;
                        if (serNum >= 0)
                        {
                            if (!((plant == 'J' || plant == 'W') && (serNum >= 300000 && serNum < 500000) || serNum > 800000))
                            {
                                oldVer = true;
                            }
                        }
                    }
                }
            }
            else
            {
                oldVer = true;
            }

            return oldVer ? "VW36" : "VW37";
        }

        // ReSharper disable once InconsistentNaming
        static string ChassisNameAX(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2017)
                {
                    oldVer = true;
                }
                else
                {
                    if (string.Compare(fileNameResolver.Manufacturer, "3VV", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        oldVer = true;
                    }
                }
            }

            return oldVer ? "VW36" : "VW37";
        }

        // ReSharper disable once InconsistentNaming
        static string ChassisNameKE(UdsReader udsReader, FileNameResolver fileNameResolver)
        {
            bool oldVer = false;
            if (fileNameResolver.ModelYear >= 0)
            {
                if (fileNameResolver.ModelYear < 2016)
                {
                    oldVer = true;
                }
            }

            return oldVer ? "SE25" : "SE26";
        }

        private static readonly Dictionary<string, ChassisInfo> ChassisMapFixed = new Dictionary<string, ChassisInfo>()
        {
            { "1K", new ChassisInfo(ChassisName1K) },
            { "6R", new ChassisInfo(ChassisName6R) },
            { "3C", new ChassisInfo(ChassisName3C) },
            { "1T", new ChassisInfo(ChassisName1T) },
            { "6J", new ChassisInfo(ChassisName6J) },
            { "5N", new ChassisInfo(ChassisName5N) },
            { "AX", new ChassisInfo(ChassisNameAX) },
            { "KE", new ChassisInfo(ChassisNameKE) },
        };

        private static readonly HashSet<string> ChassisInvalid = new HashSet<string>()
        {
            "AU58", "AU65", "LB63", "BG", "BY63", "VW27", "VW416", "VW53", "VN54", "SE27", "SK27"
        };

        private Dictionary<string, string> _redirMap;
        private Dictionary<UInt32, string[]> _textMap;
        private Dictionary<UInt32, string[]> _unitMap;
        private Dictionary<UInt32, FixedEncodingEntry> _fixedEncodingMap;
        private ILookup<UInt32, string[]> _ttdopLookup;
        private ILookup<UInt32, string[]> _muxLookup;
        private Dictionary<string, ChassisInfo> _chassisMap;

        public DataReader DataReader { get; private set; }
        public string LanguageDir { get; set; }

        public UdsReader()
        {
            LanguageDir = string.Empty;
        }

        private static readonly Dictionary<byte, string> Type28Dict = new Dictionary<byte, string>()
        {
            {1, "OBD II (CARB)"},
            {2, "OBD (EPA)"},
            {3, "OBD + OBD II"},
            {4, "OBD I"},
            {6, "Euro-OBD"},
            {7, "EOBD + OBD II"},
            {8, "OBD + EOBD"},
            {9, "OBD+OBD II+EOBD"},
            {10, "JOBD"},
            {11, "JOBD + OBD II"},
            {12, "JOBD + EOBD"},
            {13, "JOBD+EOBD+OBD II"},
            {14, "HD Euro IV/B1"},
            {15, "HD Euro V/B2"},
            {16, "HD EURO EEC/C"},
            {17, "Eng. Manuf. Diag"},
            {18, "Eng. Manuf. Diag +"},
            {19, "HD OBD-C"},
            {20, "HD OBD"},
            {21, "WWH OBD"},
            {23, "HD EOBD-I"},
            {24, "HD EOBD-I M"},
            {25, "HD EOBD-II"},
            {26, "HD EOBD-II N"},
            {28, "OBDBr-1"},
            {29, "OBDBr-2"},
            {30, "KOBD"},
            {31, "IOBD I"},
            {32, "IOBD II"},
            {33, "HD EOBD-VI"},
            {34, "OBD+OBDII+HDOBD"},
            {35, "OBDBr-3"},
        };

        public static string GetTextMapText(UdsReader udsReader, UInt32 key)
        {
            if (udsReader?._textMap == null)
            {
                return null;
            }
            if (udsReader._textMap.TryGetValue(key, out string[] nameArray1)) // Keine
            {
                if (nameArray1.Length > 0)
                {
                    return nameArray1[0];
                }
            }

            return null;
        }

        public static string GetUnitMapText(UdsReader udsReader, UInt32 key)
        {
            if (udsReader?._unitMap == null)
            {
                return null;
            }
            if (udsReader._unitMap.TryGetValue(key, out string[] nameArray1)) // Keine
            {
                if (nameArray1.Length > 0)
                {
                    return nameArray1[0];
                }
            }

            return null;
        }

        private static string Type2Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            byte value0 = data[0];
            if ((value0 & 0xC0) != 0)
            {
                sb.Append(((value0 & 0xC0) == 0x40) ? "C" : "U");
            }
            else
            {
                sb.Append("P");
            }

            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:X02}{1:X02}", value0 & 0x3F, data[1]));

            return sb.ToString();
        }

        private static string Type3Convert(UdsReader udsReader, byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                if (data.Length < i + 1)
                {
                    break;
                }
                int value = data[i] & 0x1F;
                if (value == 0)
                {
                    break;
                }

                UInt32 textKey;
                switch (value)
                {
                    case 1:
                        textKey = 152138;    // Regelkreis offen, Voraussetzungen für geschlossenen Regelkreis nicht erfüllt
                        break;

                    case 2:
                        textKey = 152137;    // Regelkreis geschlossen, benutze Lambdasonden
                        break;

                    case 4:
                        textKey = 152136;    // Regelkreis offen, wegen Fahrbedingungen
                        break;

                    case 8:
                        textKey = 152135;    // Regelkreis offen, wegen Systemfehler erkannt
                        break;

                    case 16:
                        textKey = 152134;    // Regelkreis geschlossen, aber Fehler Lambdasonde
                        break;

                    default:
                        textKey = 99014;    // Unbekannt
                        break;
                }

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(GetTextMapText(udsReader, textKey) ?? string.Empty);
            }

            return sb.ToString();
        }

        private static string Type18Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            int value = data[0] & 0x07;
            UInt32 textKey;
            switch (value)
            {
                case 1:
                    textKey = 167178;    // Vor erstem Katalysator
                    break;

                case 2:
                    textKey = 152751;    // Nach erstem Katalysator
                    break;

                case 3:
                    textKey = 159156;    // Außenluft/AUS
                    break;

                default:
                    textKey = 99014;    // Unbekannt
                    break;
            }

            return GetTextMapText(udsReader, textKey) ?? string.Empty;
        }

        private static string Type19Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            byte value = data[0];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append($"B{(i >> 2) + 1}S{(i & 0x3) + 1}");
                }
            }

            return sb.ToString();
        }

        private static string Type20Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            double value1 = data[0] * 0.005;
            double value2 = (data[1] - 128.0) * 100.0 / 128.0;

            sb.Append($"{value1:0.000} ");
            sb.Append(GetUnitMapText(udsReader, 9) ?? string.Empty);  // V

            sb.Append("; ");
            sb.Append($"{value2:0.00} ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %
            // > 0 fett, < 0 mager

            return sb.ToString();
        }

        private static string Type28Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            byte value = data[0];
            if (Type28Dict.TryGetValue(value, out string text))
            {
                return text;
            }

            if (value == 5)
            {
                return GetTextMapText(udsReader, 98661) ?? string.Empty; // Keine
            }

            return GetTextMapText(udsReader, 99014) ?? string.Empty; // Unbekannt
        }

        private static string Type29Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            byte value = data[0];

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 8; i++)
            {
                if ((value & (1 << i)) != 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append($"B{(i >> 1) + 1}S{(i & 0x1) + 1}");
                }
            }

            return sb.ToString();
        }

        private static string Type30Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            UInt32 textKey;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if ((data[0] & 0x01) != 0)
            {
                textKey = 098360;    // aktiv
            }
            else
            {
                textKey = 098671;    // inaktiv
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(GetTextMapText(udsReader, 064207) ?? string.Empty);   // Nebenantrieb
            sb.Append(" ");
            sb.Append(GetTextMapText(udsReader, textKey) ?? string.Empty);

            return sb.ToString();
        }

        private static string Type37_43a52_59Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            double value1 = (data[1] | (data[0] << 8)) / 32783.0;
            sb.Append($"{value1:0.000} ");
            sb.Append(GetUnitMapText(udsReader, 113) ?? string.Empty);  // Lambda

            double value2 = ((data[3] | (data[2] << 8)) - 32768.0) / 256.0;
            sb.Append($"{value2:0.000} ");
            sb.Append(GetUnitMapText(udsReader, 123) ?? string.Empty);  // mA

            return sb.ToString();
        }

        private static string Type50Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            UInt32 value = (UInt32)(data[1] | ((data[0]) << 8));
            double displayValue = (value & 0x7FFF) / 4.0;
            if ((value & 0x8000) != 0)
            {
                displayValue = -displayValue;
            }
            sb.Append($"{displayValue:0.} ");
            sb.Append(GetUnitMapText(udsReader, 79) ?? string.Empty);  // Pa

            return sb.ToString();
        }

        private static string Type60_63Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            double value = (data[1] | (data[0] << 8)) * 0.1 - 40.0;
            sb.Append($"{value:0.000} ");
            sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

            return sb.ToString();
        }

        private static string Type77_78Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            int value = (data[1] | (data[0] << 8));
            return $"{value / 60}h {value % 60}min";
        }

        private static string Type81Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            UInt32 textKey;
            byte value = data[0];
            if (value > 8)
            {
                value -= 8;
                sb.Append("Bifuel: ");
            }

            switch (value)
            {
                case 1:
                    textKey = 018273;   // Benzin
                    break;

                case 2:
                    textKey = 152301;   // Methanol
                    break;

                case 3:
                    textKey = 016086;   // Ethanol
                    break;

                case 4:
                    textKey = 000586;   // Diesel
                    break;

                case 5:
                    textKey = 090173;   // LPG
                    break;

                case 6:
                    textKey = 090209;   // CNG
                    break;

                case 7:
                    textKey = 167184;   // Propan
                    break;

                case 8:
                    textKey = 022443;   // Batterie / elektrisch
                    break;

                default:
                    return string.Empty;
            }

            sb.Append(GetTextMapText(udsReader, textKey) ?? string.Empty);

            return sb.ToString();
        }

        private static string Type84Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            UInt32 value = (UInt32) (data[1] | ((data[0]) << 8));
            double displayValue = value & 0x7FFF;
            if ((value & 0x8000) != 0)
            {
                displayValue = -displayValue;
            }
            sb.Append($"{displayValue:0.} ");
            sb.Append(GetUnitMapText(udsReader, 79) ?? string.Empty);  // Pa

            return sb.ToString();
        }

        private static string Type85_88Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            double value1 = (data[0] - 128.0) * 100.0 / 128.0;
            sb.Append($"{value1:0.0}");
            if (data[1] != 0)
            {
                sb.Append("/");
                double value2 = (data[1] - 128.0) * 100.0 / 128.0;
                sb.Append($"{value2:0.0}");
            }
            sb.Append(" ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

            return sb.ToString();
        }

        private static string Type95Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            byte value = data[0];

            switch (value)
            {
                case 14:
                    return "HD Euro IV/B1";

                case 15:
                    return "HD Euro V/B2";

                case 16:
                    return "HD EURO EEC/C";
            }
            return GetTextMapText(udsReader, 99014) ?? string.Empty;  // Unbekannt
        }

        private static string Type100Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 5)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            double value1 = (data[0] - 125.0) * 0.01;
            double value2 = (data[1] - 125.0) * 0.01;
            sb.Append($"TQ_Max 1/2: {value1:0.}/{value2:0.} ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

            double value3 = (data[2] - 125.0) * 0.01;
            double value4 = (data[3] - 125.0) * 0.01;
            sb.Append("; ");
            sb.Append($"TQ_Max 3/4: {value3:0.}/{value4:0.} ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

            double value5 = (data[4] - 125.0) * 0.01;
            sb.Append("; ");
            sb.Append($"TQ_Max 5: {value5:0.} ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %
            return sb.ToString();
        }

        private static string Type101Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            byte valueData = data[1];
            StringBuilder sb = new StringBuilder();

            if ((maskData & 0x01) != 0)
            {
                sb.Append("PTO_STAT: ");
                sb.Append((valueData & 0x01) != 0 ? "ON" : "OFF");
            }

            if ((maskData & 0x02) != 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append("N/D_STAT: ");
                sb.Append((valueData & 0x02) != 0 ? "NEUTR" : "DRIVE");
            }

            if ((maskData & 0x04) != 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append("MT_GEAR: ");
                sb.Append((valueData & 0x04) != 0 ? "NEUTR" : "GEAR");
            }
            return sb.ToString();
        }

        private static string Type102Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbValue = new StringBuilder();
                if ((maskData & (1 << i)) != 0)
                {
                    char name = (char)('A' + i);
                    sbValue.Append($"MAF{name}: ");

                    int offset = i * 2 + 1;
                    int value = (data[offset] << 8) | data[offset + 1];
                    double displayValue = value / 32.0;

                    sbValue.Append($"{displayValue:0.00} ");
                    sbValue.Append(GetUnitMapText(udsReader, 26) ?? string.Empty); // g/s

                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(sbValue);
                }
            }

            return sb.ToString();
        }

        private static string Type103Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                if ((maskData & (1 << i)) != 0)
                {
                    byte value = data[i + 1];
                    double displayValue = value - 40.0;

                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append($"ECT {i + 1}: {displayValue:0.} ");
                    sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C
                }
            }

            return sb.ToString();
        }

        private static string Type104Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 6 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                StringBuilder sbValue = new StringBuilder();
                bool bData1 = (maskData & (1 << i)) != 0;
                bool bData2 = (maskData & (1 << (i + 3))) != 0;
                if (bData1 || bData2)
                {
                    sbValue.Append("IAT ");
                    if (bData1)
                    {
                        sbValue.Append($"1{i + 1}");
                    }
                    if (bData2)
                    {
                        if (bData1)
                        {
                            sbValue.Append("/");
                        }
                        sbValue.Append($"2{i + 1}");
                    }
                    sbValue.Append(": ");

                    if (bData1)
                    {
                        byte value = data[i + 1];
                        double displayValue = value - 40.0;
                        sbValue.Append($"{displayValue:0.}");
                    }
                    if (bData2)
                    {
                        byte value = data[i + 3 + 1];
                        double displayValue = value - 40.0;
                        if (bData1)
                        {
                            sbValue.Append("/");
                        }
                        sbValue.Append($"{displayValue:0.}");
                    }
                    sbValue.Append(" ");
                    sbValue.Append(GetUnitMapText(udsReader, 3) ?? string.Empty); // °C

                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(sbValue);
                }
            }

            return sb.ToString();
        }

        private static string Type105Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 6 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbValue = new StringBuilder();
                char name = (char)('A' + i);
                sbValue.Append($"EGR {name}: ");
                for (int j = 0; j < 3; j++)
                {
                    byte value = data[index + 1];
                    double displayValue;
                    if (j < 2)
                    {
                        displayValue = (value - 128.0) * 100.0 / 128.0;
                    }
                    else
                    {
                        displayValue = value * 100.0 / 255.0;
                    }

                    if (j > 0)
                    {
                        sbValue.Append("/");
                    }
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if ((maskData & (1 << index)) != 0)
                    {
                        sbValue.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbValue.Append("---");
                    }
                    index++;
                }
                sbValue.Append(" ");
                sbValue.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbValue);
            }

            return sb.ToString();
        }

        private static string Type106Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbValue = new StringBuilder();
                char name = (char)('A' + i);
                sbValue.Append($"IAF_{name} cmd/rel: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbValue.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        byte value = data[index + 1];
                        double displayValue = value * 100.0 / 255.0;
                        sbValue.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbValue.Append("---");
                    }

                    index++;
                }
                sbValue.Append(" ");
                sbValue.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbValue);
            }

            return sb.ToString();
        }

        private static string Type107Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                sbVal.Append($"EGR Temp {i + 1}1/{i + 1}2: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        byte value = data[index + 1];
                        double displayValue = value - 40.0;
                        sbVal.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbVal.Append("---");
                    }
                    index++;
                }
                sbVal.Append(" ");
                sbVal.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type108Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                char name = (char)('A' + i);
                sbVal.Append($"THR {name} cmd/rel: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        byte value = data[index + 1];
                        double displayValue = value * 100.0 / 255.0;
                        sbVal.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbVal.Append("---");
                    }
                    index++;
                }
                sbVal.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type110Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 * 2 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                char name = (char)('A' + i);
                sbVal.Append($"ICP_{name} cmd/rel: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        int offset = index * 2 + 1;
                        int value = (data[offset] << 8) | data[offset + 1];
                        double displayValue = value * 10.0;
                        sbVal.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbVal.Append("---");
                    }
                    index++;
                }
                sbVal.Append(GetUnitMapText(udsReader, 79) ?? string.Empty);  // Pa

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type111Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                char name = (char)('A' + i);
                sbVal.Append($"TC{name}_PRESS: ");

                if ((maskData & (1 << i)) != 0)
                {
                    double displayValue = data[i + 1];
                    sbVal.Append($"{displayValue:0.} ");
                }
                else
                {
                    sbVal.Append("--- ");
                }
                sbVal.Append(GetUnitMapText(udsReader, 103) ?? string.Empty);  // kpa

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type112Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 8 + 2)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int offset = 1;
            for (int i = 0; i < 2; i++)
            {
                int bitStart = i * 3;
                int infoData = (maskData >> bitStart) & 0x7;
                bool cmd = (infoData & 0x01) != 0;
                bool act = (infoData & 0x02) != 0;
                bool status = (infoData & 0x04) != 0;
                StringBuilder sbType = new StringBuilder();
                if (cmd)
                {
                    sbType.Append("cmd");
                }
                if (act)
                {
                    if (sbType.Length > 0)
                    {
                        sbType.Append("/");
                    }
                    sbType.Append("act");
                }
                if (sbType.Length > 0)
                {
                    sbType.Append(": ");
                }

                StringBuilder sbValue = new StringBuilder();
                for (int j = 0; j < 2; j++)
                {
                    if ((infoData & (1 << j)) != 0)
                    {
                        double displayValue = (data[offset + 1] | (data[offset] << 8)) / 32.0;
                        if (sbValue.Length > 0)
                        {
                            sbValue.Append("/");
                        }
                        sbValue.Append($"{displayValue:0.}");
                    }

                    offset += 2;
                }
                if (sbValue.Length > 0)
                {
                    sbValue.Append(" ");
                    sbValue.Append(GetUnitMapText(udsReader, 103) ?? string.Empty); // kPa
                }

                StringBuilder sbStat = new StringBuilder();
                if (status)
                {
                    int value = (data[9] >> (i * 2)) & 0x03;
                    UInt32 textKey;
                    switch (value)
                    {
                        case 1:
                            textKey = 152138;    // Regelkreis offen
                            break;

                        case 2:
                            textKey = 152137;    // Regelkreis geschlossen
                            break;

                        case 3:
                            textKey = 101955;    // Fehler vorhanden
                            break;

                        default:
                            textKey = 99014;    // Unbekannt
                            break;
                    }
                    sbStat.Append(GetTextMapText(udsReader, textKey) ?? string.Empty);
                }

                if (sbType.Length > 0 || sbValue.Length > 0 || sbStat.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    char name = (char)('A' + i);
                    sb.Append($"BP_{name} ");
                    sb.Append(sbType);
                    sb.Append(sbValue);
                    if (sbStat.Length > 0)
                    {
                        sb.Append(" ");
                        sb.Append(sbStat);
                    }
                }
            }

            return sb.ToString();
        }

        private static string Type113Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbValue = new StringBuilder();

                char name = (char)('A' + i);
                sbValue.Append($"VGT_{name} cmd/act: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbValue.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        byte value = data[index + 1];
                        double displayValue = value * 100.0 / 255.0;
                        sbValue.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbValue.Append("---");
                    }

                    index++;
                }
                sbValue.Append(" ");
                sbValue.Append(GetUnitMapText(udsReader, 1) ?? string.Empty); // %

                StringBuilder sbStat = new StringBuilder();
                if((maskData & (1 << index)) != 0)
                {
                    int value = (data[5] >> (i * 2)) & 0x03;
                    UInt32 textKey;
                    switch (value)
                    {
                        case 1:
                            textKey = 152138;    // Regelkreis offen
                            break;

                        case 2:
                            textKey = 152137;    // Regelkreis geschlossen
                            break;

                        case 3:
                            textKey = 101955;    // Fehler vorhanden
                            break;

                        default:
                            textKey = 99014;    // Unbekannt
                            break;
                    }
                    sbStat.Append(GetTextMapText(udsReader, textKey) ?? string.Empty);
                }
                index++;

                if (sbValue.Length > 0 || sbStat.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(sbValue);
                    if (sbStat.Length > 0)
                    {
                        sb.Append(" ");
                        sb.Append(sbStat);
                    }
                }
            }

            return sb.ToString();
        }

        private static string Type114Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                char name = (char)('A' + i);
                sbVal.Append($"WG_{name} cmd/act: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        byte value = data[index + 1];
                        double displayValue = value * 100.0 / 255.0;
                        sbVal.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbVal.Append("---");
                    }
                    index++;
                }
                sbVal.Append(" ");
                sbVal.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type115Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                if ((maskData & (1 << (i * 2))) != 0)
                {
                    StringBuilder sbVal = new StringBuilder();
                    sbVal.Append($"EP{i + 1}: ");

                    int offset = i * 2 + 1;
                    int value = (data[offset] << 8) | data[offset + 1];
                    double displayValue = value * 0.01;
                    sbVal.Append($"{displayValue:0.00} ");
                    sbVal.Append(GetUnitMapText(udsReader, 103) ?? string.Empty);  // kpa

                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(sbVal);
                }
            }

            return sb.ToString();
        }

        private static string Type116Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                if ((maskData & (1 << (i * 2))) != 0)
                {
                    StringBuilder sbVal = new StringBuilder();
                    char name = (char)('A' + i);
                    sbVal.Append($"TC{name}_RPM: ");
                    int offset = i * 2 + 1;
                    double displayValue = (data[offset] << 8) | data[offset + 1];
                    sbVal.Append($"{displayValue:0.} ");
                    sbVal.Append(GetUnitMapText(udsReader, 21) ?? string.Empty);  // /min

                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(sbVal);
                }
            }

            return sb.ToString();
        }

        private static string Type117_118Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 6 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();

            sb.Append(GetTextMapText(udsReader, 175748) ?? string.Empty);   // Kompressor
            sb.Append(" ");
            sb.Append(GetTextMapText(udsReader, 098311) ?? string.Empty);   // ein
            sb.Append("/");
            sb.Append(GetTextMapText(udsReader, 098310) ?? string.Empty);   // aus
            sb.Append(": ");

            for (int i = 0; i < 2; i++)
            {
                if (i > 0)
                {
                    sb.Append("/");
                }
                if ((maskData & (1 << i)) != 0)
                {
                    byte value = data[i + 1];
                    double displayValue = value - 40.0;
                    sb.Append($"{displayValue:0.}");
                }
                else
                {
                    sb.Append("---");
                }
            }
            sb.Append(" ");
            sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

            sb.Append("; ");
            sb.Append(GetTextMapText(udsReader, 175748) ?? string.Empty);   // Kompressor
            sb.Append(" ");
            sb.Append(GetTextMapText(udsReader, 098311) ?? string.Empty);   // ein
            sb.Append("/");
            sb.Append(GetTextMapText(udsReader, 098310) ?? string.Empty);   // aus
            sb.Append(": ");

            for (int i = 0; i < 2; i++)
            {
                if (i > 0)
                {
                    sb.Append("/");
                }
                if ((maskData & (1 << (i + 2))) != 0)
                {
                    int value = (data[(i *2) + 3] << 8) | data[(i * 2) + 4];
                    double displayValue = value * 0.1 - 40.0;
                    sb.Append($"{displayValue:0.}");
                }
                else
                {
                    sb.Append("---");
                }
            }
            sb.Append(" ");
            sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

            return sb.ToString();
        }

        private static string Type119Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbValue = new StringBuilder();
                int maskIndex = i * 2;
                bool b1 = (maskData & (1 << maskIndex)) != 0;
                bool b2 = (maskData & (1 << (maskIndex + 1))) != 0;
                int bNum = i + 1;
                if (b1 & b2)
                {
                    sbValue.Append($"B{bNum}S1/B{bNum}S2: ");
                }
                else if (b1)
                {
                    sbValue.Append($"B{bNum}S1: ");
                }
                else if (b2)
                {
                    sbValue.Append($"B{bNum}S2: ");
                }
                else
                {
                    continue;
                }

                if (b1)
                {
                    byte value = data[(i * 2) + 1];
                    double displayValue = value - 40.0;
                    sbValue.Append($"{displayValue:0.}");
                }
                if (b2)
                {
                    byte value = data[(i * 2) + 2];
                    double displayValue = value - 40.0;
                    if (b1)
                    {
                        sbValue.Append("/");
                    }
                    sbValue.Append($"{displayValue:0.}");
                }
                sbValue.Append(" ");
                sbValue.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbValue);
            }

            return sb.ToString();
        }

        private static string Type120_121Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 8 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append("/");
                }
                if ((maskData & (1 << i)) != 0)
                {
                    int value = (data[(i * 2) + 1] << 8) | data[(i * 2) + 2];
                    double displayValue = value * 0.1 - 40.0;
                    sb.Append($"{displayValue:0.} ");
                }
                else
                {
                    sb.Append("--- ");
                }
                sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C
            }

            sb.Insert(0, "S1/S2/S3/S4: ");

            return sb.ToString();
        }

        private static string Type122_123Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 6 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            sb.Append("Delta/In/Out: ");

            if ((maskData & 0x01) != 0)
            {
                int value = (data[1] << 8) | data[2];
                double displayValue = (value & 0x7FFF) * 0.01;
                if ((value & 0x8000) != 0)
                {
                    displayValue = -displayValue;
                }
                sb.Append($"{displayValue:0.00}");
            }
            else
            {
                sb.Append("---");
            }
            sb.Append("/");

            if ((maskData & 0x02) != 0)
            {
                int value = (data[3] << 8) | data[4];
                double displayValue = value * 0.01;
                sb.Append($"{displayValue:0.00}");
            }
            else
            {
                sb.Append("---");
            }
            sb.Append("/");

            if ((maskData & 0x02) != 0)
            {
                int value = (data[5] << 8) | data[6];
                double displayValue = value * 0.01;
                sb.Append($"{displayValue:0.00}");
            }
            else
            {
                sb.Append("---");
            }
            sb.Append(" ");

            sb.Append(GetUnitMapText(udsReader, 103) ?? string.Empty);  // kpa

            return sb.ToString();
        }

        private static string Type124Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 2 * 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                sbVal.Append($"B{i + 1}: ");

                for (int j = 0; j < 2; j++)
                {
                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    if ((maskData & (1 << index)) != 0)
                    {
                        int offset = index * 2 + 1;
                        int value = (data[offset] << 8) | data[offset + 1];
                        double displayValue = value * 0.1 - 40.0;
                        sbVal.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbVal.Append("---");
                    }
                    index++;
                }
                sbVal.Append(" ");
                sbVal.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type125_126Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();

            if ((maskData & 0x01) != 0)
            {
                sb.Append("NTE:In");
            }

            if ((maskData & 0x02) != 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append("NTE:Out");
            }

            if ((maskData & 0x04) != 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append("NTE:Carve-out");
            }

            if ((maskData & 0x08) != 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append("NTE:Def");
            }
            return sb.ToString();
        }

        private static string Type131Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append($"NOx{i + 1}1: ");
                if ((maskData & (1 << i)) != 0)
                {
                    int value = (data[(i * 2) + 1] << 8) | data[(i * 2) + 2];
                    double displayValue = value;
                    sb.Append($"{displayValue:0.} ");
                }
                else
                {
                    sb.Append("--- ");
                }
                sb.Append(GetUnitMapText(udsReader, 128) ?? string.Empty);  // ppm
            }

            return sb.ToString();
        }

        private static string Type127Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 3 * 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                if ((maskData & (1 << i)) != 0)
                {
                    StringBuilder sbValue = new StringBuilder();
                    switch (i)
                    {
                        case 1:
                            sbValue.Append(GetTextMapText(udsReader, 001565) ?? string.Empty);// Leerlauf
                            break;

                        case 2:
                            sbValue.Append("PTO");
                            break;

                        default:
                            sbValue.Append("Total");
                            break;
                    }
                    sbValue.Append(": ");

                    int offset = i * 4 + 1;
                    UInt32 value = (UInt32)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
                    sbValue.Append($"{value / 3600}H {value % 3600}s");

                    if (sb.Length > 0)
                    {
                        sb.Append("; ");
                    }
                    sb.Append(sbValue);
                }
            }

            return sb.ToString();
        }

        private static string Type129_130Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 5 * 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    StringBuilder sbValue = new StringBuilder();
                    if ((maskData & (1 << index)) != 0)
                    {
                        int offset = index * 4 + 1;
                        UInt32 value = (UInt32)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
                        if (sbValue.Length > 0)
                        {
                            sbValue.Append(" / ");
                        }
                        sbValue.Append($"{value / 3600}H {value % 3600}s");
                        if (i == 2)
                        {   // abort last round
                            break;
                        }
                    }

                    if (sbValue.Length > 0)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append("; ");
                        }
                        sb.Append(sbValue);
                    }

                    index++;
                }
            }

            return sb.ToString();
        }

        private static string Type133Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 10)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();

            sb.Append("ReAg Rate/Demand: ");
            if ((maskData & 0x01) != 0)
            {
                int value = (data[1] << 8) | data[2];
                double displayValue = value * 0.005;
                sb.Append($"{displayValue:0.}");
            }
            else
            {
                sb.Append("---");
            }
            sb.Append("/");

            if ((maskData & 0x02) != 0)
            {
                int value = (data[3] << 8) | data[4];
                double displayValue = value * 0.005;
                sb.Append($"{displayValue:0.}");
            }
            else
            {
                sb.Append("---");
            }
            sb.Append(" ");
            sb.Append(GetUnitMapText(udsReader, 110) ?? string.Empty);  // l/h

            sb.Append("; ");
            sb.Append("ReAg Level: ");
            if ((maskData & 0x04) != 0)
            {
                int value = data[5];
                double displayValue = value * 100 / 255.0;
                sb.Append($"{displayValue:0.}");
            }
            else
            {
                sb.Append("---");
            }
            sb.Append(" ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %

            sb.Append("; ");
            sb.Append("NWI ");
            sb.Append(GetTextMapText(udsReader, 099068) ?? string.Empty);   // Time
            sb.Append(": ");
            if ((maskData & 0x08) != 0)
            {
                UInt32 value = (UInt32) ((data[6] << 24) | (data[7] << 16) | (data[8] << 8) | data[9]);
                sb.Append($"{value / 3600}H {value % 3600}s");
            }
            else
            {
                sb.Append("---");
            }

            return sb.ToString();
        }

        private static string Type134Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                sbVal.Append($"PM{i + 1}1: ");
                if ((maskData & (1 << (i * 2))) != 0)
                {
                    int offset = i * 2 + 1;
                    int value = (data[offset] << 8) | data[offset + 1];
                    double displayValue = value / 80.0;
                    sbVal.Append($"{displayValue:0.00} ");
                    sbVal.Append(GetUnitMapText(udsReader, 127) ?? string.Empty);  // mg/m3
                }
                else
                {
                    sbVal.Append("---");
                }

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type135Convert(UdsReader udsReader, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                StringBuilder sbVal = new StringBuilder();
                char name = (char)('A' + i);
                sbVal.Append($"MAP_{name}: ");
                if ((maskData & (1 << (i * 2))) != 0)
                {
                    int offset = i * 2 + 1;
                    int value = (data[offset] << 8) | data[offset + 1];
                    double displayValue = value / 32.0;
                    sbVal.Append($"{displayValue:0.00} ");
                    sbVal.Append(GetUnitMapText(udsReader, 103) ?? string.Empty);  // kPa abs
                    sbVal.Append(" abs");
                }
                else
                {
                    sbVal.Append("---");
                }

                if (sb.Length > 0)
                {
                    sb.Append("; ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static readonly FixedEncodingEntry[] FixedEncodingArray =
        {
            new FixedEncodingEntry(new UInt32[]{1, 65, 79, 80, 109, 136, 139, 140, 141, 142, 143, 152, 153, 159}, 0), // not existing
            new FixedEncodingEntry(new UInt32[]{2}, Type2Convert),
            new FixedEncodingEntry(new UInt32[]{3}, Type3Convert),
            new FixedEncodingEntry(new UInt32[]{4, 17, 44, 46, 47, 91}, 1, 1, 1, 0, 100.0 / 255), // Unit %
            new FixedEncodingEntry(new UInt32[]{5, 15, 70, 92, 132}, 1, 3, 0, -40, 1.0), // Unit °C
            new FixedEncodingEntry(new UInt32[]{6, 7, 8, 9, 45}, 1, 1, 1, -128, 100 / 128), // Unit %
            new FixedEncodingEntry(new UInt32[]{10}, 1, 103, 0, 0, 3.0, "rel"), // Unit kPa rel
            new FixedEncodingEntry(new UInt32[]{11, 51}, 1, 103, 0, null, null, "abs"), // Unit kPa abs
            new FixedEncodingEntry(new UInt32[]{12}, 2, 21, 0, 0, 0.25), // Unit /min
            new FixedEncodingEntry(new UInt32[]{13}, 1, 109, 0), // Unit km/h
            new FixedEncodingEntry(new UInt32[]{33, 49}, 2, 109, 0), // Unit km/h
            new FixedEncodingEntry(new UInt32[]{14}, 1, 1, 1, -128, 1 / 2.0), // Unit %
            new FixedEncodingEntry(new UInt32[]{16}, 2, 26, 2, 0, 0.01), // Unit g/s
            new FixedEncodingEntry(new UInt32[]{18}, Type18Convert),
            new FixedEncodingEntry(new UInt32[]{19}, Type19Convert),
            new FixedEncodingEntry(new UInt32[]{20}, Type20Convert),
            new FixedEncodingEntry(new UInt32[]{28}, Type28Convert),
            new FixedEncodingEntry(new UInt32[]{29}, Type29Convert),
            new FixedEncodingEntry(new UInt32[]{30}, Type30Convert),
            new FixedEncodingEntry(new UInt32[]{31}, 2, 8, 0), // Unit s
            new FixedEncodingEntry(new UInt32[]{34}, 2, 103, 2, 0, 0.8), // Unit kPa
            new FixedEncodingEntry(new UInt32[]{35}, 2, 103, 0, 0, 10.0, "rel"), // Unit kPa rel
            new FixedEncodingEntry(new UInt32[]{36, 37, 38, 39, 40, 41, 42, 43, 52, 53, 54, 55, 56, 57, 58, 59}, Type37_43a52_59Convert),
            new FixedEncodingEntry(new UInt32[]{48}, 1, null, 0),
            new FixedEncodingEntry(new UInt32[]{50}, Type50Convert),
            new FixedEncodingEntry(new UInt32[]{60, 61, 62, 63}, Type60_63Convert),
            new FixedEncodingEntry(new UInt32[]{77, 78}, Type77_78Convert),
            new FixedEncodingEntry(new UInt32[]{66}, 2, 9, 3, 0, 0.001), // Unit V
            new FixedEncodingEntry(new UInt32[]{67}, 2, 1, 0, 0, 100 / 255), // Unit %
            new FixedEncodingEntry(new UInt32[]{68}, 2, 113, 3, 0, 1.0 / 32783.0), // Unit Lambda
            new FixedEncodingEntry(new UInt32[]{69, 71, 72, 73, 74, 75, 76, 82, 90}, 1, 1, 0, 0, 100 / 255), // Unit %
            new FixedEncodingEntry(new UInt32[]{81}, Type81Convert),
            new FixedEncodingEntry(new UInt32[]{83}, 2, 103, 0, 0, 5.0, "abs"), // Unit kPa abs
            new FixedEncodingEntry(new UInt32[]{84}, Type84Convert),
            new FixedEncodingEntry(new UInt32[]{85, 86, 87, 88}, Type85_88Convert),
            new FixedEncodingEntry(new UInt32[]{89}, 2, 103, 0, 0, 10.0, "abs"), // Unit kPa abs
            new FixedEncodingEntry(new UInt32[]{93}, 2, 2, 2, -26880, 1 / 128.0), // Unit °
            new FixedEncodingEntry(new UInt32[]{94}, 2, 110, 2, 0, 1 / 20.0), // Unit l/h
            new FixedEncodingEntry(new UInt32[]{95}, Type95Convert),
            new FixedEncodingEntry(new UInt32[]{97, 98}, 1, 1, 0, -125, 1.0), // Unit %
            new FixedEncodingEntry(new UInt32[]{99}, 1, 7, 0), // Unit Nm
            new FixedEncodingEntry(new UInt32[]{100}, Type100Convert),
            new FixedEncodingEntry(new UInt32[]{101}, Type101Convert),
            new FixedEncodingEntry(new UInt32[]{102}, Type102Convert),
            new FixedEncodingEntry(new UInt32[]{103}, Type103Convert),
            new FixedEncodingEntry(new UInt32[]{104}, Type104Convert),
            new FixedEncodingEntry(new UInt32[]{105}, Type105Convert),
            new FixedEncodingEntry(new UInt32[]{106}, Type106Convert),
            new FixedEncodingEntry(new UInt32[]{107}, Type107Convert),
            new FixedEncodingEntry(new UInt32[]{108}, Type108Convert),
            new FixedEncodingEntry(new UInt32[]{110}, Type110Convert),
            new FixedEncodingEntry(new UInt32[]{111}, Type111Convert),
            new FixedEncodingEntry(new UInt32[]{112}, Type112Convert),
            new FixedEncodingEntry(new UInt32[]{113}, Type113Convert),
            new FixedEncodingEntry(new UInt32[]{114}, Type114Convert),
            new FixedEncodingEntry(new UInt32[]{115}, Type115Convert),
            new FixedEncodingEntry(new UInt32[]{116}, Type116Convert),
            new FixedEncodingEntry(new UInt32[]{117, 118}, Type117_118Convert),
            new FixedEncodingEntry(new UInt32[]{119}, Type119Convert),
            new FixedEncodingEntry(new UInt32[]{120, 121}, Type120_121Convert),
            new FixedEncodingEntry(new UInt32[]{122, 123}, Type122_123Convert),
            new FixedEncodingEntry(new UInt32[]{124}, Type124Convert),
            new FixedEncodingEntry(new UInt32[]{125, 126}, Type125_126Convert),
            new FixedEncodingEntry(new UInt32[]{127}, Type127Convert),
            new FixedEncodingEntry(new UInt32[]{129, 130}, Type129_130Convert),
            new FixedEncodingEntry(new UInt32[]{131}, Type131Convert),
            new FixedEncodingEntry(new UInt32[]{133}, Type133Convert),
            new FixedEncodingEntry(new UInt32[]{134}, Type134Convert),
            new FixedEncodingEntry(new UInt32[]{135}, Type135Convert),
        };

        public bool Init(string rootDir, HashSet<SegmentType> requiredSegments = null)
        {
            return Init(rootDir, requiredSegments, out _);
        }

        public bool Init(string rootDir, HashSet<SegmentType> requiredSegments, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                DataReader = new DataReader();
                if (!DataReader.Init(rootDir))
                {
                    return false;
                }

                string udsDir = Path.Combine(rootDir, UdsDir);
                List<string[]> redirList = ExtractFileSegment(new List<string> {Path.Combine(udsDir, "redir" + FileExtension)}, "DIR", out errorMessage);
                if (redirList == null)
                {
                    return false;
                }

                _redirMap = new Dictionary<string, string>();
                foreach (string[] redirArray in redirList)
                {
                    if (redirArray.Length != 3)
                    {
                        return false;
                    }
                    _redirMap.Add(redirArray[1].ToLowerInvariant(), redirArray[2]);
                }

                _textMap = CreateTextDict(udsDir, "tttext*" + FileExtension, "TXT", out errorMessage);
                if (_textMap == null)
                {
                    return false;
                }

                _unitMap = CreateTextDict(udsDir, "unit*" + FileExtension, "UNT", out errorMessage);
                if (_unitMap == null)
                {
                    return false;
                }

                _fixedEncodingMap = new Dictionary<uint, FixedEncodingEntry>();
                foreach (FixedEncodingEntry fixedEncoding in FixedEncodingArray)
                {
                    foreach (UInt32 key in fixedEncoding.KeyArray)
                    {
                        _fixedEncodingMap[key] = fixedEncoding;
                    }
                }

                List<string[]> ttdopList = ExtractFileSegment(new List<string> { Path.Combine(udsDir, "ttdop" + FileExtension) }, "DOP", out errorMessage);
                if (ttdopList == null)
                {
                    return false;
                }
                _ttdopLookup = ttdopList.ToLookup(item => UInt32.Parse(item[0]));

                List<string[]> muxList = ExtractFileSegment(new List<string> { Path.Combine(udsDir, "mux" + FileExtension) }, "MUX", out errorMessage);
                if (muxList == null)
                {
                    return false;
                }
                _muxLookup = muxList.ToLookup(item => UInt32.Parse(item[0]));

                _chassisMap = CreateChassisDict(Path.Combine(udsDir, "chassis" + DataReader.FileExtension), out errorMessage);
                if (_chassisMap == null)
                {
                    return false;
                }

                foreach (SegmentInfo segmentInfo in SegmentInfos)
                {
                    segmentInfo.Ignored = false;
                    if (string.IsNullOrEmpty(segmentInfo.FileName))
                    {
                        continue;
                    }

                    if (requiredSegments != null)
                    {
                        if (!requiredSegments.Contains(segmentInfo.SegmentType))
                        {
                            segmentInfo.Ignored = true;
                            continue;
                        }
                    }

                    string fileName = Path.Combine(udsDir, Path.ChangeExtension(segmentInfo.FileName ?? string.Empty, FileExtension));
                    List<string[]> lineList = ExtractFileSegment(new List<string> {fileName}, segmentInfo.SegmentName, out errorMessage);
                    if (lineList == null)
                    {
                        return false;
                    }

                    segmentInfo.LineList = lineList;
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        // ReSharper disable once UnusedMember.Global
        public string TestFixedTypes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (FixedEncodingEntry entry in FixedEncodingArray)
            {
                sb.Append($"{entry.KeyArray[0]}:");
                sb.Append(" \"");
                sb.Append(entry.ToString(this, new byte[] { 0x10 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(entry.ToString(this, new byte[] { 0x10, 0x20 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(entry.ToString(this, new byte[] { 0xFF, 0x10 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(entry.ToString(this, new byte[] { 0xFF, 0x10, 0x20 }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(entry.ToString(this, new byte[] { 0xFF, 0xAB, 0xCD }));
                sb.Append("\"");

                sb.Append(" \"");
                sb.Append(entry.ToString(this, new byte[] { 0xFF, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67, 0x89 }));
                sb.Append("\"");

                sb.AppendLine();
            }
            return sb.ToString();
        }

        public SegmentInfo GetSegmentInfo(SegmentType segmentType)
        {
            foreach (SegmentInfo segmentInfo in SegmentInfos)
            {
                if (segmentInfo.SegmentType == segmentType)
                {
                    return segmentInfo;
                }
            }

            return null;
        }

        public List<ParseInfoAdp> GetAdpParseInfoList(string fileName)
        {
            try
            {
                if (!_adpParseInfoDict.TryGetValue(fileName, out List<ParseInfoAdp> adpSegmentList))
                {
                    List<string> includeFiles = FileNameResolver.GetAllFiles(fileName);
                    if (includeFiles == null)
                    {
                        return null;
                    }
                    List<ParseInfoBase> adpSegmentListExtract = ExtractFileSegment(includeFiles, SegmentType.Adp);
                    if (adpSegmentListExtract == null)
                    {
                        return null;
                    }
                    adpSegmentList = new List<ParseInfoAdp>();
                    foreach (ParseInfoBase parseInfo in adpSegmentListExtract)
                    {
                        if (parseInfo is ParseInfoAdp parseInfoAdp)
                        {
                            adpSegmentList.Add(parseInfoAdp);
                        }
                    }
                    _adpParseInfoDict.Add(fileName, adpSegmentList);
                }

                return adpSegmentList;
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public ParseInfoMwb GetMwbParseInfo(string fileName, string uniqueIdString)
        {
            try
            {
                if (!_mwbParseInfoDict.TryGetValue(fileName, out Dictionary<string, ParseInfoMwb> mbwSegmentDict))
                {
                    List<string> includeFiles = FileNameResolver.GetAllFiles(fileName);
                    if (includeFiles == null)
                    {
                        return null;
                    }
                    List<ParseInfoBase> mwbSegmentList = ExtractFileSegment(includeFiles, SegmentType.Mwb);
                    if (mwbSegmentList == null)
                    {
                        return null;
                    }

                    mbwSegmentDict = new Dictionary<string, ParseInfoMwb>();
                    foreach (ParseInfoBase parseInfo in mwbSegmentList)
                    {
                        if (parseInfo is ParseInfoMwb parseInfoMwb)
                        {
                            if (!mbwSegmentDict.ContainsKey(parseInfoMwb.UniqueIdString))
                            {
                                mbwSegmentDict.Add(parseInfoMwb.UniqueIdString, parseInfoMwb);
                            }
                            if (!mbwSegmentDict.ContainsKey(parseInfoMwb.UniqueIdStringOld))
                            {
                                mbwSegmentDict.Add(parseInfoMwb.UniqueIdStringOld, parseInfoMwb);
                            }

                            string[] uniqueParts1 = parseInfoMwb.UniqueIdString.Split('-');
                            if (uniqueParts1.Length > 0 && !string.IsNullOrEmpty(uniqueParts1[0]))
                            {
                                if (!mbwSegmentDict.ContainsKey(uniqueParts1[0]))
                                {
                                    mbwSegmentDict.Add(uniqueParts1[0], parseInfoMwb);
                                }
                            }
                        }
                    }

                    _mwbParseInfoDict.Add(fileName, mbwSegmentDict);
                }

                if (mbwSegmentDict.TryGetValue(uniqueIdString, out ParseInfoMwb parseInfoMwbMatch))
                {
                    return parseInfoMwbMatch;
                }

                string[] uniqueParts2 = uniqueIdString.Split('-');   // try with service id only
                if (uniqueParts2.Length > 0 && !string.IsNullOrEmpty(uniqueParts2[0]))
                {
                    if (mbwSegmentDict.TryGetValue(uniqueParts2[0], out parseInfoMwbMatch))
                    {
                        return parseInfoMwbMatch;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public ParseInfoDtc GetDtcParseInfo(string fileName, uint errorCode, uint detailCode)
        {
            try
            {
                if (!_dtcParseInfoDict.TryGetValue(fileName, out Dictionary<uint, ParseInfoDtc> dtcSegmentDict))
                {
                    List<string> includeFiles = FileNameResolver.GetAllFiles(fileName);
                    if (includeFiles == null)
                    {
                        return null;
                    }
                    List<ParseInfoBase> dtcSegmentList = ExtractFileSegment(includeFiles, SegmentType.Dtc);
                    if (dtcSegmentList == null)
                    {
                        return null;
                    }

                    dtcSegmentDict = new Dictionary<uint, ParseInfoDtc>();
                    foreach (ParseInfoBase parseInfo in dtcSegmentList)
                    {
                        if (parseInfo is ParseInfoDtc parseInfoDtc)
                        {
                            uint addKey = parseInfoDtc.ErrorCode << 8;
                            if (!dtcSegmentDict.ContainsKey(addKey))
                            {
                                dtcSegmentDict.Add(addKey, parseInfoDtc);
                            }

                            if (parseInfoDtc.DetailCode.HasValue && parseInfoDtc.DetailCode.Value != 0)
                            {
                                addKey |= parseInfoDtc.DetailCode.Value & 0xFF;
                                if (!dtcSegmentDict.ContainsKey(addKey))
                                {
                                    dtcSegmentDict.Add(addKey, parseInfoDtc);
                                }
                            }
                        }
                    }

                    _dtcParseInfoDict.Add(fileName, dtcSegmentDict);
                }

                uint matchKey = errorCode << 8;
                uint matchKeyDetail = matchKey | (detailCode & 0xFF);
                if (dtcSegmentDict.TryGetValue(matchKeyDetail, out ParseInfoDtc parseInfoDtcMatch))
                {
                    return parseInfoDtcMatch;
                }
                if (dtcSegmentDict.TryGetValue(matchKey, out parseInfoDtcMatch))
                {
                    return parseInfoDtcMatch;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public ParseInfoSlv.SlaveInfo GetSlvInfo(string fileName, uint slvAddr)
        {
            try
            {
                if (!_slvParseInfoDict.TryGetValue(fileName, out List<ParseInfoSlv> slvSegmentListMatch))
                {
                    List<string> includeFiles = FileNameResolver.GetAllFiles(fileName);
                    if (includeFiles == null)
                    {
                        return null;
                    }
                    List<ParseInfoBase> slvSegmentList = ExtractFileSegment(includeFiles, SegmentType.Slv);
                    if (slvSegmentList == null)
                    {
                        return null;
                    }

                    slvSegmentListMatch = new List<ParseInfoSlv>();
                    foreach (ParseInfoBase parseInfo in slvSegmentList)
                    {
                        if (parseInfo is ParseInfoSlv parseInfoSlv)
                        {
                            if (parseInfoSlv.SlaveList != null)
                            {
                                slvSegmentListMatch.Add(parseInfoSlv);
                            }
                        }
                    }

                    _slvParseInfoDict.Add(fileName, slvSegmentListMatch);
                }

                if (_slvParseInfoDict.TryGetValue(fileName, out List<ParseInfoSlv> parseInfoSlvMatch))
                {
                    foreach (ParseInfoSlv parseInfoSlv in parseInfoSlvMatch)
                    {
                        if (parseInfoSlv.SlaveList != null)
                        {
                            foreach (ParseInfoSlv.SlaveInfo slaveInfo in parseInfoSlv.SlaveList)
                            {
                                if (slaveInfo.MinAddr >= slvAddr && slaveInfo.MaxAddr <= slvAddr)
                                {
                                    return slaveInfo;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        public List<string> ErrorCodeToString(string fileName, uint errorCode, uint detailCode)
        {
            UdsReader udsReader = this;
            List<string> resultList = new List<string>();
            ParseInfoDtc parseInfoDtc = GetDtcParseInfo(fileName, errorCode, detailCode);
            if (parseInfoDtc != null)
            {
                if (!string.IsNullOrWhiteSpace(parseInfoDtc.PcodeText))
                {
                    resultList.Add(string.Format(CultureInfo.InvariantCulture, "{0} - {1:000}", parseInfoDtc.PcodeText, detailCode));
                }
                if (!string.IsNullOrWhiteSpace(parseInfoDtc.ErrorText))
                {
                    resultList.Add(parseInfoDtc.ErrorText);
                }
                if (!string.IsNullOrWhiteSpace(parseInfoDtc.ErrorDetail))
                {
                    resultList.Add(parseInfoDtc.ErrorDetail);
                }

                if ((detailCode & 0x80) != 0x00)
                {
                    resultList.Add((GetTextMapText(udsReader, 066900) ?? string.Empty) + " " + (GetTextMapText(udsReader, 000085) ?? string.Empty));   // Warnleuchte EIN
                }
                if ((detailCode & 0x01) == 0x00)
                {
                    resultList.Add(GetTextMapText(udsReader, 002693) ?? string.Empty);   // Sporadisch
                }
                if ((detailCode & 0x08) != 0x00)
                {
                    resultList.Add(GetTextMapText(udsReader, 022457) ?? string.Empty);   // Bestätigt
                }
                else
                {
                    resultList.Add(GetTextMapText(udsReader, 023505) ?? string.Empty);   // Unbestätigt
                }
#if false   // bad translation
                if ((detailCode & 0x10) == 0x00)
                {
                    resultList.Add(GetTextMapText(udsReader, 170986) ?? string.Empty);   // geprüft seit letzter Löschung
                }
                else
                {
                    resultList.Add(GetTextMapText(udsReader, 151076) ?? string.Empty);   // ungeprüft seit letzter Löschung
                }
#endif
            }

            return resultList;
        }

        public List<string> ErrorDetailBlockToString(string fileName, byte[] data)
        {
            UdsReader udsReader = this;
            List<string> resultList = new List<string>();
            if (data.Length < 1)
            {
                return null;
            }

            if (data[0] != 0x59)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            UInt32 value;
            if (data.Length >= 6 + 1)
            {
                value = data[6];
                if (value != 0xFF)
                {
                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 018478) ?? string.Empty); // Fehlerstatus
                    sb.Append(": ");
                    sb.Append(Convert.ToString(value, 2).PadLeft(8, '0'));
                    resultList.Add(sb.ToString());
                }
            }

            if (data.Length >= 7 + 1)
            {
                value = data[7];
                if (value != 0xFF)
                {
                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 016693) ?? string.Empty); // Fehlerpriorität
                    sb.Append(": ");
                    sb.Append($"{value & 0x0F:0}");
                    resultList.Add(sb.ToString());
                }
            }

            if (data.Length >= 8 + 1)
            {
                value = data[8];
                if (value != 0xFF)
                {
                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 061517) ?? string.Empty); // Fehlerhäufigkeit
                    sb.Append(": ");
                    sb.Append($"{value:0}");
                    resultList.Add(sb.ToString());
                }
            }

            if (data.Length >= 10 + 1)
            {
                value = data[10];
                if (value != 0xFF)
                {
                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 099026) ?? string.Empty); // Verlernzähler
                    sb.Append(": ");
                    sb.Append($"{value:0}");
                    resultList.Add(sb.ToString());
                }
            }

            if (data.Length >= 11 + 3)
            {
                value = (UInt32) ((data[11] << 16) | (data[12] << 8) | data[13]);
                if (value != 0xFFFFF)
                {
                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 018858) ?? string.Empty); // Kilometerstand
                    sb.Append(": ");
                    sb.Append($"{value:0}");
                    sb.Append(" ");
                    sb.Append(GetUnitMapText(udsReader, 000108) ?? string.Empty); // km
                    resultList.Add(sb.ToString());
                }
            }

            if (data.Length >= 15 + 5)
            {
                // date time
                UInt64 timeValue = 0;
                for (int i = 0; i < 5; i++)
                {
                    timeValue <<= 8;
                    timeValue += data[15 + i];
                }

                if (timeValue != 0 && timeValue != 0x1FFFFFFFF)
                {
                    UInt64 tempValue = timeValue;
                    UInt64 sec = tempValue & 0x3F;
                    tempValue >>= 6;
                    UInt64 min = tempValue & 0x3F;
                    tempValue >>= 6;
                    UInt64 hour = tempValue & 0x1F;
                    tempValue >>= 5;
                    UInt64 day = tempValue & 0x1F;
                    tempValue >>= 5;
                    UInt64 month = tempValue & 0x0F;
                    tempValue >>= 4;
                    UInt64 year = tempValue & 0x7F;

                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 098044) ?? string.Empty); // Datum
                    sb.Append(": ");
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:00}.{1:00}.{2:00}", year + 2000, month,
                        day));
                    resultList.Add(sb.ToString());

                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 099068) ?? string.Empty); // Zeit
                    sb.Append(": ");
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}", hour, min, sec));
                    resultList.Add(sb.ToString());
                }
            }
#if false
            if (data.Length >= 20 + 1)
            {
                value = data[20];
                if (value != 0xFF)
                {
                    sb.Clear();
                    sb.Append(GetTextMapText(udsReader, 098291) ?? string.Empty); // Spannung Klemme 30
                    sb.Append(": ");
                    double valueDouble = value * 0.2;
                    sb.Append($"{valueDouble:0.0} ");
                    sb.Append(GetUnitMapText(udsReader, 9) ?? string.Empty);    // V
                    resultList.Add(sb.ToString());
                }
            }
#endif
            List<string> resultListMwb = ErrorMbwDetailsToString(fileName, data);
            if (resultListMwb != null && resultListMwb.Count > 0)
            {
                resultList.AddRange(resultListMwb);
            }

            if (resultList.Count > 0)
            {
                sb.Clear();
                sb.Append(GetTextMapText(udsReader, 003356) ?? string.Empty);  // Umgebungsbedingungen
                sb.Append(":");
                resultList.Insert(0, sb.ToString());
            }
            return resultList;
        }

        private List<string> ErrorMbwDetailsToString(string fileName, byte[] data)
        {
            int offset = 21;
            if (data.Length <= offset)
            {
                return null;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (!_dtcMwbParseInfoDict.TryGetValue(fileName, out List<ParseInfoBase> mwbSegmentList))
            {
                List<string> includeFiles = FileNameResolver.GetAllFiles(fileName);
                if (includeFiles == null)
                {
                    return null;
                }
                mwbSegmentList = ExtractFileSegment(includeFiles, SegmentType.Mwb);
                if (mwbSegmentList == null)
                {
                    return null;
                }

                _dtcMwbParseInfoDict.Add(fileName, mwbSegmentList);
            }

            List<string> resultList = new List<string>();
            while (data.Length - offset > 0)
            {
                List<string> resultListSub = ErrorMbwDetailToString(mwbSegmentList, data, ref offset);
                if (resultListSub == null)
                {
                    return null;
                }
                resultList.AddRange(resultListSub);
            }

            return resultList;
        }

        private List<string> ErrorMbwDetailToString(List<ParseInfoBase> mwbSegmentList, byte[] data, ref int offset)
        {
            List<string> resultList = new List<string>();
            if (data.Length - offset < 3)
            {
                return null;
            }

            UInt32 serviceId = (UInt32) (data[offset + 0] << 8) | data[offset + 1];
            if (serviceId == 0)
            {
                return null;
            }
            List<ParseInfoMwb> parseInfoList = new List<ParseInfoMwb>();
            foreach (ParseInfoBase parseInfo in mwbSegmentList)
            {
                if (parseInfo is ParseInfoMwb parseInfoMwb)
                {
                    if (parseInfoMwb.ServiceId == serviceId)
                    {
                        parseInfoList.Add(parseInfoMwb);
                    }
                }
            }

            if (parseInfoList.Count == 0)
            {
                return null;
            }

            int telLength = 0;
            foreach (ParseInfoMwb parseInfoMwb in parseInfoList)
            {
                byte[] subData = new byte[data.Length - offset - 2];
                Array.Copy(data, offset + 2, subData, 0, subData.Length);
                string dataString = parseInfoMwb.DataTypeEntry.ToString(subData);
                if (!string.IsNullOrEmpty(dataString))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(parseInfoMwb.Name);
                    if (!string.IsNullOrEmpty(parseInfoMwb.DataTypeEntry.NameDetail))
                    {
                        sb.Append("-");
                        sb.Append(parseInfoMwb.DataTypeEntry.NameDetail);
                    }
                    sb.Append(": ");
                    sb.Append(dataString);
                    resultList.Add(sb.ToString());
                }

                if (!parseInfoMwb.DataTypeEntry.MinTelLength.HasValue)
                {
                    return null;
                }
                if (telLength < parseInfoMwb.DataTypeEntry.MinTelLength.Value)
                {
                    telLength = (int) parseInfoMwb.DataTypeEntry.MinTelLength.Value;
                }
            }

            if (telLength <= 0)
            {
                return null;
            }
            offset += telLength + 2;

            return resultList;
        }

        public List<ParseInfoBase> ExtractFileSegment(List<string> fileList, SegmentType segmentType)
        {
            SegmentInfo segmentInfoSel = GetSegmentInfo(segmentType);
            if (segmentInfoSel == null || segmentInfoSel.Ignored)
            {
                return null;
            }

            List<string[]> lineList = ExtractFileSegment(fileList, segmentInfoSel.SegmentName, out _);
            if (lineList == null)
            {
                return null;
            }

            List<ParseInfoBase> resultList = new List<ParseInfoBase>();
            foreach (string[] line in lineList)
            {
                if (line.Length < 2)
                {
                    return null;
                }

                if (!UInt32.TryParse(line[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 value))
                {
                    return null;
                }

                string[] lineArray = null;
                if (segmentInfoSel.LineList != null)
                {
                    if (value < 1 || value > segmentInfoSel.LineList.Count)
                    {
                        if (segmentType == SegmentType.Dtc)
                        {
                            continue;
                        }
                        return null;
                    }
                    lineArray = segmentInfoSel.LineList[(int)value - 1];
                }

                ParseInfoBase parseInfo;
                switch (segmentType)
                {
                    case SegmentType.Adp:
                    {
                        if (lineArray == null)
                        {
                            return null;
                        }
                        if (lineArray.Length == 2)
                        {
                            continue;
                        }
                        if (lineArray.Length < 15)
                        {
                            return null;
                        }

                        if (!UInt32.TryParse(lineArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 nameKey))
                        {
                            return null;
                        }

                        if (!_textMap.TryGetValue(nameKey, out string[] nameArray))
                        {
                            return null;
                        }

                        if (!UInt32.TryParse(lineArray[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 serviceId))
                        {
                            return null;
                        }

                        UInt32? subItem = null;
                        if (lineArray[1].Length > 0)
                        {
                            if (!UInt32.TryParse(lineArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 item))
                            {
                                return null;
                            }

                            subItem = item;
                        }

                        DataTypeEntry dataTypeEntry;
                        try
                        {
                            dataTypeEntry = new DataTypeEntry(this, lineArray, 3);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                        parseInfo = new ParseInfoAdp(serviceId, subItem, lineArray, nameKey, nameArray, dataTypeEntry);
                        break;
                    }

                    case SegmentType.Mwb:
                    {
                        if (lineArray == null || lineArray.Length < 14)
                        {
                            return null;
                        }
                        if (!UInt32.TryParse(lineArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 nameKey))
                        {
                            return null;
                        }

                        if (!_textMap.TryGetValue(nameKey, out string[] nameArray))
                        {
                            return null;
                        }

                        if (!UInt32.TryParse(lineArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 serviceId))
                        {
                            return null;
                        }

                        DataTypeEntry dataTypeEntry;
                        try
                        {
                            dataTypeEntry = new DataTypeEntry(this, lineArray, 2);
                        }
                        catch (Exception)
                        {
                            return null;
                        }

                        parseInfo = new ParseInfoMwb(serviceId, lineArray, nameKey, nameArray, dataTypeEntry);
                        break;
                    }

                    case SegmentType.Dtc:
                    {
                        if (lineArray == null || lineArray.Length < 8)
                        {
                            return null;
                        }

                        if (!UInt32.TryParse(lineArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 errorCode))
                        {
                            return null;
                        }

                        if (!UInt32.TryParse(lineArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 textKey))
                        {
                            return null;
                        }

                        UInt32? detailCode = null;
                        if (UInt32.TryParse(lineArray[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt32 detailCodeTemp))
                        {
                            detailCode = detailCodeTemp;
                        }

                        bool keyUsed = false;
                        if (!DataReader.CodeMap.TryGetValue(errorCode, out string errorText))
                        {
                            if (!DataReader.CodeMap.TryGetValue(textKey, out errorText))
                            {
                                errorText = string.Empty;
                            }
                            else
                            {
                                keyUsed = true;
                            }
                        }

                        UInt32 pcodeNum;
                        UInt32 pcodeDetailNum;
                        if (keyUsed)
                        {
                            pcodeNum = (textKey - 100000) & 0xFFFF;
                            pcodeDetailNum = 0;
                        }
                        else
                        {
                            pcodeNum = (errorCode >> 8) & 0xFFFF;
                            pcodeDetailNum = errorCode & 0xFF;
                        }

                        if (detailCode.HasValue)
                        {
                            pcodeDetailNum = detailCode.Value;
                        }

                        string pcodeText = string.Format(CultureInfo.InvariantCulture, "{0} {1:X02}", DataReader.SaePcodeToString(pcodeNum), pcodeDetailNum);

                        string errorDetail = string.Empty;
                        if (detailCode.HasValue && detailCode.Value > 0)
                        {
                            uint detailKey = detailCode.Value + 96000;
                            if (DataReader.CodeMap.TryGetValue(detailKey, out string detailText))
                            {
                                errorDetail = detailText;
                            }
                        }
                        if (string.IsNullOrEmpty(errorDetail))
                        {
                            int colonIndex = errorText.LastIndexOf(':');
                            if (colonIndex >= 0)
                            {
                                errorDetail = errorText.Substring(colonIndex + 1).Trim();
                                errorText = errorText.Substring(0, colonIndex);
                            }
                        }

                        parseInfo = new ParseInfoDtc(lineArray, errorCode, pcodeText, errorText, detailCode, errorDetail);
                        break;
                    }

                    case SegmentType.Slv:
                    {
                        List<ParseInfoSlv.SlaveInfo> slaveList = null;
                        UInt32? tableKey = null;
                        if (value != 0)
                        {
                            tableKey = value;
                            slaveList = new List<ParseInfoSlv.SlaveInfo>();
                            IEnumerable<string[]> bitList = _ttdopLookup[tableKey.Value];
                            foreach (string[] ttdopArray in bitList)
                            {
                                if (ttdopArray.Length >= 4)
                                {
                                    if (!UInt32.TryParse(ttdopArray[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 minAddr))
                                    {
                                        return null;
                                    }
                                    if (!UInt32.TryParse(ttdopArray[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 maxAddr))
                                    {
                                        return null;
                                    }
                                    if (!UInt32.TryParse(ttdopArray[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 nameKey))
                                    {
                                        return null;
                                    }
                                    if (!_textMap.TryGetValue(nameKey, out string[] nameArray))
                                    {
                                        return null;
                                    }

                                    string slvName = string.Empty;
                                    if (nameArray.Length >= 1)
                                    {
                                        slvName = nameArray[0];
                                    }
                                    slaveList.Add(new ParseInfoSlv.SlaveInfo(minAddr, maxAddr, slvName));
                                }
                            }
                        }

                        parseInfo = new ParseInfoSlv(line, tableKey, slaveList);
                        break;
                    }

                    default:
                        parseInfo = new ParseInfoBase(lineArray);
                        break;
                }
                resultList.Add(parseInfo);
            }

            return resultList;
        }

        public static Dictionary<uint, string[]> CreateTextDict(string dirName, string fileSpec, string segmentName, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                string[] files = Directory.GetFiles(dirName, fileSpec, SearchOption.TopDirectoryOnly);
                if (files.Length != 1)
                {
                    return null;
                }
                List<string[]> textList = ExtractFileSegment(files.ToList(), segmentName, out errorMessage);
                if (textList == null)
                {
                    return null;
                }

                Dictionary<uint, string[]> dict = new Dictionary<uint, string[]>();
                foreach (string[] textArray in textList)
                {
                    if (textArray.Length < 2)
                    {
                        return null;
                    }
                    if (!UInt32.TryParse(textArray[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 key))
                    {
                        return null;
                    }

                    dict.Add(key, textArray.Skip(1).ToArray());
                }

                return dict;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Dictionary<string, ChassisInfo> CreateChassisDict(string fileName, out string errorMessage)
        {
            errorMessage = null;
            try
            {
                List<string[]> textList = DataReader.ReadFileLines(fileName);
                if (textList == null)
                {
                    return null;
                }

                Dictionary<string, ChassisInfo> dict = new Dictionary<string, ChassisInfo> (ChassisMapFixed);
                foreach (string[] textArray in textList)
                {
                    if (textArray.Length != 2)
                    {
                        continue;
                    }

                    if (textArray[0].Length != 2)
                    {
                        continue;
                    }

                    string key = textArray[0];
                    string value = textArray[1];
                    if (dict.TryGetValue(key, out ChassisInfo chassisInfo))
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Multiple chassis entry for key: " + key);
#endif
                    }
                    else
                    {
                        dict.Add(key, new ChassisInfo(value));
                    }
                }

                return dict;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return null;
            }
        }

        public static string GetMd5Hash(string text)
        {
            //Prüfen ob Daten übergeben wurden.
            if ((text == null) || (text.Length == 0))
            {
                return string.Empty;
            }

            //MD5 Hash aus dem String berechnen. Dazu muss der string in ein Byte[]
            //zerlegt werden. Danach muss das Resultat wieder zurück in ein string.
            using (MD5 md5 = MD5.Create())
            {
                byte[] textToHash = Encoding.Default.GetBytes(text);
                byte[] result = md5.ComputeHash(textToHash);

                return BitConverter.ToString(result).Replace("-", "");
            }
        }

        public static List<string[]> ExtractFileSegment(List<string> fileList, string segmentName, out string errorMessage)
        {
            errorMessage = null;
            string segmentStart = "[" + segmentName + "]";
            string segmentEnd = "[/" + segmentName + "]";

            List<string[]> lineList = new List<string[]>();
            foreach (string fileName in fileList)
            {
                ZipFile zf = null;
                try
                {
                    Stream zipStream = null;
                    Encoding encoding = DataReader.GetEncodingForFileName(fileName);
                    string fileNameBase = Path.GetFileName(fileName);
                    FileStream fs = File.OpenRead(fileName);
                    zf = new ZipFile(fs)
                    {
                        Password = GetMd5Hash(Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant())
                    };
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue; // Ignore directories
                        }
                        if (string.Compare(zipEntry.Name, fileNameBase, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            zipStream = zf.GetInputStream(zipEntry);
                            break;
                        }
                    }

                    if (zipStream == null)
                    {
                        return null;
                    }
                    try
                    {
                        using (StreamReader sr = new StreamReader(zipStream, encoding))
                        {
                            bool inSegment = false;
                            for (; ; )
                            {
                                string line = sr.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }

                                if (line.StartsWith("["))
                                {
                                    if (string.Compare(line, segmentStart, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        inSegment = true;
                                    }
                                    else if (string.Compare(line, segmentEnd, StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        inSegment = false;
                                    }
                                    continue;
                                }

                                if (!inSegment)
                                {
                                    continue;
                                }
                                string[] lineArray = line.Split(',');
                                if (lineArray.Length > 0)
                                {
                                    lineList.Add(lineArray);
                                }
                            }
                        }
                    }
                    catch (NotImplementedException)
                    {
                        // closing of encrypted stream throws execption
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    return null;
                }
                finally
                {
                    if (zf != null)
                    {
                        zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                        zf.Close(); // Ensure we release resources
                    }
                }
            }
            return lineList;
        }

        public List<string> GetFileList(string fileName)
        {
            string dirName = Path.GetDirectoryName(fileName);
            if (dirName == null)
            {
                return null;
            }
            string fullName = Path.ChangeExtension(fileName, FileExtension);
            if (!File.Exists(fullName))
            {
                string key = Path.GetFileNameWithoutExtension(fileName)?.ToLowerInvariant();
                if (key == null)
                {
                    return null;
                }

                if (!_redirMap.TryGetValue(key, out string mappedName))
                {
                    return null;
                }

                if (string.Compare(mappedName, "EMPTY", StringComparison.OrdinalIgnoreCase) == 0)
                {   // no entry
                    return null;
                }

                fullName = Path.ChangeExtension(mappedName, FileExtension);
                if (fullName == null)
                {
                    return null;
                }
                fullName = Path.Combine(dirName, fullName.ToLowerInvariant());

                if (!File.Exists(fullName))
                {
                    return null;
                }
            }

            List<string> includeFiles = new List<string> {fullName};
            if (!GetIncludeFiles(fullName, includeFiles))
            {
                return null;
            }

            return includeFiles;
        }

        public static bool GetIncludeFiles(string fileName, List<string> includeFiles)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return false;
                }

                string dir = Path.GetDirectoryName(fileName);
                if (dir == null)
                {
                    return false;
                }

                List<string[]> lineList = ExtractFileSegment(new List<string> { fileName }, "INC", out _);
                if (lineList == null)
                {
                    return false;
                }

                foreach (string[] line in lineList)
                {
                    if (line.Length >= 2)
                    {
                        string file = line[1];
                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            string fileNameInc = Path.Combine(dir, Path.ChangeExtension(file.ToLowerInvariant(), FileExtension));
                            if (File.Exists(fileNameInc) && !includeFiles.Contains(fileNameInc))
                            {
                                includeFiles.Add(fileNameInc);
                                if (!GetIncludeFiles(fileNameInc, includeFiles))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
