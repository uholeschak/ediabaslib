using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BmwFileReader;
using HarmonyLib;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Data.Sqlite;
using PsdzClient.Core;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace PsdzClient
{
    public partial class PsdzDatabase
    {
        [XmlInclude(typeof(TestModuleData))]
        [XmlType("TestModules")]
        public class TestModules
        {
            public TestModules() : this(null, null)
            {
            }

            public TestModules(VehicleStructsBmw.VersionInfo versionInfo, SerializableDictionary<string, TestModuleData> moduleDataDict, int convertFailures = 0)
            {
                Version = versionInfo;
                ConvertFailures = convertFailures;
                ModuleDataDict = moduleDataDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }

            [XmlElement("ConvertFailures")] public int ConvertFailures { get; set; }

            [XmlElement("ModuleDataDict"), DefaultValue(null)] public SerializableDictionary<string, TestModuleData> ModuleDataDict { get; set; }
        }

        [XmlType("TestModuleData")]
        public class TestModuleData
        {
            public TestModuleData() : this(null, null)
            {
            }

            public TestModuleData(SerializableDictionary<string, List<string>> refDict, string moduleRef)
            {
                RefDict = refDict;
                ModuleRef = moduleRef;
            }

            [XmlElement("RefDict"), DefaultValue(null)] public SerializableDictionary<string, List<string>> RefDict { get; set; }

            [XmlElement("ModuleRef"), DefaultValue(null)] public string ModuleRef { get; set; }
        }

        [XmlInclude(typeof(ServiceModuleData))]
        [XmlInclude(typeof(ServiceModuleTextData))]
        [XmlType("ServiceModules")]
        public class ServiceModules
        {
            public ServiceModules() : this(null)
            {
            }

            public ServiceModules(VehicleStructsBmw.VersionInfo versionInfo, SerializableDictionary<string, ServiceModuleData> moduleDataDict = null,
                SerializableDictionary<string, ServiceModuleTextData> moduleTextDict = null, bool completed = false, int lastProgress = 0, int convertFailures = 0)
            {
                Version = versionInfo;
                Completed = completed;
                LastProgress = lastProgress;
                ConvertFailures = convertFailures;
                ModuleDataDict = moduleDataDict;
                ModuleTextDict = moduleTextDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }

            [XmlElement("Completed")] public bool Completed { get; set; }

            [XmlElement("LastProgress")] public int LastProgress { get; set; }

            [XmlElement("ConvertFailures")] public int ConvertFailures { get; set; }

            [XmlElement("ModuleDataDict"), DefaultValue(null)] public SerializableDictionary<string, ServiceModuleData> ModuleDataDict { get; set; }

            [XmlElement("ModuleTextDict"), DefaultValue(null)] public SerializableDictionary<string, ServiceModuleTextData> ModuleTextDict { get; set; }
        }

        [XmlInclude(typeof(ServiceModuleDataItem))]
        [XmlType("ServiceModuleData")]
        public class ServiceModuleData
        {
            public ServiceModuleData() : this(null, null, null)
            {
            }

            public ServiceModuleData(string infoObjId, List<string> diagObjIds, SerializableDictionary<string, ServiceModuleDataItem> dataDict)
            {
                InfoObjId = infoObjId;
                DiagObjIds = diagObjIds;
                DataDict = dataDict;
            }

            [XmlElement("InfoObjId"), DefaultValue(null)] public string InfoObjId { get; set; }

            [XmlElement("DiagObjIds"), DefaultValue(null)] public List<string> DiagObjIds { get; set; }

            [XmlElement("DataDict"), DefaultValue(null)] public SerializableDictionary<string, ServiceModuleDataItem> DataDict { get; set; }
        }

        [XmlType("ServiceModuleResultItem")]
        public class ServiceModuleResultItem : IEquatable<ServiceModuleResultItem>
        {
            public ServiceModuleResultItem() : this(null, null, null)
            {
            }

            public ServiceModuleResultItem(string dataName, string data, string dataType)
            {
                DataName = dataName;
                Data = data;
                DataType = dataType;
                hashCode = new Random().Next();
            }

            [XmlElement("DataName"), DefaultValue(null)] public string DataName { get; set; }

            [XmlElement("Data"), DefaultValue(null)] public string Data { get; set; }

            [XmlElement("DataType"), DefaultValue(null)] public string DataType { get; set; }

            private readonly int hashCode;

            public bool Equals(ServiceModuleResultItem other)
            {
                try
                {
                    if (other == null)
                    {
                        return false;
                    }

                    if (ReferenceEquals(this, other))
                    {
                        return true;
                    }

                    if (DataName != other.DataName)
                    {
                        return false;
                    }

                    if (Data != other.Data)
                    {
                        return false;
                    }

                    if (Data != other.DataType)
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ServiceModuleResultItem);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }

            public static bool operator ==(ServiceModuleResultItem item1, ServiceModuleResultItem item2)
            {
                if (ReferenceEquals(item1, item2))
                {
                    return true;
                }

                if (ReferenceEquals(item1, null))
                {
                    return false;
                }

                if (ReferenceEquals(item2, null))
                {
                    return false;
                }

                return item1.Equals(item2);
            }

            public static bool operator !=(ServiceModuleResultItem item1, ServiceModuleResultItem item2)
            {
                return !(item1 == item2);
            }
        }

        [XmlInclude(typeof(ServiceModuleResultItem))]
        [XmlType("ServiceModuleInvokeItem")]
        public class ServiceModuleInvokeItem : IEquatable<ServiceModuleInvokeItem>
        {
            public ServiceModuleInvokeItem() : this(null, null, null, null, null, null)
            {
            }

            public ServiceModuleInvokeItem(string method, object inParam, object outParam, object inoutParam, object dscResult, SerializableDictionary<string, string> textIds)
            {
                Method = method;
                ResultItems = new List<ServiceModuleResultItem>();
                TextHashes = null;
                OutParamValues = new SerializableDictionary<string, string>();
                TextIds = textIds;
                InParam = inParam;
                OutParam = outParam;
                InoutParam = inoutParam;
                DscResult = dscResult;
                hashCode = new Random().Next();
            }

            [XmlElement("Method"), DefaultValue(null)] public string Method { get; set; }

            [XmlElement("ResultItems"), DefaultValue(null)] public List<ServiceModuleResultItem> ResultItems { get; set; }

            [XmlElement("TextHashes"), DefaultValue(null)] public List<string> TextHashes { get; set; }

            [XmlElement("OutParamValues"), DefaultValue(null)] public SerializableDictionary<string, string> OutParamValues { get; set; }

            [XmlIgnore, DefaultValue(null)] public SerializableDictionary<string, string> TextIds { get; set; }

            [XmlIgnore, DefaultValue(null)] public object InParam { get; set; }

            [XmlIgnore, DefaultValue(null)] public object OutParam { get; set; }

            [XmlIgnore, DefaultValue(null)] public object InoutParam { get; set; }

            [XmlIgnore, DefaultValue(null)] public object DscResult { get; set; }

            private readonly int hashCode;

            public bool Equals(ServiceModuleInvokeItem other)
            {
                try
                {
                    if (other == null)
                    {
                        return false;
                    }

                    if (ReferenceEquals(this, other))
                    {
                        return true;
                    }

                    if (Method != other.Method)
                    {
                        return false;
                    }

                    if (ResultItems != null && other.ResultItems != null)
                    {
                        if (ResultItems.Count != other.ResultItems.Count)
                        {
                            return false;
                        }

                        for (int i = 0; i < ResultItems.Count; i++)
                        {
                            if (ResultItems[i] != other.ResultItems[i])
                            {
                                return false;
                            }
                        }
                    }

                    if (TextHashes != null && other.TextHashes != null)
                    {
                        if (TextHashes.Count != other.TextHashes.Count)
                        {
                            return false;
                        }

                        for (int i = 0; i < TextHashes.Count; i++)
                        {
                            if (TextHashes[i] != other.TextHashes[i])
                            {
                                return false;
                            }
                        }
                    }

                    if (OutParamValues != null && other.OutParamValues != null)
                    {
                        if (OutParamValues.Count != other.OutParamValues.Count)
                        {
                            return false;
                        }

                        if (!OutParamValues.Keys.All(other.OutParamValues.ContainsKey))
                        {
                            return false;
                        }
                    }

                    if (TextIds != null && other.TextIds != null)
                    {
                        if (TextIds.Count != other.TextIds.Count)
                        {
                            return false;
                        }

                        if (TextIds.Count > 0)
                        {
                            if (!TextIds.Keys.All(other.TextIds.ContainsKey))
                            {
                                return false;
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

            public override bool Equals(object obj)
            {
                return Equals(obj as ServiceModuleInvokeItem);
            }

            public override int GetHashCode()
            {
                return hashCode;
            }

            public static bool operator ==(ServiceModuleInvokeItem item1, ServiceModuleInvokeItem item2)
            {
                if (ReferenceEquals(item1, item2))
                {
                    return true;
                }

                if (ReferenceEquals(item1, null))
                {
                    return false;
                }

                if (ReferenceEquals(item2, null))
                {
                    return false;
                }

                return item1.Equals(item2);
            }

            public static bool operator !=(ServiceModuleInvokeItem item1, ServiceModuleInvokeItem item2)
            {
                return !(item1 == item2);
            }

            public void CleanupInternal()
            {
                InParam = null;
                OutParam = null;
                InoutParam = null;
                DscResult = null;
                TextIds.Clear();
            }
        }

        [XmlInclude(typeof(ServiceModuleInvokeItem))]
        [XmlType("ServiceModuleDataItem")]
        public class ServiceModuleDataItem
        {
            public ServiceModuleDataItem() : this(null, null, null, null, null, null, null)
            {
            }

            public ServiceModuleDataItem(string methodName, string elementNo, string controlId, string serviceDialogName, object inParams, object inoutParams, string containerXml, SerializableDictionary<string, string> runOverrides = null)
            {
                MethodName = methodName;
                ElementNo = elementNo;
                ControlId = controlId;
                ServiceDialogName = serviceDialogName;
                RunOverrides = runOverrides;
                EdiabasJobBare = null;
                EdiabasJobOverride = null;
                InvokeItems = new List<ServiceModuleInvokeItem>();
                OutParamValues = new SerializableDictionary<string, string>();
                CallsCount = 1;
                InParams = inParams;
                InoutParams = inoutParams;
                ContainerXml = containerXml;
                ServiceDialogs = new HashSet<object>();
                DialogStateDict = new SerializableDictionary<string, int>();
            }

            [XmlElement("MethodName"), DefaultValue(null)] public string MethodName { get; set; }

            [XmlElement("ElementNo"), DefaultValue(null)] public string ElementNo { get; set; }

            [XmlElement("ControlId"), DefaultValue(null)] public string ControlId { get; set; }

            [XmlElement("ServiceDialogName"), DefaultValue(null)] public string ServiceDialogName { get; set; }

            [XmlElement("RunOverrides"), DefaultValue(null)] public SerializableDictionary<string, string> RunOverrides { get; set; }

            [XmlElement("EdiabasJobBare"), DefaultValue(null)] public string EdiabasJobBare { get; set; }

            [XmlElement("EdiabasJobOverride"), DefaultValue(null)] public string EdiabasJobOverride { get; set; }

            [XmlElement("InvokeItems"), DefaultValue(null)] public List<ServiceModuleInvokeItem> InvokeItems { get; set; }

            [XmlElement("OutParamValues"), DefaultValue(null)] public SerializableDictionary<string, string> OutParamValues { get; set; }

            [XmlElement("CallsCount"), DefaultValue(null)] public int CallsCount { get; set; }

            [XmlIgnore, DefaultValue(null)] public object InParams { get; set; }

            [XmlIgnore, DefaultValue(null)] public object InoutParams { get; set; }

            [XmlIgnore, DefaultValue(null)] public string ContainerXml { get; set; }

            [XmlIgnore, DefaultValue(null)] public HashSet<object> ServiceDialogs { get; set; }

            [XmlIgnore, DefaultValue(null)] public SerializableDictionary<string, int> DialogStateDict { get; set; }

            public void CleanupInternal()
            {
                InParams = null;
                InoutParams = null;
                ContainerXml = null;
                ServiceDialogs.Clear();
                DialogStateDict.Clear();

                foreach (ServiceModuleInvokeItem invokeItem in InvokeItems)
                {
                    invokeItem.CleanupInternal();
                }
            }

            public void RemoveDuplicates()
            {
                int itemsCountOld = InvokeItems.Count;
                int itemIndex = 0;
                while (itemIndex < InvokeItems.Count)
                {
                    ServiceModuleInvokeItem invokeItem = InvokeItems[itemIndex];
                    bool duplicate = false;
                    for (int i = 0; i < itemIndex; i++)
                    {
                        if (invokeItem == InvokeItems[i])
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (duplicate)
                    {
                        InvokeItems.RemoveAt(itemIndex);
                    }
                    else
                    {
                        itemIndex++;
                    }
                }

                if (itemsCountOld != InvokeItems.Count)
                {
                    log.InfoFormat("RemoveDuplicates OldItems: {0}, NewItems: {1}", itemsCountOld, InvokeItems.Count);
                }
            }
        }

        public class ServiceModuleTextData
        {
            public ServiceModuleTextData() : this(null)
            {
            }

            public ServiceModuleTextData(EcuFunctionStructs.EcuTranslation translation)
            {
                Translation = translation;
                Hash = CalculateHash();
            }

            [XmlElement("Translation"), DefaultValue(null)] public EcuFunctionStructs.EcuTranslation Translation { get; set; }

            [XmlIgnore, DefaultValue(null)] public string Hash { get; set; }

            public string CalculateHash()
            {
                if (Translation == null)
                {
                    return string.Empty;
                }

                return Translation.PropertyList().MD5Hash();
            }
        }

        [XmlInclude(typeof(VehicleStructsBmw.VersionInfo))]
        [XmlType("EcuCharacteristicsXml")]
        public class EcuCharacteristicsData
        {
            public EcuCharacteristicsData() : this(null, null)
            {
            }

            public EcuCharacteristicsData(VehicleStructsBmw.VersionInfo versionInfo, SerializableDictionary<string, string> ecuXmlDict)
            {
                Version = versionInfo;
                EcuXmlDict = ecuXmlDict;
            }

            [XmlElement("Version"), DefaultValue(null)] public VehicleStructsBmw.VersionInfo Version { get; set; }
            [XmlElement("EcuXmlDict"), DefaultValue(null)] public SerializableDictionary<string, string> EcuXmlDict { get; set; }
        }

        private class EcuCharacteristicsMatch
        {
            public EcuCharacteristicsMatch(BaseEcuCharacteristics ecuCharacteristics, RuleDate ruleDate, List<VehicleStructsBmw.VehicleEcuInfo> ruleEcus, string ruleFormula)
            {
                EcuCharacteristics = ecuCharacteristics;
                RuleDate = ruleDate;
                RuleEcus = ruleEcus;
                RuleFormula = ruleFormula;
            }

            public BaseEcuCharacteristics EcuCharacteristics { get; set; }
            public RuleDate RuleDate { get; set; }
            public string Series { get; set; }
            public List<VehicleStructsBmw.VehicleEcuInfo> RuleEcus { get; set; }
            public string RuleFormula { get; set; }
        }

        private class EcuCharacteristicsInfo
        {
            public EcuCharacteristicsInfo(BaseEcuCharacteristics ecuCharacteristics, string series, string modelSeries, BNType? bnType, string brand, string sgbdAdd, RuleDate ruleDate, List<VehicleStructsBmw.VehicleEcuInfo> ruleEcus, string ruleFormula)
            {
                EcuCharacteristics = ecuCharacteristics;
                Series = series;
                ModelSeries = modelSeries;
                BnType = bnType;
                Brand = brand;
                SgbdAdd = sgbdAdd;
                RuleDate = ruleDate;
                RuleEcus = ruleEcus;
                RuleFormula = ruleFormula;
            }

            public BaseEcuCharacteristics EcuCharacteristics { get; set; }
            public string Series { get; set; }
            public string ModelSeries { get; set; }
            public BNType? BnType { get; set; }
            public string Brand { get; set; }
            public string SgbdAdd { get; set; }
            public RuleDate RuleDate { get; set; }
            public List<VehicleStructsBmw.VehicleEcuInfo> RuleEcus { get; set; }
            public string RuleFormula { get; set; }
        }

        private class RuleDate
        {
            public RuleDate(string date, string dateCompare)
            {
                Date = date;
                DateCompare = dateCompare;
            }

            public string Date { get; set; }

            public string DateCompare { get; set; }

            public long GetValue(long yearOffset = 0)
            {
                if (string.IsNullOrEmpty(Date))
                {
                    return 0;
                }

                if (Date.Length != 6)
                {
                    return 0;
                }

                long dateValue = Date.ConvertToInt();
                if (dateValue == 0)
                {
                    return 0;
                }

                return dateValue + (yearOffset * 100);
            }

            public string GetYear(long yearOffset = 0)
            {
                long dateValue = GetValue(yearOffset);
                if (dateValue == 0)
                {
                    return string.Empty;
                }

                return string.Format(CultureInfo.InvariantCulture, "{0:0000}", dateValue / 100);
            }

            public string GetMonth(long yearOffset = 0)
            {
                long dateValue = GetValue(yearOffset);
                if (dateValue == 0)
                {
                    return string.Empty;
                }

                return string.Format(CultureInfo.InvariantCulture, "{0:00}", dateValue % 100);
            }

            public override string ToString()
            {
                return GetYear() + GetMonth();
            }
        }

        public class MethodAbortedException : Exception
        {
            public MethodAbortedException(string method, string message) : base(method + ": " + message)
            {
            }
        }

        private const int MaxCallsLimit = 10;
        private const int MaxCallsStorage = 2;
        private const int MaxResultDataLen = 20;

        public delegate bool ProgressDelegate(bool startConvert, int progress = -1, int failures = -1);

        private static string _moduleRefPath;
        private static SerializableDictionary<string, List<string>> _moduleRefDict;
        private static SerializableDictionary<string, ServiceModuleDataItem> _serviceDialogDict;
        private static object _moduleThreadLock = new object();
        private static object _defaultObject = new object();
        private static SerializableDictionary<string, int> _serviceDialogInvokeCallsDict;
        private static SerializableDictionary<string, int> _characteristicCallsDict;
        private static HashSet<string> _serviceDialogTextHashes;
        private static ConstructorInfo _istaServiceDialogDlgCmdBaseConstructor;
        private static ConstructorInfo _istaEdiabasAdapterDeviceResultConstructor;
        private static ConstructorInfo _vehicleEcuResultConstructor;
        private static Type _istaServiceDialogFactoryType;
        private static Type _istaServiceDialogConfigurationType;
        private static Type _coreContractsDocumentLocatorType;
        private static Type _ecuResultType;

        private static void CheckThreadInterrupted()
        {
            try
            {
                // Thread.Sleep throws ThreadInterruptedException when the thread is interrupted
                Thread.Sleep(0);
            }
            catch (ThreadInterruptedException)
            {
                log.ErrorFormat("CheckThreadInterrupted");
                throw new MethodAbortedException("CheckThreadInterrupted", "Thread interrupted");
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CallModuleRefPrefix(string refPath, object inParameters, ref object outParameters, ref object inAndOutParameters)
        {
            log.InfoFormat("CallModuleRefPrefix refPath: {0}", refPath);
            _moduleRefPath = refPath;
            if (inParameters != null)
            {
                try
                {
                    PropertyInfo propertyParameter = inParameters.GetType().GetProperty("Parameter");
                    if (propertyParameter == null)
                    {
                        log.ErrorFormat("CallModuleRefPrefix Parameter not found");
                    }
                    else
                    {
                        object parameters = propertyParameter.GetValue(inParameters);
                        Dictionary<string, object> paramDictionary = parameters as Dictionary<string, object>;
                        if (paramDictionary == null)
                        {
                            log.ErrorFormat("CallModuleRefPrefix Parameter Dict not found");
                        }
                        else
                        {
                            log.InfoFormat("CallModuleRefPrefix Parameter Dict items: {0}", paramDictionary.Count);
                            lock (_moduleThreadLock)
                            {
                                _moduleRefDict = new SerializableDictionary<string, List<string>>();
                                foreach (KeyValuePair<string, object> keyValuePair in paramDictionary)
                                {
                                    if (keyValuePair.Value is List<string> elements)
                                    {
                                        _moduleRefDict.Add(keyValuePair.Key, elements);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CallModuleRefPrefix Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CallWriteFaPrefix()
        {
            CheckThreadInterrupted();
            log.InfoFormat("CallWriteFaPrefix");
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CallGetDatabaseProviderSQLitePrefix(ref object __result)
        {
            CheckThreadInterrupted();
            log.InfoFormat("CallGetDatabaseProviderSQLitePrefix");
            __result = null;
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CreateServiceDialogPrefix(ref object __result, object callingModule, string methodName, string path, object globalTabModuleISTA, int elementNo, object inParameters, ref object inoutParameters)
        {
            CheckThreadInterrupted();
            log.InfoFormat("CreateServiceDialogPrefix, Method: {0}, Path: {1}, Element: {2}", methodName, path, elementNo);

            string dialogRef = null;
            if (_istaServiceDialogFactoryType != null)
            {
                try
                {
                    MethodInfo methodResolveDialogRef = _istaServiceDialogFactoryType.GetMethod("ResolveDialogRef", BindingFlags.Public | BindingFlags.Static);
                    if (methodResolveDialogRef == null)
                    {
                        log.ErrorFormat("CreateServiceDialogPrefix ResolveDialogRef not found");
                    }
                    else
                    {
                        dialogRef = methodResolveDialogRef.Invoke(null, new object[] { path, true }) as string;
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix ResolveDialogRef Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }

            log.InfoFormat("CreateServiceDialogPrefix, DialogRef: '{0}'", dialogRef ?? string.Empty);

            string controlIdString = null;
            string serviceDialogConfigName = null;
            if (_istaServiceDialogConfigurationType != null && !string.IsNullOrEmpty(dialogRef))
            {
                try
                {
                    MethodInfo methodGetRegisteredConfiguration = _istaServiceDialogConfigurationType.GetMethod("GetRegisteredConfiguration", BindingFlags.Public | BindingFlags.Static);
                    if (methodGetRegisteredConfiguration == null)
                    {
                        log.ErrorFormat("CreateServiceDialogPrefix GetRegisteredConfiguration not found");
                    }
                    else
                    {
                        dynamic serviceDialogConfiguration = methodGetRegisteredConfiguration.Invoke(null, new object[] { dialogRef });
                        if (serviceDialogConfiguration == null)
                        {
                            log.ErrorFormat("CreateServiceDialogPrefix ServiceDialogConfiguration not found");
                        }
                        else
                        {
                            decimal controlId = serviceDialogConfiguration.ControlId;
                            controlIdString = controlId.ToString(CultureInfo.InvariantCulture);
                            serviceDialogConfigName = serviceDialogConfiguration.Name;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix ResolveDialogRef Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }

            log.InfoFormat("CreateServiceDialogPrefix, ControlId: {0}, ServiceDialogName: {1}",
                controlIdString ?? string.Empty, serviceDialogConfigName ?? string.Empty);

            string configurationContainerXml = string.Empty;
            SerializableDictionary<string, string> runOverridesDict = new SerializableDictionary<string, string>();
            dynamic inParametersDyn = inParameters;
            if (inParametersDyn != null)
            {
                try
                {
                    dynamic dscConfig = inParametersDyn.getParameter("/WurzelIn/DSCConfig", _defaultObject);
                    if (dscConfig != null && dscConfig != _defaultObject)
                    {
                        dynamic paramOverrides = dscConfig.ParametrizationOverrides;
                        if (paramOverrides != null)
                        {
                            object containerXml = paramOverrides.getParameter(ConfigurationContainerXMLPar, _defaultObject);
                            if (containerXml != null && containerXml != _defaultObject)
                            {
                                configurationContainerXml = containerXml as string;
                            }
                        }

                        dynamic runOverrides = dscConfig.RunOverrides;
                        if (runOverrides != null)
                        {
                            object runParameter = runOverrides.Parameter;
                            if (runParameter is Dictionary<string, object> paramDictionary)
                            {
                                foreach (KeyValuePair<string, object> keyValuePair in paramDictionary)
                                {
                                    if (keyValuePair.Value is string valueString)
                                    {
                                        log.InfoFormat("CreateServiceDialogPrefix, Override: '{0}', '{1}'", keyValuePair.Key, valueString);
                                        runOverridesDict.Add(keyValuePair.Key, valueString);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        log.ErrorFormat("CreateServiceDialogPrefix No DSCConfig");
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix DSCConfig Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }

            string elementNoString = elementNo.ToString(CultureInfo.InvariantCulture);
            string key = methodName + ";" + path + ";" + elementNoString;

            object serviceDialog = null;
            if (_istaServiceDialogDlgCmdBaseConstructor != null)
            {
                object[] args = new object[] { callingModule, methodName, path, globalTabModuleISTA, elementNo };
                try
                {
                    serviceDialog = _istaServiceDialogDlgCmdBaseConstructor.Invoke(args);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }
            else
            {
                log.ErrorFormat("CreateServiceDialogPrefix No service dialog construtor");
            }

            int callsCount;
            lock (_moduleThreadLock)
            {
                if (_serviceDialogDict == null)
                {
                    _serviceDialogDict = new SerializableDictionary<string, ServiceModuleDataItem>();
                }

                if (string.IsNullOrWhiteSpace(configurationContainerXml))
                {
                    log.InfoFormat("CreateServiceDialogPrefix No container XML");
                }

                _serviceDialogDict.TryGetValue(key, out ServiceModuleDataItem serviceModuleDataItem);
                if (serviceModuleDataItem == null)
                {
                    log.InfoFormat("CreateServiceDialogPrefix Adding Key: {0}", key);
                    serviceModuleDataItem = new ServiceModuleDataItem(methodName, elementNoString, controlIdString, serviceDialogConfigName, inParameters, inoutParameters, configurationContainerXml);
                    if (runOverridesDict.Count > 0)
                    {
                        serviceModuleDataItem.RunOverrides = runOverridesDict;
                    }
                    _serviceDialogDict.Add(key, serviceModuleDataItem);
                    //log.Info(configurationContainerXml);
                }
                else
                {
                    log.InfoFormat("CreateServiceDialogPrefix Key present: {0}", key);
                    serviceModuleDataItem.CallsCount++;
                }

                serviceModuleDataItem.ServiceDialogs.Add(serviceDialog);
                callsCount = serviceModuleDataItem.CallsCount;
            }

            log.InfoFormat("CreateServiceDialogPrefix Calls: {0}", callsCount);
            if (callsCount > MaxCallsLimit)
            {
                string callStack = string.Empty;
                try
                {
                    StackTrace stackTrace = new StackTrace();
                    callStack = stackTrace.ToString();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CreateServiceDialogPrefix StackTrace Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                }

                log.ErrorFormat("CreateServiceDialogPrefix Aborting Method: {0}", methodName);
                if (!string.IsNullOrEmpty(callStack))
                {
                    log.Error(callStack);
                }

                throw new MethodAbortedException("CreateServiceDialogPrefix", "Max calls limit reached");
            }

            __result = serviceDialog;
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool ServiceDialogCmdBaseInvokePrefix(object __instance, string method, object inParam, ref object outParam, ref object inoutParam)
        {
            CheckThreadInterrupted();
            log.InfoFormat("ServiceDialogCmdBaseInvokePrefix, Method: {0}", method);

            string stateKey = method ?? string.Empty;
            ServiceModuleDataItem serviceModuleDataItem = null;
            int invokeCalls = 1;
            lock (_moduleThreadLock)
            {
                if (__instance != null && _serviceDialogDict != null)
                {
                    foreach (KeyValuePair<string, ServiceModuleDataItem> serviceKeyValuePair in _serviceDialogDict)
                    {
                        if (serviceKeyValuePair.Value.ServiceDialogs.Contains(__instance))
                        {
                            serviceModuleDataItem = serviceKeyValuePair.Value;
                            break;
                        }
                    }
                }

                if (serviceModuleDataItem == null)
                {
                    if (_serviceDialogInvokeCallsDict == null)
                    {
                        _serviceDialogInvokeCallsDict = new SerializableDictionary<string, int>();
                    }

                    if (!_serviceDialogInvokeCallsDict.ContainsKey(stateKey))
                    {
                        _serviceDialogInvokeCallsDict.Add(stateKey, invokeCalls);
                    }
                    else
                    {
                        invokeCalls = _serviceDialogInvokeCallsDict[stateKey];
                        invokeCalls++;
                        _serviceDialogInvokeCallsDict[stateKey] = invokeCalls;
                    }
                }
            }

            string dialogName = string.Empty;
            string dialogMethodName = string.Empty;
            int dialogState = 0;
            if (serviceModuleDataItem == null)
            {
                log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix, Service module data not found, Invokes: {0}", invokeCalls);
            }
            else
            {
                dialogName = serviceModuleDataItem.ServiceDialogName ?? string.Empty;
                dialogMethodName = serviceModuleDataItem.MethodName ?? string.Empty;
                log.InfoFormat("ServiceDialogCmdBaseInvokePrefix, DialogName: {0}, DialogMethodName: {1}", dialogName, dialogMethodName);

                if (!serviceModuleDataItem.DialogStateDict.TryGetValue(stateKey, out dialogState))
                {
                    serviceModuleDataItem.DialogStateDict[stateKey] = dialogState;
                }
            }

            int dialogStateOld = dialogState;
            SerializableDictionary<string, string> textIds = new SerializableDictionary<string, string>();
            dynamic inParamDyn = inParam;
            if (inParamDyn != null)
            {
                try
                {
                    dynamic txtParam = inParamDyn.getParameter("txtParam", _defaultObject);
                    if (txtParam != null && txtParam != _defaultObject)
                    {
                        try
                        {
                            string txtParamText = txtParam.ToString();
                            if (!string.IsNullOrEmpty(txtParamText))
                            {
                                string textId = txtParamText.Trim(' ', '#');
                                log.InfoFormat("ServiceDialogCmdBaseInvokePrefix Param Text ID: {0}", textId);
                                if (!textIds.ContainsKey(textId))
                                {
                                    textIds.Add(textId, method);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix txtParam Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                        }
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix GetParameter txtParam Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }
            else
            {
                log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix No inParam");
            }

            dynamic inoutParamDyn =  inoutParam;
            if (inoutParamDyn != null)
            {
                try
                {
                    string[] istaSysVar = new string[20];
                    istaSysVar[0] = "DE"; // country
                    istaSysVar[1] = DateTime.Now.ToString("dd.MM.yyyy");
                    istaSysVar[2] = DateTime.Now.ToString("HH:mm:ss");
                    inoutParamDyn.setParameter("ISTA_Systemvariable", istaSysVar);

                    string[] specialFeatures = new[] { "606", "609", "6UM", "6UN", "6UP" };
                    inoutParamDyn.setParameter("Sonderausstattungen", specialFeatures);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix SetParameter ISTA_Systemvariable Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }
            else
            {
                log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix No inoutParam");
            }

            lock (_moduleThreadLock)
            {
                if (serviceModuleDataItem != null && _serviceDialogTextHashes != null)
                {
                    foreach (string textId in _serviceDialogTextHashes)
                    {
                        log.InfoFormat("ServiceDialogCmdBaseInvokePrefix Text ID: {0}", textId);
                        if (!textIds.ContainsKey(textId))
                        {
                            textIds.Add(textId, method);
                        }
                    }

                    _serviceDialogTextHashes = null;
                }
            }

            dynamic outParmDyn = outParam;
            if (outParmDyn != null)
            {
                try
                {
                    outParmDyn.setParameter("Quit", true);

                    object ediabasAdapterDeviceResult = _istaEdiabasAdapterDeviceResultConstructor.Invoke(null);
                    if (ediabasAdapterDeviceResult != null)
                    {
                        PropertyInfo propertyEcuJob = ediabasAdapterDeviceResult.GetType().GetProperty("ECUJob");
                        if (propertyEcuJob != null)
                        {
                            dynamic ecuJob = propertyEcuJob.GetValue(ediabasAdapterDeviceResult);
                            if (ecuJob != null)
                            {
                                Type listType = typeof(List<>).MakeGenericType(new[] { _ecuResultType });
                                dynamic ecuResultList = Activator.CreateInstance(listType);
                                if (ecuResultList != null)
                                {
                                    dynamic ecuResultVariant = _vehicleEcuResultConstructor.Invoke(null);
                                    ecuResultVariant.Name = "VARIANTE";
                                    ecuResultVariant.Value = "[VARIANTE]";
                                    ecuResultList.Add(ecuResultVariant);

                                    dynamic ecuResultJobStatus1 = _vehicleEcuResultConstructor.Invoke(null);
                                    ecuResultJobStatus1.Name = "JOBSTATUS";
                                    ecuResultJobStatus1.Value = "OKAY";
                                    ecuResultList.Add(ecuResultJobStatus1);

                                    dynamic ecuResultJobStatus2 = _vehicleEcuResultConstructor.Invoke(null);
                                    ecuResultJobStatus2.Name = "JOB_STATUS";
                                    ecuResultJobStatus2.Value = "OKAY";
                                    ecuResultList.Add(ecuResultJobStatus2);

                                    dynamic ecuResultJobSets = _vehicleEcuResultConstructor.Invoke(null);
                                    ecuResultJobSets.Name = "SAETZE";
                                    ecuResultJobSets.Value = (ushort)1;
                                    ecuResultList.Add(ecuResultJobSets);
                                }
                                ecuJob.JobResult = ecuResultList;
                            }
                        }
                        outParmDyn.setParameter("/WurzelOut/DSCResult", ediabasAdapterDeviceResult);

                        if (dialogName == "EnterServiceDlg")
                        {
                            string resultText = "0123";
                            switch (dialogMethodName)
                            {
                                case "Reset_AU_HU":
                                    resultText = "12";
                                    break;
                            }

                            outParmDyn.setParameter("Result", resultText);
                        }
                        else
                        {
                            int resultValue = 1;
                            if (dialogName == "QuestionServiceDlg")
                            {
                                resultValue = (dialogState + 1) % 2;
                                dialogState++;
                            }
                            else if (dialogName == "QuestionSelectServiceDlg_20")
                            {
                                resultValue = (dialogState + 1) % 20;
                                dialogState++;
                            }

                            log.InfoFormat("ServiceDialogCmdBaseInvokePrefix, Setting result: {0}", resultValue);
                            outParmDyn.setParameter("Result", resultValue);
                        }

                        if (dialogName == "DatumeingabeDlg")
                        {
                            outParmDyn.setParameter("Datum", DateTime.Now);
                        }

                        if (dialogName == "Identifikationstyp")
                        {
                            outParmDyn.setParameter("Typ", 1);
                        }

                        if (dialogName == "SYS_VAR_ISTA")
                        {
                            outParmDyn.setParameter("PLandTester", "DE");
                        }

                        if (dialogName == "ISTA_Kontext_FZG_Daten")
                        {
                            outParmDyn.setParameter("Produktionsdatum_Jahr", 2000);
                            outParmDyn.setParameter("Produktionsdatum_Monat", 01);
                        }

                        if (dialogName == "ISTA_Zeit")
                        {
                            outParmDyn.setParameter("Datum_Jahr", 2000);
                            outParmDyn.setParameter("Datum_Monat", 01);
                        }

                        if (dialogName == "ISTA_Kontext_FZG_Daten")
                        {
                            outParmDyn.setParameter("IStufeHO_JJMMIII", 2001345);
                        }

                        lock (_moduleThreadLock)
                        {
                            if (serviceModuleDataItem != null)
                            {
                                ServiceModuleInvokeItem newItem = new ServiceModuleInvokeItem(method, inParam, outParam, inoutParam, ediabasAdapterDeviceResult, textIds);
                                serviceModuleDataItem.InvokeItems.Add(newItem);
                            }
                        }
                    }
                    else
                    {
                        log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix EdiabasAdapterDeviceResult empty");
                    }
                }
                catch (Exception e)
                {
                    log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix SetParameter Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }
            else
            {
                log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix No out param");
            }

            if (serviceModuleDataItem != null)
            {
                if (dialogStateOld != dialogState)
                {
                    log.InfoFormat("ServiceDialogCmdBaseInvokePrefix, State: {0} -> {1}", dialogStateOld, dialogState);
                }
                serviceModuleDataItem.DialogStateDict[stateKey] = dialogState;
            }

            if (invokeCalls > MaxCallsLimit)
            {
                string callStack = string.Empty;
                try
                {
                    StackTrace stackTrace = new StackTrace();
                    callStack = stackTrace.ToString();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix StackTrace Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                }

                log.ErrorFormat("ServiceDialogCmdBaseInvokePrefix Aborting Method: {0}", method);
                if (!string.IsNullOrEmpty(callStack))
                {
                    log.Error(callStack);
                }

                throw new MethodAbortedException("ServiceDialogCmdBaseInvokePrefix", "Max calls limit reached");
            }

            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool ConfigurationContainerDeserializePrefix(string configurationContainer)
        {
            CheckThreadInterrupted();
            log.InfoFormat("ConfigurationContainerDeserializePrefix");
            return true;
        }

        // ReSharper disable once UnusedMember.Local
        private static void ConfigurationContainerDeserializePostfix(ref object __result, string configurationContainer)
        {
            CheckThreadInterrupted();
            string resultType = __result != null ? __result.GetType().FullName : string.Empty;
            log.InfoFormat("ConfigurationContainerDeserializePostfix Result: {0}", resultType);
            dynamic resultDyn = __result;
            if (resultDyn != null)
            {
                try
                {
                    resultDyn.AddParametrizationOverride(ConfigurationContainerXMLPar, configurationContainer);
                }
                catch (Exception e)
                {
                    log.ErrorFormat("ConfigurationContainerDeserializePostfix AddParametrizationOverride Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(e));
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private static bool IndirectDocumentPrefix3(ref object __result, string title, string heading, string informationsTyp)
        {
            CheckThreadInterrupted();
            log.InfoFormat("IndirectDocumentPrefix3 Title: {0}, Heading: {1}, Info: {2}", title ?? string.Empty, heading ?? string.Empty, informationsTyp ?? string.Empty);

            object documentList = null;
            try
            {
                Type listType = typeof(List<>).MakeGenericType(new[] { _coreContractsDocumentLocatorType });
                documentList = Activator.CreateInstance(listType);
            }
            catch (Exception e)
            {
                log.ErrorFormat("IndirectDocumentPrefix3 new List<IDocumentLocator>() Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(e));
            }

            __result = documentList;
            return false;
        }

        // ReSharper disable once UnusedMember.Local
        private static bool CharacteristicsPrefix(ref object __result, string controlId)
        {
            CheckThreadInterrupted();
            log.InfoFormat("CharacteristicsPrefix ControlId: {0}", controlId ?? string.Empty);

            string stateKey = controlId ?? string.Empty;
            int invokeCalls = 1;
            lock (_moduleThreadLock)
            {
                if (_characteristicCallsDict == null)
                {
                    _characteristicCallsDict = new SerializableDictionary<string, int>();
                }

                if (!_characteristicCallsDict.ContainsKey(stateKey))
                {
                    _characteristicCallsDict.Add(stateKey, invokeCalls);
                }
                else
                {
                    invokeCalls = _characteristicCallsDict[stateKey];
                    invokeCalls++;
                    _characteristicCallsDict[stateKey] = invokeCalls;
                }
            }

            if (invokeCalls > MaxCallsLimit)
            {
                string callStack = string.Empty;
                try
                {
                    StackTrace stackTrace = new StackTrace();
                    callStack = stackTrace.ToString();
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CharacteristicsPrefix StackTrace Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                }

                log.ErrorFormat("CharacteristicsPrefix Aborting ControlId: {0}", controlId);
                if (!string.IsNullOrEmpty(callStack))
                {
                    log.Error(callStack);
                }

                throw new MethodAbortedException("CharacteristicsPrefix", "Max calls limit reached");
            }

            __result = null;
            return false;
        }

        private static bool ModuleTextPrefix2(ref object __result, string value, object paramArray)
        {
            CheckThreadInterrupted();
            log.InfoFormat("ModuleTextPrefix2 Value: {0}", value ?? string.Empty);

            lock (_moduleThreadLock)
            {
                if (_serviceDialogTextHashes == null)
                {
                    _serviceDialogTextHashes = new HashSet<string>();
                }

                _serviceDialogTextHashes.Add(value);
            }

            __result = null;
            return true;
        }

        private static bool GetIstaResultAsTypePrefix(object __instance, ref object __result, string resultName, Type targetType)
        {
            CheckThreadInterrupted();
            log.InfoFormat("GetIstaResultAsTypePrefix Value: '{0}', Type: {1}", resultName ?? string.Empty, targetType);

            __result = null;
            return true;
        }

        private static void GetIstaResultAsTypePostfix(object __instance, ref object __result, string resultName, Type targetType)
        {
            CheckThreadInterrupted();
            string resultData = __result != null ? __result.ToString() : string.Empty;
            log.InfoFormat("GetIstaResultAsTypePostfix Data: '{0}', Value: '{1}', Type: {2}", resultData, resultName ?? string.Empty, targetType);

            ServiceModuleInvokeItem serviceModuleInvokeItem = null;
            lock (_moduleThreadLock)
            {
                if (__instance != null && _serviceDialogDict != null)
                {
                    foreach (KeyValuePair<string, ServiceModuleDataItem> serviceKeyValuePair in _serviceDialogDict)
                    {
                        foreach (ServiceModuleInvokeItem invokeItem in serviceKeyValuePair.Value.InvokeItems)
                        {
                            if (invokeItem.DscResult == __instance)
                            {
                                serviceModuleInvokeItem = invokeItem;
                                break;
                            }
                        }
                    }
                }
            }

            switch (resultName)
            {
                case "/Result/Rows/$Count":
                    if (targetType == typeof(int))
                    {
                        __result = 1;
                        log.InfoFormat("GetIstaResultAsTypePostfix Overriding value with: '{0}'", __result);
                    }
                    break;

                case "/Result/Rows/Row[0]/CBS_VERSION_TEXT":
                    if (targetType == typeof(string))
                    {
                        __result = "CBS 4";
                        log.InfoFormat("GetIstaResultAsTypePostfix Overriding value with: '{0}'", __result);
                    }
                    break;

                case "/Result/Rows/Row[0]/STAT_GETRIEBEVORADA_ZUSTAND":
                    if (targetType == typeof(byte))
                    {
                        __result = 0;
                        log.InfoFormat("GetIstaResultAsTypePostfix Overriding value with: '{0}'", __result);
                    }
                    break;
            }

            if (serviceModuleInvokeItem != null)
            {
                serviceModuleInvokeItem.ResultItems.Add(new ServiceModuleResultItem(resultName, resultData, targetType.ToString()));
            }
            else
            {
                log.InfoFormat("GetIstaResultAsTypePostfix ResultItem not found, Value: '{0}' ", resultName ?? string.Empty);
            }
        }

        private static bool GetModuleParameterPrefix1(object __instance, ref object __result, string name)
        {
            CheckThreadInterrupted();
            log.InfoFormat("GetModuleParameterPrefix1 Name: '{0}'", name ?? string.Empty);

            __result = null;
            return true;
        }

        private static void GetModuleParameterPostfix1(object __instance, ref object __result, string name)
        {
            CheckThreadInterrupted();
            string resultData = __result != null ? __result.ToString() : string.Empty;
            log.InfoFormat("GetModuleParameterPostfix1 Name: '{0}', Data: '{1}'", name ?? string.Empty, resultData);
            ServiceModuleDataItem serviceModuleDataItem = GetServiceModuleItemForParameter(__instance, out bool isInParam, out ServiceModuleInvokeItem serviceModuleInvokeItem);
            if (serviceModuleDataItem == null)
            {
                log.ErrorFormat("GetModuleParameterPostfix1 Service module item not found Name: '{0}'", name ?? string.Empty);
            }
            else
            {
                log.InfoFormat("GetModuleParameterPostfix1 Service module found, Method: '{0}', IsInParam: '{1}', InvokeItem: {2}",
                    serviceModuleDataItem.MethodName ?? string.Empty, isInParam, serviceModuleInvokeItem != null);
                StoreParamResult(name, __result, serviceModuleDataItem, serviceModuleInvokeItem, isInParam);
            }
        }

        private static bool GetModuleParameterPrefix2(object __instance, ref object __result, string name, object defaultValue)
        {
            CheckThreadInterrupted();
            if (defaultValue != _defaultObject)
            {
                string defaultData = defaultValue != null ? defaultValue.ToString() : string.Empty;
                log.InfoFormat("GetModuleParameterPrefix2 Name: '{0}', Default: '{1}'", name ?? string.Empty, defaultData);
            }

            __result = null;
            return true;
        }

        private static void GetModuleParameterPostfix2(object __instance, ref object __result, string name, object defaultValue)
        {
            CheckThreadInterrupted();
            if (defaultValue != _defaultObject)
            {
                string defaultData = defaultValue != null ? defaultValue.ToString() : string.Empty;
                string resultData = __result != null ? __result.ToString() : string.Empty;
                log.InfoFormat("GetModuleParameterPostfix2 Name: '{0}', Default: '{1}', Data: '{2}'", name ?? string.Empty, defaultData, resultData);

                ServiceModuleDataItem serviceModuleDataItem = GetServiceModuleItemForParameter(__instance, out bool isInParam, out ServiceModuleInvokeItem serviceModuleInvokeItem);
                if (serviceModuleDataItem == null)
                {
                    log.ErrorFormat("GetModuleParameterPostfix2 Service module not found Name: '{0}'", name ?? string.Empty);
                }
                else
                {
                    log.InfoFormat("GetModuleParameterPostfix2 Service module found, Method: '{0}', IsInParam: '{1}', InvokeItem: {2}",
                        serviceModuleDataItem.MethodName ?? string.Empty, isInParam, serviceModuleInvokeItem != null);
                    StoreParamResult(name, __result, serviceModuleDataItem, serviceModuleInvokeItem, isInParam);
                }
            }
        }

        private static bool GetModuleParameterGetParameterPrefix(object __instance, ref object __result, object name, object defaultValue)
        {
            CheckThreadInterrupted();
            string nameString = name?.ToString() ?? string.Empty;
            log.InfoFormat("GetModuleParameterGetParameterPrefix Name: '{0}'", nameString);

            return true;
        }

        private static void GetModuleParameterGetParameterPostfix(object __instance, ref object __result, object name, object defaultValue)
        {
            CheckThreadInterrupted();
            if (__result == null)
            {
                string nameString = name?.ToString() ?? string.Empty;
                log.InfoFormat("GetModuleParameterGetParameterPostfix No Result Name: '{0}' ", nameString);
            }
        }

        private static bool ModuleSleepPrefix(object __instance, int millisecondsTimeout)
        {
            CheckThreadInterrupted();
            log.InfoFormat("ModuleSleepPrefix Time: {0}", millisecondsTimeout);
            return false;
        }

        private static bool ModuleClearErrorInfoMemoryPrefix(object __instance)
        {
            CheckThreadInterrupted();
            log.InfoFormat("ModuleClearErrorInfoMemoryPrefix");
            return false;
        }

        private static bool ModuleReadErrorInfoMemoryPrefix(object __instance)
        {
            CheckThreadInterrupted();
            log.InfoFormat("ModuleReadErrorInfoMemoryPrefix");
            return false;
        }

        private static bool ModulePrivateMethodPrefix(object __instance)
        {
            CheckThreadInterrupted();
            if (DetectRecursion())
            {
                return false;
            }

            return true;
        }

        private static bool CtorEcuKomStatemenPrefix(object __instance, object callingModule)
        {
            CheckThreadInterrupted();
            log.InfoFormat("CtorEcuKomStatemenPrefix");
            return false;
        }

        private static ServiceModuleDataItem GetServiceModuleItemForParameter(object parameterInst, out bool isInParam, out ServiceModuleInvokeItem serviceModuleInvokeItem)
        {
            ServiceModuleDataItem serviceModuleDataItem = null;
            isInParam = false;
            serviceModuleInvokeItem = null;
            lock (_moduleThreadLock)
            {
                if (parameterInst != null && _serviceDialogDict != null)
                {
                    foreach (KeyValuePair<string, ServiceModuleDataItem> serviceKeyValuePair in _serviceDialogDict)
                    {
                        ServiceModuleDataItem dataItem = serviceKeyValuePair.Value;
                        foreach (ServiceModuleInvokeItem invokeItem in dataItem.InvokeItems)
                        {
                            if (invokeItem.InParam == parameterInst || invokeItem.OutParam == parameterInst || invokeItem.InoutParam == parameterInst)
                            {
                                isInParam = invokeItem.InParam == parameterInst;
                                serviceModuleDataItem = dataItem;
                                serviceModuleInvokeItem = invokeItem;
                                break;
                            }
                        }

                        if (dataItem.InParams == parameterInst || dataItem.InoutParams == parameterInst)
                        {
                            isInParam = dataItem.InParams == parameterInst;
                            serviceModuleDataItem = dataItem;
                            break;
                        }
                    }
                }
            }

            return serviceModuleDataItem;
        }

        private static bool StoreParamResult(string paramName, object result, ServiceModuleDataItem serviceModuleDataItem, ServiceModuleInvokeItem serviceModuleInvokeItem, bool isInParam)
        {
            if (isInParam || string.IsNullOrEmpty(paramName) || serviceModuleDataItem == null)
            {
                return false;
            }

            if (serviceModuleDataItem.CallsCount > MaxCallsStorage)
            {
                return false;
            }
            string resultData = string.Empty;
            if (result != null)
            {
                Type dataType = result.GetType();
                if (dataType.IsPrimitive || dataType == typeof(string))
                {
                    resultData = result.ToString();
                }
                else
                {
                    resultData = dataType.Name;
                }

                if (resultData.Length > MaxResultDataLen)
                {
                    resultData = resultData.Substring(0, MaxResultDataLen);
                }
            }

            if (serviceModuleInvokeItem != null)
            {
                if (!serviceModuleInvokeItem.OutParamValues.ContainsKey(paramName))
                {
                    serviceModuleInvokeItem.OutParamValues.Add(paramName, resultData);
                    return true;
                }
            }
            else
            {
                if (!serviceModuleDataItem.OutParamValues.ContainsKey(paramName))
                {
                    serviceModuleDataItem.OutParamValues.Add(paramName, resultData);
                    return true;
                }
            }

            return false;
        }

        private static bool DetectRecursion(int level = 2)
        {
            try
            {
                Dictionary<string, int> methodsDict = new Dictionary<string, int>();
                StackTrace stackTrace = new StackTrace();
                StackFrame[] stackFrames = stackTrace.GetFrames();
                if (stackFrames != null)
                {
                    foreach (StackFrame stackFrame in stackFrames)
                    {
                        string methodName = stackFrame.GetMethod().Name;
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            if (!methodsDict.ContainsKey(methodName))
                            {
                                methodsDict.Add(methodName, 1);
                            }
                            else
                            {
                                int count = methodsDict[methodName];
                                count++;
                                methodsDict[methodName] = count;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<string, int> keyValue in methodsDict)
                {
                    if (keyValue.Value > level)
                    {
                        log.ErrorFormat("DetectRecursion Method: {0}", keyValue.Key);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DetectRecursion Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                return false;
            }
        }

        public TestModuleData GetTestModuleData(string moduleName)
        {
            log.InfoFormat("GetTestModuleData Name: {0}", moduleName);
            if (TestModuleStorage == null)
            {
                return null;
            }

            string key = moduleName.ToUpperInvariant();
            if (!TestModuleStorage.ModuleDataDict.TryGetValue(key, out TestModuleData testModuleData))
            {
                log.InfoFormat("GetTestModuleData Module not found: {0}", moduleName);
                return null;
            }

            return testModuleData;
        }

        public TestModuleData ReadTestModule(string moduleName, out bool failure)
        {
            log.InfoFormat("ReadTestModule Name: {0}", moduleName);
            failure = false;
            try
            {
                if (string.IsNullOrEmpty(moduleName))
                {
                    return null;
                }

                string fileName = moduleName + ".dll";
                string moduleFile = Path.Combine(_testModulePath, fileName);
                if (!File.Exists(moduleFile))
                {
                    log.ErrorFormat("ReadTestModule File not found: {0}", moduleFile);
                    return null;
                }

                string coreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldCoreFramework.dll");
                if (!File.Exists(coreFrameworkFile))
                {
                    log.ErrorFormat("ReadTestModule Core framework not found: {0}", coreFrameworkFile);
                    return null;
                }
                Assembly coreFrameworkAssembly = Assembly.LoadFrom(coreFrameworkFile);

                string istaCoreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldISTACoreFramework.dll");
                if (!File.Exists(istaCoreFrameworkFile))
                {
                    log.ErrorFormat("ReadTestModule ISTA core framework not found: {0}", istaCoreFrameworkFile);
                    return null;
                }
                Assembly istaCoreFrameworkAssembly = Assembly.LoadFrom(istaCoreFrameworkFile);

                Type istaModuleType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ISTAModule");
                if (istaModuleType == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule not found");
                    return null;
                }

                string istaCommonFile = Path.Combine(_frameworkPath, "BMW.ISPI.TRIC.ISTA.Common.dll");
                if (!File.Exists(istaCommonFile))
                {
                    log.ErrorFormat("ReadTestModule ISTA common not found: {0}", istaCommonFile);
                    return null;
                }
                Assembly istaCommonAssembly = Assembly.LoadFrom(istaCommonFile);

                Type sessionInfoAccessorType = istaCommonAssembly.GetType("BMW.ISPI.TRIC.ISTA.Common.Session.SessionInfoAccessor");
                if (sessionInfoAccessorType == null)
                {
                    log.WarnFormat("PatchCommonMethods SessionInfoAccessor not found, ignoring");
                }
                else
                {
                    Type sessionInfoType = istaCommonAssembly.GetType("BMW.ISPI.TRIC.ISTA.Common.Session.SessionInfo");
                    if (sessionInfoType == null)
                    {
                        log.ErrorFormat("PatchCommonMethods SessionInfo not found");
                        return null;
                    }

                    object sessionInfoModule = Activator.CreateInstance(sessionInfoType);
                    MethodInfo methodAccessorInitialize = sessionInfoAccessorType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static,
                        null, new Type[] { sessionInfoType }, null);
                    if (methodAccessorInitialize == null)
                    {
                        log.ErrorFormat("ReadTestModule SessionInfoAccessor.Initialize not found");
                        return null;
                    }

                    methodAccessorInitialize.Invoke(null, new object[] { sessionInfoModule });
                }

                MethodInfo methodWriteFaPrefix = typeof(PsdzDatabase).GetMethod("CallWriteFaPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodWriteFaPrefix == null)
                {
                    log.ErrorFormat("ReadTestModule CallWriteFaPrefix not found");
                    return null;
                }

                if (!PatchCommonMethods(coreFrameworkAssembly, istaModuleType))
                {
                    log.ErrorFormat("ReadTestModule PatchCommonMethods failed");
                    return null;
                }

                Assembly moduleAssembly = Assembly.LoadFrom(moduleFile);
                Type[] exportedTypes = moduleAssembly.GetExportedTypes();
                Type moduleType = null;
                foreach (Type type in exportedTypes)
                {
                    log.InfoFormat("ReadTestModule Exported type: {0}", type.FullName);
                    if (moduleType == null)
                    {
                        if (!string.IsNullOrEmpty(type.FullName) &&
                            type.FullName.StartsWith("BMW.Rheingold.Module.", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleType = type;
                        }
                    }
                }

                if (moduleType == null)
                {
                    log.ErrorFormat("ReadTestModule No module type found");
                    return null;
                }

                log.InfoFormat("ReadTestModule Using module type: {0}", moduleType.FullName);
                object moduleParamContainerInst = CreateModuleParamContainerInst(coreFrameworkAssembly, out Type moduleParamContainerType);
                if (moduleParamContainerInst == null)
                {
                    log.ErrorFormat("ReadTestModule CreateModuleParamContainerInst failed");
                    return null;
                }

                MethodInfo methodeTestModuleStartType = moduleType.GetMethod("Start");
                if (methodeTestModuleStartType == null)
                {
                    log.ErrorFormat("ReadTestModule Test module Start methode not found");
                    return null;
                }

                MethodInfo methodTestModuleChangeFa = moduleType.GetMethod("Change_FA", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodTestModuleChangeFa == null)
                {
                    log.ErrorFormat("ReadTestModule Test module Change_FA methode not found");
                    return null;
                }

                MethodInfo methodTestModuleWriteFa = moduleType.GetMethod("FA_schreiben", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodTestModuleWriteFa != null)
                {
                    log.InfoFormat("ReadTestModule Patching: {0}", methodTestModuleWriteFa.Name);
                    _harmony.Patch(methodTestModuleWriteFa, new HarmonyMethod(methodWriteFaPrefix));
                }

                object testModule = Activator.CreateInstance(moduleType, moduleParamContainerInst);
                log.InfoFormat("ReadTestModule Module loaded: {0}, Type: {1}", fileName, moduleType.FullName);

                _moduleRefPath = null;
                _moduleRefDict = null;
                object moduleRunInContainerInst = Activator.CreateInstance(moduleParamContainerType);
                object moduleRunOutContainerInst = Activator.CreateInstance(moduleParamContainerType);
                object moduleRunInOutContainerInst = Activator.CreateInstance(moduleParamContainerType);
                object[] startArguments = { moduleRunInContainerInst, moduleRunOutContainerInst, moduleRunInOutContainerInst };
                methodeTestModuleStartType.Invoke(testModule, startArguments);

                string moduleRef = _moduleRefPath;
                if (!string.IsNullOrEmpty(moduleRef))
                {
                    log.ErrorFormat("ReadTestModule RefPath: {0}", moduleRef);
                }

                if (_moduleRefDict == null)
                {
                    bool ignore = moduleName.StartsWith("ABL_AUS_RETROFITLANGUAGE", StringComparison.OrdinalIgnoreCase);
                    if (ignore)
                    {
                        log.InfoFormat("ReadTestModule Ignored No data from test module: {0}", moduleName);
                    }
                    else
                    {
                        log.ErrorFormat("ReadTestModule No data from test module: {0}", moduleName);
                        failure = true;
                    }
                    return null;
                }

                SerializableDictionary<string, List<string>> moduleRefDict = _moduleRefDict;
                _moduleRefDict = null;
                log.ErrorFormat("ReadTestModule Test module items: {0}", moduleRefDict.Count);
                foreach (KeyValuePair<string, List<string>> keyValuePair in moduleRefDict)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format(CultureInfo.InvariantCulture, "Key: {0}", keyValuePair.Key));
                    sb.Append(", Values: ");
                    if (keyValuePair.Value is List<string> elements)
                    {
                        foreach (string element in elements)
                        {
                            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0} ", element));
                        }
                    }

                    log.InfoFormat("ReadTestModule Entry {0}", sb);
                }

                log.InfoFormat("ReadTestModule Finished: {0}", fileName);

                return new TestModuleData(moduleRefDict, moduleRef);
            }
            catch (Exception e)
            {
                failure = true;
                log.ErrorFormat("ReadTestModule Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        private bool PatchCommonMethods(Assembly coreFrameworkAssembly, Type istaModuleType)
        {
            try
            {
                Type databaseProviderType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderFactory");
                if (databaseProviderType == null)
                {
                    log.ErrorFormat("PatchCommonMethods GetDatabaseProviderSQLite not found");
                    return false;
                }

                MethodInfo methodGetDatabaseProviderSQLite = databaseProviderType.GetMethod("GetDatabaseProviderSQLite", BindingFlags.Public | BindingFlags.Static);
                if (methodGetDatabaseProviderSQLite == null)
                {
                    log.ErrorFormat("PatchCommonMethods GetDatabaseProviderSQLite not found");
                    return false;
                }

                MethodInfo methodGetDatabasePrefix = typeof(PsdzDatabase).GetMethod("CallGetDatabaseProviderSQLitePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodGetDatabasePrefix == null)
                {
                    log.ErrorFormat("PatchCommonMethods CallGetDatabaseProviderSQLitePrefix not found");
                    return false;
                }

                MethodInfo methodIstaModuleModuleRef = istaModuleType.GetMethod("callModuleRef", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (methodIstaModuleModuleRef == null)
                {
                    log.ErrorFormat("PatchCommonMethods ISTAModule callModuleRef not found");
                    return false;
                }

                MethodInfo methodModuleRefPrefix = typeof(PsdzDatabase).GetMethod("CallModuleRefPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleRefPrefix == null)
                {
                    log.ErrorFormat("PatchCommonMethods CallModuleRefPrefix not found");
                    return false;
                }

                bool patchedGetDatabase = false;
                bool patchedModuleRef = false;
                foreach (MethodBase methodBase in _harmony.GetPatchedMethods())
                {
                    //log.InfoFormat("PatchCommonMethods Patched: {0}", methodBase.Name);

                    if (methodBase == methodGetDatabaseProviderSQLite)
                    {
                        patchedGetDatabase = true;
                    }

                    if (methodBase == methodIstaModuleModuleRef)
                    {
                        patchedModuleRef = true;
                    }
                }

                if (!patchedGetDatabase)
                {
                    log.InfoFormat("PatchCommonMethods Patching: {0}", methodGetDatabaseProviderSQLite.Name);
                    _harmony.Patch(methodGetDatabaseProviderSQLite, new HarmonyMethod(methodGetDatabasePrefix));
                }

                if (!patchedModuleRef)
                {
                    log.InfoFormat("ReadTestModule Patching: {0}", methodIstaModuleModuleRef.Name);
                    _harmony.Patch(methodIstaModuleModuleRef, new HarmonyMethod(methodModuleRefPrefix));
                }

                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("PatchCommonMethods Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }
        }

        private object CreateModuleParamContainerInst(Assembly coreFrameworkAssembly, out Type moduleParamContainerType)
        {
            moduleParamContainerType = null;
            try
            {
                string sessionControllerFile = Path.Combine(_frameworkPath, "RheingoldSessionController.dll");
                if (!File.Exists(sessionControllerFile))
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Session controller not found: {0}", sessionControllerFile);
                    return null;
                }
                Assembly sessionControllerAssembly = Assembly.LoadFrom(sessionControllerFile);

                string diagnosticsFile = Path.Combine(_frameworkPath, "RheingoldDiagnostics.dll");
                if (!File.Exists(diagnosticsFile))
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Diagnostics file not found: {0}", diagnosticsFile);
                    return null;
                }
                Assembly diagnosticsAssembly = Assembly.LoadFrom(diagnosticsFile);

                moduleParamContainerType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.ParameterContainer");
                if (moduleParamContainerType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ParameterContainer not found");
                    return null;
                }

                object moduleParamContainerInst = Activator.CreateInstance(moduleParamContainerType);

                Type moduleParamType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.ModuleParameter");
                if (moduleParamType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ModuleParameter not found");
                    return null;
                }

                Type paramNameType = moduleParamType.GetNestedType("ParameterName", BindingFlags.Public | BindingFlags.DeclaredOnly);
                if (paramNameType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ParameterName type not found");
                    return null;
                }
                object parameterNameLogic = Enum.Parse(paramNameType, "Logic", true);
                object parameterNameVehicle = Enum.Parse(paramNameType, "Vehicle", true);

                object moduleParamInst = Activator.CreateInstance(moduleParamType);
                Type logicType = sessionControllerAssembly.GetType("BMW.Rheingold.RheingoldSessionController.Logic");
                if (logicType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Logic not found");
                    return null;
                }
                dynamic logicInst = Activator.CreateInstance(logicType);

                Type vehicleType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.DatabaseProvider.Vehicle");
                if (vehicleType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Vehicle not found");
                    return null;
                }
                dynamic vehicleInst = Activator.CreateInstance(vehicleType);
                logicInst.VecInfo = vehicleInst;

                Type vehicleIdentType = diagnosticsAssembly.GetType("BMW.Rheingold.Diagnostics.VehicleIdent");
                if (vehicleIdentType == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst VehicleIdent not found");
                    return null;
                }

                ConstructorInfo vehicleIdentConstructor = vehicleIdentType.GetConstructor(new Type[] { vehicleType });
                if (vehicleIdentConstructor == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst VehicleIdent constructor not found");
                    return null;
                }

                FieldInfo fieldVecIdent = logicType.GetField("vecIdent", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fieldVecIdent == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst Field vecIdent not found");
                    return null;
                }

                dynamic vehicleIdent = vehicleIdentConstructor.Invoke(new object[] { vehicleInst });
                fieldVecIdent.SetValue(logicInst, vehicleIdent);

                MethodInfo methodContainerSetParameter = moduleParamContainerType.GetMethod("setParameter");
                if (methodContainerSetParameter == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ParameterContainer setParameter not found");
                    return null;
                }

                MethodInfo methodSetParameter = moduleParamType.GetMethod("setParameter");
                if (methodSetParameter == null)
                {
                    log.ErrorFormat("CreateModuleParamContainerInst ModuleParameter setParameter not found");
                    return null;
                }

                methodSetParameter.Invoke(moduleParamInst, new object[] { parameterNameLogic, logicInst });
                methodSetParameter.Invoke(moduleParamInst, new object[] { parameterNameVehicle, vehicleInst });
                methodContainerSetParameter.Invoke(moduleParamContainerInst, new object[] { "__RheinGoldCoreModuleParameters__", moduleParamInst });

                return moduleParamContainerInst;
            }
            catch (Exception e)
            {
                log.ErrorFormat("CreateModuleParamContainerInst Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public bool GenerateVehicleServiceData(ServiceModules serviceModules)
        {
            try
            {
                VehicleStructsBmw.ServiceData serviceData = null;
                XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.ServiceData));
                string serviceDataZipFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.ServiceDataZipFile);
                if (serviceModules == null)
                {
                    log.ErrorFormat("GenerateVehicleServiceData Data missing");

                    if (File.Exists(serviceDataZipFile))
                    {
                        log.InfoFormat("GenerateVehicleServiceData Deleting old file: '{0}'", serviceDataZipFile);
                        File.Delete(serviceDataZipFile);
                    }

                    return true;
                }

                if (File.Exists(serviceDataZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(serviceDataZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, VehicleStructsBmw.ServiceDataXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        serviceData = serializer.Deserialize(reader) as VehicleStructsBmw.ServiceData;
                                    }
                                }
                            }
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
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateVehicleServiceData Deserialize Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                    }
                }

                bool dataValid = true;
                if (serviceData != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (serviceData.Version == null || !serviceData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateVehicleServiceData Version mismatch");
                        dataValid = false;
                    }
                }

                if (serviceData == null || !dataValid)
                {
                    log.InfoFormat("GenerateVehicleServiceData Converting test modules");
                    serviceData = ConvertServiceModulesToVehicleData(serviceModules);
                    if (serviceData == null)
                    {
                        log.ErrorFormat("GenerateVehicleServiceData ConvertServiceModulesToVehicleData failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                        {
                            serializer.Serialize(writer, serviceData);
                        }
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(serviceDataZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(VehicleStructsBmw.ServiceDataXmlFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("GenerateVehicleServiceData Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }
        }

        public bool GenerateServiceModuleData(ProgressDelegate progressHandler, bool checkOnly)
        {
            try
            {
                ServiceModules serviceModules = null;
                XmlSerializer serializer = new XmlSerializer(typeof(ServiceModules));
                string serviceModulesZipFile = Path.Combine(_databaseExtractPath, ServiceModulesZipFile);
                if (File.Exists(serviceModulesZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(serviceModulesZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, ServiceModulesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        serviceModules = serializer.Deserialize(reader) as ServiceModules;
                                    }
                                }
                            }
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
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateServiceModuleData Deserialize Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                    }
                }

                bool dataValid = true;
                bool completed = false;
                int lastProgress = 0;
                int convertFailures = 0;
                if (serviceModules != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (serviceModules.Version == null || !serviceModules.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateServiceModuleData Version mismatch");
                        dataValid = false;
                    }

                    if (dataValid)
                    {
                        completed = serviceModules.Completed;
                        lastProgress = serviceModules.LastProgress;
                        convertFailures = serviceModules.ConvertFailures;
                    }
                }

                if (serviceModules == null || !dataValid || !completed)
                {
                    if (checkOnly)
                    {
                        log.InfoFormat("GenerateServiceModuleData Data not valid, Valid: {0}, Complete: {1}, Progress: {2}%, Failures: {3}",
                            dataValid, completed, lastProgress, convertFailures);
                        // delete old service data file
                        GenerateVehicleServiceData(null);

                        if (progressHandler != null)
                        {
                            progressHandler.Invoke(true, lastProgress, convertFailures);
                            progressHandler.Invoke(false, lastProgress, convertFailures);
                        }

                        return false;
                    }

                    log.InfoFormat("GenerateServiceModuleData Converting modules");
                    if (!AllowDbGeneration)
                    {
                        log.ErrorFormat("GenerateServiceModuleData Started from DLL");
                        return false;
                    }

                    if (progressHandler != null)
                    {
                        if (progressHandler.Invoke(true, 0, 0))
                        {
                            log.ErrorFormat("GenerateServiceModuleData Aborted");
                            return false;
                        }
                    }

                    if (serviceModules != null && dataValid)
                    {
                        serviceModules = ConvertAllServiceModules(progressHandler, serviceModules);
                    }
                    else
                    {
                        serviceModules = ConvertAllServiceModules(progressHandler, serviceModules);
                    }

                    if (serviceModules == null)
                    {
                        log.ErrorFormat("GenerateServiceModuleData ConvertAllServiceModules failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                        {
                            serializer.Serialize(writer, serviceModules);
                        }
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(serviceModulesZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(ServiceModulesXmlFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                if (checkOnly && serviceModules.Completed)
                {
                    if (!GenerateVehicleServiceData(serviceModules))
                    {
                        log.ErrorFormat("GenerateVehicleServiceData failed");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("GenerateServiceModuleData Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }
        }

        public ServiceModules ConvertAllServiceModules(ProgressDelegate progressHandler, ServiceModules lastServiceModules = null)
        {
            try
            {
                RestartRequired = true;

                List<SwiDiagObj> diagObjsNodeClass = GetInfoObjectsTreeForNodeclassName(DiagObjServiceRoot, null, new List<string> { AblFilter });
                if (diagObjsNodeClass == null)
                {
                    log.ErrorFormat("ConvertAllServiceModules GetInfoObjectsTreeForNodeclassName failed");
                    return null;
                }

                List<SwiInfoObj> completeInfoObjects = new List<SwiInfoObj>();
                foreach (SwiDiagObj swiDiagObj in diagObjsNodeClass)
                {
                    completeInfoObjects.AddRange(swiDiagObj.CompleteInfoObjects);
                }

                SerializableDictionary<string, ServiceModuleData> moduleDataDict = lastServiceModules?.ModuleDataDict;
                SerializableDictionary<string, ServiceModuleTextData> moduleTextDict = lastServiceModules?.ModuleTextDict;
                int lastFailCount = 0;
                if (lastServiceModules != null)
                {
                    lastFailCount = lastServiceModules.ConvertFailures;
                }

                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (moduleDataDict == null)
                {
                    moduleDataDict = new SerializableDictionary<string, ServiceModuleData>();
                }

                if (moduleTextDict == null)
                {
                    moduleTextDict = new SerializableDictionary<string, ServiceModuleTextData>();
                }

                bool completed = true;
                int failCount = 0;
                int index = 0;
                foreach (SwiInfoObj swiInfoObj in completeInfoObjects)
                {
                    if (progressHandler != null)
                    {
                        int percent = index * 100 / completeInfoObjects.Count;
                        if (progressHandler.Invoke(false, percent, failCount))
                        {
                            log.ErrorFormat("ConvertAllServiceModules Aborted at {0}%", percent);
                            return null;
                        }

                        string moduleName = swiInfoObj.ModuleName;
                        string key = moduleName.ToUpperInvariant();
                        if (!moduleDataDict.ContainsKey(key))
                        {
                            ServiceModuleData moduleData = ReadServiceModule(moduleName, swiInfoObj, moduleTextDict, out bool failure);
                            if (moduleData == null)
                            {
                                log.ErrorFormat("ConvertAllServiceModules ReadServiceModule failed for: {0}", moduleName);
                                if (failure)
                                {
                                    log.ErrorFormat("ConvertAllServiceModules ReadServiceModule generation failure for: {0}", moduleName);
                                    failCount++;
                                }
                            }

                            moduleDataDict.Add(key, moduleData);

                            GC.Collect();
                            Process currentProcess = Process.GetCurrentProcess();
                            long usedMemory = currentProcess.PrivateMemorySize64;
                            long usedMemoryMB = usedMemory / (1024 * 1024);
                            log.InfoFormat("ConvertAllServiceModules Memory: {0}MB", usedMemoryMB);
                            int memLimit = Environment.Is64BitProcess ? 2000 : 200;
                            if (usedMemoryMB > memLimit)
                            {
                                log.InfoFormat("ConvertAllServiceModules Memory limit reached: {0}", memLimit);
                                completed = false;
                                break;
                            }
                        }
                        else
                        {
                            log.ErrorFormat("ConvertAllServiceModules ReadServiceModule Module present: {0}", moduleName);
                        }
                    }

                    index++;
                }

                int percentFinish = 100;
                if (!completed)
                {
                    percentFinish = index * 100 / completeInfoObjects.Count;
                    if (percentFinish >= 100)
                    {
                        percentFinish = 99;
                    }
                }

                progressHandler?.Invoke(false, percentFinish, failCount);
                log.InfoFormat("ConvertAllServiceModules Count: {0}, Failures: {1}", moduleDataDict.Count, failCount);
                if (moduleDataDict.Count == 0)
                {
                    log.ErrorFormat("ConvertAllServiceModules No test modules generated");
                    return null;
                }

                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                return new ServiceModules(versionInfo, moduleDataDict, moduleTextDict, completed, percentFinish, failCount + lastFailCount);
            }
            catch (Exception e)
            {
                log.ErrorFormat("ConvertAllServiceModules Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public ServiceModuleData ReadServiceModule(string moduleName, SwiInfoObj swiInfoObj, SerializableDictionary<string, ServiceModuleTextData> moduleTextDict, out bool failure)
        {
            log.InfoFormat("ReadServiceModule Name: {0}", moduleName);
            failure = false;
            try
            {
                if (string.IsNullOrEmpty(moduleName))
                {
                    return null;
                }

                string fileName = moduleName + ".dll";
                string moduleFile = Path.Combine(_testModulePath, fileName);
                if (!File.Exists(moduleFile))
                {
                    log.ErrorFormat("ReadServiceModule File not found: {0}", moduleFile);
                    return null;
                }

                string coreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldCoreFramework.dll");
                if (!File.Exists(coreFrameworkFile))
                {
                    log.ErrorFormat("ReadServiceModule Core framework not found: {0}", coreFrameworkFile);
                    return null;
                }
                Assembly coreFrameworkAssembly = Assembly.LoadFrom(coreFrameworkFile);

                string coreContractsFile = Path.Combine(_frameworkPath, "RheingoldCoreContracts.dll");
                if (!File.Exists(coreContractsFile))
                {
                    log.ErrorFormat("ReadServiceModule Core contracts not found: {0}", coreContractsFile);
                    return null;
                }
                Assembly coreContractsAssembly = Assembly.LoadFrom(coreContractsFile);

                string vehicleCommunicationFile = Path.Combine(_frameworkPath, "RheingoldVehicleCommunication.dll");
                if (!File.Exists(vehicleCommunicationFile))
                {
                    log.ErrorFormat("ReadServiceModule RheingoldVehicleCommunication not found: {0}", vehicleCommunicationFile);
                    return null;
                }
                Assembly vehicleCommunicationAssembly = Assembly.LoadFrom(vehicleCommunicationFile);

                if (_coreContractsDocumentLocatorType == null)
                {
                    Type coreContractsDocumentLocatorType = coreContractsAssembly.GetType("BMW.Rheingold.CoreFramework.Contracts.IDocumentLocator");
                    if (coreContractsDocumentLocatorType == null)
                    {
                        log.ErrorFormat("ReadServiceModule IDocumentLocator not found");
                        return null;
                    }

                    _coreContractsDocumentLocatorType = coreContractsDocumentLocatorType;
                }

                if (_vehicleEcuResultConstructor == null)
                {
                    Type ecuResultType = vehicleCommunicationAssembly.GetType("BMW.Rheingold.VehicleCommunication.ECUResult");
                    if (ecuResultType == null)
                    {
                        log.ErrorFormat("ReadServiceModule ECUResult not found");
                        return null;
                    }

                    _ecuResultType = ecuResultType;
                    ConstructorInfo vehicleEcuResultConstructor = ecuResultType.GetConstructor(Type.EmptyTypes);
                    if (vehicleEcuResultConstructor == null)
                    {
                        log.ErrorFormat("ReadServiceModule ECUResult constructor not found");
                        return null;
                    }

                    _vehicleEcuResultConstructor = vehicleEcuResultConstructor;
                }

                string istaCoreFrameworkFile = Path.Combine(_frameworkPath, "RheingoldISTACoreFramework.dll");
                if (!File.Exists(istaCoreFrameworkFile))
                {
                    log.ErrorFormat("ReadServiceModule ISTA core framework not found: {0}", istaCoreFrameworkFile);
                    return null;
                }
                Assembly istaCoreFrameworkAssembly = Assembly.LoadFrom(istaCoreFrameworkFile);

                Type istaServiceDialogFactoryType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ServiceDialogFactory");
                if (istaServiceDialogFactoryType == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogFactory not found");
                    return null;
                }

                _istaServiceDialogFactoryType = istaServiceDialogFactoryType;

                Type istaServiceDialogConfigurationType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ServiceDialogConfiguration");
                if (istaServiceDialogConfigurationType == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogConfiguration not found");
                    return null;
                }

                _istaServiceDialogConfigurationType = istaServiceDialogConfigurationType;

                MethodInfo methodCreateServiceDialog = istaServiceDialogFactoryType.GetMethod("CreateServiceDialog", BindingFlags.Public | BindingFlags.Instance);
                if (methodCreateServiceDialog == null)
                {
                    log.ErrorFormat("ReadServiceModule CreateServiceDialog not found");
                    return null;
                }

                MethodInfo methodCreateServiceDialogPrefix = typeof(PsdzDatabase).GetMethod("CreateServiceDialogPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodCreateServiceDialogPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule CreateServiceDialogPrefix not found");
                    return null;
                }

                Type istaServiceDialogDlgCmdBaseType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ServiceDialogCmdBase");
                if (istaServiceDialogDlgCmdBaseType == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogCmdBase not found");
                    return null;
                }

                MethodInfo methodIstaServiceDialogDlgCmdBaseInvoke = istaServiceDialogDlgCmdBaseType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
                if (methodIstaServiceDialogDlgCmdBaseInvoke == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogDlgCmd Invoke not found");
                    return null;
                }

                MethodInfo methodServiceDialogCmdBaseInvokePrefix = typeof(PsdzDatabase).GetMethod("ServiceDialogCmdBaseInvokePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodServiceDialogCmdBaseInvokePrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ServiceDialogCmdBaseInvokePrefix not found");
                    return null;
                }

                Type istaConfigurationContainerType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ConfigurationContainer");
                if (istaConfigurationContainerType == null)
                {
                    log.ErrorFormat("ReadServiceModule ConfigurationContainer not found");
                    return null;
                }

                MethodInfo methodConfigurationContainerDeserialize = istaConfigurationContainerType.GetMethod("Deserialize", BindingFlags.Public | BindingFlags.Static);
                if (methodConfigurationContainerDeserialize == null)
                {
                    log.ErrorFormat("ReadServiceModule ConfigurationContainer Deserialize not found");
                    return null;
                }

                MethodInfo methodConfigurationContainerDeserializePrefix = typeof(PsdzDatabase).GetMethod("ConfigurationContainerDeserializePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodConfigurationContainerDeserializePrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ConfigurationContainerDeserializePrefix not found");
                    return null;
                }

                MethodInfo methodConfigurationContainerDeserializePostfix = typeof(PsdzDatabase).GetMethod("ConfigurationContainerDeserializePostfix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodConfigurationContainerDeserializePostfix == null)
                {
                    log.ErrorFormat("ReadServiceModule methodConfigurationContainerDeserializePostfix not found");
                    return null;
                }

                MethodInfo methodCtorEcuKomStatemenPrefix = typeof(PsdzDatabase).GetMethod("CtorEcuKomStatemenPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodCtorEcuKomStatemenPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule CtorEcuKomStatemenPrefix not found");
                    return null;
                }

                if (_istaServiceDialogDlgCmdBaseConstructor == null)
                {
                    ConstructorInfo[] istaServiceDialogDlgCmdBaseConstructors = istaServiceDialogDlgCmdBaseType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                    if (istaServiceDialogDlgCmdBaseConstructors.Length != 1)
                    {
                        log.ErrorFormat("ReadServiceModule ServiceDialogCmdBase constructor not found");
                        return null;
                    }

                    ConstructorInfo istaServiceDialogDlgCmdBaseConstructor = istaServiceDialogDlgCmdBaseConstructors[0];
                    ParameterInfo[] istaServiceDialogDlgCmdBaseConstructorParameters = istaServiceDialogDlgCmdBaseConstructor.GetParameters();
                    if (istaServiceDialogDlgCmdBaseConstructorParameters.Length != 5)
                    {
                        log.ErrorFormat("ReadServiceModule ServiceDialogCmdBase parameter count invalid: {0}", istaServiceDialogDlgCmdBaseConstructorParameters.Length);
                        return null;
                    }

                    _istaServiceDialogDlgCmdBaseConstructor = istaServiceDialogDlgCmdBaseConstructor;
                }

                Type istaEdiabasAdapterDeviceResultType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.ISTA.CoreFramework.EDIABASAdapterDeviceResult");
                if (istaEdiabasAdapterDeviceResultType == null)
                {
                    log.ErrorFormat("ReadServiceModule EDIABASAdapterDeviceResult not found");
                    return null;
                }

                MethodInfo methodIstaResultAsType = istaEdiabasAdapterDeviceResultType.GetMethod("getISTAResultAsType", BindingFlags.Instance | BindingFlags.Public);
                if (methodIstaResultAsType == null)
                {
                    log.ErrorFormat("ReadServiceModule getISTAResultAsType not found");
                    return null;
                }

                MethodInfo methodIstaResultAsTypePrefix = typeof(PsdzDatabase).GetMethod("GetIstaResultAsTypePrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodIstaResultAsTypePrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule GetIstaResultAsTypePrefix not found");
                    return null;
                }

                MethodInfo methodIstaResultAsTypePostfix = typeof(PsdzDatabase).GetMethod("GetIstaResultAsTypePostfix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodIstaResultAsTypePostfix == null)
                {
                    log.ErrorFormat("ReadServiceModule GetIstaResultAsTypePostfix not found");
                    return null;
                }

                if (_istaEdiabasAdapterDeviceResultConstructor == null)
                {
                    ConstructorInfo istaEdiabasAdapterDeviceResultConstructor = istaEdiabasAdapterDeviceResultType.GetConstructor(Type.EmptyTypes);
                    if (istaEdiabasAdapterDeviceResultConstructor == null)
                    {
                        log.ErrorFormat("ReadServiceModule EDIABASAdapterDeviceResult constructor not found");
                        return null;
                    }

                    _istaEdiabasAdapterDeviceResultConstructor = istaEdiabasAdapterDeviceResultConstructor;
                }

                Type ecuKomStatementType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.ISTA.CoreFramework.Module.EcuKomStatement");
                if (ecuKomStatementType == null)
                {
                    log.ErrorFormat("ReadServiceModule EcuKomStatement not found");
                    return null;
                }

                ConstructorInfo[] ecuKomStatementConstructors = ecuKomStatementType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                if (ecuKomStatementConstructors.Length != 1)
                {
                    log.ErrorFormat("ReadServiceModule EcuKomStatement constructor not found");
                    return null;
                }

                ConstructorInfo ecuKomStatementConstructor = ecuKomStatementConstructors[0];

                Type istaModuleType = istaCoreFrameworkAssembly.GetType("BMW.Rheingold.Module.ISTA.ISTAModule");
                if (istaModuleType == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule not found");
                    return null;
                }

                // __IndirectDocument with 2 arguments calls __IndirectDocument with 3 arguments
                MethodInfo methodIstaModuleIndirectDocument3 = istaModuleType.GetMethod("__IndirectDocument", BindingFlags.Instance | BindingFlags.NonPublic,
                    null, new Type[] { typeof(string), typeof(string), typeof(string) }, null);
                if (methodIstaModuleIndirectDocument3 == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule __IndirectDocument 3 not found");
                    return null;
                }

                MethodInfo methodIndirectDocumentPrefix3 = typeof(PsdzDatabase).GetMethod("IndirectDocumentPrefix3", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodIndirectDocumentPrefix3 == null)
                {
                    log.ErrorFormat("ReadServiceModule IndirectDocumentPrefix3 not found");
                    return null;
                }

                MethodInfo methodIstaModuleCharacteristics = istaModuleType.GetMethod("__Characteristics", BindingFlags.Instance | BindingFlags.Public,
                    null, new Type[] { typeof(string) }, null);
                if (methodIstaModuleCharacteristics == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule __Characteristics not found");
                    return null;
                }

                MethodInfo methodCharacteristicsPrefix = typeof(PsdzDatabase).GetMethod("CharacteristicsPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodCharacteristicsPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule CharacteristicsPrefix not found");
                    return null;
                }

                Type textParameterType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.__TextParameter");
                if (textParameterType == null)
                {
                    log.ErrorFormat("ReadServiceModule __TextParameter type not found");
                    return null;
                }

                Type moduleParamContainerType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.ParameterContainer");
                if (moduleParamContainerType == null)
                {
                    log.ErrorFormat("ReadServiceModule ParameterContainer not found");
                    return null;
                }

                MethodInfo methodParamContainerGetParameter1 = moduleParamContainerType.GetMethod("getParameter", BindingFlags.Instance | BindingFlags.Public,
                    null, new Type[] { typeof(string) }, null);
                if (methodParamContainerGetParameter1 == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule getParameter1 not found");
                    return null;
                }

                MethodInfo methodParamContainerGetParameter2 = moduleParamContainerType.GetMethod("getParameter", BindingFlags.Instance | BindingFlags.Public,
                    null, new Type[] { typeof(string), typeof(object) }, null);
                if (methodParamContainerGetParameter2 == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule getParameter2 not found");
                    return null;
                }

                Type moduleParameterType = coreFrameworkAssembly.GetType("BMW.Rheingold.CoreFramework.ModuleParameter");
                if (moduleParameterType == null)
                {
                    log.ErrorFormat("ReadServiceModule ModuleParameter not found");
                    return null;
                }

                MethodInfo[] methodsModuleParameter = moduleParameterType.GetMethods();
                MethodInfo methodModuleParameterGetParameter = null;
                foreach (MethodInfo method in methodsModuleParameter)
                {
                    if (method.Name == "getParameter" && method.GetParameters().Length == 2)
                    {
                        methodModuleParameterGetParameter = method;
                        break;
                    }
                }

                if (methodModuleParameterGetParameter == null)
                {
                    log.ErrorFormat("ReadTestModule ModuleParameter getParameter not found");
                    return null;
                }

                MethodInfo methodModuleParameterPrefix1 = typeof(PsdzDatabase).GetMethod("GetModuleParameterPrefix1", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleParameterPrefix1 == null)
                {
                    log.ErrorFormat("ReadServiceModule GetModuleParameterPrefix1 not found");
                    return null;
                }

                MethodInfo methodModuleParameterPostfix1 = typeof(PsdzDatabase).GetMethod("GetModuleParameterPostfix1", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleParameterPostfix1 == null)
                {
                    log.ErrorFormat("ReadServiceModule GetModuleParameterPostfix1 not found");
                    return null;
                }

                MethodInfo methodModuleParameterPrefix2 = typeof(PsdzDatabase).GetMethod("GetModuleParameterPrefix2", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleParameterPrefix2 == null)
                {
                    log.ErrorFormat("ReadServiceModule GetModuleParameterPrefix2 not found");
                    return null;
                }

                MethodInfo methodModuleParameterPostfix2 = typeof(PsdzDatabase).GetMethod("GetModuleParameterPostfix2", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleParameterPostfix2 == null)
                {
                    log.ErrorFormat("ReadServiceModule GetModuleParameterPostfix2 not found");
                    return null;
                }

                MethodInfo methodModuleParameterGetParameterPrefix = typeof(PsdzDatabase).GetMethod("GetModuleParameterGetParameterPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleParameterGetParameterPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule GetModuleParameterGetParameterPrefix not found");
                    return null;
                }

                MethodInfo methodModuleParameterGetParameterPostfix = typeof(PsdzDatabase).GetMethod("GetModuleParameterGetParameterPostfix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleParameterGetParameterPostfix == null)
                {
                    log.ErrorFormat("ReadServiceModule GetModuleParameterGetParameterPostfix not found");
                    return null;
                }

                // __Text with 1 argument calls __Text with 2 arguments
                MethodInfo methodIstaModuleText2 = istaModuleType.GetMethod("__Text", BindingFlags.Instance | BindingFlags.Public,
                    null, new Type[] { typeof(string), textParameterType.MakeArrayType() }, null);
                if (methodIstaModuleText2 == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule methodIstaModuleText2 not found");
                    return null;
                }

                MethodInfo methodModuleTextPrefix2 = typeof(PsdzDatabase).GetMethod("ModuleTextPrefix2", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleTextPrefix2 == null)
                {
                    log.ErrorFormat("ReadServiceModule ModuleTextPrefix2 not found");
                    return null;
                }

                MethodInfo methodIstaModuleSleep = istaModuleType.GetMethod("Sleep", BindingFlags.Instance | BindingFlags.Public);
                if (methodIstaModuleSleep == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule Sleep not found");
                    return null;
                }

                MethodInfo methodModuleSleepPrefix = typeof(PsdzDatabase).GetMethod("ModuleSleepPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleSleepPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ModuleSleepPrefix not found");
                    return null;
                }

                MethodInfo methodIstaModuleClearErrorInfoMemory = istaModuleType.GetMethod("ClearErrorInfoMemory", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodIstaModuleClearErrorInfoMemory == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule ClearErrorInfoMemory not found");
                    return null;
                }

                MethodInfo methodModuleClearErrorInfoMemoryPrefix = typeof(PsdzDatabase).GetMethod("ModuleClearErrorInfoMemoryPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleClearErrorInfoMemoryPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ModuleClearErrorInfoMemoryPrefix not found");
                    return null;
                }

                MethodInfo methodIstaModuleReadErrorInfoMemory = istaModuleType.GetMethod("ReadErrorInfoMemory", BindingFlags.Instance | BindingFlags.NonPublic);
                if (methodIstaModuleReadErrorInfoMemory == null)
                {
                    log.ErrorFormat("ReadTestModule ISTAModule ReadErrorInfoMemory not found");
                    return null;
                }

                MethodInfo methodModuleReadErrorInfoMemoryPrefix = typeof(PsdzDatabase).GetMethod("ModuleReadErrorInfoMemoryPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModuleReadErrorInfoMemoryPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ModuleReadErrorInfoMemoryPrefix not found");
                    return null;
                }

                if (!PatchCommonMethods(coreFrameworkAssembly, istaModuleType))
                {
                    log.ErrorFormat("ReadServiceModule PatchCommonMethods failed");
                    return null;
                }

                bool patchedecuKomStatementConstructor = false;
                bool patchedCreateServiceDialog = false;
                bool patchedServiceDialogCmdBaseInvoke = false;
                bool patchedConfigurationContainerDeserialize = false;
                bool patchedIstaResultAsType = false;
                bool patchedIndirectDocument = false;
                bool patchedCharacteristics = false;
                bool patchedParamContainerGetParameter = false;
                bool patchedModuleParameterGetParameter = false;
                bool patchedModuleText = false;
                bool patchedModuleSleep = false;
                bool patchedModuleClearErrorInfoMemory = false;
                bool patchedModuleReadErrorInfoMemory = false;
                foreach (MethodBase methodBase in _harmony.GetPatchedMethods())
                {
                    //log.InfoFormat("ReadServiceModule Patched: {0}", methodBase.Name);

                    if (methodBase == ecuKomStatementConstructor)
                    {
                        patchedecuKomStatementConstructor = true;
                    }

                    if (methodBase == methodCreateServiceDialog)
                    {
                        patchedCreateServiceDialog = true;
                    }

                    if (methodBase == methodIstaServiceDialogDlgCmdBaseInvoke)
                    {
                        patchedServiceDialogCmdBaseInvoke = true;
                    }

                    if (methodBase == methodConfigurationContainerDeserialize)
                    {
                        patchedConfigurationContainerDeserialize = true;
                    }

                    if (methodBase == methodIstaResultAsType)
                    {
                        patchedIstaResultAsType = true;
                    }

                    if (methodBase == methodIstaModuleIndirectDocument3)
                    {
                        patchedIndirectDocument = true;
                    }

                    if (methodBase == methodIstaModuleCharacteristics)
                    {
                        patchedCharacteristics = true;
                    }

                    if (methodBase == methodParamContainerGetParameter1)
                    {
                        patchedParamContainerGetParameter = true;
                    }

                    if (methodBase == methodModuleParameterGetParameter)
                    {
                        patchedModuleParameterGetParameter = true;
                    }

                    if (methodBase == methodIstaModuleText2)
                    {
                        patchedModuleText = true;
                    }

                    if (methodBase == methodIstaModuleSleep)
                    {
                        patchedModuleSleep = true;
                    }

                    if (methodBase == methodIstaModuleClearErrorInfoMemory)
                    {
                        patchedModuleClearErrorInfoMemory = true;
                    }

                    if (methodBase == methodIstaModuleReadErrorInfoMemory)
                    {
                        patchedModuleReadErrorInfoMemory = true;
                    }
                }

                if (!patchedecuKomStatementConstructor)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", ecuKomStatementConstructor.Name);
                    _harmony.Patch(ecuKomStatementConstructor, new HarmonyMethod(methodCtorEcuKomStatemenPrefix));
                }

                if (!patchedCreateServiceDialog)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodCreateServiceDialog.Name);
                    _harmony.Patch(methodCreateServiceDialog, new HarmonyMethod(methodCreateServiceDialogPrefix));
                }

                if (!patchedServiceDialogCmdBaseInvoke)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaServiceDialogDlgCmdBaseInvoke.Name);
                    _harmony.Patch(methodIstaServiceDialogDlgCmdBaseInvoke, new HarmonyMethod(methodServiceDialogCmdBaseInvokePrefix));
                }

                if (!patchedConfigurationContainerDeserialize)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodConfigurationContainerDeserializePrefix.Name);
                    _harmony.Patch(methodConfigurationContainerDeserialize,
                        new HarmonyMethod(methodConfigurationContainerDeserializePrefix), new HarmonyMethod(methodConfigurationContainerDeserializePostfix));
                }

                if (!patchedIstaResultAsType)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaResultAsType.Name);
                    _harmony.Patch(methodIstaResultAsType, new HarmonyMethod(methodIstaResultAsTypePrefix), new HarmonyMethod(methodIstaResultAsTypePostfix));
                }

                if (!patchedIndirectDocument)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleIndirectDocument3.Name);
                    _harmony.Patch(methodIstaModuleIndirectDocument3, new HarmonyMethod(methodIndirectDocumentPrefix3));
                }

                if (!patchedCharacteristics)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleCharacteristics.Name);
                    _harmony.Patch(methodIstaModuleCharacteristics, new HarmonyMethod(methodCharacteristicsPrefix));
                }

                if (!patchedParamContainerGetParameter)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodParamContainerGetParameter1.Name);
                    _harmony.Patch(methodParamContainerGetParameter1, new HarmonyMethod(methodModuleParameterPrefix1), new HarmonyMethod(methodModuleParameterPostfix1));

                    log.InfoFormat("ReadServiceModule Patching: {0}", methodParamContainerGetParameter2.Name);
                    _harmony.Patch(methodParamContainerGetParameter2, new HarmonyMethod(methodModuleParameterPrefix2), new HarmonyMethod(methodModuleParameterPostfix2));
                }

                if (!patchedModuleParameterGetParameter)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodModuleParameterGetParameter.Name);
                    _harmony.Patch(methodModuleParameterGetParameter, new HarmonyMethod(methodModuleParameterGetParameterPrefix), new HarmonyMethod(methodModuleParameterGetParameterPostfix));
                }

                if (!patchedModuleText)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleText2.Name);
                    _harmony.Patch(methodIstaModuleText2, new HarmonyMethod(methodModuleTextPrefix2));
                }

                if (!patchedModuleSleep)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleSleep.Name);
                    _harmony.Patch(methodIstaModuleSleep, new HarmonyMethod(methodModuleSleepPrefix));
                }

                if (!patchedModuleClearErrorInfoMemory)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleClearErrorInfoMemory.Name);
                    _harmony.Patch(methodIstaModuleClearErrorInfoMemory, new HarmonyMethod(methodModuleClearErrorInfoMemoryPrefix));
                }

                if (!patchedModuleReadErrorInfoMemory)
                {
                    log.InfoFormat("ReadServiceModule Patching: {0}", methodIstaModuleReadErrorInfoMemory.Name);
                    _harmony.Patch(methodIstaModuleReadErrorInfoMemory, new HarmonyMethod(methodModuleReadErrorInfoMemoryPrefix));
                }

                Assembly moduleAssembly = Assembly.LoadFrom(moduleFile);
                Type[] exportedTypes = moduleAssembly.GetExportedTypes();
                Type moduleType = null;
                foreach (Type type in exportedTypes)
                {
                    log.InfoFormat("ReadTestModule Exported type: {0}", type.FullName);
                    if (moduleType == null)
                    {
                        if (!string.IsNullOrEmpty(type.FullName) &&
                            type.FullName.StartsWith("BMW.Rheingold.Module.", StringComparison.OrdinalIgnoreCase))
                        {
                            moduleType = type;
                        }
                    }
                }

                if (moduleType == null)
                {
                    log.ErrorFormat("ReadServiceModule No module type found");
                    return null;
                }

                log.InfoFormat("ReadServiceModule Using module type: {0}", moduleType.FullName);

                TextContentManager textContentManager = TextContentManager.Create(this, EcuTranslation.GetLanguages(), swiInfoObj) as TextContentManager;
                if (textContentManager == null)
                {
                    log.ErrorFormat("ReadServiceModule No TextContentManager");
                    return null;
                }

                MethodInfo methodModulePrivateMethodPrefix = typeof(PsdzDatabase).GetMethod("ModulePrivateMethodPrefix", BindingFlags.NonPublic | BindingFlags.Static);
                if (methodModulePrivateMethodPrefix == null)
                {
                    log.ErrorFormat("ReadServiceModule ModulePrivateMethodPrefix not found");
                    return null;
                }

                List<MethodInfo> simpleMethods = new List<MethodInfo>();
                MethodInfo[] privateMethods = moduleType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                List<string> invalidMethods = new List<string>
                {
                    "Finalize",
                    "MemberwiseClone",
                    "Auswertung",
                    "Auswahl",
                    "Meldung_01_s",
                    "Fehlerspeicher_Lesen_02_s"
                };

                foreach (MethodInfo privateMethod in privateMethods)
                {
                    string methodName = privateMethod.Name;
                    if (privateMethod.DeclaringType != moduleType)
                    {
                        continue;
                    }

                    if (methodName.StartsWith("__"))
                    {
                        continue;
                    }

                    if (methodName.StartsWith("get_"))
                    {
                        continue;
                    }

                    if (methodName.StartsWith("Get"))
                    {
                        continue;
                    }

                    if (invalidMethods.Contains(methodName))
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = privateMethod.GetParameters();
                    if (parameters.Any())
                    {
                        continue;
                    }

                    simpleMethods.Add(privateMethod);

                    try
                    {
                        log.InfoFormat("ReadServiceModule Patching Module Method: {0}", privateMethod.Name);
                        _harmony.Patch(privateMethod, new HarmonyMethod(methodModulePrivateMethodPrefix));
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("ReadServiceModule Patching Module Method: {0}, Exception: '{1}'", privateMethod.Name, EdiabasLib.EdiabasNet.GetExceptionText(ex));
                    }
                }

                if (simpleMethods.Count > 0)
                {
                    StringBuilder sbSimpleMethods = new StringBuilder();
                    foreach (MethodInfo simpleMethod in simpleMethods)
                    {
                        if (sbSimpleMethods.Length > 0)
                        {
                            sbSimpleMethods.Append(", ");
                        }

                        sbSimpleMethods.Append(simpleMethod.Name);
                    }

                    log.InfoFormat("ReadServiceModule Simple methods: {0}", sbSimpleMethods);

                    object moduleParamContainerInst = CreateModuleParamContainerInst(coreFrameworkAssembly, out _);
                    if (moduleParamContainerInst == null)
                    {
                        log.ErrorFormat("ReadServiceModule CreateModuleParamContainerInst failed");
                        return null;
                    }

                    dynamic testModule = Activator.CreateInstance(moduleType, moduleParamContainerInst);
                    log.InfoFormat("ReadTestModule Module loaded: {0}, Type: {1}", fileName, moduleType.FullName);

                    lock (_moduleThreadLock)
                    {
                        _serviceDialogDict = null;
                    }

                    foreach (MethodInfo simpleMethod in simpleMethods)
                    {
                        Thread moduleThread = new Thread(() =>
                        {
                            try
                            {
                                lock (_moduleThreadLock)
                                {
                                    _serviceDialogTextHashes = null;
                                    _serviceDialogInvokeCallsDict = null;
                                    _characteristicCallsDict = null;
                                    _moduleRefPath = null;
                                    _moduleRefDict = null;
                                    if (_serviceDialogDict != null)
                                    {
                                        foreach (KeyValuePair<string, ServiceModuleDataItem> keyValuePair in _serviceDialogDict)
                                        {
                                            keyValuePair.Value.CallsCount = 0;
                                        }
                                    }
                                }

                                log.InfoFormat("ReadServiceModule Method executing: {0}", simpleMethod.Name);
                                simpleMethod.Invoke(testModule, null);
                                log.InfoFormat("ReadServiceModule Method executed: {0}", simpleMethod.Name);
                            }
                            catch (Exception e)
                            {
                                log.ErrorFormat("ReadServiceModule Method: {0}, Exception: '{1}'", simpleMethod.Name,
                                    EdiabasLib.EdiabasNet.GetExceptionText(e));
                            }
                        });

                        moduleThread.Start();
                        DateTime startTime = DateTime.Now;
                        int waitCount = 0;
                        while (!moduleThread.Join(5000))
                        {
                            log.ErrorFormat("ReadServiceModule Method processing slow: {0}, Module: {1}, Count: {2}", simpleMethod.Name, moduleName, waitCount);
                            waitCount++;
                            if (waitCount > 20)
                            {
                                log.ErrorFormat("ReadServiceModule Method timeout, aborting: {0}, Module: {1}, Aborting", simpleMethod.Name, moduleName);
                                moduleThread.Interrupt();
                                failure = true;
                            }
                        }

                        TimeSpan diffTime = DateTime.Now - startTime;
                        log.InfoFormat("ReadServiceModule Method {0}, Time: {1}s", simpleMethod.Name, diffTime.Seconds);
                    }
                }

                SerializableDictionary<string, ServiceModuleDataItem> serviceDialogDict = _serviceDialogDict;
                lock (_moduleThreadLock)
                {
                    _serviceDialogDict = null;
                    _serviceDialogTextHashes = null;
                    _moduleRefDict = null;
                }

                if (serviceDialogDict == null || serviceDialogDict.Count == 0)
                {
                    log.ErrorFormat("ReadServiceModule No data for: {0}", fileName);
                    return null;
                }

                log.InfoFormat("ReadServiceModule Items: {0}", serviceDialogDict.Count);

                foreach (KeyValuePair<string, ServiceModuleDataItem> dictEntry in serviceDialogDict)
                {
                    ServiceModuleDataItem dataItem = dictEntry.Value;
                    if (!string.IsNullOrEmpty(dataItem.ContainerXml))
                    {
                        string ediabasJobBare = DetectVehicle.ConvertContainerXml(dataItem.ContainerXml);
                        if (!string.IsNullOrEmpty(ediabasJobBare))
                        {
                            log.InfoFormat("ReadServiceModule EdiabasJob bare: '{0}'", ediabasJobBare);
                            dataItem.EdiabasJobBare = ediabasJobBare;
                        }
                        else
                        {
                            log.ErrorFormat("ReadServiceModule ConvertContainerXml failed: '{0}'", dataItem.MethodName);
                        }

                        if (dataItem.RunOverrides != null && dataItem.RunOverrides.Count > 0)
                        {
                            Dictionary<string, string> runOverrides = new Dictionary<string, string>();
                            foreach (KeyValuePair<string, string> runOverride in dataItem.RunOverrides)
                            {
                                string value = runOverride.Value;
                                if (string.IsNullOrWhiteSpace(value))
                                {
                                    value = "[OVERRIDE]";
                                }
                                runOverrides.Add(runOverride.Key, value);
                            }

                            string ediabasJobOverride = DetectVehicle.ConvertContainerXml(dataItem.ContainerXml, runOverrides);
                            if (!string.IsNullOrEmpty(ediabasJobOverride))
                            {
                                log.InfoFormat("ReadServiceModule EdiabasJob override: '{0}'", ediabasJobOverride);
                                dataItem.EdiabasJobOverride = ediabasJobOverride;
                            }
                            else
                            {
                                log.ErrorFormat("ReadServiceModule ConvertContainerXml failed: '{0}'", dataItem.MethodName);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(dataItem.ControlId))
                    {
                        SwiInfoObj infoObject = GetInfoObjectByControlId(dataItem.ControlId);
                        if (infoObject != null)
                        {
                            log.InfoFormat("ReadServiceModule InfoObject Id: {0}, Identifer: {1}", infoObject.Id, infoObject.Identifier);
                            if (TextContentManager.Create(this, EcuTranslation.GetLanguages(), infoObject, dataItem.ServiceDialogName) is TextContentManager textCollection)
                            {
                                IList<string> textIds = textCollection.CreateTextItemIdList();
                                if (textIds != null)
                                {
                                    foreach (string textId in textIds)
                                    {
                                        log.InfoFormat("ReadServiceModule Text Id: {0}", textId);
                                        try
                                        {
                                            ITextLocator textLocator = textCollection.__Text(textId);
                                            TextContent textContent = textLocator?.TextContent as TextContent;
                                            IList<LocalizedText> textItems = textContent?.CreatePlainText(textCollection.Langs);
                                            if (textItems != null)
                                            {
                                                foreach (LocalizedText textItem in textItems)
                                                {
                                                    if (!string.IsNullOrWhiteSpace(textItem.TextItem))
                                                    {
                                                        log.InfoFormat("ReadServiceModule Text Lang: {0}, Text: '{1}'", textItem.Language, textItem.TextItem);
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            log.ErrorFormat("ReadServiceModule Text ID: {0}, Exception: '{1}'", textId, EdiabasLib.EdiabasNet.GetExceptionText(e));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (ServiceModuleInvokeItem invokeItem in dataItem.InvokeItems)
                    {
                        List<string> textHashes = new List<string>();
                        foreach (KeyValuePair<string, string> textIdPair in invokeItem.TextIds)
                        {
                            string textId = textIdPair.Key;
                            string methodName = textIdPair.Value;
                            try
                            {
                                log.InfoFormat("ReadServiceModule SingleText Method: {0}", methodName);

                                ITextLocator textLocator = textContentManager.__Text(textId);
                                TextContent textContent = textLocator?.TextContent as TextContent;
                                IList<LocalizedText> textItems = textContent?.CreatePlainText(textContentManager.Langs);
                                if (textItems != null)
                                {
                                    EcuFunctionStructs.EcuTranslation ecuTranslation = new EcuFunctionStructs.EcuTranslation();
                                    foreach (LocalizedText textItem in textItems)
                                    {
                                        if (!string.IsNullOrWhiteSpace(textItem.TextItem))
                                        {
                                            log.InfoFormat("ReadServiceModule SingleText Lang: {0}, Text: '{1}'", textItem.Language, textItem.TextItem);
                                            ecuTranslation.SetTranslation(textItem.Language, textItem.TextItem);
                                        }
                                    }

                                    string textHash = string.Empty;
                                    if (ecuTranslation.TranslationCount() > 0)
                                    {
                                        ServiceModuleTextData moduleTextData = new ServiceModuleTextData(ecuTranslation);
                                        textHash = moduleTextData.Hash;
                                        if (!string.IsNullOrEmpty(textHash))
                                        {
                                            if (!moduleTextDict.ContainsKey(textHash))
                                            {
                                                moduleTextDict.Add(textHash, moduleTextData);
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(textHash))
                                    {
                                        if (!textHashes.Contains(textHash))
                                        {
                                            textHashes.Add(textHash);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                log.ErrorFormat("ReadServiceModule Text ID: {0}, Exception: '{1}'", textId, EdiabasLib.EdiabasNet.GetExceptionText(e));
                            }
                        }

                        invokeItem.TextHashes = textHashes;
                    }

                    dataItem.CleanupInternal();
                    dataItem.RemoveDuplicates();
                }

                log.InfoFormat("ReadServiceModule Finished: {0}", fileName);

                List<string> diagObjIds = new List<string>();
                foreach (SwiDiagObj diagObj in swiInfoObj.DiagObjectPath)
                {
                    diagObjIds.Add(diagObj.Id);
                }

                return new ServiceModuleData(swiInfoObj.Id, diagObjIds, serviceDialogDict);
            }
            catch (Exception e)
            {
                log.ErrorFormat("ReadServiceModule Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));

                if (string.Compare(moduleName, "ABL_WAR_AS6127_HVS04ECURESET", StringComparison.OrdinalIgnoreCase) != 0)
                {   // known error in constructor, overridden variable not initialized
                    failure = true;
                }

                return null;
            }
        }

        public VehicleStructsBmw.ServiceData ConvertServiceModulesToVehicleData(ServiceModules serviceModules)
        {
            log.InfoFormat("ConvertServiceModulesToVehicleData Start");
            try
            {
                List<VehicleStructsBmw.ServiceDataItem> serviceDataList = new List<VehicleStructsBmw.ServiceDataItem>();
                SerializableDictionary<string, VehicleStructsBmw.ServiceTextData> textDict = new SerializableDictionary<string, VehicleStructsBmw.ServiceTextData>();
                SerializableDictionary<string, ServiceModuleTextData> moduleTextDict = serviceModules.ModuleTextDict;
                foreach (KeyValuePair<string, ServiceModuleData> keyValueModuleData in serviceModules.ModuleDataDict)
                {
                    ServiceModuleData serviceModuleData = keyValueModuleData.Value;
                    if (serviceModuleData == null)
                    {
                        continue;
                    }

                    List<string> diagObjTextHashes = new List<string>();
                    List<VehicleStructsBmw.ServiceInfoData> infoDataList = new List<VehicleStructsBmw.ServiceInfoData>();

                    string infoObjId = serviceModuleData.InfoObjId;
                    string infoObjTextHash = infoObjId;
                    SwiInfoObj swiInfoObj = GetInfoObjectById(infoObjId, SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink.ToString());
                    if (swiInfoObj != null)
                    {
                        VehicleStructsBmw.ServiceTextData infoObjTextData = new VehicleStructsBmw.ServiceTextData(ConvertEcuTranslation(swiInfoObj.EcuTranslation));
                        if (!textDict.ContainsKey(infoObjId))
                        {
                            textDict.Add(infoObjId, infoObjTextData);
                        }
                    }

                    foreach (string diagObjId in serviceModuleData.DiagObjIds)
                    {
                        SwiDiagObj swiDiagObj = GetDiagObjectById(diagObjId);
                        if (swiDiagObj != null)
                        {
                            VehicleStructsBmw.ServiceTextData diagObjTextData = new VehicleStructsBmw.ServiceTextData(ConvertEcuTranslation(swiDiagObj.EcuTranslation));
                            if (!textDict.ContainsKey(diagObjId))
                            {
                                textDict.Add(diagObjId, diagObjTextData);
                            }
                        }

                        diagObjTextHashes.Add(diagObjId);
                    }

                    foreach (KeyValuePair<string, ServiceModuleDataItem> keyValueDataItem in serviceModuleData.DataDict)
                    {
                        List<string> textHashes = new List<string>();
                        ServiceModuleDataItem serviceModuleDataItem = keyValueDataItem.Value;
                        if (serviceModuleDataItem == null)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(serviceModuleDataItem.EdiabasJobBare))
                        {
                            continue;
                        }

                        List<string> resultList = new List<string>();
                        foreach (ServiceModuleInvokeItem invokeItem in serviceModuleDataItem.InvokeItems)
                        {
                            if (invokeItem == null)
                            {
                                continue;
                            }

                            foreach (string textHash in invokeItem.TextHashes)
                            {
                                foreach (ServiceModuleResultItem resultItem in invokeItem.ResultItems)
                                {
                                    string dataName = resultItem.DataName;
                                    string dataType = resultItem.DataType;
                                    if (string.IsNullOrEmpty(dataName) || string.IsNullOrEmpty(dataType))
                                    {
                                        continue;
                                    }

                                    dataName = Regex.Replace(dataName, @"^/Result/Status/", string.Empty);
                                    dataName = Regex.Replace(dataName, @"^/Result/Rows/", string.Empty);
                                    dataName = Regex.Replace(dataName, @"^Row\[(\d+)\]/", @"$$$1#");

                                    if (string.Compare(dataName, "JOB_STATUS", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        continue;
                                    }

                                    if (string.Compare(dataName, "SAETZE", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        continue;
                                    }

                                    if (string.Compare(dataName, "$Count", StringComparison.OrdinalIgnoreCase) == 0)
                                    {
                                        continue;
                                    }

                                    string resultEntry = dataName + "#" + dataType;
                                    if (!resultList.Contains(resultEntry))
                                    {
                                        resultList.Add(resultEntry);
                                    }
                                }

                                if (moduleTextDict.TryGetValue(textHash, out ServiceModuleTextData serviceModuleTextData))
                                {
                                    VehicleStructsBmw.ServiceTextData serviceTextData = new VehicleStructsBmw.ServiceTextData(serviceModuleTextData.Translation);
                                    string hashKey = VehicleStructsBmw.HashPrefix + serviceTextData.Hash;
                                    if (!textDict.ContainsKey(hashKey))
                                    {
                                        textDict.Add(hashKey, serviceTextData);
                                    }

                                    if (!textHashes.Contains(hashKey))
                                    {
                                        textHashes.Add(hashKey);
                                    }
                                }
                                else
                                {
                                    log.ErrorFormat("ConvertServiceModulesToVehicleData Text hash not found: {0}", textHash);
                                }
                            }
                        }

                        VehicleStructsBmw.ServiceInfoData serviceInfoData = new VehicleStructsBmw.ServiceInfoData(serviceModuleDataItem.MethodName, serviceModuleDataItem.ControlId,
                            serviceModuleDataItem.EdiabasJobBare, serviceModuleDataItem.EdiabasJobOverride, resultList, textHashes);
                        infoDataList.Add(serviceInfoData);
                    }

                    VehicleStructsBmw.ServiceDataItem serviceDataItem = new VehicleStructsBmw.ServiceDataItem(infoObjId, infoObjTextHash, serviceModuleData.DiagObjIds, diagObjTextHashes, infoDataList);
                    serviceDataList.Add(serviceDataItem);
                }

                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                VehicleStructsBmw.ServiceData serviceData = new VehicleStructsBmw.ServiceData(versionInfo, serviceDataList, textDict);
                return serviceData;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ConvertServiceModulesToVehicleData Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        EcuFunctionStructs.EcuTranslation ConvertEcuTranslation(EcuTranslation ecuTranslation)
        {
            EcuFunctionStructs.EcuTranslation ecuTranslationVehicle = new EcuFunctionStructs.EcuTranslation();
            List<string> languages = EcuTranslation.GetLanguages();
            foreach (string language in languages)
            {
                string translation = ecuTranslation.GetTitleTranslated(language);
                if (!string.IsNullOrEmpty(translation))
                {
                    ecuTranslationVehicle.SetTranslation(language, translation);
                }
            }

            return ecuTranslationVehicle;
        }

        public bool GenerateTestModuleData(ProgressDelegate progressHandler, bool checkOnly)
        {
            try
            {
                TestModules testModules = null;
                XmlSerializer serializer = new XmlSerializer(typeof(TestModules));
                string testModulesZipFile = Path.Combine(_databaseExtractPath, TestModulesZipFile);
                if (File.Exists(testModulesZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(testModulesZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, TestModulesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        testModules = serializer.Deserialize(reader) as TestModules;
                                    }
                                }
                            }
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
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateTestModuleData Deserialize Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                    }
                }

                bool dataValid = true;
                int convertFailures = 0;
                if (testModules != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (testModules.Version == null || !testModules.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateTestModuleData Version mismatch");
                        dataValid = false;
                    }

                    if (dataValid)
                    {
                        convertFailures = testModules.ConvertFailures;
                    }
                }

                if (testModules == null || !dataValid)
                {
                    if (checkOnly)
                    {
                        log.InfoFormat("GenerateServiceModuleData Data not valid, Valid: {0}", dataValid);
                        if (progressHandler != null)
                        {
                            progressHandler.Invoke(true, 100, convertFailures);
                            progressHandler.Invoke(false, 100, convertFailures);
                        }

                        return false;
                    }

                    log.InfoFormat("GenerateTestModuleData Converting test modules");
                    if (!AllowDbGeneration)
                    {
                        log.ErrorFormat("GenerateTestModuleData Started from DLL");
                        return false;
                    }

                    if (progressHandler != null)
                    {
                        if (progressHandler.Invoke(true, 0, 0))
                        {
                            log.ErrorFormat("GenerateTestModuleData Aborted");
                            return false;
                        }
                    }

                    testModules = ConvertAllTestModules(progressHandler);
                    if (testModules == null)
                    {
                        log.ErrorFormat("GenerateTestModuleData ConvertAllTestModules failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                        {
                            serializer.Serialize(writer, testModules);
                        }
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(testModulesZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(TestModulesXmlFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                TestModuleStorage = testModules;
                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("StoreTestModuleData Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }
        }

        public TestModules ConvertAllTestModules(ProgressDelegate progressHandler)
        {
            try
            {
                RestartRequired = true;

                ReadSwiRegister(null, null);
                List<SwiAction> swiActions = CollectSwiActionsForNode(SwiRegisterTree, true);
                if (swiActions == null)
                {
                    log.ErrorFormat("ConvertAllTestModules CollectSwiActionsForNode failed");
                    return null;
                }

                SerializableDictionary<string, TestModuleData> moduleDataDict = new SerializableDictionary<string, TestModuleData>();
                int failCount = 0;
                int index = 0;
                foreach (SwiAction swiAction in swiActions)
                {
                    if (progressHandler != null)
                    {
                        int percent = index * 100 / swiActions.Count;
                        if (progressHandler.Invoke(false, percent, failCount))
                        {
                            log.ErrorFormat("ConvertAllTestModules Aborted at {0}%", percent);
                            return null;
                        }
                    }

                    foreach (SwiInfoObj infoInfoObj in swiAction.SwiInfoObjs)
                    {
                        if (infoInfoObj.LinkType == SwiInfoObj.SwiActionDatabaseLinkType.SwiActionActionSelectionLink)
                        {
                            string moduleName = infoInfoObj.ModuleName;
                            string key = moduleName.ToUpperInvariant();
                            if (!moduleDataDict.ContainsKey(key))
                            {
                                TestModuleData moduleData = ReadTestModule(moduleName, out bool failure);
                                if (moduleData == null)
                                {
                                    log.ErrorFormat("ConvertAllTestModules ReadTestModule failed for: {0}", moduleName);
                                    if (failure)
                                    {
                                        log.ErrorFormat("ConvertAllTestModules ReadTestModule generation failure for: {0}", moduleName);
                                        failCount++;
                                    }
                                }
                                else
                                {
                                    moduleDataDict.Add(key, moduleData);
                                }
                            }
                        }
                    }

                    index++;
                }

                progressHandler?.Invoke(false, 100, failCount);

                log.InfoFormat("ConvertAllTestModules Count: {0}, Failures: {1}", moduleDataDict.Count, failCount);
                if (moduleDataDict.Count == 0)
                {
                    log.ErrorFormat("ConvertAllTestModules No test modules generated");
                    return null;
                }

                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                return new TestModules(versionInfo, moduleDataDict, failCount);
            }
            catch (Exception e)
            {
                log.ErrorFormat("ConvertAllTestModules Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public bool GenerateEcuCharacteristicsData()
        {
            try
            {
                EcuCharacteristicsData ecuCharacteristicsData = null;
                XmlSerializer serializer = new XmlSerializer(typeof(EcuCharacteristicsData));
                string ecuCharacteristicsZipFile = Path.Combine(_databaseExtractPath, EcuCharacteristicsZipFile);
                if (File.Exists(ecuCharacteristicsZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(ecuCharacteristicsZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, EcuCharacteristicsXmFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        ecuCharacteristicsData = serializer.Deserialize(reader) as EcuCharacteristicsData;
                                    }
                                }
                            }
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
                    catch (Exception e)
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Deserialize Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                    }
                }

                bool dataValid = true;
                if (ecuCharacteristicsData != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (ecuCharacteristicsData.Version == null || !ecuCharacteristicsData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Version mismatch");
                        dataValid = false;
                    }
                }

                if (ecuCharacteristicsData == null || !dataValid)
                {
                    log.InfoFormat("GenerateEcuCharacteristicsData Converting Xml");
                    if (!AllowDbGeneration)
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Started from DLL");
                        return false;
                    }

                    ecuCharacteristicsData = ReadEcuCharacteristicsXml();
                    if (ecuCharacteristicsData == null)
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData ReadEcuCharacteristicsXml failed");
                        return false;
                    }

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                        {
                            serializer.Serialize(writer, ecuCharacteristicsData);
                        }
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(ecuCharacteristicsZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(EcuCharacteristicsXmFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }

                EcuCharacteristicsStorage = ecuCharacteristicsData;
                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("GenerateEcuCharacteristicsData Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }
        }

        public EcuCharacteristicsData ReadEcuCharacteristicsXml()
        {
            try
            {
                SerializableDictionary<string, string> ecuXmlDict = new SerializableDictionary<string, string>();
                string diagnosticsFile = Path.Combine(_frameworkPath, "RheingoldDiagnostics.dll");
                if (!File.Exists(diagnosticsFile))
                {
                    log.ErrorFormat("ReadEcuCharacteristicsXml Diagnostics file not found: {0}", diagnosticsFile);
                    return null;
                }

                Assembly diagnosticsAssembly = Assembly.LoadFrom(diagnosticsFile);
                string[] resourceNames = diagnosticsAssembly.GetManifestResourceNames();
                foreach (string resourceName in resourceNames)
                {
                    log.InfoFormat("ReadEcuCharacteristicsXml Resource: {0}", resourceName);

                    string[] resourceParts = resourceName.Split('.');
                    if (resourceParts.Length < 2)
                    {
                        log.ErrorFormat("ReadEcuCharacteristicsXml Invalid resource parts: {0}", resourceParts.Length);
                        continue;
                    }

                    string fileName = resourceParts[resourceParts.Length - 2];
                    if (string.IsNullOrEmpty(fileName))
                    {
                        log.ErrorFormat("ReadEcuCharacteristicsXml Invalid file name: {0}", resourceName);
                        continue;
                    }

                    string fileExt = resourceParts[resourceParts.Length - 1];
                    if (string.Compare(fileExt, "xml", StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        continue;
                    }

                    using (Stream resourceStream = diagnosticsAssembly.GetManifestResourceStream(resourceName))
                    {
                        if (resourceStream == null)
                        {
                            log.ErrorFormat("ReadEcuCharacteristicsXml Reading stream failed for: {0}", resourceName);
                            continue;
                        }

                        using (StreamReader reader = new StreamReader(resourceStream))
                        {
                            string xmlContent = reader.ReadToEnd();
                            ecuXmlDict.Add(fileName.ToUpperInvariant(), xmlContent);
                        }
                    }
                }

                log.InfoFormat("ReadEcuCharacteristicsXml Resources: {0}", ecuXmlDict.Count);
                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                EcuCharacteristicsData ecuCharacteristicsData = new EcuCharacteristicsData(versionInfo, ecuXmlDict);
                return ecuCharacteristicsData;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ReadEcuCharacteristicsXml Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public bool SaveVehicleSeriesInfo(ClientContext clientContext, ProgressDelegate progressHandler)
        {
            try
            {
                VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = null;
                XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.VehicleSeriesInfoData));
                string vehicleSeriesZipFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.VehicleSeriesZipFile);
                if (File.Exists(vehicleSeriesZipFile))
                {
                    try
                    {
                        ZipFile zf = null;
                        try
                        {
                            FileStream fs = File.OpenRead(vehicleSeriesZipFile);
                            zf = new ZipFile(fs);
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    continue; // Ignore directories
                                }

                                if (string.Compare(zipEntry.Name, VehicleStructsBmw.VehicleSeriesXmlFile, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    Stream zipStream = zf.GetInputStream(zipEntry);
                                    using (TextReader reader = new StreamReader(zipStream))
                                    {
                                        vehicleSeriesInfoData = serializer.Deserialize(reader) as VehicleStructsBmw.VehicleSeriesInfoData;
                                    }
                                }
                            }
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
                    catch (Exception e)
                    {
                        log.ErrorFormat("SaveVehicleSeriesInfo Deserialize Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                    }
                }

                bool dataValid = true;
                if (vehicleSeriesInfoData != null)
                {
                    DbInfo dbInfo = GetDbInfo();
                    if (vehicleSeriesInfoData.Version == null || !vehicleSeriesInfoData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("GenerateEcuCharacteristicsData Version mismatch");
                        dataValid = false;
                    }
                }

                if (vehicleSeriesInfoData == null || !dataValid)
                {
                    if (progressHandler.Invoke(true))
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo ExtractVehicleSeriesInfo aborted");
                        return false;
                    }

                    vehicleSeriesInfoData = ExtractVehicleSeriesInfo(clientContext);
                    if (vehicleSeriesInfoData == null)
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo ExtractVehicleSeriesInfo failed");
                        return false;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo Saving: {0}", vehicleSeriesZipFile);
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t"
                        };
                        using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                        {
                            serializer.Serialize(writer, vehicleSeriesInfoData);
                        }
                        memStream.Seek(0, SeekOrigin.Begin);

                        FileStream fsOut = File.Create(vehicleSeriesZipFile);
                        ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                        zipStream.SetLevel(3);

                        try
                        {
                            ZipEntry newEntry = new ZipEntry(VehicleStructsBmw.VehicleSeriesXmlFile)
                            {
                                DateTime = DateTime.Now,
                                Size = memStream.Length
                            };
                            zipStream.PutNextEntry(newEntry);

                            byte[] buffer = new byte[4096];
                            StreamUtils.Copy(memStream, zipStream, buffer);
                            zipStream.CloseEntry();
                        }
                        finally
                        {
                            zipStream.IsStreamOwner = true;
                            zipStream.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveVehicleSeriesInfo Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return true;
        }

        public VehicleStructsBmw.VehicleSeriesInfoData ExtractVehicleSeriesInfo(ClientContext clientContext)
        {
            try
            {
                Regex dateFormulaRegex = new Regex(@"(RuleNum\(""Baustand""\))\s*([<>=]+)\s*([0-9]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Regex ecuCliqueFormulaRegex = new Regex(@"IsValidRuleString\(""EcuClique""\s*,\s*""([a-zA-Z0-9_\-]+)""\s*\)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                RuleExpression.FormulaConfig formulaConfig = new RuleExpression.FormulaConfig("RuleString", "RuleNum", "IsValidRuleString", "IsValidRuleNum", "IsFaultRuleValid", false, null, "|");

                List<string> typeKeys = GetAllTypeKeys();
                if (typeKeys == null)
                {
                    log.ErrorFormat("ExtractEcuCharacteristicsVehicles No TypeKeys");
                    return null;
                }

                IDiagnosticsBusinessData service = ServiceLocator.Current.GetService<IDiagnosticsBusinessData>();
                List<BordnetsData> boardnetsList = GetAllBordnetRules();
                Dictionary<string, Tuple<Vehicle, List<string>>> vehicleTypeKeyHashes = new Dictionary<string, Tuple<Vehicle, List<string>>>();

                foreach (string typeKey in typeKeys)
                {
                    List<Characteristics> characteristicsList = GetVehicleIdentByTypeKey(typeKey, false);
                    if (characteristicsList != null)
                    {
                        Vehicle vehicleIdent = new Vehicle(clientContext);
                        vehicleIdent.VehicleIdentLevel = IdentificationLevel.VINBasedFeatures;
                        vehicleIdent.VINRangeType = typeKey;
                        vehicleIdent.VCI.VCIType = VCIDeviceType.EDIABAS;
                        vehicleIdent.Modelljahr = "2100";
                        vehicleIdent.Modellmonat = "01";
                        vehicleIdent.Modelltag = "01";

                        if (!PsdzContext.UpdateAllVehicleCharacteristics(characteristicsList, this, vehicleIdent))
                        {
                            log.ErrorFormat("ExtractEcuCharacteristicsVehicles UpdateAllVehicleCharacteristics failed");
                        }

                        string series = vehicleIdent.Ereihe;
                        string modelSeries = vehicleIdent.Baureihenverbund;
                        string productType = vehicleIdent.Prodart;
                        string productLine = vehicleIdent.Produktlinie;

                        if (string.IsNullOrEmpty(series))
                        {
                            log.ErrorFormat("ExtractEcuCharacteristicsVehicles Vehicle series missing");
                            continue;
                        }

                        string vehicleHash = (series ?? string.Empty) + ";" + (modelSeries ?? string.Empty) + ";" + (productType ?? string.Empty) + ";" + (productLine ?? string.Empty);
                        if (!vehicleTypeKeyHashes.TryGetValue(vehicleHash, out Tuple<Vehicle, List<string>> typeKeyTuple))
                        {
                            vehicleTypeKeyHashes.Add(vehicleHash, new Tuple<Vehicle, List<string>>(vehicleIdent, new List<string> { typeKey }));
                        }
                        else
                        {
                            typeKeyTuple.Item2.Add(typeKey);
                        }
                    }
                }

                List<EcuCharacteristicsInfo> vehicleSeriesList = new List<EcuCharacteristicsInfo>();
                foreach (KeyValuePair<string, Tuple<Vehicle, List<string>>> keyValuePair in vehicleTypeKeyHashes)
                {
                    Vehicle vehicleIdent = keyValuePair.Value.Item1;
                    List<EcuCharacteristicsMatch> validCharacteristics = new List<EcuCharacteristicsMatch>();

                    foreach (BordnetsData bordnetsData in boardnetsList)
                    {
                        List<RuleDate> ruleDates = new List<RuleDate>();
                        List<Tuple<EcuVar, EcuGroup>> ruleEcus = new List<Tuple<EcuVar, EcuGroup>>();
                        BaseEcuCharacteristics baseEcuCharacteristics = null;
                        if (bordnetsData.DocData != null)
                        {
                            baseEcuCharacteristics = VehicleLogistics.CreateCharacteristicsInstance<GenericEcuCharacteristics>(vehicleIdent, bordnetsData.DocData, bordnetsData.InfoObjIdent);
                        }

                        if (baseEcuCharacteristics != null && bordnetsData.XepRule != null)
                        {
                            string ruleFormulaStd = bordnetsData.XepRule.GetRuleFormula(vehicleIdent);
                            string ruleFormulaRegEx = bordnetsData.XepRule.GetRuleFormula(vehicleIdent, formulaConfig);
                            string[] formulaParts = ruleFormulaRegEx.Split('|');

                            foreach (string formulaPart in formulaParts)
                            {
                                MatchCollection dateMatches = dateFormulaRegex.Matches(formulaPart);
                                foreach (Match match in dateMatches)
                                {
                                    if (match.Groups.Count == 4 && match.Groups[2].Success && match.Groups[3].Success)
                                    {
                                        string date = match.Groups[3].Value.Trim();
                                        string dateCompare = match.Groups[2].Value.Trim();
                                        if (!string.IsNullOrEmpty(date) && date.Length == 6 && !string.IsNullOrEmpty(dateCompare))
                                        {
                                            RuleDate ruleDate = new RuleDate(date, dateCompare);
                                            if (ruleDate.GetValue() != 0)
                                            {
                                                ruleDates.Add(ruleDate);
                                            }
                                        }
                                    }
                                }

                                MatchCollection ecuCliqueMatches = ecuCliqueFormulaRegex.Matches(formulaPart);
                                foreach (Match match in ecuCliqueMatches)
                                {
                                    if (match.Groups.Count == 2 && match.Groups[1].Success)
                                    {
                                        string ecuName = match.Groups[1].Value.Trim();
                                        if (!string.IsNullOrEmpty(ecuName))
                                        {
                                            bool ecuFound = false;
                                            EcuVar ecuVar = GetEcuVariantByName(ecuName);
                                            if (ecuVar != null)
                                            {
                                                EcuGroup ecuGroup = GetEcuGroupById(ecuVar.EcuGroupId);
                                                if (ecuGroup != null)
                                                {
                                                    ecuFound = true;
                                                    if (ruleEcus.All(x => string.Compare(x.Item1.Name, ecuName, StringComparison.OrdinalIgnoreCase) != 0))
                                                    {
                                                        ruleEcus.Add(new Tuple<EcuVar, EcuGroup>(ecuVar, ecuGroup));
                                                    }
                                                }
                                            }

                                            if (!ecuFound)
                                            {
                                                log.InfoFormat("ExtractEcuCharacteristicsVehicles ECU not found : {0}", ecuName);
                                            }
                                        }
                                    }
                                }
                            }

                            StringBuilder sbRuleEcus = new StringBuilder();
                            foreach (Tuple<EcuVar, EcuGroup> tupleEcu in ruleEcus)
                            {
                                if (sbRuleEcus.Length > 0)
                                {
                                    sbRuleEcus.Append(", ");
                                }

                                sbRuleEcus.Append("[Var='");
                                sbRuleEcus.Append(tupleEcu.Item1.Name);
                                sbRuleEcus.Append("'");
                                sbRuleEcus.Append(" Group=");
                                sbRuleEcus.Append(tupleEcu.Item2.Name);
                                sbRuleEcus.Append("']");
                            }

                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Rule dates: {0}, Rule ECUS: {1}", ruleDates.ToStringItems(), sbRuleEcus);

                            ObservableCollection<ECU> ecuListOrg = new ObservableCollection<ECU>();
                            ObservableCollection<ECU> ecuList1 = new ObservableCollection<ECU>();
                            ObservableCollection<ECU> ecuList2 = new ObservableCollection<ECU>();
                            ObservableCollection<ECU> ecuList3 = new ObservableCollection<ECU>();
                            List<string> ruleEcusUsed = new List<string>();
                            int maxEcuList = 1;
                            foreach (IEcuLogisticsEntry ecuLogisticsEntry in baseEcuCharacteristics.ecuTable)
                            {
                                ECU ecuOrg = new ECU();
                                ecuOrg.ID_SG_ADR = ecuLogisticsEntry.DiagAddress;
                                ecuOrg.ECU_NAME = ecuLogisticsEntry.Name;
                                ecuOrg.ECU_GRUPPE = ecuLogisticsEntry.GroupSgbd;
                                ecuListOrg.Add(ecuOrg);

                                ECU ecu = new ECU();
                                ecu.ID_SG_ADR = ecuOrg.ID_SG_ADR;
                                ecu.ECU_NAME = ecuOrg.ECU_NAME;
                                ecu.ECU_GRUPPE = ecuOrg.ECU_GRUPPE;

                                List<string> ecuNamesAdd = new List<string>();
                                if (!string.IsNullOrEmpty(ecuLogisticsEntry.GroupSgbd))
                                {
                                    string[] groupArray = ecuLogisticsEntry.GroupSgbd.Split('|');
                                    foreach (Tuple<EcuVar, EcuGroup> tupleEcu in ruleEcus)
                                    {
                                        foreach (string groupName in groupArray)
                                        {
                                            if (string.Compare(groupName, tupleEcu.Item2.Name, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                if (!ecuNamesAdd.Contains(tupleEcu.Item1.Name, StringComparer.OrdinalIgnoreCase))
                                                {
                                                    ecuNamesAdd.Add(tupleEcu.Item1.Name);
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach (string ecuName in ecuNamesAdd)
                                {
                                    if (!ruleEcusUsed.Contains(ecuName, StringComparer.OrdinalIgnoreCase))
                                    {
                                        ruleEcusUsed.Add(ecuName);
                                    }
                                }

                                if (ecuNamesAdd.Count > 0)
                                {
                                    ecu.ECU_SGBD = ecuNamesAdd[0];
                                }
                                ecuList1.Add(ecu);

                                if (ecuNamesAdd.Count > 1)
                                {
                                    ECU ecuExtra = new ECU();
                                    ecuExtra.ID_SG_ADR = ecu.ID_SG_ADR;
                                    ecuExtra.ECU_NAME = ecu.ECU_NAME;
                                    ecuExtra.ECU_GRUPPE = ecu.ECU_GRUPPE;
                                    ecuExtra.ECU_SGBD = ecuNamesAdd[1];
                                    ecuList2.Add(ecuExtra);
                                    if (maxEcuList < 2)
                                    {
                                        maxEcuList = 2;
                                    }
                                }
                                else
                                {
                                    ecuList2.Add(ecu);
                                }

                                if (ecuNamesAdd.Count > 2)
                                {
                                    ECU ecuExtra = new ECU();
                                    ecuExtra.ID_SG_ADR = ecu.ID_SG_ADR;
                                    ecuExtra.ECU_NAME = ecu.ECU_NAME;
                                    ecuExtra.ECU_GRUPPE = ecu.ECU_GRUPPE;
                                    ecuExtra.ECU_SGBD = ecuNamesAdd[2];
                                    ecuList3.Add(ecuExtra);
                                    if (maxEcuList < 3)
                                    {
                                        maxEcuList = 3;
                                    }
                                }
                                else
                                {
                                    ecuList3.Add(ecu);
                                }
                            }

                            if (ruleEcus.Count != ruleEcusUsed.Count)
                            {
                                log.ErrorFormat("ExtractEcuCharacteristicsVehicles Not all ECUs used: {0}, Used: {1}", sbRuleEcus, ruleEcusUsed.ToStringItems());
                            }

                            List<ObservableCollection<ECU>> ecuListCollection = new List<ObservableCollection<ECU>>
                            {
                                ecuListOrg, ecuList1
                            };

                            if (maxEcuList >= 2)
                            {
                                ecuListCollection.Add(ecuList2);
                            }

                            if (maxEcuList >= 3)
                            {
                                ecuListCollection.Add(ecuList3);
                            }

                            RuleDate ruleDateUse = null;
                            ObservableCollection<ECU> ecuListUse = null;
                            bool ruleValid = false;
                            for (int ecuListIndex = 0; ecuListIndex < ecuListCollection.Count; ecuListIndex++)
                            {
                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Trying ecu list index: {0}", ecuListIndex);
                                vehicleIdent.ECU = ecuListCollection[ecuListIndex];

                                ruleDateUse = null;
                                int ruleDateIndex = 0;
                                for (; ; )
                                {
                                    if (ruleDates.Count > ruleDateIndex)
                                    {
                                        ruleDateUse = ruleDates[ruleDateIndex];
                                    }

                                    if (ruleDateUse != null)
                                    {
                                        vehicleIdent.Modelljahr = ruleDateUse.GetYear(-1);
                                        vehicleIdent.Modellmonat = ruleDateUse.GetMonth(-1);
                                    }

                                    bordnetsData.XepRule.ResetResult();
                                    ruleValid = bordnetsData.XepRule.EvaluateRule(vehicleIdent, null);
                                    if (!ruleValid && ruleDateUse != null)
                                    {
                                        vehicleIdent.Modelljahr = ruleDateUse.GetYear(1);
                                        vehicleIdent.Modellmonat = ruleDateUse.GetMonth(1);

                                        bordnetsData.XepRule.ResetResult();
                                        ruleValid = bordnetsData.XepRule.EvaluateRule(vehicleIdent, null);
                                        if (ruleValid)
                                        {
                                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Date required: {0}-{1} for formula: {2}", vehicleIdent.Modelljahr, vehicleIdent.Modellmonat, ruleFormulaStd);
                                        }
                                    }

                                    if (ruleValid)
                                    {
                                        break;
                                    }

                                    ruleDateIndex++;
                                    if (ruleDateIndex >= ruleDates.Count)
                                    {
                                        break;
                                    }

                                    log.InfoFormat("ExtractEcuCharacteristicsVehicles Trying date index: {0}", ruleDateIndex);
                                }

                                if (ruleValid)
                                {
                                    ecuListUse = vehicleIdent.ECU;
                                    break;
                                }
                            }

                            if (ruleValid)
                            {
                                List<VehicleStructsBmw.VehicleEcuInfo> usedEcus = new List<VehicleStructsBmw.VehicleEcuInfo>();
                                if (ecuListUse != null)
                                {
                                    foreach (ECU ecu in ecuListUse)
                                    {
                                        if (!string.IsNullOrEmpty(ecu.ECU_SGBD))
                                        {
                                            if (usedEcus.All(x => string.Compare(x.Name, ecu.ECU_SGBD, StringComparison.OrdinalIgnoreCase) != 0))
                                            {
                                                usedEcus.Add(new VehicleStructsBmw.VehicleEcuInfo((int)ecu.ID_SG_ADR, ecu.ECU_NAME, ecu.ECU_GRUPPE, ecu.ECU_SGBD));
                                            }
                                        }
                                    }
                                }

                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Boardnets rule valid: ECUs: {0}, rule: {1}", usedEcus.ToStringItems(), ruleFormulaStd);
                                validCharacteristics.Add(new EcuCharacteristicsMatch(baseEcuCharacteristics, ruleDateUse, usedEcus, ruleFormulaStd));
                            }
                            else
                            {
                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Boardnets rule not valid: rule: {0}", ruleFormulaStd);
                            }
                        }
                    }

                    if (validCharacteristics.Count >= 1)
                    {
                        log.InfoFormat("ExtractEcuCharacteristicsVehicles Characteristics: ER={0}, BR={1}, Brand={2}", vehicleIdent.Ereihe, vehicleIdent.Baureihenverbund, vehicleIdent.Marke);
                        if (validCharacteristics.Count > 1)
                        {
                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Multiple characteristicts found: Count={0}", validCharacteristics.Count);
                            foreach (EcuCharacteristicsMatch characteristicsMatch in validCharacteristics)
                            {
                                string ruleDateString = characteristicsMatch.RuleDate?.ToString() ?? string.Empty;
                                List<VehicleStructsBmw.VehicleEcuInfo> ruleEcus = characteristicsMatch.RuleEcus;
                                string ruleFormula = characteristicsMatch.RuleFormula;
                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Match ECUs: {0}, Date: {1}, Rule: {2}", ruleEcus.ToStringItems(), ruleDateString, ruleFormula);
                            }
                        }

                        foreach (EcuCharacteristicsMatch characteristicsMatch in validCharacteristics)
                        {
                            string series = vehicleIdent.Ereihe;
                            string modelSeries = vehicleIdent.Baureihenverbund;
                            string brandName = vehicleIdent.BrandName?.ToString();
                            RuleDate ruleDate = characteristicsMatch.RuleDate;
                            string ruleFormula = characteristicsMatch.RuleFormula;
                            List<VehicleStructsBmw.VehicleEcuInfo> ruleEcus = characteristicsMatch.RuleEcus;

                            BNType bnType = service.GetBNType(vehicleIdent);
                            string sgbdAdd = service.GetMainSeriesSgbdAdditional(vehicleIdent);

                            vehicleSeriesList.Add(new EcuCharacteristicsInfo(characteristicsMatch.EcuCharacteristics, series, modelSeries, bnType, brandName, sgbdAdd, ruleDate, ruleEcus, ruleFormula));
                        }
                    }
                    else
                    {
                        bool ignoreMissing = false;
                        if (!string.IsNullOrEmpty(vehicleIdent.Ereihe))
                        {
                            switch (vehicleIdent.Ereihe.ToUpperInvariant())
                            {
                                // cars
                                case "E30":
                                case "E31":
                                case "E32":
                                case "E34":
                                case "U06":

                                // motorbikes
                                case "246":
                                case "247":
                                case "247E":
                                case "248":
                                case "259":
                                case "259C":
                                case "259R":
                                case "259S":
                                case "C01":
                                case "EXX":
                                case "E169":
                                case "R13":
                                case "R21":
                                case "R22":
                                case "R28":
                                case "K14":
                                case "K15":
                                case "K16":
                                case "K30":
                                case "K41":
                                case "K569":
                                case "K589":
                                    ignoreMissing = true;
                                    break;
                            }
                        }

                        if (!ignoreMissing)
                        {
                            log.ErrorFormat("ExtractEcuCharacteristicsVehicles No characteristics for: ER={0}, BR={1}, Brand={2}", vehicleIdent.Ereihe, vehicleIdent.Baureihenverbund, vehicleIdent.Marke);
                        }
                    }
                }

                SerializableDictionary<string, List<VehicleStructsBmw.VehicleSeriesInfo>> sgbdDict = new SerializableDictionary<string, List<VehicleStructsBmw.VehicleSeriesInfo>>();
                foreach (EcuCharacteristicsInfo ecuCharacteristicsInfo in vehicleSeriesList)
                {
                    BaseEcuCharacteristics ecuCharacteristics = ecuCharacteristicsInfo.EcuCharacteristics;
                    string brSgbd = ecuCharacteristics.brSgbd.Trim().ToUpperInvariant();
                    BNType? bnType = ecuCharacteristicsInfo.BnType;

                    string bnTypeName = null;
                    if (bnType.HasValue)
                    {
                        bnTypeName = bnType.Value.ToString();
                    }

                    List<VehicleStructsBmw.VehicleEcuInfo> ecuList = new List<VehicleStructsBmw.VehicleEcuInfo>();
                    foreach (IEcuLogisticsEntry ecuLogisticsEntry in ecuCharacteristics.ecuTable)
                    {
                        ecuList.Add(new VehicleStructsBmw.VehicleEcuInfo(ecuLogisticsEntry.DiagAddress, ecuLogisticsEntry.Name, ecuLogisticsEntry.GroupSgbd));
                    }

                    string series = ecuCharacteristicsInfo.Series;
                    string modelSeries = ecuCharacteristicsInfo.ModelSeries;
                    string ruleDate = ecuCharacteristicsInfo.RuleDate?.Date;
                    string ruleDateCompare = ecuCharacteristicsInfo.RuleDate?.DateCompare;
                    string ruleFormula = ecuCharacteristicsInfo.RuleFormula;
                    List<VehicleStructsBmw.VehicleEcuInfo> ruleEcus = ecuCharacteristicsInfo.RuleEcus;
                    if (string.IsNullOrEmpty(series))
                    {
                        log.ErrorFormat("ExtractEcuCharacteristicsVehicles Series missing for ModelsSeries: {0}", modelSeries);
                        continue;
                    }

                    string key = series.Trim().ToUpperInvariant();
                    VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfoAdd = new VehicleStructsBmw.VehicleSeriesInfo(
                        series, modelSeries, brSgbd, ecuCharacteristicsInfo.SgbdAdd, bnTypeName, ecuCharacteristicsInfo.Brand, ruleDate, ruleDateCompare, ruleEcus, ecuList, ruleFormula);

                    if (sgbdDict.TryGetValue(key, out List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList))
                    {
                        VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfoMatch = null;
                        foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo in vehicleSeriesInfoList)
                        {
                            // ReSharper disable once ReplaceWithSingleAssignment.True
                            bool identical = true;
                            if (string.Compare(vehicleSeriesInfo.BrSgbd ?? string.Empty, vehicleSeriesInfoAdd.BrSgbd ?? string.Empty, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                identical = false;
                            }

                            if (string.Compare(vehicleSeriesInfo.SgbdAdd ?? string.Empty, vehicleSeriesInfoAdd.SgbdAdd ?? string.Empty, StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                identical = false;
                            }

                            if (vehicleSeriesInfo.EcuList.Count != vehicleSeriesInfoAdd.EcuList.Count)
                            {
                                identical = false;
                            }
                            else
                            {
                                for (int ecu = 0; ecu < vehicleSeriesInfo.EcuList.Count; ecu++)
                                {
                                    VehicleStructsBmw.VehicleEcuInfo ecuInfo1 = vehicleSeriesInfo.EcuList[ecu];
                                    VehicleStructsBmw.VehicleEcuInfo ecuInfo2 = vehicleSeriesInfoAdd.EcuList[ecu];

                                    if (ecuInfo1.DiagAddr != ecuInfo2.DiagAddr)
                                    {
                                        identical = false;
                                        break;
                                    }

                                    if (string.Compare(ecuInfo1.Name ?? string.Empty, ecuInfo2.Name ?? string.Empty, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        identical = false;
                                        break;
                                    }

                                    if (string.Compare(ecuInfo1.GroupSgbd ?? string.Empty, ecuInfo2.GroupSgbd ?? string.Empty, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        identical = false;
                                        break;
                                    }
                                }
                            }

                            if (identical)
                            {
                                vehicleSeriesInfoMatch = vehicleSeriesInfo;
                                break;
                            }
                        }

                        if (vehicleSeriesInfoMatch == null)
                        {
                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Multiple entries for Series: {0}", series);
                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Add: ModelSeries: {0}, Sgbd: {1}, Brand: {2}, Rule ECUs: '{3}', Rule Date: '{4}'",
                                vehicleSeriesInfoAdd.ModelSeries, vehicleSeriesInfoAdd.BrSgbd, vehicleSeriesInfoAdd.Brand ?? string.Empty, vehicleSeriesInfoAdd.RuleEcus.ToStringItems(), vehicleSeriesInfoAdd.Date ?? string.Empty);
                            log.InfoFormat("ExtractEcuCharacteristicsVehicles Rule formula: '{0}'",
                                vehicleSeriesInfoAdd.RuleFormula);
                            foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfoLog in vehicleSeriesInfoList)
                            {
                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Exist: ModelSeries: {0}, Sgbd: {1}, Brand: {2}, Rule ECUs: '{3}', Rule Date: '{4}'",
                                    vehicleSeriesInfoLog.ModelSeries, vehicleSeriesInfoLog.BrSgbd, vehicleSeriesInfoLog.Brand ?? string.Empty, vehicleSeriesInfoLog.RuleEcus.ToStringItems(), vehicleSeriesInfoLog.Date ?? string.Empty);
                                log.InfoFormat("ExtractEcuCharacteristicsVehicles Rule formula: '{0}'",
                                    vehicleSeriesInfoLog.RuleFormula);
                            }
                            vehicleSeriesInfoList.Add(vehicleSeriesInfoAdd);
                        }
                    }
                    else
                    {
                        sgbdDict.Add(key, new List<VehicleStructsBmw.VehicleSeriesInfo> { vehicleSeriesInfoAdd });
                    }
                }

                foreach (KeyValuePair<string, List<VehicleStructsBmw.VehicleSeriesInfo>> keyValue in sgbdDict)
                {
                    List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList = keyValue.Value;
                    if (vehicleSeriesInfoList.Count == 1)
                    {
                        vehicleSeriesInfoList[0].ResetDate();
                    }
                }

                string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                DbInfo dbInfo = GetDbInfo();
                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                VehicleStructsBmw.VehicleSeriesInfoData vehicleSeriesInfoData = new VehicleStructsBmw.VehicleSeriesInfoData(timeStamp, versionInfo, sgbdDict);
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<string, List<VehicleStructsBmw.VehicleSeriesInfo>> keyValue in vehicleSeriesInfoData.VehicleSeriesDict.OrderBy(x => x.Key))
                {
                    List<VehicleStructsBmw.VehicleSeriesInfo> vehicleSeriesInfoList = keyValue.Value;
                    foreach (VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo in vehicleSeriesInfoList)
                    {
                        sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[{0}, {1}, '{2}' {3} {4}]",
                            vehicleSeriesInfo.BrSgbd, vehicleSeriesInfo.Series, vehicleSeriesInfo.Brand, vehicleSeriesInfo.DateCompare ?? string.Empty, vehicleSeriesInfo.Date ?? string.Empty));
                    }
                }

                log.InfoFormat("ExtractEcuCharacteristicsVehicles Count: {0}", vehicleSeriesInfoData.VehicleSeriesDict.Count);
                log.Info(Environment.NewLine + sb);
                return vehicleSeriesInfoData;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractEcuCharacteristicsVehicles Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public bool SaveFaultRulesInfo(ClientContext clientContext, ProgressDelegate progressHandler)
        {
            try
            {
                string rulesZipFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.RulesZipFile);
                string rulesCsFile = Path.Combine(_databaseExtractPath, VehicleStructsBmw.RulesCsFile);
                VehicleStructsBmw.RulesInfoData rulesInfoData = null;
                if (File.Exists(rulesZipFile) && File.Exists(rulesCsFile))
                {
                    rulesInfoData = VehicleInfoBmw.ReadRulesInfoFromFile(_databaseExtractPath);
                }

                DbInfo dbInfo = GetDbInfo();
                bool dataValid = true;
                if (rulesInfoData != null)
                {
                    if (rulesInfoData.Version == null || !rulesInfoData.Version.IsIdentical(dbInfo?.Version, dbInfo?.DateTime))
                    {
                        log.ErrorFormat("SaveFaultRulesInfo Version mismatch");
                        dataValid = false;
                    }
                }

                if (rulesInfoData != null && dataValid)
                {
                    return true;
                }

                if (progressHandler.Invoke(true))
                {
                    log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo ExtractFaultRulesInfo aborted");
                    return false;
                }

                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> faultRulesDict = ExtractFaultRulesInfo(clientContext);
                if (faultRulesDict == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo ExtractFaultRulesInfo failed");
                    return false;
                }

                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ecuFuncRulesDict = ExtractEcuFuncRulesInfo(clientContext);
                if (ecuFuncRulesDict == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo ExtractEcuFuncRulesInfo failed");
                    return false;
                }

                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> diagObjectRulesDict = ExtractDiagObjRulesInfo(clientContext);
                if (diagObjectRulesDict == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo ExtractDiagObjRulesInfo failed");
                    return false;
                }

                VehicleStructsBmw.VersionInfo versionInfo = new VehicleStructsBmw.VersionInfo(dbInfo?.Version, dbInfo?.DateTime);
                rulesInfoData = new VehicleStructsBmw.RulesInfoData(versionInfo, faultRulesDict, ecuFuncRulesDict, diagObjectRulesDict);
                if (!SaveRulesClass(rulesInfoData, rulesCsFile))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo SaveFaultRulesFunction failed");
                    return false;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo Saving: {0}", rulesZipFile);

                using (MemoryStream memStream = new MemoryStream())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(VehicleStructsBmw.RulesInfoData));
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "\t"
                    };
                    using (XmlWriter writer = XmlWriter.Create(memStream, settings))
                    {
                        serializer.Serialize(writer, rulesInfoData);
                    }
                    memStream.Seek(0, SeekOrigin.Begin);

                    FileStream fsOut = File.Create(rulesZipFile);
                    ZipOutputStream zipStream = new ZipOutputStream(fsOut);
                    zipStream.SetLevel(3);

                    try
                    {
                        ZipEntry newEntry = new ZipEntry(VehicleStructsBmw.RulesXmlFile)
                        {
                            DateTime = DateTime.Now,
                            Size = memStream.Length
                        };
                        zipStream.PutNextEntry(newEntry);

                        byte[] buffer = new byte[4096];
                        StreamUtils.Copy(memStream, zipStream, buffer);
                        zipStream.CloseEntry();
                    }
                    finally
                    {
                        zipStream.IsStreamOwner = true;
                        zipStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return true;
        }

        private SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ExtractDiagObjRulesInfo(ClientContext clientContext)
        {
            try
            {
                List<SwiDiagObj> diagObjsNodeClass = GetInfoObjectsTreeForNodeclassName(DiagObjServiceRoot, null, new List<string> { AblFilter });
                if (diagObjsNodeClass == null)
                {
                    log.ErrorFormat("ExtractDiagObjRulesInfo GetInfoObjectsTreeForNodeclassName failed");
                    return null;
                }

                List<SwiInfoObj> completeInfoObjects = new List<SwiInfoObj>();
                foreach (SwiDiagObj swiDiagObj in diagObjsNodeClass)
                {
                    completeInfoObjects.AddRange(swiDiagObj.CompleteInfoObjects);
                }

                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> diagObjectRulesDict = new SerializableDictionary<string, VehicleStructsBmw.RuleInfo>();
                Vehicle vehicle = new Vehicle(clientContext);
                List<string> subRuleIDs = new List<string>();
                foreach (SwiInfoObj infoObject in completeInfoObjects)
                {
                    string infoObjId = infoObject.Id;
                    if (infoObjId.ConvertToInt() > 0)
                    {
                        if (!diagObjectRulesDict.ContainsKey(infoObjId))
                        {
                            XepRule xepRule = GetRuleById(infoObjId);
                            if (xepRule != null)
                            {
                                string ruleFormula = xepRule.GetRuleFormula(vehicle, null, subRuleIDs);
                                if (!string.IsNullOrEmpty(ruleFormula))
                                {
                                    diagObjectRulesDict.Add(infoObjId, new VehicleStructsBmw.RuleInfo(infoObjId, ruleFormula));
                                }
                            }
                        }
                    }

                    foreach (SwiDiagObj diagObj in infoObject.DiagObjectPath)
                    {
                        string diagObjId = diagObj.Id;
                        if (diagObjId.ConvertToInt() > 0)
                        {
                            if (!diagObjectRulesDict.ContainsKey(diagObjId))
                            {
                                XepRule xepRule = GetRuleById(diagObjId);
                                if (xepRule != null)
                                {
                                    string ruleFormula = xepRule.GetRuleFormula(vehicle, null, subRuleIDs);
                                    if (!string.IsNullOrEmpty(ruleFormula))
                                    {
                                        diagObjectRulesDict.Add(diagObjId, new VehicleStructsBmw.RuleInfo(diagObjId, ruleFormula));
                                    }
                                }
                            }
                        }
                    }
                }

                if (!AddSubRules(vehicle, diagObjectRulesDict, subRuleIDs))
                {
                    log.ErrorFormat("ExtractDiagObjRulesInfo AddSubRules failed");
                    return null;
                }

                return diagObjectRulesDict;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractDiagObjRulesInfo Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public bool SaveRulesClass(VehicleStructsBmw.RulesInfoData rulesInfoData, string fileName)
        {
            try
            {
                log.InfoFormat(CultureInfo.InvariantCulture, "SaveRulesClass Saving: {0}", fileName);

                if (rulesInfoData == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveRulesClass faultRulesInfoData missing");
                    return false;
                }

                List<string> ruleNames = new List<string>();
                if (!ExtractRuleNames(rulesInfoData.FaultRuleDict, ruleNames))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveRulesClass ExtractRuleNames FaultRuleDict failed");
                    return false;
                }

                if (!ExtractRuleNames(rulesInfoData.EcuFuncRuleDict, ruleNames))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveRulesClass ExtractRuleNames EcuFuncRuleDict failed");
                    return false;
                }

                if (!ExtractRuleNames(rulesInfoData.DiagObjectRuleDict, ruleNames))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveRulesClass ExtractRuleNames DiagObjectRuleDict failed");
                    return false;
                }

                StringBuilder sbRuleNames = new StringBuilder();
                foreach (string ruleName in ruleNames)
                {
                    if (sbRuleNames.Length > 0)
                    {
                        sbRuleNames.Append(", ");
                    }

                    sbRuleNames.Append("\"");
                    sbRuleNames.Append(VehicleInfoBmw.RemoveNonAsciiChars(ruleName));
                    sbRuleNames.Append("\"");
                }

                DbInfo dbInfo = GetDbInfo();
                if (dbInfo == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "SaveRulesClass GetDbInfo failed");
                    return false;
                }

                List<VehicleStructsBmw.RuleInfo> faultRuleList = rulesInfoData.FaultRuleDict.Values.ToList();
                List<VehicleStructsBmw.RuleInfo> faultRuleListOrder =  faultRuleList.OrderBy(x => x.RuleFormula).ToList();

                List<VehicleStructsBmw.RuleInfo> ecuFuncRuleList = rulesInfoData.EcuFuncRuleDict.Values.ToList();
                List<VehicleStructsBmw.RuleInfo> ecuFuncRuleListOrder = ecuFuncRuleList.OrderBy(x => x.RuleFormula).ToList();

                List<VehicleStructsBmw.RuleInfo> diagObjectRuleList = rulesInfoData.DiagObjectRuleDict.Values.ToList();
                List<VehicleStructsBmw.RuleInfo> diagObjectRuleListOrder = diagObjectRuleList.OrderBy(x => x.RuleFormula).ToList();
                Dictionary<string, VehicleStructsBmw.RuleInfo> rulesFuncDict = new Dictionary<string, VehicleStructsBmw.RuleInfo>();

                StringBuilder sb = new StringBuilder();
                sb.Append(
$@"using BmwFileReader;
using System.Collections.Generic;

// This file is auto generated by PsdzClient, do not edit manually, changes will be lost.
// Edit {GetCallingSourceFileName()} method {GetCallingMemberName()} instead.

public class RulesInfo
{{
    public const string DatabaseVersion = ""{dbInfo.Version}"";

    public const string DatabaseDate = ""{dbInfo.DateTime.ToString(CultureInfo.InvariantCulture)}"";

    public static List<string> RuleNames = new List<string> {{ {sbRuleNames} }};
");

                sb.Append(
@"
    public RuleEvalBmw RuleEvalClass { get; private set; }

    private delegate bool RuleDelegate();

    private readonly Dictionary<ulong, RuleDelegate> _faultRuleDict;

    private readonly Dictionary<ulong, RuleDelegate> _ecuFuncRuleDict;

    private readonly Dictionary<ulong, RuleDelegate> _diagObjectRuleDict;

    public RulesInfo(RuleEvalBmw ruleEvalBmw)
    {
        RuleEvalClass = ruleEvalBmw;

        _faultRuleDict = new Dictionary<ulong, RuleDelegate>()
        {
");
                {
                    List<VehicleStructsBmw.RuleInfo> ruleList = faultRuleListOrder;
                    string funcNameLast = null;
                    VehicleStructsBmw.RuleInfo ruleInfoLast = null;
                    foreach (VehicleStructsBmw.RuleInfo ruleInfo in ruleList)
                    {
                        if (ruleInfoLast != null && string.Compare(ruleInfo.RuleFormula, ruleInfoLast.RuleFormula, StringComparison.Ordinal) != 0)
                        {
                            funcNameLast = null;
                        }

                        if (funcNameLast == null)
                        {
                            funcNameLast = "FaultRule_" + ruleInfo.Id.Trim();
                        }

                        if (!rulesFuncDict.ContainsKey(funcNameLast))
                        {
                            rulesFuncDict.Add(funcNameLast, ruleInfo);
                        }

                        sb.Append(
$@"            {{{ruleInfo.Id.Trim()}, {funcNameLast}}},
");
                        ruleInfoLast = ruleInfo;
                    }
                }

                sb.Append(
@"      };

        _ecuFuncRuleDict = new Dictionary<ulong, RuleDelegate>()
        {
");
                {
                    List<VehicleStructsBmw.RuleInfo> ruleList = ecuFuncRuleListOrder;
                    string funcNameLast = null;
                    VehicleStructsBmw.RuleInfo ruleInfoLast = null;
                    foreach (VehicleStructsBmw.RuleInfo ruleInfo in ruleList)
                    {
                        if (ruleInfoLast != null && string.Compare(ruleInfo.RuleFormula, ruleInfoLast.RuleFormula, StringComparison.Ordinal) != 0)
                        {
                            funcNameLast = null;
                        }

                        if (funcNameLast == null)
                        {
                            funcNameLast = "EcuFuncRule_" + ruleInfo.Id.Trim();
                        }

                        if (!rulesFuncDict.ContainsKey(funcNameLast))
                        {
                            rulesFuncDict.Add(funcNameLast, ruleInfo);
                        }

                        sb.Append(
$@"            {{{ruleInfo.Id.Trim()}, {funcNameLast}}},
");

                        ruleInfoLast = ruleInfo;
                    }
                }

                sb.Append(
@"      };

        _diagObjectRuleDict = new Dictionary<ulong, RuleDelegate>()
        {
");
                {
                    List<VehicleStructsBmw.RuleInfo> ruleList = diagObjectRuleListOrder;
                    string funcNameLast = null;
                    VehicleStructsBmw.RuleInfo ruleInfoLast = null;
                    foreach (VehicleStructsBmw.RuleInfo ruleInfo in ruleList)
                    {
                        if (ruleInfoLast != null && string.Compare(ruleInfo.RuleFormula, ruleInfoLast.RuleFormula, StringComparison.Ordinal) != 0)
                        {
                            funcNameLast = null;
                        }

                        if (funcNameLast == null)
                        {
                            funcNameLast = "DiagObjectRule_" + ruleInfo.Id.Trim();
                        }

                        if (!rulesFuncDict.ContainsKey(funcNameLast))
                        {
                            rulesFuncDict.Add(funcNameLast, ruleInfo);
                        }

                        sb.Append(
$@"            {{{ruleInfo.Id.Trim()}, {funcNameLast}}},
");

                        ruleInfoLast = ruleInfo;
                    }
                }

                sb.Append(
@"      };
    }

    public bool IsFaultRuleValid(ulong id)
    {
        if (_faultRuleDict.TryGetValue(id, out RuleDelegate ruleDelegate))
        {
            return ruleDelegate();
        }

        RuleNotFound(id);
        return true;
    }

    public bool IsEcuFuncRuleValid(ulong id)
    {
        if (_ecuFuncRuleDict.TryGetValue(id, out RuleDelegate ruleDelegate))
        {
            return ruleDelegate();
        }

        RuleNotFound(id);
        return true;
    }

    public bool IsDiagObjectRuleValid(ulong id)
    {
        if (_diagObjectRuleDict.TryGetValue(id, out RuleDelegate ruleDelegate))
        {
            return ruleDelegate();
        }

        RuleNotFound(id);
        return true;
    }

    private void RuleNotFound(ulong id)
    {
        if (RuleEvalClass != null)
        {
            RuleEvalClass.RuleNotFound(id);
        }
    }

    private string RuleString(string name)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.RuleString(name);
        }
        return string.Empty;
    }

    private long RuleNum(string name)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.RuleNum(name);
        }
        return -1;
    }

    private bool IsValidRuleString(string name, string value)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.IsValidRuleString(name, value);
        }
        return false;
    }

    private bool IsValidRuleNum(string name, long value)
    {
        if (RuleEvalClass != null)
        {
            return RuleEvalClass.IsValidRuleNum(name, value);
        }
        return false;
    }
");

                foreach (KeyValuePair<string, VehicleStructsBmw.RuleInfo> funcKeyValuePair in rulesFuncDict)
                {
                    sb.Append(
$@"
    private bool {funcKeyValuePair.Key}()
    {{
        bool result = {VehicleInfoBmw.RemoveNonAsciiChars(funcKeyValuePair.Value.RuleFormula)};
        return result;
    }}
");
                }

                sb.Append(
@"
}
");

            File.WriteAllText(fileName, sb.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SaveFaultRulesInfo Exception: {0}", EdiabasLib.EdiabasNet.GetExceptionText(ex));
                return false;
            }

            return true;
        }

        public SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ExtractFaultRulesInfo(ClientContext clientContext)
        {
            try
            {
                List<EcuFunctionStructs.EcuFaultCode> ecuFaultCodeList = new List<EcuFunctionStructs.EcuFaultCode>();
                string sql = @"SELECT ID, CODE, DATATYPE, RELEVANCE FROM XEP_FAULTCODES";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EcuFunctionStructs.EcuFaultCode ecuFaultCode = new EcuFunctionStructs.EcuFaultCode(
                                reader["ID"].ToString()?.Trim(),
                                reader["CODE"].ToString(),
                                reader["DATATYPE"].ToString(),
                                reader["RELEVANCE"].ToString());
                            ecuFaultCodeList.Add(ecuFaultCode);
                        }
                    }
                }

                Vehicle vehicle = new Vehicle(clientContext);
                List<string> subRuleIDs = new List<string>();
                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ruleDict = new SerializableDictionary<string, VehicleStructsBmw.RuleInfo>();
                foreach (EcuFunctionStructs.EcuFaultCode ecuFaultCode in ecuFaultCodeList)
                {
                    if (ecuFaultCode.Relevance.ConvertToInt() > 0)
                    {
                        if (!ruleDict.ContainsKey(ecuFaultCode.Id))
                        {
                            XepRule xepRule = GetRuleById(ecuFaultCode.Id);
                            if (xepRule != null)
                            {
                                string ruleFormula = xepRule.GetRuleFormula(vehicle, null, subRuleIDs);
                                if (!string.IsNullOrEmpty(ruleFormula))
                                {
                                    ruleDict.Add(ecuFaultCode.Id, new VehicleStructsBmw.RuleInfo(ecuFaultCode.Id, ruleFormula));
                                }
                            }
                        }
                    }
                }

                if (!AddSubRules(vehicle, ruleDict, subRuleIDs))
                {
                    log.ErrorFormat("ExtractFaultRulesInfo AddSubRules failed");
                    return null;
                }

                return ruleDict;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractFaultRulesInfo Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ExtractEcuFuncRulesInfo(ClientContext clientContext)
        {
            try
            {
                List<string> ecuFixedFuncList = new List<string>();
                string sql = @"SELECT ID FROM XEP_ECUFIXEDFUNCTIONS";
                using (SqliteCommand command = _mDbConnection.CreateCommand())
                {
                    command.CommandText = sql;
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ecuFixedFuncList.Add(reader["ID"].ToString()?.Trim());
                        }
                    }
                }

                Vehicle vehicle = new Vehicle(clientContext);
                List<string> subRuleIDs = new List<string>();
                SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ruleDict = new SerializableDictionary<string, VehicleStructsBmw.RuleInfo>();
                foreach (string ecuFixedFuncId in ecuFixedFuncList)
                {
                    if (ecuFixedFuncId.ConvertToInt() > 0)
                    {
                        if (!ruleDict.ContainsKey(ecuFixedFuncId))
                        {
                            XepRule xepRule = GetRuleById(ecuFixedFuncId);
                            if (xepRule != null)
                            {
                                string ruleFormula = xepRule.GetRuleFormula(vehicle, null, subRuleIDs);
                                if (!string.IsNullOrEmpty(ruleFormula))
                                {
                                    ruleDict.Add(ecuFixedFuncId, new VehicleStructsBmw.RuleInfo(ecuFixedFuncId, ruleFormula));
                                }
                            }
                        }
                    }
                }

                if (!AddSubRules(vehicle, ruleDict, subRuleIDs))
                {
                    log.ErrorFormat("ExtractEcuFuncRulesInfo AddSubRules failed");
                    return null;
                }

                return ruleDict;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractEcuFuncRulesInfo Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return null;
            }
        }

        public bool AddSubRules(Vehicle vehicle, SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ruleDict, List<string> subRuleIDs)
        {
            try
            {
                foreach (string subRule in subRuleIDs)
                {
                    if (!ruleDict.ContainsKey(subRule))
                    {
                        XepRule xepRule = GetRuleById(subRule);
                        if (xepRule != null)
                        {
                            string ruleFormula = xepRule.GetRuleFormula(vehicle, null, subRuleIDs);
                            if (!string.IsNullOrEmpty(ruleFormula))
                            {
                                ruleDict.Add(subRule, new VehicleStructsBmw.RuleInfo(subRule, ruleFormula));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("AddSubRules Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }

            return true;
        }

        public bool ExtractRuleNames(SerializableDictionary<string, VehicleStructsBmw.RuleInfo> ruleDict, List<string> ruleNames)
        {
            try
            {
                Regex formulaNameRegex = new Regex(@"(RuleString|RuleNum|IsValidRuleString|IsValidRuleNum)\(""([^""]+)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                foreach (KeyValuePair<string, VehicleStructsBmw.RuleInfo> keyValuePair in ruleDict)
                {
                    string ruleFormula = keyValuePair.Value.RuleFormula;
                    MatchCollection nameMatches = formulaNameRegex.Matches(ruleFormula);
                    foreach (Match match in nameMatches)
                    {
                        if (match.Groups.Count == 3 && match.Groups[2].Success)
                        {
                            string name = match.Groups[2].Value.Trim();
                            if (!ruleNames.Contains(name))
                            {
                                ruleNames.Add(name);
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                log.ErrorFormat("ExtractRuleNames Exception: '{0}'", EdiabasLib.EdiabasNet.GetExceptionText(e));
                return false;
            }
        }
        public string GetCallingMemberName([System.Runtime.CompilerServices.CallerMemberName] string memberName = null)
        {
            return memberName ?? string.Empty;
        }

        public string GetCallingSourceFileName([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = null)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                return string.Empty;
            }

            return Path.GetFileName(sourceFilePath);
        }
    }
}
