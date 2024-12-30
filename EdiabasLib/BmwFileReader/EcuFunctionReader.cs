using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ICSharpCode.SharpZipLib.Zip;
// ReSharper disable ConvertToUsingDeclaration

namespace BmwFileReader
{
    public class EcuFunctionReader
    {
        public const string EcuFuncFileName = "EcuFunctions.zip";
        public const string FaultDataBaseName = "faultdata_";
        public const string FaultDataTypeFault = "F";
        public const string FaultDataTypeInfo = "I";
        public delegate bool ProgressDelegate(long progress);

        private readonly string _rootDir;
        private readonly object _lockObject = new object();
        private readonly Dictionary<string, EcuFunctionStructs.EcuVariant> _ecuVariantDict;
        private readonly Dictionary<string, EcuFunctionStructs.EcuFaultCodeLabel> _ecuFaultCodeLabelDict;
        private readonly Dictionary<string, EcuFunctionStructs.EcuFaultModeLabel> _ecuFaultModeLabelDict;
        private readonly Dictionary<string, EcuFunctionStructs.EcuEnvCondLabel> _ecuEnvCondLabelDict;
        private EcuFunctionStructs.EcuFaultData _ecuFaultData;
        private string _ecuFaultDataLanguage;

        public string FaultDataId
        {
            get;
            private set;
        }

        private class StreamEventReader : StreamReader
        {
            public delegate bool ReadEventHandler(int bytesRead);

            private ReadEventHandler _readHandler;
            private bool _aborted = false;

            public bool Aborted => _aborted;

            public StreamEventReader(Stream stream, ReadEventHandler readHandler) : base(stream)
            {
                _readHandler = readHandler;
            }

            public override int Read()
            {
                int readValue = base.Read();
                if (readValue != -1)
                {
                    CallHandler(1);
                }
                return readValue;
            }

            public override int Read(char[] buffer, int index, int count)
            {
                int readCount = base.Read(buffer, index, count);
                CallHandler(readCount);
                return readCount;
            }

            public override int ReadBlock(char[] buffer, int index, int count)
            {
                int readCount = base.ReadBlock(buffer, index, count);
                CallHandler(readCount);
                return readCount;
            }

            public override string ReadLine()
            {
                string line = base.ReadLine();
                CallHandler(line?.Length ?? 0);
                return line;
            }

            private void CallHandler(int bytesRead)
            {
                if (_readHandler != null)
                {
                    if (_readHandler(bytesRead))
                    {
                        _aborted = true;
                    }
                }

                if (_aborted)
                {
                    throw new IOException("Read aborted");
                }
            }
        }

        public EcuFunctionReader(string rootDir)
        {
            _rootDir = rootDir;
            _ecuVariantDict = new Dictionary<string, EcuFunctionStructs.EcuVariant>();
            _ecuFaultCodeLabelDict = new Dictionary<string, EcuFunctionStructs.EcuFaultCodeLabel>();
            _ecuFaultModeLabelDict = new Dictionary<string, EcuFunctionStructs.EcuFaultModeLabel>();
            _ecuEnvCondLabelDict = new Dictionary<string, EcuFunctionStructs.EcuEnvCondLabel>();
        }

        public bool Init(string language, out string errorMessage, ProgressDelegate progressHandler)
        {
            errorMessage = null;
            EcuFunctionStructs.EcuFaultData ecuFaultData = GetEcuFaultDataCached(language, progressHandler);
            if (ecuFaultData == null)
            {
                return false;
            }

            try
            {
                StringBuilder sbDataId = new StringBuilder();

                string databaseVersion = ecuFaultData.DatabaseVersion;
                if (!string.IsNullOrEmpty(databaseVersion))
                {
                    sbDataId.Append(databaseVersion);
                }

                if (sbDataId.Length > 0)
                {
                    sbDataId.Append("_");
                }

                string faultDataDate = ecuFaultData.DatabaseDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                sbDataId.Append(faultDataDate);

                FaultDataId = sbDataId.ToString();

                DateTime rulesInfoDate = DateTime.Parse(RulesInfo.DatabaseDate, CultureInfo.InvariantCulture);
                VehicleStructsBmw.VersionInfo rulesVersionInfo = new VehicleStructsBmw.VersionInfo(RulesInfo.DatabaseVersion, rulesInfoDate);
                if (!rulesVersionInfo.IsMinVersion(ecuFaultData.DatabaseVersion, ecuFaultData.DatabaseDate))
                {
                    return false;
                }

                VehicleStructsBmw.VersionInfo seriesVersionInfo = VehicleInfoBmw.GetVehicleSeriesInfoVersion();
                if (seriesVersionInfo == null)
                {
                    return false;
                }

                if (!seriesVersionInfo.IsMinVersion(ecuFaultData.DatabaseVersion, ecuFaultData.DatabaseDate))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }

            return true;
        }

        public bool IsInitRequired(string language)
        {
            return _ecuFaultData == null || _ecuFaultDataLanguage == null ||
                    string.Compare(_ecuFaultDataLanguage, language, StringComparison.OrdinalIgnoreCase) != 0;
        }

        public void Reset()
        {
            _ecuFaultDataLanguage = null;
            _ecuFaultCodeLabelDict.Clear();
            _ecuFaultModeLabelDict.Clear();
            _ecuEnvCondLabelDict.Clear();
            _ecuVariantDict.Clear();
        }

        public List<Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>> GetFixedFuncStructList(EcuFunctionStructs.EcuVariant ecuVariant)
        {
            List<Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>> fixedFuncStructList =
                new List<Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>>();

            if (ecuVariant != null)
            {
                if (ecuVariant.RefEcuVariantList != null)
                {
                    foreach (EcuFunctionStructs.RefEcuVariant refEcuVariant in ecuVariant.RefEcuVariantList)
                    {
                        if (refEcuVariant.FixedFuncStructList != null)
                        {
                            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in refEcuVariant.FixedFuncStructList)
                            {
                                fixedFuncStructList.Add(new Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>(ecuFixedFuncStruct, null));
                            }
                        }
                    }
                }

                if (ecuVariant.EcuFuncStructList != null)
                {
                    foreach (EcuFunctionStructs.EcuFuncStruct ecuFuncStruct in ecuVariant.EcuFuncStructList)
                    {
                        if (ecuFuncStruct.FixedFuncStructList != null)
                        {
                            foreach (EcuFunctionStructs.EcuFixedFuncStruct ecuFixedFuncStruct in ecuFuncStruct.FixedFuncStructList)
                            {
                                fixedFuncStructList.Add(new Tuple<EcuFunctionStructs.EcuFixedFuncStruct, EcuFunctionStructs.EcuFuncStruct>(ecuFixedFuncStruct, ecuFuncStruct));
                            }
                        }
                    }
                }

            }

            return fixedFuncStructList;
        }

        public bool IsValidFaultCode(Int64 errorCode, bool info, EcuFunctionStructs.EcuVariant ecuVariant, RuleEvalBmw ruleEvalBmw, out bool isVisible)
        {
            isVisible = true;
            lock (_lockObject)
            {
                if (errorCode == 0x0000)
                {
                    return false;
                }

                if (!ecuVariant.GetEcuFaultCodeDict(info).TryGetValue(errorCode, out EcuFunctionStructs.EcuFaultCode ecuFaultCode))
                {
                    return false;
                }

                if (ecuFaultCode.Relevance.ConvertToInt() < 1)
                {
                    isVisible = false;
                }

                if (ruleEvalBmw != null)
                {
                    if (!ruleEvalBmw.EvaluateRule(ecuFaultCode.Id, RuleEvalBmw.RuleType.Fault))
                    {
                        isVisible = false;
                    }
                }

                return true;
            }
        }

        public EcuFunctionStructs.EcuFaultCodeLabel GetFaultCodeLabel(Int64 errorCode, bool info, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            lock (_lockObject)
            {
                if (!ecuVariant.GetEcuFaultCodeDict(info).TryGetValue(errorCode, out EcuFunctionStructs.EcuFaultCode ecuFaultCode))
                {
                    return null;
                }

                if (!_ecuFaultCodeLabelDict.TryGetValue(ecuFaultCode.EcuFaultCodeLabelId.ToLowerInvariant(), out EcuFunctionStructs.EcuFaultCodeLabel ecuFaultCodeLabel))
                {
                    return null;
                }

                return ecuFaultCodeLabel;
            }
        }

        public List<EcuFunctionStructs.EcuFaultModeLabel> GetFaultModeLabelMatchList(List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList, Int64 modeNumber)
        {
            // ReSharper disable once UseNullPropagation
            if (ecuFaultModeLabelList == null)
            {
                return null;
            }

            List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelMatchList =
                ecuFaultModeLabelList.Where(x => x.Code.ConvertToInt() == modeNumber).
                    OrderBy(x => x.Id.ConvertToInt()).ToList();

            return ecuFaultModeLabelMatchList;
        }

        public List<EcuFunctionStructs.EcuFaultModeLabel> GetFaultModeLabelList(Int64 errorCode, bool info, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            lock (_lockObject)
            {
                if (!ecuVariant.GetEcuFaultCodeDict(info).TryGetValue(errorCode, out EcuFunctionStructs.EcuFaultCode ecuFaultCode))
                {
                    return null;
                }

                if (ecuFaultCode.EcuFaultModeLabelIdList == null)
                {
                    return null;
                }

                List<EcuFunctionStructs.EcuFaultModeLabel> ecuFaultModeLabelList = new List<EcuFunctionStructs.EcuFaultModeLabel>();
                foreach (string ecuFaultModeId in ecuFaultCode.EcuFaultModeLabelIdList)
                {
                    if (_ecuFaultModeLabelDict.TryGetValue(ecuFaultModeId.ToLowerInvariant(), out EcuFunctionStructs.EcuFaultModeLabel ecuFaultModeLabel))
                    {
                        ecuFaultModeLabelList.Add(ecuFaultModeLabel);
                    }
                }

                return ecuFaultModeLabelList;
            }
        }

        public List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabelMatchList(List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList, Int64 envNumber)
        {
            // ReSharper disable once UseNullPropagation
            if (ecuEnvCondLabelList == null)
            {
                return null;
            }

            List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelMatchList =
                ecuEnvCondLabelList.Where(x => string.Compare(x.IdentStr, envNumber.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) == 0).
                    OrderBy(x => x.Id.ConvertToInt()).ToList();

            return ecuEnvCondLabelMatchList;
        }

        public List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabelMatchList(List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList, string envName)
        {
            // ReSharper disable once UseNullPropagation
            if (ecuEnvCondLabelList == null)
            {
                return null;
            }

            List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelMatchList =
                ecuEnvCondLabelList.Where(x => string.Compare(x.IdentStr, envName, StringComparison.OrdinalIgnoreCase) == 0).
                    OrderBy(x => x.Id.ConvertToInt()).ToList();

            return ecuEnvCondLabelMatchList;
        }

        public List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabelList(Int64 errorCode, bool info, EcuFunctionStructs.EcuVariant ecuVariant)
        {
            lock (_lockObject)
            {
                if (!ecuVariant.GetEcuFaultCodeDict(info).TryGetValue(errorCode, out EcuFunctionStructs.EcuFaultCode ecuFaultCode))
                {
                    return null;
                }

                if (ecuFaultCode.EcuEnvCondLabelIdList == null)
                {
                    return null;
                }

                List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = new List<EcuFunctionStructs.EcuEnvCondLabel>();
                foreach (string ecuEnvCondId in ecuFaultCode.EcuEnvCondLabelIdList)
                {
                    if (_ecuEnvCondLabelDict.TryGetValue(ecuEnvCondId.ToLowerInvariant(), out EcuFunctionStructs.EcuEnvCondLabel ecuEnvCondLabel))
                    {
                        ecuEnvCondLabelList.Add(ecuEnvCondLabel);
                    }
                }

                return ecuEnvCondLabelList;
            }
        }

        // for tesing result states only!
        public List<EcuFunctionStructs.EcuEnvCondLabel> GetEnvCondLabelListWithResultStates(EcuFunctionStructs.EcuVariant ecuVariant, bool info)
        {
            lock (_lockObject)
            {
                List<EcuFunctionStructs.EcuEnvCondLabel> ecuEnvCondLabelList = new List<EcuFunctionStructs.EcuEnvCondLabel>();
                foreach (KeyValuePair<Int64, EcuFunctionStructs.EcuFaultCode> ecuFaultCodePair in ecuVariant.GetEcuFaultCodeDict(info))
                {
                    if (ecuFaultCodePair.Value.EcuEnvCondLabelIdList != null)
                    {
                        foreach (string ecuEnvCondId in ecuFaultCodePair.Value.EcuEnvCondLabelIdList)
                        {
                            if (_ecuEnvCondLabelDict.TryGetValue(ecuEnvCondId.ToLowerInvariant(), out EcuFunctionStructs.EcuEnvCondLabel ecuEnvCondLabel))
                            {
                                if (ecuEnvCondLabel.EcuResultStateValueList != null && ecuEnvCondLabel.EcuResultStateValueList.Count > 0)
                                {
                                    ecuEnvCondLabelList.Add(ecuEnvCondLabel);
                                }
                            }
                        }
                    }
                }

                return ecuEnvCondLabelList;
            }
        }

        public EcuFunctionStructs.EcuVariant GetEcuVariantCached(string ecuName)
        {
            lock (_lockObject)
            {
                try
                {
                    if (string.IsNullOrEmpty(ecuName))
                    {
                        return null;
                    }

                    string key = ecuName.ToLowerInvariant();
                    if (_ecuVariantDict.TryGetValue(key, out EcuFunctionStructs.EcuVariant ecuVariant))
                    {
                        return ecuVariant;
                    }

                    ecuVariant = GetEcuVariant(ecuName);
                    _ecuVariantDict[key] = ecuVariant;

                    return ecuVariant;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public EcuFunctionStructs.EcuFaultData GetEcuFaultDataCached(string language, ProgressDelegate progressHandler)
        {
            lock (_lockObject)
            {
                if (IsInitRequired(language))
                {
                    Reset();
                    _ecuFaultData = GetEcuFaultData(language, progressHandler);
                    if (_ecuFaultData != null)
                    {
                        _ecuFaultDataLanguage = language;
                        if (_ecuFaultData.EcuFaultCodeLabelList != null)
                        {
                            foreach (EcuFunctionStructs.EcuFaultCodeLabel ecuFaultCodeLabel in _ecuFaultData.EcuFaultCodeLabelList)
                            {
                                string key = ecuFaultCodeLabel.Id.ToLowerInvariant();
                                _ecuFaultCodeLabelDict.TryAdd(key, ecuFaultCodeLabel);
                            }
                        }

                        if (_ecuFaultData.EcuFaultModeLabelList != null)
                        {
                            foreach (EcuFunctionStructs.EcuFaultModeLabel ecuFaultModeLabel in _ecuFaultData.EcuFaultModeLabelList)
                            {
                                string key = ecuFaultModeLabel.Id.ToLowerInvariant();
                                _ecuFaultModeLabelDict.TryAdd(key, ecuFaultModeLabel);
                            }
                        }

                        if (_ecuFaultData.EcuEnvCondLabelList != null)
                        {
                            foreach (EcuFunctionStructs.EcuEnvCondLabel ecuEnvCondLabel in _ecuFaultData.EcuEnvCondLabelList)
                            {
                                string key = ecuEnvCondLabel.Id.ToLowerInvariant();
                                _ecuEnvCondLabelDict.TryAdd(key, ecuEnvCondLabel);
                            }
                        }
                    }
                }

                return _ecuFaultData;
            }
        }

        protected EcuFunctionStructs.EcuVariant GetEcuVariant(string ecuName)
        {
            EcuFunctionStructs.EcuVariant ecuVariant = GetEcuDataObject(ecuName, typeof(EcuFunctionStructs.EcuVariant)) as EcuFunctionStructs.EcuVariant;
            if (ecuVariant != null)
            {
                ecuVariant.EcuName = ecuName;
                if (ecuVariant.EcuFaultCodeList != null)
                {
                    Dictionary<Int64, EcuFunctionStructs.EcuFaultCode> ecuFaultCodeDictFault = new Dictionary<Int64, EcuFunctionStructs.EcuFaultCode>();
                    Dictionary<Int64, EcuFunctionStructs.EcuFaultCode> ecuFaultCodeDictInfo = new Dictionary<Int64, EcuFunctionStructs.EcuFaultCode>();
                    foreach (EcuFunctionStructs.EcuFaultCode ecuFaultCode in ecuVariant.EcuFaultCodeList)
                    {
                        Int64 errorCode = ecuFaultCode.Code.ConvertToInt();
                        if (errorCode != 0 && !string.IsNullOrEmpty(ecuFaultCode.DataType))
                        {
                            if (string.Compare(ecuFaultCode.DataType, FaultDataTypeFault, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                ecuFaultCodeDictFault.TryAdd(errorCode, ecuFaultCode);
                            }
                            else if (string.Compare(ecuFaultCode.DataType, FaultDataTypeInfo, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                ecuFaultCodeDictInfo.TryAdd(errorCode, ecuFaultCode);
                            }
                        }
                    }

                    ecuVariant.EcuFaultCodeDictFault = ecuFaultCodeDictFault;
                    ecuVariant.EcuFaultCodeDictInfo = ecuFaultCodeDictInfo;
                }
            }
            return ecuVariant;
        }

        protected EcuFunctionStructs.EcuFaultData GetEcuFaultData(string language, ProgressDelegate progressHandler)
        {
            string fileName = FaultDataBaseName + language.ToLowerInvariant();
            EcuFunctionStructs.EcuFaultData ecuFaultData = GetEcuDataObject(fileName, typeof(EcuFunctionStructs.EcuFaultData), progressHandler) as EcuFunctionStructs.EcuFaultData;
            if (ecuFaultData == null)
            {
                fileName = FaultDataBaseName + "en";
                ecuFaultData = GetEcuDataObject(fileName, typeof(EcuFunctionStructs.EcuFaultData), progressHandler) as EcuFunctionStructs.EcuFaultData;
            }
            return ecuFaultData;
        }

        protected object GetEcuDataObject(string name, Type type, ProgressDelegate progressHandler = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }

                object ecuObject = null;
                ZipFile zf = null;
                try
                {
                    string fileName = name.ToLowerInvariant() + ".xml";
                    FileStream fs = File.OpenRead(Path.Combine(_rootDir, EcuFuncFileName));
                    zf = new ZipFile(fs);
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            continue; // Ignore directories
                        }
                        if (string.Compare(zipEntry.Name, fileName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            using (Stream zipStream = zf.GetInputStream(zipEntry))
                            {
                                long maxLength = zipEntry.Size;
                                long readPos = 0;
                                using (StreamEventReader reader = new StreamEventReader(zipStream, read =>
                                       {
                                           if (progressHandler == null)
                                           {
                                               return false;
                                           }

                                           readPos += read;
                                           return progressHandler(readPos * 100 / maxLength);
                                       }))
                                {
                                    XmlSerializer serializer = new XmlSerializer(type);
                                    ecuObject = serializer.Deserialize(reader);
                                    if (reader.Aborted)
                                    {
                                        return null;
                                    }
                                }
                            }
                            break;
                        }
                    }

                    return ecuObject;
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
            catch (Exception)
            {
                return null;
            }
        }

    }
}
