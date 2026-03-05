using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
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
        private const string RESOURCE_CHARACTERISTIC_CONFIGURATION_PATH = "BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.EcuCharacteristics.Xml.";
        public string BordnetName { get; set; }
        public string CompatibilityInfo => compatibilityInfo;

        internal BaseEcuCharacteristics()
        {
        }

        internal BaseEcuCharacteristics(string xmlCharacteristic)
        {
            IEcuTreeConfiguration ecuTreeConfiguration = null;
            ValidationEventHandler veh = delegate (object sender, ValidationEventArgs e)
            {
                EcuTreeLogger.Instance.Warning("BaseEcuCharacteristics.Constructor", string.Format(CultureInfo.InvariantCulture, "Validation: {0}", e.Message));
            };
            if (xmlCharacteristic.Contains("<?xml"))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlCharacteristic);
                EcuTreeConfiguration.Deserialize(xmlDocument.InnerXml, out var obj, out var _);
                ecuTreeConfiguration = obj;
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
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BMW.ISPI.TRIC.ISTA.EcuTree.Bordnet.EcuCharacteristics.Xml." + xmlCharacteristic))
            {
                return LoadCharacteristics(xmlCharacteristic, veh, stream);
            }
        }

        private IEcuTreeConfiguration LoadCharacteristics(string xmlCharacteristic, ValidationEventHandler veh, Stream stream)
        {
            if (stream != null)
            {
                if (!EcuTreeConfiguration.ValidateFile(stream, veh, EcuTreeLogger.Instance))
                {
                    EcuTreeLogger.Instance.Error("BaseEcuCharacteristics.Constructor", string.Format(CultureInfo.InvariantCulture, "Validation failed: {0}", xmlCharacteristic));
                }

                return EcuTreeConfiguration.ReadFromStream(stream);
            }

            throw new IOException(string.Format(CultureInfo.InvariantCulture, "XmlCharacteristic ('{0}') could not be found!", xmlCharacteristic));
        }

        public IEcuLogisticsEntry GetEcuLogisticsEntry(IEcuTreeVehicle vecInfo, IEcuTreeEcu ecu)
        {
            if (ecuTable != null && ecu != null)
            {
                return ecuTable.FirstOrDefault((IEcuLogisticsEntry item) => item.DiagAddress == ecu.ID_SG_ADR && item.SubDiagAddress == ecu.ID_LIN_SLAVE_ADR);
            }

            return null;
        }

        public virtual bool HasBus(BusType busType, IEcuTreeVehicle vecInfo, IEcuTreeEcu ecu)
        {
            if (variantTable != null && ecu != null && !string.IsNullOrEmpty(ecu.VARIANT))
            {
                ISGBDBusLogisticsEntry iSGBDBusLogisticsEntry = variantTable.FirstOrDefault((ISGBDBusLogisticsEntry x) => string.Equals(x.Variant, ecu.VARIANT, StringComparison.OrdinalIgnoreCase));
                if (iSGBDBusLogisticsEntry != null)
                {
                    return busType == iSGBDBusLogisticsEntry.Bus || (iSGBDBusLogisticsEntry.SubBusList != null && iSGBDBusLogisticsEntry.SubBusList.Contains(busType));
                }
            }

            IEcuLogisticsEntry ecuLogisticsEntry = GetEcuLogisticsEntry(vecInfo, ecu);
            if (ecuLogisticsEntry != null)
            {
                return ecuLogisticsEntry.Bus == busType || (ecuLogisticsEntry.SubBusList != null && ecuLogisticsEntry.SubBusList.Contains(busType));
            }

            return false;
        }

        public virtual void CalculateECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            CalculateECUConfiguration(vecInfo, null, null);
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

        public virtual void ShapeECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            if (vecInfo == null)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".ShapeECUConfiguration()", "vecInfo was null");
                return;
            }

            if (vecInfo.ECU == null)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".ShapeECUConfiguration()", "vecInfo.ecu was null");
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
                    foreach (int num in array)
                    {
                        IEcuTreeEcu eCU = vecInfo.getECU(num);
                        if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                        {
                            validAdr = num;
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
                        IEcuTreeEcu eCU2 = vecInfo.getECU(item2);
                        if (eCU2 != null && !eCU2.IDENT_SUCCESSFULLY)
                        {
                            vecInfo.RemoveEcu(eCU2);
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
                IEcuTreeEcu eCU3 = vecInfo.getECU(item3);
                if (eCU3 != null && !eCU3.IDENT_SUCCESSFULLY)
                {
                    EcuTreeLogger.Instance.Info(EcuTreeLogger.Instance.CurrentMethod(), $"This ECU would be deleted by the 'UnsureConfiguration': Address: {item3}, Variant: {eCU3.VARIANT}");
                }
            }
        }

        public virtual BusType GetBus(long? sgAdr, string group = null)
        {
            if (!sgAdr.HasValue)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getBus()", "sgAdr was null");
                return BusType.UNKNOWN;
            }

            if (sgAdr < 0 && sgAdr > 255)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getBus()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
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
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".getBus()", exception);
            }

            EcuTreeLogger.Instance.Info(GetType().Name + ".getBus()", "no bus found for ecu address: " + sgAdr.Value.ToString("X2"));
            return BusType.UNKNOWN;
        }

        public BusType GetBus(long? sgAdr, long? subAdr, VCIDeviceType? deviceType, string group = null)
        {
            if (!sgAdr.HasValue)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".getBus()", "sgAdr was null");
                return BusType.UNKNOWN;
            }

            if (sgAdr < 0 && sgAdr > 255)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".getBus()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
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

                EcuTreeLogger.Instance.Warning(GetType().Name + ".getBus()", $"no bus found for ecu address/subaddress: {sgAdr:X2} {subAdr:X2}");
            }
            catch (Exception exception)
            {
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".getBus()", exception);
            }

            return BusType.UNKNOWN;
        }

        public virtual string GetECU_GROBNAME(long? sgAdr)
        {
            if (!sgAdr.HasValue)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getECU_GROBNAME()", "sgAdr was null");
                return null;
            }

            if (sgAdr < 0 && sgAdr > 255)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getECU_GROBNAME()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
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
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".getECU_GROBNAME()", exception);
            }

            EcuTreeLogger.Instance.Info(GetType().Name + ".getECU_GROBNAME()", "no ECU_GROBNAME found for ecu address: " + FormatConverterBase.Dec2Hex(sgAdr));
            return null;
        }

        public virtual string GetECU_GROBNAMEByEcuGroup(string ecuGroup)
        {
            if (string.IsNullOrEmpty(ecuGroup))
            {
                EcuTreeLogger.Instance.Info(EcuTreeLogger.Instance.CurrentMethod(), "The Ecu Group was null or empty");
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
                EcuTreeLogger.Instance.WarningException(EcuTreeLogger.Instance.CurrentMethod(), exception);
            }

            EcuTreeLogger.Instance.Info(EcuTreeLogger.Instance.CurrentMethod(), "no ECU_GROBNAME found for ECu Group: {0}", ecuGroup);
            return null;
        }

        public virtual string GetECU_GRUPPE(long? sgAdr)
        {
            if (!sgAdr.HasValue)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getECU_GRUPPE()", "sgAdr was null");
                return string.Empty;
            }

            if (sgAdr < 0 && sgAdr > 255)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getECU_GRUPPE()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
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
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".getECU_GRUPPE()", exception);
            }

            return string.Empty;
        }

        public virtual bool GetEcuCoordinates(long? sgAdr, out int col, out int row)
        {
            if (!sgAdr.HasValue)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getEcuCoordinates()", "sgAdr was null");
                col = -1;
                row = -1;
                return false;
            }

            if (sgAdr < 0 && sgAdr > 255)
            {
                EcuTreeLogger.Instance.Info(GetType().Name + ".getEcuCoordinates()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
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
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".getECU_GRUPPE()", exception);
            }

            col = -1;
            row = -1;
            return false;
        }

        public bool GetEcuCoordinates(long? sgAdr, long? subAdr, out int col, out int row)
        {
            if (!sgAdr.HasValue)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".getEcuCoordinates()", "sgAdr was null");
                col = -1;
                row = -1;
                return false;
            }

            if (sgAdr < 0 && sgAdr > 255)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".getEcuCoordinates()", "sgAdr out of range. sgAdr was: {0}", sgAdr);
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
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".getECU_GRUPPE()", exception);
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
                string[] array2 = array;
                foreach (string text in array2)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(text) && text.Length >= 10)
                        {
                            string[] array3 = text.Trim().Split(';');
                            if (array3[2].Contains(typeKey))
                            {
                                EcuTreeLogger.Instance.Info(GetType().Name + ".IsTypeKeyListed()", "type key: {0} found", typeKey);
                                return true;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        EcuTreeLogger.Instance.WarningException(GetType().Name + ".IsTypeKeyListed()", exception);
                    }
                }
            }

            EcuTreeLogger.Instance.Info(GetType().Name + ".IsTypeKeyListed()", "type key: {0} NOT found", typeKey);
            return false;
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

        protected IEcuTreeEcu CreateECU(long adr, string group)
        {
            EcuTreeEcu ecuTreeEcu = new EcuTreeEcu();
            ecuTreeEcu.ID_SG_ADR = adr;
            ecuTreeEcu.IDENT_SUCCESSFULLY = false;
            ecuTreeEcu.BUS = GetBus(adr, group);
            ecuTreeEcu.ECU_GRUPPE = group;
            ecuTreeEcu.ECU_GROBNAME = GetECU_GROBNAME(adr);
            return ecuTreeEcu;
        }

        protected IEcuTreeEcu CreateECU(long adr)
        {
            EcuTreeEcu ecuTreeEcu = new EcuTreeEcu();
            ecuTreeEcu.ID_SG_ADR = adr;
            ecuTreeEcu.IDENT_SUCCESSFULLY = false;
            ecuTreeEcu.ECU_GRUPPE = GetECU_GRUPPE(adr);
            ecuTreeEcu.BUS = GetBus(adr, ecuTreeEcu.ECU_GRUPPE);
            ecuTreeEcu.ECU_GROBNAME = GetECU_GROBNAME(adr);
            return ecuTreeEcu;
        }

        protected void CalculateECUConfiguration(IEcuTreeVehicle vecInfo, ICollection<int> sgList, ICollection<int> removeList)
        {
            if (vecInfo == null)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".CalculateECUConfiguration()", "vecInfo was null");
                return;
            }

            if (sgList != null)
            {
                foreach (int sg in sgList)
                {
                    IEcuTreeEcu ecu = CreateECU(sg);
                    vecInfo.AddEcu(ecu);
                }
            }

            CalculateECUConfigurationConfigured(vecInfo);
            if (removeList == null)
            {
                return;
            }

            foreach (int remove in removeList)
            {
                IEcuTreeEcu eCU = vecInfo.getECU(remove);
                if (eCU != null)
                {
                    EcuTreeLogger.Instance.Info(GetType().Name + ".CalculateECUConfiguration()", "Removing ECU: {0}", eCU);
                    vecInfo.RemoveEcu(eCU);
                }
            }
        }

        private void CalculateECUConfigurationConfigured(IEcuTreeVehicle vecInfo)
        {
            if (vecInfo == null)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".CalculateECUConfigurationConfigured()", "vecInfo was null");
                return;
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
                        IEcuTreeEcu eCU = vecInfo.getECU(item);
                        if (eCU != null)
                        {
                            vecInfo.RemoveEcu(eCU);
                        }
                    }
                }

                foreach (IEcuTreeEcu item2 in vecInfo.ECU)
                {
                    EcuTreeLogger.Instance.Info(GetType().Name + ".CalculateECUConfigurationConfigured()", "Expected ecu at address: {0:X2} / '{1}'", item2.ID_SG_ADR, item2.ECU_GRUPPE);
                }
            }
            catch (Exception exception)
            {
                EcuTreeLogger.Instance.WarningException(GetType().Name + ".CalculateECUConfigurationConfigured()", exception);
            }
        }

        private void ProcessCompatibilityInfo(IEcuTreeVehicle vecInfo, string compatibilityInfo)
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
                            goto IL_0299;
                        }

                        continue;
                    }

                    if (array3.Length != 7)
                    {
                        goto IL_0299;
                    }

                    if (!string.IsNullOrEmpty(vecInfo.ILevelWerk) && !string.IsNullOrEmpty(array3[4]) && !array3[4].Contains(vecInfo.ILevelWerk))
                    {
                        EcuTreeLogger.Instance.Info(GetType().Name + ".ProcessCompatibilityInfo()", "checking iLevel: '{0}'", array3[4]);
                        continue;
                    }

                    EcuTreeLogger.Instance.Info(GetType().Name + ".ProcessCompatibilityInfo()", "checking production date from: '{0}' to '{1}'", array3[5], array3[6]);
                    if (string.IsNullOrEmpty(array3[5]) || array3[5].Length != 6)
                    {
                        goto IL_0235;
                    }

                    DateTime dateTime = DateTime.ParseExact(array3[5], "MMyyyy", CultureInfo.InvariantCulture);
                    if (!vecInfo.ProductionDateSpecified || !(vecInfo.ProductionDate < dateTime))
                    {
                        goto IL_0235;
                    }

                    goto end_IL_003f;
                    IL_0299:
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
                        IEcuTreeEcu ecu = CreateECU(result, text2);
                        vecInfo.AddEcu(ecu);
                    }

                    goto end_IL_003f;
                    IL_0235:
                        if (string.IsNullOrEmpty(array3[6]) || array3[6].Length != 6)
                        {
                            goto IL_0299;
                        }

                    DateTime dateTime2 = DateTime.ParseExact(array3[6], "MMyyyy", CultureInfo.InvariantCulture);
                    if (!vecInfo.ProductionDateSpecified || !(vecInfo.ProductionDate > dateTime2.AddMonths(1)))
                    {
                        goto IL_0299;
                    }

                    end_IL_003f:
                        ;
                }
                catch (Exception exception)
                {
                    EcuTreeLogger.Instance.WarningException(GetType().Name + ".ProcessCompatibilityInfo()", exception);
                }
            }
        }

        private void SetupMinimalECUConfiguration(IEcuTreeVehicle vecInfo)
        {
            if (vecInfo == null)
            {
                EcuTreeLogger.Instance.Warning(GetType().Name + ".SetupMinimalECUConfiguration()", "vecInfo was null");
            }
            else
            {
                if (minimalConfiguration == null)
                {
                    return;
                }

                foreach (int item in minimalConfiguration)
                {
                    if (vecInfo.getECU(item) == null)
                    {
                        IEcuTreeEcu ecuTreeEcu = CreateECU(item);
                        ecuTreeEcu.ID_SG_ADR = item;
                        ecuTreeEcu.IDENT_SUCCESSFULLY = false;
                        ecuTreeEcu.ECU_GRUPPE = GetECU_GRUPPE(item);
                        ecuTreeEcu.BUS = GetBus(item, ecuTreeEcu.ECU_GRUPPE);
                        vecInfo.AddEcu(ecuTreeEcu);
                    }
                }
            }
        }

        [PreserveSource(Hint = "XEP_SALAPAS replaced", SignatureModified = true)]
        public virtual ObservableCollectionEx<PsdzDatabase.SaLaPa> GetAvailableSALAPAs(Vehicle vecInfo)
        {
            //[-] ObservableCollectionEx<XEP_SALAPAS> observableCollectionEx = new ObservableCollectionEx<XEP_SALAPAS>();
            //[+] ObservableCollectionEx<PsdzDatabase.SaLaPa> observableCollectionEx = new ObservableCollectionEx<PsdzDatabase.SaLaPa>();
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
                            if (string.IsNullOrEmpty(text) || text.Length < 10)
                            {
                                continue;
                            }

                            string[] array2 = text.Split(';');
                            if (!array2[2].Contains(vecInfo.Typ))
                            {
                                continue;
                            }

                            string[] array3 = array2[3].Split('&');
                            foreach (string text2 in array3)
                            {
                                if (!string.IsNullOrEmpty(text2))
                                {
                                    //[-] XEP_SALAPAS saLaPaByProductTypeAndSalesKey = DatabaseProviderFactory.Instance.GetSaLaPaByProductTypeAndSalesKey("M", text2.Replace("-", string.Empty));
                                    //[+] PsdzDatabase.SaLaPa saLaPaByProductTypeAndSalesKey = ClientContext.GetDatabase(vecInfo)?.GetSaLaPaByProductTypeAndSalesKey("M", text2.Replace("-", string.Empty));
                                    PsdzDatabase.SaLaPa saLaPaByProductTypeAndSalesKey = ClientContext.GetDatabase(vecInfo)?.GetSaLaPaByProductTypeAndSalesKey("M", text2.Replace("-", string.Empty));
                                    if (saLaPaByProductTypeAndSalesKey != null)
                                    {
                                        observableCollectionEx.AddIfNotContains(saLaPaByProductTypeAndSalesKey);
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Log.WarningException(GetType().Name + ".GetAvailableSALAPAs()", exception);
                        }
                    }
                }
                catch (Exception exception2)
                {
                    Log.WarningException(GetType().Name + ".GetAvailableSALAPAs()", exception2);
                }
            }

            return observableCollectionEx;
        }

        [PreserveSource(Hint = "database replaced", SignatureModified = true)]
        protected bool IsGroupValid(string groupName, Vehicle vecInfo, IFFMDynamicResolver ffmResolver)
        {
            //[-] if (DatabaseProviderFactory.Instance != null && DatabaseProviderFactory.Instance.DatabaseAccessType != DatabaseType.None)
            //[-] {
            //[-] XEP_ECUGROUPS ecuGroupByName = DatabaseProviderFactory.Instance.GetEcuGroupByName(groupName);
            //[-] if (ecuGroupByName != null)
            //[-] {
            //[-] return DatabaseProviderFactory.Instance.EvaluateXepRulesById(ecuGroupByName.Id, vecInfo, ffmResolver);
            //[-] }
            //[-] }
            //[+] PsdzDatabase database = ClientContext.GetDatabase(vecInfo);
            PsdzDatabase database = ClientContext.GetDatabase(vecInfo);
            //[+] if (database == null)
            if (database == null)
            //[+] {
            {
                //[+] return false;
                return false;
            //[+] }
            }

            //[+] PsdzDatabase.EcuGroup ecuGroupByName = database.GetEcuGroupByName(groupName);
            PsdzDatabase.EcuGroup ecuGroupByName = database.GetEcuGroupByName(groupName);
            //[+] if (ecuGroupByName != null)
            if (ecuGroupByName != null)
            //[+] {
            {
                //[+] return database.EvaluateXepRulesById(ecuGroupByName.Id, vecInfo, ffmResolver);
                return database.EvaluateXepRulesById(ecuGroupByName.Id, vecInfo, ffmResolver);
            //[+] }
            }

            return false;
        }

        [PreserveSource(Cleaned = true)]
        private void ValidateIfDiagnosticsHasValidLicense()
        {
        }
    }
}