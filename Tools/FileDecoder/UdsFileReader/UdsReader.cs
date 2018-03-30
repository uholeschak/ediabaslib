using System;
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
        private static readonly Encoding Encoding = Encoding.GetEncoding(1252);
        public const string FileExtension = ".uds";

        public enum SegmentType
        {
            Adp,
            Dtc,
            Ffmux,
            Ges,
            Mwb,
            Sot,
            Xpl,
        }

        public enum DataType
        {
            Float = 0,
            Binary1 = 1,
            Integer = 2,
            ValueName = 3,
            FixedEncoding = 4,
            Binary2 = 5,
            MuxTable = 6,
            HexBytes = 7,
            String = 8,
            IntHex = 9,
            IntUnscaled = 10,
            Invalid = 0x3F,
        }

        public const int DataTypeMaskSwapped = 0x40;
        public const int DataTypeMaskSigned = 0x80;
        public const int DataTypeMaskEnum = 0x3F;

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
            public delegate string ConvertDelegate(UdsReader udsReader, int typeId, byte[] data);

            public FixedEncodingEntry(UInt32[] keyArray, UInt32 dataTypeId, UInt32? unitKey = null, Int64? numberOfDigits = null, double? scaleOffset = null, double? scaleMult = null)
            {
                KeyArray = keyArray;
                DataTypeId = dataTypeId;
                UnitKey = unitKey;
                NumberOfDigits = numberOfDigits;
                ScaleOffset = scaleOffset;
                ScaleMult = scaleMult;
            }

            public FixedEncodingEntry(UInt32[] keyArray, ConvertDelegate convertFunc)
            {
                KeyArray = keyArray;
                ConvertFunc = convertFunc;
            }

            public UInt32[] KeyArray { get; }
            public UInt32 DataTypeId { get; }
            public UInt32? UnitKey { get; }
            public Int64? NumberOfDigits { get; }
            public double? ScaleOffset { get; }
            public double? ScaleMult { get; }
            public ConvertDelegate ConvertFunc { get; }
        }

        public class DataTypeEntry
        {
            public DataTypeEntry(UdsReader udsReader, string[] lineArray, int offset)
            {
                LineArray = lineArray;

                if (lineArray.Length >= offset + 10)
                {
                    if (!UInt32.TryParse(lineArray[offset + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 dataTypeId))
                    {
                        throw new Exception("No data type id");
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

                    if (UInt32.TryParse(lineArray[offset + 9], NumberStyles.Integer, CultureInfo.InvariantCulture, out UInt32 nameDetailKey))
                    {
                        if (!udsReader._textMap.TryGetValue(nameDetailKey, out string[] nameDetailArray))
                        {
                            throw new Exception("No name detail found");
                        }
                        NameDetailArray = nameDetailArray;
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
                        case DataType.Float:
                        case DataType.Integer:
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
                            DataTypeId = fixedEncodingEntry.DataTypeId;
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

            public string[] LineArray { get; }
            public UInt32 DataTypeId { get; }
            public string[] NameDetailArray { get; }
            public Int64? NumberOfDigits { get; }
            public UInt32? FixedEncodingId { get; }
            public double? ScaleOffset { get; }
            public double? ScaleMult { get; }
            public double? ScaleDiv { get; }
            public string UnitText { get; }
            public UInt32? ByteOffset { get; }
            public UInt32? BitOffset { get; }
            public UInt32? BitLength { get; }
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
            public ParseInfoMwb(UInt32 serviceId, string[] lineArray, string[] nameArray, DataTypeEntry dataTypeEntry) : base(lineArray)
            {
                ServiceId = serviceId;
                NameArray = nameArray;
                DataTypeEntry = dataTypeEntry;
            }

            public UInt32 ServiceId { get; }
            public string[] NameArray { get; }
            public DataTypeEntry DataTypeEntry { get; }
        }

        private class SegmentInfo
        {
            public SegmentInfo(SegmentType segmentType, string segmentName, string fileName)
            {
                SegmentType = segmentType;
                SegmentName = segmentName;
                FileName = fileName;
            }

            public SegmentType SegmentType { get; }
            public string SegmentName { get; }
            public string FileName { get; }
            public List<string[]> LineList { set; get; }
        }

        private readonly SegmentInfo[] _segmentInfos =
        {
            new SegmentInfo(SegmentType.Adp, "ADP", "RA"),
            new SegmentInfo(SegmentType.Dtc, "DTC", "RD"),
            new SegmentInfo(SegmentType.Ffmux, "FFMUX", "RF"),
            new SegmentInfo(SegmentType.Ges, "GES", "RG"),
            new SegmentInfo(SegmentType.Mwb, "MWB", "RM"),
            new SegmentInfo(SegmentType.Sot, "SOT", "RS"),
            new SegmentInfo(SegmentType.Xpl, "XPL", "RX"),
        };

        private Dictionary<string, string> _redirMap;
        private Dictionary<UInt32, string[]> _textMap;
        private Dictionary<UInt32, string[]> _unitMap;
        private Dictionary<UInt32, FixedEncodingEntry> _fixedEncodingMap;
        private ILookup<UInt32, string[]> _ttdopLookup;
        private ILookup<UInt32, string[]> _muxLookup;

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

        private static string GetTextMapText(UdsReader udsReader, UInt32 key)
        {
            if (udsReader._textMap.TryGetValue(key, out string[] nameArray1)) // Keine
            {
                if (nameArray1.Length > 0)
                {
                    return nameArray1[0];
                }
            }

            return null;
        }

        private static string GetUnitMapText(UdsReader udsReader, UInt32 key)
        {
            if (udsReader._unitMap.TryGetValue(key, out string[] nameArray1)) // Keine
            {
                if (nameArray1.Length > 0)
                {
                    return nameArray1[0];
                }
            }

            return null;
        }

        private static string Type2Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type3Convert(UdsReader udsReader, int typeId, byte[] data)
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
                    sb.Append(", ");
                }
                sb.Append(GetTextMapText(udsReader, textKey) ?? string.Empty);
            }

            return sb.ToString();
        }

        private static string Type18Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type19Convert(UdsReader udsReader, int typeId, byte[] data)
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
                    sb.Append($"B{(i >> 2) + 1}D{(i & 0x3) + 1} ");
                }
            }

            return sb.ToString();
        }

        private static string Type28Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type37_43a52_59Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type60_63Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type77_78Convert(UdsReader udsReader, int typeId, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            int value = (data[1] | (data[0] << 8));
            return $"{value / 60}h {value % 60}min";
        }

        private static string Type81Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type85_88Convert(UdsReader udsReader, int typeId, byte[] data)
        {
            if (data.Length < 2)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            double value1 = (data[0] - 128.0) * 100.0 / 128.0;
            sb.Append($"{value1:0.0} ");
            sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %
            if (data[1] != 0)
            {
                double value2 = (data[1] - 128.0) * 100.0 / 128.0;
                sb.Append($"{value2:0.0} ");
                sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %
            }

            return sb.ToString();
        }

        private static string Type103Convert(UdsReader udsReader, int typeId, byte[] data)
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
                        sb.Append(", ");
                    }
                    sb.Append($"ECT {i + 1}: {displayValue:0.} ");
                    sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C
                }
            }

            return sb.ToString();
        }

        private static string Type105Convert(UdsReader udsReader, int typeId, byte[] data)
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

                    if (sb.Length > 0)
                    {
                        sb.Append("/");
                    }
                    char name = (char)('A' + i);
                    sb.Append($"EGR {name}: ");
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if ((maskData & (1 << index)) != 0)
                    {
                        sb.Append($"{displayValue:0.} ");
                    }
                    else
                    {
                        sb.Append("--- ");
                    }
                    sb.Append(GetUnitMapText(udsReader, 1) ?? string.Empty);  // %
                    index++;
                }
            }

            return sb.ToString();
        }

        private static string Type107Convert(UdsReader udsReader, int typeId, byte[] data)
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
                    byte value = data[index + 1];
                    double displayValue = value - 40.0;

                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if ((maskData & (1 << index)) != 0)
                    {
                        sbVal.Append($"{displayValue:0.}");
                    }
                    else
                    {
                        sbVal.Append("---");
                    }
                    index++;
                }
                sbVal.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C

                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type108Convert(UdsReader udsReader, int typeId, byte[] data)
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
                    byte value = data[index + 1];
                    double displayValue = value * 100.0 / 255.0;

                    if (j > 0)
                    {
                        sbVal.Append("/");
                    }
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if ((maskData & (1 << index)) != 0)
                    {
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
                    sb.Append(", ");
                }
                sb.Append(sbVal);
            }

            return sb.ToString();
        }

        private static string Type112Convert(UdsReader udsReader, int typeId, byte[] data)
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
                        sb.Append(", ");
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

        private static string Type113Convert(UdsReader udsReader, int typeId, byte[] data)
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
                    if ((maskData & (1 << index)) != 0)
                    {
                        byte value = data[index + 1];
                        double displayValue = value * 100.0 / 255.0;
                        if (j > 0)
                        {
                            sbValue.Append("/");
                        }
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
                        sb.Append(", ");
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

        private static string Type117_118Convert(UdsReader udsReader, int typeId, byte[] data)
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

            sb.Append(" ");
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

        private static string Type119Convert(UdsReader udsReader, int typeId, byte[] data)
        {
            if (data.Length < 4 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 2; i++)
            {
                int maskIndex = i * 2;
                bool b1 = (maskData & (1 << maskIndex)) != 0;
                bool b2 = (maskData & (1 << (maskIndex + 1))) != 0;
                int bNum = i + 1;
                if (b1 & b2)
                {
                    sb.Append($"B{bNum}S1/B{bNum}S2: ");
                }
                else if (b1)
                {
                    sb.Append($"B{bNum}S1: ");
                }
                else if (b2)
                {
                    sb.Append($"B{bNum}S2: ");
                }

                if (b1)
                {
                    byte value = data[(i * 2) + 1];
                    double displayValue = value - 40.0;
                    sb.Append($"{displayValue:0.} ");
                    sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C
                }
                if (b2)
                {
                    byte value = data[(i * 2) + 2];
                    double displayValue = value - 40.0;
                    if (b1)
                    {
                        sb.Append("/");
                    }
                    sb.Append($"{displayValue:0.} ");
                    sb.Append(GetUnitMapText(udsReader, 3) ?? string.Empty);  // °C
                }
            }

            return sb.ToString();
        }

        private static string Type120_121Convert(UdsReader udsReader, int typeId, byte[] data)
        {
            if (data.Length < 8 + 1)
            {
                return string.Empty;
            }

            byte maskData = data[0];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                if ((maskData & (1 << i)) != 0)
                {
                    int value = (data[(i * 2) + 1] << 8) | data[(i * 2) + 2];
                    double displayValue = value * 0.1 - 40.0;
                    if (sb.Length > 0)
                    {
                        sb.Append("/");
                    }
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

        private static string Type122_123Convert(UdsReader udsReader, int typeId, byte[] data)
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

        private static string Type131Convert(UdsReader udsReader, int typeId, byte[] data)
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
                    sb.Append(", ");
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

        private static string Type133Convert(UdsReader udsReader, int typeId, byte[] data)
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

            sb.Append(" ");
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

            sb.Append(" ");
            sb.Append("NWI ");
            sb.Append(GetTextMapText(udsReader, 099068) ?? string.Empty);   // Time
            sb.Append(": ");
            if ((maskData & 0x08) != 0)
            {
                UInt32 value = (UInt32) ((data[6] << 24) | (data[7] << 16) | (data[8] << 8) | data[9]);
                sb.Append($"{value / 3600}H{value % 3600}s");
            }
            else
            {
                sb.Append("---");
            }

            return sb.ToString();
        }

        private static readonly FixedEncodingEntry[] FixedEncodingArray =
        {
            new FixedEncodingEntry(new UInt32[]{2}, Type2Convert),
            new FixedEncodingEntry(new UInt32[]{3}, Type3Convert),
            new FixedEncodingEntry(new UInt32[]{1, 65, 79, 80, 109, 136, 139}, (UInt32)DataType.Invalid), // not existing
            new FixedEncodingEntry(new UInt32[]{4, 17, 44, 46, 47, 91}, (UInt32)DataType.Float, 1, 1, 0, 100.0 / 255), // Unit %
            new FixedEncodingEntry(new UInt32[]{5, 15, 70, 92, 132}, (UInt32)DataType.Float, 3, 0, -40, 1.0), // Unit °C
            new FixedEncodingEntry(new UInt32[]{6, 7, 8, 9, 45}, (UInt32)DataType.Float, 1, 1, -128, 100 / 128), // Unit %
            new FixedEncodingEntry(new UInt32[]{10}, (UInt32)DataType.Float, 103, 0, 0, 3.0), // Unit kPa rel
            new FixedEncodingEntry(new UInt32[]{11, 51}, (UInt32)DataType.Float, 103, 0), // Unit kPa abs
            new FixedEncodingEntry(new UInt32[]{12}, (UInt32)DataType.Float, 21, 0, 0, 0.25), // Unit /min
            new FixedEncodingEntry(new UInt32[]{13, 33, 49}, (UInt32)DataType.Float, 109, 0), // Unit km/h
            new FixedEncodingEntry(new UInt32[]{14}, (UInt32)DataType.Float, 1, 1, -128, 1 / 2.0), // Unit %
            new FixedEncodingEntry(new UInt32[]{16}, (UInt32)DataType.Float, 26, 2, 0, 0.01), // Unit g/s
            new FixedEncodingEntry(new UInt32[]{18}, Type18Convert),
            new FixedEncodingEntry(new UInt32[]{19}, Type19Convert),
            new FixedEncodingEntry(new UInt32[]{20}, (UInt32)DataType.Float, 9, 3, 0, 0.005), // Unit V
            new FixedEncodingEntry(new UInt32[]{28}, Type28Convert),
            new FixedEncodingEntry(new UInt32[]{31}, (UInt32)DataType.Float, 8, 0), // Unit s
            new FixedEncodingEntry(new UInt32[]{35}, (UInt32)DataType.Float, 103, 0, 0, 10.0), // Unit kPa rel
            new FixedEncodingEntry(new UInt32[]{36, 37, 38, 39, 40, 41, 42, 43, 52, 53, 54, 55, 56, 57, 58, 59}, Type37_43a52_59Convert),
            new FixedEncodingEntry(new UInt32[]{48}, (UInt32)DataType.Float, null, 0),
            new FixedEncodingEntry(new UInt32[]{60, 61, 62, 63}, Type60_63Convert),
            new FixedEncodingEntry(new UInt32[]{77, 78}, Type77_78Convert),
            new FixedEncodingEntry(new UInt32[]{66}, (UInt32)DataType.Float, 9, 3, 0, 0.001), // Unit V
            new FixedEncodingEntry(new UInt32[]{68}, (UInt32)DataType.Float, 113, 3, 0, 1.0 / 32783.0), // Unit Lambda
            new FixedEncodingEntry(new UInt32[]{67, 69, 71, 72, 73, 74, 75, 76}, (UInt32)DataType.Float, 1, 0, 0, 100 / 255), // Unit %
            new FixedEncodingEntry(new UInt32[]{81}, Type81Convert),
            new FixedEncodingEntry(new UInt32[]{83}, (UInt32)DataType.Float, 103, 0, 0, 5.0), // Unit kPa abs
            new FixedEncodingEntry(new UInt32[]{85, 86, 87, 88}, Type85_88Convert),
            new FixedEncodingEntry(new UInt32[]{93}, (UInt32)DataType.Float, 2, 2, -26880, 1 / 128.0), // Unit °
            new FixedEncodingEntry(new UInt32[]{94}, (UInt32)DataType.Float, 110, 2, 0, 1 / 20.0), // Unit l/h
            new FixedEncodingEntry(new UInt32[]{97, 98}, (UInt32)DataType.Float, 1, 0, -125, 1.0), // Unit %
            new FixedEncodingEntry(new UInt32[]{99}, (UInt32)DataType.Float, 7, 0), // Unit Nm
            new FixedEncodingEntry(new UInt32[]{103}, Type103Convert),
            new FixedEncodingEntry(new UInt32[]{105}, Type105Convert),
            new FixedEncodingEntry(new UInt32[]{107}, Type107Convert),
            new FixedEncodingEntry(new UInt32[]{108}, Type108Convert),
            new FixedEncodingEntry(new UInt32[]{112}, Type112Convert),
            new FixedEncodingEntry(new UInt32[]{113}, Type113Convert),
            new FixedEncodingEntry(new UInt32[]{117, 118}, Type117_118Convert),
            new FixedEncodingEntry(new UInt32[]{119}, Type119Convert),
            new FixedEncodingEntry(new UInt32[]{120, 121}, Type120_121Convert),
            new FixedEncodingEntry(new UInt32[]{122, 123}, Type122_123Convert),
            new FixedEncodingEntry(new UInt32[]{131}, Type131Convert),
            new FixedEncodingEntry(new UInt32[]{133}, Type133Convert),
        };

        public bool Init(string dirName)
        {
            try
            {
                List<string[]> redirList = ExtractFileSegment(new List<string> {Path.Combine(dirName, "ReDir" + FileExtension)}, "DIR");
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
                    _redirMap.Add(redirArray[1].ToUpperInvariant(), redirArray[2]);
                }

                _textMap = CreateTextDict(dirName, "TTText*" + FileExtension, "TXT");
                if (_textMap == null)
                {
                    return false;
                }

                _unitMap = CreateTextDict(dirName, "Unit*" + FileExtension, "UNT");
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

                List<string[]> ttdopList = ExtractFileSegment(new List<string> { Path.Combine(dirName, "TTDOP" + FileExtension) }, "DOP");
                if (ttdopList == null)
                {
                    return false;
                }
                _ttdopLookup = ttdopList.ToLookup(item => UInt32.Parse(item[0]));

                List<string[]> muxList = ExtractFileSegment(new List<string> { Path.Combine(dirName, "MUX" + FileExtension) }, "MUX");
                if (muxList == null)
                {
                    return false;
                }
                _muxLookup = muxList.ToLookup(item => UInt32.Parse(item[0]));

                foreach (SegmentInfo segmentInfo in _segmentInfos)
                {
                    string fileName = Path.Combine(dirName, Path.ChangeExtension(segmentInfo.FileName, FileExtension));
                    List<string[]> lineList = ExtractFileSegment(new List<string> {fileName}, segmentInfo.SegmentName);
                    if (lineList == null)
                    {
                        return false;
                    }

                    segmentInfo.LineList = lineList;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<ParseInfoBase> ExtractFileSegment(List<string> fileList, SegmentType segmentType)
        {
            SegmentInfo segmentInfoSel = null;
            foreach (SegmentInfo segmentInfo in _segmentInfos)
            {
                if (segmentInfo.SegmentType == segmentType)
                {
                    segmentInfoSel = segmentInfo;
                    break;
                }
            }

            if (segmentInfoSel?.LineList == null)
            {
                return null;
            }

            List<string[]> lineList = ExtractFileSegment(fileList, segmentInfoSel.SegmentName);
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

                if (value < 1 || value > segmentInfoSel.LineList.Count)
                {
                    return null;
                }

                string[] lineArray = segmentInfoSel.LineList[(int) value - 1];

                ParseInfoBase parseInfo;
                switch (segmentType)
                {
                    case SegmentType.Mwb:
                    {
                        if (lineArray.Length < 14)
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

                        parseInfo = new ParseInfoMwb(serviceId, lineArray, nameArray, dataTypeEntry);
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

        public static Dictionary<uint, string[]> CreateTextDict(string dirName, string fileSpec, string segmentName)
        {
            try
            {
                string[] files = Directory.GetFiles(dirName, fileSpec, SearchOption.TopDirectoryOnly);
                if (files.Length != 1)
                {
                    return null;
                }
                List<string[]> textList = ExtractFileSegment(files.ToList(), segmentName);
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

        public static string GetMd5Hash(string text)
        {
            //Prüfen ob Daten übergeben wurden.
            if ((text == null) || (text.Length == 0))
            {
                return string.Empty;
            }

            //MD5 Hash aus dem String berechnen. Dazu muss der string in ein Byte[]
            //zerlegt werden. Danach muss das Resultat wieder zurück in ein string.
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(text);
            byte[] result = md5.ComputeHash(textToHash);

            return BitConverter.ToString(result).Replace("-", "");
        }

        public static List<string[]> ExtractFileSegment(List<string> fileList, string segmentName)
        {
            string segmentStart = "[" + segmentName + "]";
            string segmentEnd = "[/" + segmentName + "]";

            List<string[]> lineList = new List<string[]>();
            foreach (string fileName in fileList)
            {
                ZipFile zf = null;
                try
                {
                    Stream zipStream = null;
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
                        using (StreamReader sr = new StreamReader(zipStream, Encoding))
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
                catch (Exception)
                {
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
                string key = Path.GetFileNameWithoutExtension(fileName)?.ToUpperInvariant();
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
                fullName = Path.Combine(dirName, fullName);

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

                List<string[]> lineList = ExtractFileSegment(new List<string> { fileName }, "INC");
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
                            string fileNameInc = Path.Combine(dir, Path.ChangeExtension(file, FileExtension));
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
