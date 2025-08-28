using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BmwFileReader;
using PsdzClient.Utility;

namespace PsdzClient.Core
{
	public abstract class BaseEcuCharacteristics
	{
        internal string brSgbd;

        internal string compatibilityInfo;

        internal string sitInfo;

        internal double? rootHorizontalBusStep;

        internal ReadOnlyCollection<IEcuLogisticsEntry> ecuTable;

        internal ReadOnlyCollection<IBusLogisticsEntry> busTable;

        internal ReadOnlyCollection<ICombinedEcuHousingEntry> combinedEcuHousingTable;

        internal ReadOnlyCollection<IBusInterConnectionEntry> interConnectionTable;

        internal ReadOnlyCollection<ISGBDBusLogisticsEntry> variantTable;

        internal ReadOnlyCollection<IBusNameEntry> busNameTable;

        internal ReadOnlyCollection<IXGBMBusLogisticsEntry> xgbdTable;

        internal HashSet<int> minimalConfiguration = new HashSet<int>();

        internal HashSet<int> excludedConfiguration = new HashSet<int>();

        internal HashSet<int> optionalConfiguration = new HashSet<int>();

        internal HashSet<int> unsureConfiguration = new HashSet<int>();

        internal HashSet<int[]> xorConfiguration = new HashSet<int[]>();

        private const string RESOURCE_CHARACTERISTIC_CONFIGURATION_PATH = "BMW.Rheingold.Diagnostics.EcuCharacteristics.Xml.";

        public string BordnetName { get; set; }

        public BaseEcuCharacteristics()
        {
        }

        public BaseEcuCharacteristics(string xmlCharacteristic)
        {
            IEcuTreeConfiguration ecuTreeConfiguration = null;
            ValidationEventHandler veh = delegate (object sender, ValidationEventArgs e)
            {
                Log.Warning("BaseEcuCharacteristics.Constructor", string.Format(CultureInfo.InvariantCulture, "Validation: {0}", e.Message));
            };
            if (xmlCharacteristic.Contains("<?xml"))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlCharacteristic);
                EcuTreeConfiguration.Deserialize(xmlDocument.InnerXml, out var obj, out var _);
                ecuTreeConfiguration = obj;
            }
            else if (ConfigSettings.getConfigStringAsBoolean("EcuCharacteristicsVerificationEnabled", defaultValue: false))
            {
                string text = Path.Combine(ConfigSettings.GetTempFolder(), xmlCharacteristic);
                if (File.Exists(text))
                {
                    XmlTextReader reader = new XmlTextReader(text);
                    XmlDocument xmlDocument2 = new XmlDocument();
                    xmlDocument2.Load(reader);
                    EcuTreeConfiguration.Deserialize(xmlDocument2.InnerXml, out var obj2, out var _);
                    ecuTreeConfiguration = obj2;
                    Log.Info("BaseEcuCharateristics.Constructor(xml)", "Characteristics {0} loaded.", text);
                }
                else
                {
                    ecuTreeConfiguration = LoadCharacteristicsFromAssembly(xmlCharacteristic, veh);
                    Log.Info("BaseEcuCharateristics.Constructor(xml)", "Characteristics {0} loaded from Assembly.", xmlCharacteristic);
                }
            }
            else
            {
                ecuTreeConfiguration = LoadCharacteristicsFromAssembly(xmlCharacteristic, veh);
            }
            if (ecuTreeConfiguration == null)
            {
                throw new ArgumentNullException("EcuTreeConfiguration");
            }
            brSgbd = ecuTreeConfiguration.MainSeriesSgbd;
            compatibilityInfo = ecuTreeConfiguration.CompatibilityInfo;
            sitInfo = ecuTreeConfiguration.SitInfo;
            rootHorizontalBusStep = ecuTreeConfiguration.RootHorizontalBusStep;
            ecuTable = ecuTreeConfiguration.EcuLogisticsList;
            busTable = ecuTreeConfiguration.BusLogisticsList;
            combinedEcuHousingTable = ecuTreeConfiguration.CombinedEcuHousingList;
            interConnectionTable = ecuTreeConfiguration.BusInterConnectionList;
            variantTable = ecuTreeConfiguration.SGBDBusLogisticsList;
            busNameTable = ecuTreeConfiguration.BusNameList;
            xgbdTable = ecuTreeConfiguration.XGBMBusLogisticsList;
            minimalConfiguration = new HashSet<int>(ecuTreeConfiguration.MinimalConfigurationList);
            excludedConfiguration = new HashSet<int>(ecuTreeConfiguration.ExcludedConfigurationList);
            optionalConfiguration = new HashSet<int>(ecuTreeConfiguration.OptionalConfigurationList);
            unsureConfiguration = new HashSet<int>(ecuTreeConfiguration.UnsureConfigurationList);
            xorConfiguration = new HashSet<int[]>(ecuTreeConfiguration.XorConfigurationList);
        }

        private IEcuTreeConfiguration LoadCharacteristicsFromAssembly(string xmlCharacteristic, ValidationEventHandler veh)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BMW.Rheingold.Diagnostics.EcuCharacteristics.Xml." + xmlCharacteristic))
            {
                return LoadCharacteristics(xmlCharacteristic, veh, stream);
            }
        }

        private IEcuTreeConfiguration LoadCharacteristics(string xmlCharacteristic, ValidationEventHandler veh, Stream stream)
        {
            if (stream != null)
            {
                if (!EcuTreeConfiguration.ValidateFile(stream, veh))
                {
                    Log.Error("BaseEcuCharacteristics.Constructor", string.Format(CultureInfo.InvariantCulture, "Validation failed: {0}", xmlCharacteristic));
                }
                return EcuTreeConfiguration.ReadFromStream(stream);
            }
            throw new IOException(string.Format(CultureInfo.InvariantCulture, "XmlCharacteristic ('{0}') could not be found!", xmlCharacteristic));
        }

        public IEcuLogisticsEntry GetEcuLogisticsEntry(Vehicle vecInfo, ECU ecu)
        {
            return GetEcuLogisticsEntry(vecInfo, (IEcu)ecu);
        }

        public IEcuLogisticsEntry GetEcuLogisticsEntry(Vehicle vecInfo, IEcu ecu)
        {
            if (ecuTable != null && ecu != null)
            {
                return ecuTable.FirstOrDefault((IEcuLogisticsEntry item) => item.DiagAddress == ecu.ID_SG_ADR && item.SubDiagAddress == ecu.ID_LIN_SLAVE_ADR);
            }
            return null;
        }

        public virtual bool HasBus(BusType busType, Vehicle vecInfo, ECU ecu)
        {
            if (variantTable != null && ecu != null && !string.IsNullOrEmpty(ecu.VARIANTE))
            {
                ISGBDBusLogisticsEntry iSGBDBusLogisticsEntry = variantTable.FirstOrDefault((ISGBDBusLogisticsEntry x) => string.Equals(x.Variant, ecu.VARIANTE, StringComparison.OrdinalIgnoreCase));
                if (iSGBDBusLogisticsEntry != null)
                {
                    if (busType != iSGBDBusLogisticsEntry.Bus)
                    {
                        if (iSGBDBusLogisticsEntry.SubBusList != null)
                        {
                            return iSGBDBusLogisticsEntry.SubBusList.Contains(busType);
                        }
                        return false;
                    }
                    return true;
                }
            }
            IEcuLogisticsEntry ecuLogisticsEntry = GetEcuLogisticsEntry(vecInfo, ecu);
            if (ecuLogisticsEntry != null)
            {
                if (ecuLogisticsEntry.Bus != busType)
                {
                    if (ecuLogisticsEntry.SubBusList != null)
                    {
                        return ecuLogisticsEntry.SubBusList.Contains(busType);
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        public virtual void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            CalculateECUConfiguration(vecInfo, ffmResolver, null, null);
        }

        public virtual ObservableCollectionEx<PsdzDatabase.SaLaPa> GetAvailableSALAPAs(Vehicle vecInfo)
        {
            ObservableCollectionEx<PsdzDatabase.SaLaPa> observableCollectionEx = new ObservableCollectionEx<PsdzDatabase.SaLaPa>();
            if (vecInfo != null && !string.IsNullOrEmpty(compatibilityInfo))
            {
                try
                {
                    string[] array = compatibilityInfo.Split('\n');
                    foreach (string text in array)
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(text) && text.Length >= 10)
                            {
                                string[] array2 = text.Split(';');
                                if (array2[2].Contains(vecInfo.Typ))
                                {
                                    string[] array3 = array2[3].Split('&');
                                    foreach (string text2 in array3)
                                    {
                                        if (!string.IsNullOrEmpty(text2))
                                        {
                                            PsdzDatabase.SaLaPa saLaPaByProductTypeAndSalesKey = ClientContext.GetDatabase(vecInfo)?.GetSaLaPaByProductTypeAndSalesKey("M", text2.Replace("-", string.Empty));
                                            if (saLaPaByProductTypeAndSalesKey != null)
                                            {
                                                observableCollectionEx.AddIfNotContains(saLaPaByProductTypeAndSalesKey);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.WarningException(GetType().Name + ".GetAvailableSALAPAs()", exception);
                        }
                    }
                    return observableCollectionEx;
                }
                catch (Exception exception2)
                {
                    Log.WarningException(GetType().Name + ".GetAvailableSALAPAs()", exception2);
                    return observableCollectionEx;
                }
            }
            return observableCollectionEx;
        }

        public ICollection<IBusLogisticsEntry> GetBusTable()
		{
			return busTable;
		}

		public ICollection<ICombinedEcuHousingEntry> GetCombinedEcuHousingTable()
		{
			return combinedEcuHousingTable;
		}

		public ICollection<IEcuLogisticsEntry> GetEcuLogisticsTable()
		{
			return ecuTable;
		}

		public ICollection<IBusInterConnectionEntry> GetInterConnectionTable()
		{
			return interConnectionTable;
		}

        public virtual void ShapeECUConfiguration(Vehicle vecInfo)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".ShapeECUConfiguration()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                Log.Warning(GetType().Name + ".ShapeECUConfiguration()", "vecInfo.ecu was null");
                return;
            }
            if (xorConfiguration != null)
            {
                foreach (int[] item in xorConfiguration)
                {
                    if (item == null || item.Length < 2)
                    {
                        continue;
                    }
                    int validAdr = -1;
                    int[] array = item;
                    foreach (int num2 in array)
                    {
                        ECU eCU = vecInfo.getECU(num2);
                        if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                        {
                            validAdr = num2;
                        }
                    }
                    if (validAdr <= -1)
                    {
                        continue;
                    }
                    IEnumerable<int> enumerable = item.Where((int x) => x != validAdr);
                    if (enumerable == null)
                    {
                        continue;
                    }
                    foreach (int item2 in enumerable)
                    {
                        ECU eCU2 = vecInfo.getECU(item2);
                        if (eCU2 != null && !eCU2.IDENT_SUCCESSFULLY)
                        {
                            vecInfo.ECU.Remove(eCU2);
                        }
                    }
                }
            }
            if (unsureConfiguration == null)
            {
                return;
            }
            foreach (int item3 in unsureConfiguration)
            {
                ECU eCU3 = vecInfo.getECU(item3);
                if (eCU3 != null && !eCU3.IDENT_SUCCESSFULLY)
                {
                    vecInfo.ECU.Remove(eCU3);
                }
            }
        }

        public virtual BusType GetBus(long? sgAdr, VCIDeviceType? deviceType, string group = null)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (!sgAdr.HasValue)
            {
                Log.Info(GetType().Name + ".getBus()", "sgAdr was null");
                return BusType.UNKNOWN;
            }
            if (sgAdr < 0 && sgAdr > 255)
            {
                Log.Info(GetType().Name + ".getBus()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
                return BusType.UNKNOWN;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (item.DiagAddress == sgAdr)
                    {
                        return item.Bus;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".getBus()", exception);
            }
            LogMissingBus(group, sgAdr, deviceType);
            Log.Info(GetType().Name + ".getBus()", "no bus found for ecu address: {0}", sgAdr.Value.ToString("X2"));
            return BusType.UNKNOWN;
        }

        public BusType GetBus(long? sgAdr, long? subAdr, VCIDeviceType? deviceType, string group = null)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (!sgAdr.HasValue)
            {
                Log.Warning(GetType().Name + ".getBus()", "sgAdr was null");
                return BusType.UNKNOWN;
            }
            if (sgAdr < 0 && sgAdr > 255)
            {
                Log.Warning(GetType().Name + ".getBus()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
                return BusType.UNKNOWN;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (!subAdr.HasValue || subAdr < 0)
                    {
                        if (item.DiagAddress == sgAdr)
                        {
                            return item.Bus;
                        }
                    }
                    else if (item.DiagAddress == sgAdr && item.SubDiagAddress == subAdr)
                    {
                        return item.Bus;
                    }
                }
                LogMissingBus(group, sgAdr, deviceType);
                Log.Warning(GetType().Name + ".getBus()", "no bus found for ecu address/subaddress: {0:X2} {1:X2}", sgAdr, subAdr);
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".getBus()", exception);
            }
            return BusType.UNKNOWN;
        }

        public virtual string GetECU_GROBNAME(long? sgAdr)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (!sgAdr.HasValue)
            {
                Log.Info(GetType().Name + ".getECU_GROBNAME()", "sgAdr was null");
                return null;
            }
            if (sgAdr < 0 && sgAdr > 255)
            {
                Log.Info(GetType().Name + ".getECU_GROBNAME()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
                return null;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (item.DiagAddress == sgAdr)
                    {
                        return item.Name;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".getECU_GROBNAME()", exception);
            }
            Log.Info(GetType().Name + ".getECU_GROBNAME()", "no ECU_GROBNAME found for ecu address: {0}", FormatConverterBase.Dec2Hex(sgAdr));
            return null;
        }

        public virtual string GetECU_GROBNAMEByEcuGroup(string ecuGroup)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (string.IsNullOrEmpty(ecuGroup))
            {
                Log.Info(Log.CurrentMethod(), "The Ecu Group was null or empty");
                return null;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (item.GroupSgbd == ecuGroup)
                    {
                        return item.Name;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
            Log.Info(Log.CurrentMethod(), "no ECU_GROBNAME found for ECu Group: {0}", ecuGroup);
            return null;
        }

        public virtual string GetECU_GRUPPE(long? sgAdr)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (!sgAdr.HasValue)
            {
                Log.Info(GetType().Name + ".getECU_GRUPPE()", "sgAdr was null");
                return string.Empty;
            }
            if (sgAdr < 0 && sgAdr > 255)
            {
                Log.Info(GetType().Name + ".getECU_GRUPPE()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
                return string.Empty;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (item.DiagAddress == sgAdr)
                    {
                        return item.GroupSgbd;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".getECU_GRUPPE()", exception);
            }
            return string.Empty;
        }

        public virtual bool GetEcuCoordinates(long? sgAdr, out int col, out int row)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (!sgAdr.HasValue)
            {
                Log.Info(GetType().Name + ".getEcuCoordinates()", "sgAdr was null");
                col = -1;
                row = -1;
                return false;
            }
            if (sgAdr < 0 && sgAdr > 255)
            {
                Log.Info(GetType().Name + ".getEcuCoordinates()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
                col = -1;
                row = -1;
                return false;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (item.DiagAddress == sgAdr)
                    {
                        row = item.Row;
                        col = item.Column;
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".getECU_GRUPPE()", exception);
            }
            col = -1;
            row = -1;
            return false;
        }

        public bool GetEcuCoordinates(long? sgAdr, long? subAdr, out int col, out int row)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (!sgAdr.HasValue)
            {
                Log.Warning(GetType().Name + ".getEcuCoordinates()", "sgAdr was null");
                col = -1;
                row = -1;
                return false;
            }
            if (sgAdr < 0 && sgAdr > 255)
            {
                Log.Warning(GetType().Name + ".getEcuCoordinates()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
                col = -1;
                row = -1;
                return false;
            }
            try
            {
                foreach (IEcuLogisticsEntry item in ecuTable)
                {
                    if (!subAdr.HasValue || subAdr < 0)
                    {
                        if (item.DiagAddress == sgAdr)
                        {
                            row = item.Row;
                            col = item.Column;
                            return true;
                        }
                    }
                    else if (item.DiagAddress == sgAdr && item.SubDiagAddress == subAdr)
                    {
                        row = item.Row;
                        col = item.Column;
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".getECU_GRUPPE()", exception);
            }
            col = -1;
            row = -1;
            return false;
        }

        public bool IsTypeKeyListed(string typeKey)
        {
            if (!string.IsNullOrEmpty(compatibilityInfo) && !string.IsNullOrEmpty(typeKey))
            {
                string[] array = compatibilityInfo.Split('\n');
                foreach (string text in array)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(text) || text.Length < 10 || !text.Trim().Split(';')[2].Contains(typeKey))
                        {
                            continue;
                        }
                        Log.Info(GetType().Name + ".IsTypeKeyListed()", "type key: {0} found", typeKey);
                        return true;
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException(GetType().Name + ".IsTypeKeyListed()", exception);
                    }
                }
            }
            Log.Info(GetType().Name + ".IsTypeKeyListed()", "type key: {0} NOT found", typeKey);
            return false;
        }

        public void CalculateMaxAssembledECUList(Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            ValidateIfDiagnosticsHasValidLicense();
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateMaxAssembledECUList()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }
            try
            {
                if (sitInfo == null)
                {
                    return;
                }
                string[] array = sitInfo.Split('\n');
                foreach (string text in array)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(text) || text.StartsWith("#", StringComparison.Ordinal))
                        {
                            continue;
                        }
                        string[] array2 = text.Split(';');
                        int num = Convert.ToInt32(array2[0], 16);
                        if (vecInfo.getECU(num) == null)
                        {
                            string ecuVariant = array2[1];
                            // [UH] database replaced
                            PsdzDatabase database = ClientContext.GetDatabase(vecInfo);
                            if (database != null)
                            {
                                PsdzDatabase.EcuVar ecuVariantByName = database.GetEcuVariantByName(ecuVariant);
                                if (ecuVariantByName != null && database.EvaluateXepRulesById(ecuVariantByName.Id, vecInfo, ffmResolver) && !string.IsNullOrEmpty(ecuVariantByName.EcuGroupId) && database.EvaluateXepRulesById(ecuVariantByName.Id, vecInfo, ffmResolver))
                                {
                                    ECU item = CreateECU(num, array2[2], vecInfo.VCI?.VCIType);
                                    vecInfo.ECU.Add(item);
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Log.WarningException(GetType().Name + ".CalculateMaxAssembledECUList()", exception);
                    }
                }
            }
            catch (Exception exception2)
            {
                Log.WarningException(GetType().Name + ".CalculateMaxAssembledECUList()", exception2);
            }
        }

        internal string GetBusAlias(BusType bus)
        {
            if (busNameTable != null)
            {
                foreach (IBusNameEntry item in busNameTable)
                {
                    if (item.Bus == bus)
                    {
                        return item.Name;
                    }
                }
            }
            return bus.ToString();
        }

        protected bool IsGroupValid(string groupName, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            PsdzDatabase database = ClientContext.GetDatabase(vecInfo);
            if (database == null)
            {
                return false;
            }

            PsdzDatabase.EcuGroup ecuGroupByName = database.GetEcuGroupByName(groupName);
            if (ecuGroupByName != null)
            {
                return database.EvaluateXepRulesById(ecuGroupByName.Id, vecInfo, ffmResolver);
            }
            return false;
        }

        protected ECU CreateECU(long adr, string group, VCIDeviceType? deviceType)
        {
            return new ECU
            {
                ID_SG_ADR = adr,
                IDENT_SUCCESSFULLY = false,
                BUS = GetBus(adr, deviceType, group),
                ECU_GRUPPE = group,
                ECU_GROBNAME = GetECU_GROBNAME(adr)
            };
        }

        protected ECU CreateECU(long adr, VCIDeviceType? deviceType)
        {
            ECU eCU = new ECU();
            eCU.ID_SG_ADR = adr;
            eCU.IDENT_SUCCESSFULLY = false;
            eCU.ECU_GRUPPE = GetECU_GRUPPE(adr);
            eCU.BUS = GetBus(adr, deviceType, eCU.ECU_GRUPPE);
            eCU.ECU_GROBNAME = GetECU_GROBNAME(adr);
            return eCU;
        }

        protected void CalculateECUConfiguration(Vehicle vecInfo, IFFMDynamicResolver ffmResolver, ICollection<int> sgList, ICollection<int> removeList)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }
            if (sgList != null)
            {
                foreach (int sg in sgList)
                {
                    ECU item = CreateECU(sg, vecInfo.VCI?.VCIType);
                    vecInfo.ECU.AddIfNotContains(item);
                }
            }
            CalculateECUConfigurationConfigured(vecInfo);
            if (removeList == null)
            {
                return;
            }
            foreach (int remove in removeList)
            {
                ECU eCU = vecInfo.getECU(remove);
                if (eCU != null)
                {
                    Log.Info(GetType().Name + ".CalculateECUConfiguration()", "Removing ECU: {0}", eCU);
                    vecInfo.ECU.Remove(eCU);
                }
            }
        }

        private void CalculateECUConfigurationConfigured(Vehicle vecInfo)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }
            try
            {
                if (!string.IsNullOrEmpty(compatibilityInfo))
                {
                    ProcessCompatibilityInfo(vecInfo, compatibilityInfo);
                }
                SetupMinimalECUConfiguration(vecInfo);
                if (excludedConfiguration != null)
                {
                    foreach (int item in excludedConfiguration)
                    {
                        ECU eCU = vecInfo.getECU(item);
                        if (eCU != null)
                        {
                            vecInfo.ECU.Remove(eCU);
                        }
                    }
                }
                foreach (ECU item2 in vecInfo.ECU)
                {
                    Log.Info(GetType().Name + ".CalculateECUConfigurationConfigured()", "Expected ecu at address: {0:X2} / '{1}'", item2.ID_SG_ADR, item2.ECU_GRUPPE);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(GetType().Name + ".CalculateECUConfigurationConfigured()", exception);
            }
        }

        private void ProcessCompatibilityInfo(Vehicle vecInfo, string compatibilityInfo)
        {
            string[] array = compatibilityInfo.Split('\n');
            string value = ((!string.IsNullOrEmpty(vecInfo.VINRangeType)) ? vecInfo.VINRangeType : vecInfo.Typ);
            string[] array2 = array;
            foreach (string text in array2)
            {
                try
                {
                    if (string.IsNullOrEmpty(text) || text.Length < 10)
                    {
                        continue;
                    }
                    string[] array3 = text.Trim().Split(';');
                    if (!int.TryParse(array3[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
                    {
                        continue;
                    }
                    string text2 = array3[1];
                    if (vecInfo.getECU(result) != null || !array3[2].Contains(value))
                    {
                        continue;
                    }
                    if (array3.Length == 5)
                    {
                        if (string.IsNullOrEmpty(vecInfo.ILevelWerk) || string.IsNullOrEmpty(array3[4]) || array3[4].Contains(vecInfo.ILevelWerk))
                        {
                            goto IL_0219;
                        }
                        continue;
                    }
                    if (array3.Length != 7)
                    {
                        goto IL_0219;
                    }
                    if (!string.IsNullOrEmpty(vecInfo.ILevelWerk) && !string.IsNullOrEmpty(array3[4]) && !array3[4].Contains(vecInfo.ILevelWerk))
                    {
                        Log.Info(GetType().Name + ".ProcessCompatibilityInfo()", "checking iLevel: '{0}'", array3[4]);
                        continue;
                    }
                    Log.Info(GetType().Name + ".ProcessCompatibilityInfo()", "checking production date from: '{0}' to '{1}'", array3[5], array3[6]);
                    if (string.IsNullOrEmpty(array3[5]) || array3[5].Length != 6)
                    {
                        goto IL_01cb;
                    }
                    DateTime dateTime = DateTime.ParseExact(array3[5], "MMyyyy", CultureInfo.InvariantCulture);
                    if (!vecInfo.ProductionDateSpecified || !(vecInfo.ProductionDate < dateTime))
                    {
                        goto IL_01cb;
                    }
                    goto end_IL_0039;
                    IL_0219:
                    string[] array4 = array3[3].Split('&');
                    bool flag = true;
                    string[] array5 = array4;
                    foreach (string text3 in array5)
                    {
                        if (!string.IsNullOrEmpty(text3))
                        {
                            flag = ((!text3.StartsWith("-", StringComparison.Ordinal)) ? (flag & vecInfo.HasSA(text3)) : (flag & !vecInfo.HasSA(text3.Replace("-", string.Empty))));
                        }
                    }
                    if (flag)
                    {
                        ECU item = CreateECU(result, text2, vecInfo.VCI?.VCIType);
                        vecInfo.ECU.AddIfNotContains(item);
                    }
                    goto end_IL_0039;
                    IL_01cb:
                    if (string.IsNullOrEmpty(array3[6]) || array3[6].Length != 6)
                    {
                        goto IL_0219;
                    }
                    DateTime dateTime2 = DateTime.ParseExact(array3[6], "MMyyyy", CultureInfo.InvariantCulture);
                    if (!vecInfo.ProductionDateSpecified || !(vecInfo.ProductionDate > dateTime2.AddMonths(1)))
                    {
                        goto IL_0219;
                    }
                    end_IL_0039:;
                }
                catch (Exception exception)
                {
                    Log.WarningException(GetType().Name + ".ProcessCompatibilityInfo()", exception);
                }
            }
        }

        private void SetupMinimalECUConfiguration(Vehicle vecInfo)
        {
            if (vecInfo == null)
            {
                Log.Warning(GetType().Name + ".SetupMinimalECUConfiguration()", "vecInfo was null");
                return;
            }
            if (vecInfo.ECU == null)
            {
                vecInfo.ECU = new ObservableCollection<ECU>();
            }
            if (minimalConfiguration == null)
            {
                return;
            }
            foreach (int item in minimalConfiguration)
            {
                if (vecInfo.getECU(item) == null)
                {
                    ECU eCU = CreateECU(item, vecInfo.VCI?.VCIType);
                    eCU.ID_SG_ADR = item;
                    eCU.IDENT_SUCCESSFULLY = false;
                    eCU.ECU_GRUPPE = GetECU_GRUPPE(item);
                    eCU.BUS = GetBus(item, vecInfo.VCI?.VCIType, eCU.ECU_GRUPPE);
                    vecInfo.ECU.AddIfNotContains(eCU);
                }
            }
        }

        // [UH] cleaned
        private void ValidateIfDiagnosticsHasValidLicense()
        {
        }

        private void LogMissingBus(string ecuGroup, long? ecuAddress, VCIDeviceType? deviceType)
        {
            if (!(BordnetName == "BNT-XML-FALLBACK.xml"))
            {
                if (string.IsNullOrWhiteSpace(ecuGroup))
                {
                    _ = $"EcuAddress: {ecuAddress:X2}, BNT-Filename: {BordnetName}";
                }
                else
                {
                    _ = "EcuGroup: " + ecuGroup + ", BNT-Filename: " + BordnetName;
                }
            }
        }
    }
}
