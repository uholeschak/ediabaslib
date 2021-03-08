using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using EdiabasLib;

namespace BmwFileReader
{
    public class SgFunctions
    {
        public enum TableDataType
        {
            Undefined,
            Float,
            String,
            Binary,
            Bit
        }

        public class SgFuncInfo
        {
            public SgFuncInfo(string arg, string id, string result, string info, string dataType, TableDataType tableDataType, int? length, string unit,
                string name, List<int> serviceList, string argTab, string resTab)
            {
                Arg = arg;
                Id = id;
                Result = result;
                Info = info;
                InfoTrans = null;
                DataType = dataType;
                TableDataType = tableDataType;
                Length = length;
                Unit = unit;
                Name = name;
                NameInfoList = null;
                ServiceList = serviceList;
                ArgTab = argTab;
                ArgInfoList = null;
                ResTab = resTab;
                ResInfoList = null;
            }

            public string Arg { get; }

            public string Id { get; }

            public string Result { get; }

            public string Info { get; }

            public string InfoTrans { get; set; }

            public string DataType { get; }

            public TableDataType TableDataType { get; set; }

            public int? Length { get; }

            public string Unit { get; }

            public string Name { get; }

            public List<SgFuncNameInfo> NameInfoList { get; set; }

            public List<int> ServiceList { get; }

            public string ArgTab { get; }

            public List<SgFuncArgInfo> ArgInfoList { get; set; }

            public string ResTab { get; }

            public List<SgFuncNameInfo> ResInfoList { get; set; }
        }

        public class SgFuncArgInfo
        {
            public SgFuncArgInfo(string arg, string unit, string dataType, TableDataType tableDataType, string mask, string minText, string maxText,
                double? mul, double? div, double? add, double? min, double? max, int? length, string name, string info)
            {
                Arg = arg;
                Unit = unit;
                DataType = dataType;
                TableDataType = tableDataType;
                Mask = mask;
                MinText = minText;
                MaxText = maxText;
                Mul = mul;
                Div = div;
                Add = add;
                Min = min;
                Max = max;
                Length = length;
                Name = name;
                NameInfoList = null;
                Info = info;
                InfoTrans = null;
            }

            public string Arg { get; }

            public string Unit { get; }

            public string DataType { get; }

            public TableDataType TableDataType { get; set; }

            public string Mask { get; }

            public string MinText { get; }

            public string MaxText { get; }

            public double? Mul { get; }

            public double? Div { get; }

            public double? Add { get; }

            public double? Min { get; }

            public double? Max { get; }

            public int? Length { get; set; }

            public string Name { get; }

            public List<SgFuncNameInfo> NameInfoList { get; set; }

            public string Info { get; }

            public string InfoTrans { get; set; }
        }

        public class SgFuncNameInfo
        {
        }

        public class SgFuncValNameInfo : SgFuncNameInfo
        {
            public SgFuncValNameInfo(string value, string text)
            {
                Value = value;
                Text = text;
            }

            public string Value { get; }

            public string Text { get; }
        }

        public class SgFuncBitFieldInfo : SgFuncNameInfo
        {
            public SgFuncBitFieldInfo(string resultName, string unit, string dataType, TableDataType tableDataType, string mask,
                double? mul, double? div, double? add, int? length, string name, string info)
            {
                ResultName = resultName;
                Unit = unit;
                DataType = dataType;
                TableDataType = tableDataType;
                Mask = mask;
                Mul = mul;
                Div = div;
                Add = add;
                Length = length;
                Name = name;
                NameInfoList = null;
                Info = info;
                InfoTrans = null;
            }

            public string ResultName { get; }

            public string Unit { get; }

            public string DataType { get; }

            public TableDataType TableDataType { get; set; }

            public string Mask { get; }

            public double? Mul { get; }

            public double? Div { get; }

            public double? Add { get; }

            public int? Length { get; set; }

            public string Name { get; }

            public List<SgFuncNameInfo> NameInfoList { get; set; }

            public string Info { get; }

            public string InfoTrans { get; set; }
        }

        public const string TableSgFunctions = @"SG_FUNKTIONEN";
        public const string TableMwTab = @"MESSWERTETAB";
        public const string SgFuncUnitValName = @"0-n";
        public const string SgFuncUnitBit = @"Bit";

        // data type strings
        public const string DataTypeChar = @"char";
        public const string DataTypeInt = @"int";
        public const string DataTypeLong = @"long";
        public const string DataTypeFloat = @"float";
        public const string DataTypeDouble = @"double";
        public const string DataTypeString = @"string";
        public const string DataTypeData = @"data";
        public const string DataTypeBitField = @"bitfield";
        public const string DataTypeUnsigned = @"unsigned";
        public const string DataTypeMotorola = @"motorola";

        public enum UdsServiceId
        {
            ReadDataById = 0x22,
            DynamicallyDefineId = 0x2C,
            WriteDataById = 0x2E,
            IoControlById = 0x2F,
            RoutineControl = 0x31,
            MwBlock = 0x100,
        }

        public static readonly Tuple<string, UdsServiceId>[] SgFuncJobListList =
        {
            new Tuple<string, UdsServiceId>("STATUS_LESEN", UdsServiceId.ReadDataById),
            new Tuple<string, UdsServiceId>("STATUS_BLOCK_LESEN", UdsServiceId.DynamicallyDefineId),
            new Tuple<string, UdsServiceId>("STEUERN", UdsServiceId.WriteDataById),
            new Tuple<string, UdsServiceId>("STEUERN_IO", UdsServiceId.IoControlById),
            new Tuple<string, UdsServiceId>("STEUERN_ROUTINE", UdsServiceId.RoutineControl),
            new Tuple<string, UdsServiceId>("STATUS_MESSWERTBLOCK_LESEN", UdsServiceId.MwBlock),
        };

        private bool _disposed;
        private EdiabasNet _ediabas;
        private readonly Dictionary<string, List<SgFuncArgInfo>> _sgFuncArgInfoDict = new Dictionary<string, List<SgFuncArgInfo>>();
        private readonly Dictionary<string, List<SgFuncNameInfo>> _sgFuncNameInfoDict = new Dictionary<string, List<SgFuncNameInfo>>();

        public EdiabasNet Ediabas => _ediabas;
        public Dictionary<string, List<SgFuncArgInfo>> SgFuncArgInfoDict => _sgFuncArgInfoDict;
        public Dictionary<string, List<SgFuncNameInfo>> SgFuncNameInfoDict => _sgFuncNameInfoDict;

        public SgFunctions(EdiabasNet ediabas)
        {
            _ediabas = ediabas;
        }

        public List<SgFuncInfo> ReadMwTabTable()
        {
            if (_ediabas == null)
            {
                return null;
            }

            ResetCache();

            List<SgFuncInfo> sgFuncInfoList = new List<SgFuncInfo>();
            try
            {
                _ediabas.ArgString = TableMwTab;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.NoInitForVJobs = true;
                _ediabas.ExecuteJob("_TABLE");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsTab = _ediabas.ResultSets;
                if (resultSetsTab != null && resultSetsTab.Count >= 2)
                {
                    int argIndex = -1;
                    int idIndex = -1;
                    int resultIndex = -1;
                    int infoIndex = -1;
                    int unitIndex = -1;
                    int nameIndex = -1;
                    int dictIndex = 0;
                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSetsTab)
                    {
                        if (dictIndex == 0)
                        {
                            dictIndex++;
                            continue;
                        }

                        string arg = string.Empty;
                        string id = string.Empty;
                        string result = string.Empty;
                        string info = string.Empty;
                        string unit = string.Empty;
                        string name = string.Empty;
                        for (int i = 0; ; i++)
                        {
                            if (resultDict.TryGetValue("COLUMN" + i.ToString(CultureInfo.InvariantCulture), out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    string entry = (string)resultData.OpData;
                                    if (dictIndex == 1)
                                    {   // header
                                        if (string.Compare(entry, "ARG", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            argIndex = i;
                                        }
                                        else if (string.Compare(entry, "ID", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            idIndex = i;
                                        }
                                        else if (string.Compare(entry, "RESULTNAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            resultIndex = i;
                                        }
                                        else if (string.Compare(entry, "INFO", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            infoIndex = i;
                                        }
                                        else if (string.Compare(entry, "EINHEIT", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            unitIndex = i;
                                        }
                                        else if (string.Compare(entry, "NAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            nameIndex = i;
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(entry) && entry != "-")
                                        {
                                            if (i == argIndex)
                                            {
                                                arg = entry;
                                            }
                                            else if (i == idIndex)
                                            {
                                                id = entry;
                                            }
                                            else if (i == resultIndex)
                                            {
                                                result = entry;
                                            }
                                            else if (i == infoIndex)
                                            {
                                                info = entry;
                                            }
                                            else if (i == unitIndex)
                                            {
                                                unit = entry;
                                            }
                                            else if (i == nameIndex)
                                            {
                                                name = entry;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(arg) && !string.IsNullOrEmpty(id))
                        {
                            sgFuncInfoList.Add(new SgFuncInfo(arg, id, result, info, null, TableDataType.Undefined, null, unit, name, null, null, null));
                        }

                        dictIndex++;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return sgFuncInfoList;
        }

        public List<SgFuncInfo> ReadSgFuncTable()
        {
            if (_ediabas == null)
            {
                return null;
            }

            ResetCache();

            List<SgFuncInfo> sgFuncInfoList = new List<SgFuncInfo>();
            try
            {
                _ediabas.ArgString = TableSgFunctions;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.NoInitForVJobs = true;
                _ediabas.ExecuteJob("_TABLE");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsTab = _ediabas.ResultSets;
                if (resultSetsTab != null && resultSetsTab.Count >= 2)
                {
                    int argIndex = -1;
                    int idIndex = -1;
                    int resultIndex = -1;
                    int infoIndex = -1;
                    int unitIndex = -1;
                    int dataTypeIndex = -1;
                    int nameIndex = -1;
                    int serviceIndex = -1;
                    int argTabIndex = -1;
                    int resTabIndex = -1;
                    int dictIndex = 0;
                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSetsTab)
                    {
                        if (dictIndex == 0)
                        {
                            dictIndex++;
                            continue;
                        }

                        string arg = string.Empty;
                        string id = string.Empty;
                        string result = string.Empty;
                        string info = string.Empty;
                        string unit = string.Empty;
                        string dataType = string.Empty;
                        string name = string.Empty;
                        string service = string.Empty;
                        string argTab = string.Empty;
                        string resTab = string.Empty;
                        for (int i = 0; ; i++)
                        {
                            if (resultDict.TryGetValue("COLUMN" + i.ToString(CultureInfo.InvariantCulture), out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    string entry = (string)resultData.OpData;
                                    if (dictIndex == 1)
                                    {   // header
                                        if (string.Compare(entry, "ARG", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            argIndex = i;
                                        }
                                        else if (string.Compare(entry, "ID", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            idIndex = i;
                                        }
                                        else if (string.Compare(entry, "RESULTNAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            resultIndex = i;
                                        }
                                        else if (string.Compare(entry, "INFO", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            infoIndex = i;
                                        }
                                        else if (string.Compare(entry, "EINHEIT", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            unitIndex = i;
                                        }
                                        else if (string.Compare(entry, "DATENTYP", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            dataTypeIndex = i;
                                        }
                                        else if (string.Compare(entry, "NAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            nameIndex = i;
                                        }
                                        else if (string.Compare(entry, "SERVICE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            serviceIndex = i;
                                        }
                                        else if (string.Compare(entry, "ARG_TABELLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            argTabIndex = i;
                                        }
                                        else if (string.Compare(entry, "RES_TABELLE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            resTabIndex = i;
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(entry) && entry != "-")
                                        {
                                            if (i == argIndex)
                                            {
                                                arg = entry;
                                            }
                                            else if (i == idIndex)
                                            {
                                                id = entry;
                                            }
                                            else if (i == resultIndex)
                                            {
                                                result = entry;
                                            }
                                            else if (i == infoIndex)
                                            {
                                                info = entry;
                                            }
                                            else if (i == unitIndex)
                                            {
                                                unit = entry;
                                            }
                                            else if (i == dataTypeIndex)
                                            {
                                                dataType = entry.Trim();
                                            }
                                            else if (i == nameIndex)
                                            {
                                                name = entry;
                                            }
                                            else if (i == serviceIndex)
                                            {
                                                service = entry;
                                            }
                                            else if (i == argTabIndex)
                                            {
                                                argTab = entry;
                                            }
                                            else if (i == resTabIndex)
                                            {
                                                resTab = entry;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(arg) && !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(service))
                        {
                            List<int> serviceList = new List<int>();
                            string[] serviceArray = service.Split(';');
                            foreach (string serviceEntry in serviceArray)
                            {
                                try
                                {
                                    Int64 value = Convert.ToInt64(serviceEntry, 16);
                                    serviceList.Add((int)value);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            }

                            TableDataType tableDataType = ConvertDataType(dataType, null, out double? _, out double? _, out int? dataLength);
                            sgFuncInfoList.Add(new SgFuncInfo(arg, id, result, info, dataType, tableDataType, dataLength, unit, name, serviceList, argTab, resTab));
                        }

                        dictIndex++;
                    }
                }

                foreach (SgFuncInfo sgFuncInfo in sgFuncInfoList)
                {
                    if (!string.IsNullOrEmpty(sgFuncInfo.Name))
                    {
                        sgFuncInfo.NameInfoList = ReadSgFuncNameTable(sgFuncInfo.Name, sgFuncInfo.Unit);
                    }

                    if (!string.IsNullOrEmpty(sgFuncInfo.ArgTab))
                    {
                        sgFuncInfo.ArgInfoList = ReadSgFuncArgTable(sgFuncInfo.ArgTab);
                    }

                    if (!string.IsNullOrEmpty(sgFuncInfo.ResTab))
                    {
                        sgFuncInfo.ResInfoList = ReadSgFuncNameTable(sgFuncInfo.ResTab);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return sgFuncInfoList;
        }

        public List<SgFuncArgInfo> ReadSgFuncArgTable(string tableName)
        {
            if (_ediabas == null)
            {
                return null;
            }

            string key = tableName.ToUpperInvariant();
            if (_sgFuncArgInfoDict.TryGetValue(key, out List<SgFuncArgInfo> infoList))
            {
                return infoList;
            }

            List<SgFuncArgInfo> argInfoList = new List<SgFuncArgInfo>();
            try
            {
                _ediabas.ArgString = tableName;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.NoInitForVJobs = true;
                _ediabas.ExecuteJob("_TABLE");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsTab = _ediabas.ResultSets;
                if (resultSetsTab != null && resultSetsTab.Count >= 2)
                {
                    int argIndex = -1;
                    int unitIndex = -1;
                    int dataTypeIndex = -1;
                    int maskIndex = -1;
                    int mulIndex = -1;
                    int divIndex = -1;
                    int addIndex = -1;
                    int minIndex = -1;
                    int maxIndex = -1;
                    int nameIndex = -1;
                    int infoIndex = -1;
                    int dictIndex = 0;
                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSetsTab)
                    {
                        if (dictIndex == 0)
                        {
                            dictIndex++;
                            continue;
                        }

                        string arg = string.Empty;
                        string unit = string.Empty;
                        string dataType = string.Empty;
                        string mask = string.Empty;
                        string name = string.Empty;
                        string mul = string.Empty;
                        string div = string.Empty;
                        string add = string.Empty;
                        string min = string.Empty;
                        string max = string.Empty;
                        string info = string.Empty;
                        for (int i = 0; ; i++)
                        {
                            if (resultDict.TryGetValue("COLUMN" + i.ToString(CultureInfo.InvariantCulture), out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    string entry = (string)resultData.OpData;
                                    if (dictIndex == 1)
                                    {   // header
                                        if (string.Compare(entry, "ARG", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            argIndex = i;
                                        }
                                        else if (string.Compare(entry, "EINHEIT", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            unitIndex = i;
                                        }
                                        else if (string.Compare(entry, "DATENTYP", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            dataTypeIndex = i;
                                        }
                                        else if (string.Compare(entry, "MASKE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            maskIndex = i;
                                        }
                                        else if (string.Compare(entry, "NAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            nameIndex = i;
                                        }
                                        else if (string.Compare(entry, "MUL", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            mulIndex = i;
                                        }
                                        else if (string.Compare(entry, "DIV", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            divIndex = i;
                                        }
                                        else if (string.Compare(entry, "ADD", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            addIndex = i;
                                        }
                                        else if (string.Compare(entry, "MIN", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            minIndex = i;
                                        }
                                        else if (string.Compare(entry, "MAX", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            maxIndex = i;
                                        }
                                        else if (string.Compare(entry, "INFO", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            infoIndex = i;
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(entry) && entry != "-")
                                        {
                                            if (i == argIndex)
                                            {
                                                arg = entry.Trim();
                                            }
                                            else if (i == unitIndex)
                                            {
                                                unit = entry.Trim();
                                            }
                                            else if (i == dataTypeIndex)
                                            {
                                                dataType = entry.Trim();
                                            }
                                            else if (i == maskIndex)
                                            {
                                                mask = entry.Trim();
                                            }
                                            else if (i == nameIndex)
                                            {
                                                name = entry.Trim();
                                            }
                                            else if (i == mulIndex)
                                            {
                                                mul = entry.Trim();
                                            }
                                            else if (i == divIndex)
                                            {
                                                div = entry.Trim();
                                            }
                                            else if (i == addIndex)
                                            {
                                                add = entry.Trim();
                                            }
                                            else if (i == minIndex)
                                            {
                                                min = entry.Trim();
                                            }
                                            else if (i == maxIndex)
                                            {
                                                max = entry.Trim();
                                            }
                                            else if (i == infoIndex)
                                            {
                                                info = entry.Trim();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(arg))
                        {
                            TableDataType tableDataType = ConvertDataType(dataType, null, out double? dataMinValue, out double? dataMaxValue, out int? dataLength);
                            double? mulValue = ConvertFloatValue(mul);
                            double? divValue = ConvertFloatValue(div);
                            double? addValue = ConvertFloatValue(add);

                            double? minValue = ConvertFloatValue(min);
                            minValue ??= ScaleValue(dataMinValue, mulValue, divValue, addValue);

                            double? maxValue = ConvertFloatValue(max);
                            maxValue ??= ScaleValue(dataMaxValue, mulValue, divValue, addValue);
                            argInfoList.Add(new SgFuncArgInfo(arg, unit, dataType, tableDataType, mask, min, max,
                                mulValue, divValue, addValue, minValue, maxValue, dataLength, name, info));
                        }

                        dictIndex++;
                    }
                }

                foreach (SgFuncArgInfo funcArgInfo in argInfoList)
                {
                    List<SgFuncNameInfo> nameInfoList = ReadSgFuncNameTable(funcArgInfo.Name, funcArgInfo.Unit);
                    funcArgInfo.TableDataType = ConvertDataType(funcArgInfo.DataType, nameInfoList, out double? _, out double? _, out int? dataLength);
                    funcArgInfo.NameInfoList = nameInfoList;
                    funcArgInfo.Length = dataLength;
                }

                if (argInfoList.Count > 0)
                {
                    _sgFuncArgInfoDict.Add(key, argInfoList);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return argInfoList;
        }

        public List<SgFuncNameInfo> ReadSgFuncNameTable(string tableName, string unit = null)
        {
            if (_ediabas == null)
            {
                return null;
            }

            try
            {
                if (string.IsNullOrEmpty(tableName))
                {
                    return null;
                }

                string key = tableName;
                if (!string.IsNullOrEmpty(unit))
                {
                    key += "_" + unit;
                }
                key = key.ToUpperInvariant();

                if (_sgFuncNameInfoDict.TryGetValue(key, out List<SgFuncNameInfo> infoList))
                {
                    return infoList;
                }

                List<SgFuncNameInfo> nameInfoList;
                if (!string.IsNullOrEmpty(unit) && string.Compare(unit, SgFuncUnitValName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    nameInfoList = ReadSgFuncValNameTable(tableName);
                }
                else
                {
                    nameInfoList = ReadSgFuncBitFieldTable(tableName);
                }

                if (nameInfoList != null && nameInfoList.Count > 0)
                {
                    _sgFuncNameInfoDict.Add(key, nameInfoList);
                }

                return nameInfoList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<SgFuncNameInfo> ReadSgFuncValNameTable(string tableName)
        {
            if (_ediabas == null)
            {
                return null;
            }

            List<SgFuncNameInfo> valNameInfoList = new List<SgFuncNameInfo>();
            try
            {
                _ediabas.ArgString = tableName;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.NoInitForVJobs = true;
                _ediabas.ExecuteJob("_TABLE");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsTab = _ediabas.ResultSets;
                if (resultSetsTab != null && resultSetsTab.Count >= 2)
                {
                    int valueIndex = -1;
                    int textIndex = -1;
                    int dictIndex = 0;
                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSetsTab)
                    {
                        if (dictIndex == 0)
                        {
                            dictIndex++;
                            continue;
                        }

                        string value = string.Empty;
                        string text = string.Empty;
                        for (int i = 0; ; i++)
                        {
                            if (resultDict.TryGetValue("COLUMN" + i.ToString(CultureInfo.InvariantCulture), out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    string entry = (string)resultData.OpData;
                                    if (dictIndex == 1)
                                    {   // header
                                        if (string.Compare(entry, "WERT", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            valueIndex = i;
                                        }
                                        else if (string.Compare(entry, "TEXT", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            textIndex = i;
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(entry) && entry != "-")
                                        {
                                            if (i == valueIndex)
                                            {
                                                value = entry.Trim();
                                            }
                                            else if (i == textIndex)
                                            {
                                                text = entry.Trim();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            valNameInfoList.Add(new SgFuncValNameInfo(value, text));
                        }

                        dictIndex++;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return valNameInfoList;
        }

        public List<SgFuncNameInfo> ReadSgFuncBitFieldTable(string tableName)
        {
            if (_ediabas == null)
            {
                return null;
            }

            List<SgFuncNameInfo> bitFieldInfoList = new List<SgFuncNameInfo>();
            try
            {
                _ediabas.ArgString = tableName;
                _ediabas.ArgBinaryStd = null;
                _ediabas.ResultsRequests = string.Empty;
                _ediabas.NoInitForVJobs = true;
                _ediabas.ExecuteJob("_TABLE");

                List<Dictionary<string, EdiabasNet.ResultData>> resultSetsTab = _ediabas.ResultSets;
                if (resultSetsTab != null && resultSetsTab.Count >= 2)
                {
                    int resultNameIndex = -1;
                    int unitIndex = -1;
                    int dataTypeIndex = -1;
                    int maskIndex = -1;
                    int mulIndex = -1;
                    int divIndex = -1;
                    int addIndex = -1;
                    int nameIndex = -1;
                    int infoIndex = -1;
                    int dictIndex = 0;
                    foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSetsTab)
                    {
                        if (dictIndex == 0)
                        {
                            dictIndex++;
                            continue;
                        }

                        string resultName = string.Empty;
                        string unit = string.Empty;
                        string dataType = string.Empty;
                        string mask = string.Empty;
                        string name = string.Empty;
                        string mul = string.Empty;
                        string div = string.Empty;
                        string add = string.Empty;
                        string info = string.Empty;
                        for (int i = 0; ; i++)
                        {
                            if (resultDict.TryGetValue("COLUMN" + i.ToString(CultureInfo.InvariantCulture), out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    string entry = (string)resultData.OpData;
                                    if (dictIndex == 1)
                                    {   // header
                                        if (string.Compare(entry, "RESULTNAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            resultNameIndex = i;
                                        }
                                        else if (string.Compare(entry, "EINHEIT", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            unitIndex = i;
                                        }
                                        else if (string.Compare(entry, "DATENTYP", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            dataTypeIndex = i;
                                        }
                                        else if (string.Compare(entry, "MASKE", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            maskIndex = i;
                                        }
                                        else if (string.Compare(entry, "NAME", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            nameIndex = i;
                                        }
                                        else if (string.Compare(entry, "MUL", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            mulIndex = i;
                                        }
                                        else if (string.Compare(entry, "DIV", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            divIndex = i;
                                        }
                                        else if (string.Compare(entry, "ADD", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            addIndex = i;
                                        }
                                        else if (string.Compare(entry, "INFO", StringComparison.OrdinalIgnoreCase) == 0)
                                        {
                                            infoIndex = i;
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrWhiteSpace(entry) && entry != "-")
                                        {
                                            if (i == resultNameIndex)
                                            {
                                                resultName = entry.Trim();
                                            }
                                            else if (i == unitIndex)
                                            {
                                                unit = entry.Trim();
                                            }
                                            else if (i == dataTypeIndex)
                                            {
                                                dataType = entry.Trim();
                                            }
                                            else if (i == maskIndex)
                                            {
                                                mask = entry.Trim();
                                            }
                                            else if (i == nameIndex)
                                            {
                                                name = entry.Trim();
                                            }
                                            else if (i == mulIndex)
                                            {
                                                mul = entry.Trim();
                                            }
                                            else if (i == divIndex)
                                            {
                                                div = entry.Trim();
                                            }
                                            else if (i == addIndex)
                                            {
                                                add = entry.Trim();
                                            }
                                            else if (i == infoIndex)
                                            {
                                                info = entry.Trim();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        TableDataType tableDataType = ConvertDataType(dataType, null, out double? _, out double? _, out int? dataLength);
                        double? mulValue = ConvertFloatValue(mul);
                        double? divValue = ConvertFloatValue(div);
                        double? addValue = ConvertFloatValue(add);

                        bitFieldInfoList.Add(new SgFuncBitFieldInfo(resultName, unit, dataType, tableDataType,
                            mask, mulValue, divValue, addValue, dataLength, name, info));

                        dictIndex++;
                    }
                }

                foreach (SgFuncNameInfo funcNameInfo in bitFieldInfoList)
                {
                    if (funcNameInfo is SgFuncBitFieldInfo funcBitFieldInfo)
                    {
                        List<SgFuncNameInfo> nameInfoList = ReadSgFuncNameTable(funcBitFieldInfo.Name, funcBitFieldInfo.Unit);
                        funcBitFieldInfo.NameInfoList = nameInfoList;
                        funcBitFieldInfo.TableDataType = ConvertDataType(funcBitFieldInfo.DataType, nameInfoList, out double? _, out double? _, out int? dataLength);
                        funcBitFieldInfo.Length = dataLength;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return bitFieldInfoList;
        }

        public void ResetCache()
        {
            _sgFuncArgInfoDict.Clear();
            _sgFuncNameInfoDict.Clear();
        }

        public static int GetJobService(string jobName)
        {
            if (string.IsNullOrEmpty(jobName))
            {
                return -1;
            }

            foreach (Tuple<string, UdsServiceId> sgFuncJob in SgFuncJobListList)
            {
                if (string.Compare(jobName, sgFuncJob.Item1, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return (int)sgFuncJob.Item2;
                }
            }

            return -1;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    _ediabas = null;
                    ResetCache();
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }

        private double? ConvertFloatValue(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                double value = EdiabasNet.StringToFloat(text, out bool valid);
                if (valid)
                {
                    return value;
                }
            }

            return null;
        }

        private double? ScaleValue(double? edValue, double? mul, double? div, double? add)
        {
            // EBIABAS = (ECU*MUL)/DIV + ADD
            // ECU = DIV*(EBIABAS-ADD)/MUL
            try
            {
                if (!edValue.HasValue)
                {
                    return null;
                }

                double ecuValue = edValue.Value;
                if (add.HasValue)
                {
                    ecuValue -= add.Value;
                }

                if (div.HasValue)
                {
                    ecuValue *= div.Value;
                }

                if (mul.HasValue && mul.Value != 0)
                {
                    ecuValue /= mul.Value;
                }

                return ecuValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private TableDataType ConvertDataType(string text, List<SgFuncNameInfo> nameInfoList, out double? minValue, out double? maxValue, out int? length)
        {
            TableDataType dataType = TableDataType.Undefined;
            minValue = null;
            maxValue = null;
            length = null;

            if (!string.IsNullOrEmpty(text))
            {
                bool bHasLength = false;
                string compareText = text.Trim().ToLowerInvariant();
                if (compareText.Contains(DataTypeChar))
                {
                    dataType = TableDataType.Float;
                    length = 1;
                    if (compareText.Contains(DataTypeUnsigned))
                    {
                        minValue = byte.MinValue;
                        maxValue = byte.MaxValue;
                    }
                    else
                    {
                        minValue = sbyte.MinValue;
                        maxValue = sbyte.MaxValue;
                    }
                }
                else if (compareText.Contains(DataTypeInt))
                {
                    dataType = TableDataType.Float;
                    length = 2;
                    if (compareText.Contains(DataTypeUnsigned))
                    {
                        minValue = UInt16.MinValue;
                        maxValue = UInt16.MaxValue;
                    }
                    else
                    {
                        minValue = Int16.MinValue;
                        maxValue = Int16.MaxValue;
                    }
                }
                else if (compareText.Contains(DataTypeLong))
                {
                    dataType = TableDataType.Float;
                    length = 4;
                    if (compareText.Contains(DataTypeUnsigned))
                    {
                        minValue = UInt32.MinValue;
                        maxValue = UInt32.MaxValue;
                    }
                    else
                    {
                        minValue = Int32.MinValue;
                        maxValue = Int32.MaxValue;
                    }
                }
                else if (compareText.Contains(DataTypeFloat))
                {
                    dataType = TableDataType.Float;
                    length = 4;
                    minValue = float.MinValue;
                    maxValue = float.MaxValue;
                }
                else if (compareText.Contains(DataTypeDouble))
                {
                    dataType = TableDataType.Float;
                    length = 8;
                    minValue = double.MinValue;
                    maxValue = double.MaxValue;
                }
                else if (compareText.Contains(DataTypeString))
                {
                    dataType = TableDataType.String;
                    bHasLength = true;
                }
                else if (compareText.Contains(DataTypeData))
                {
                    dataType = TableDataType.Binary;
                    bHasLength = true;
                }
                else if (compareText.Contains(DataTypeBitField))
                {
                    dataType = TableDataType.Bit;
                    if (nameInfoList != null)
                    {
                        foreach (SgFuncNameInfo sgFuncNameInfo in nameInfoList)
                        {
                            if (sgFuncNameInfo is SgFuncBitFieldInfo sgFuncBitField)
                            {
                                if (sgFuncBitField.Length.HasValue)
                                {
                                    length = sgFuncBitField.Length;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (bHasLength)
                {
                    MatchCollection matches = Regex.Matches(compareText, @"\[(\d+)\]", RegexOptions.IgnoreCase);
                    if ((matches.Count == 1) && (matches[0].Groups.Count == 2))
                    {
                        if (Int32.TryParse(matches[0].Groups[1].Value, out int value))
                        {
                            length = value;
                        }
                    }
                }
            }

            return dataType;
        }
    }
}
