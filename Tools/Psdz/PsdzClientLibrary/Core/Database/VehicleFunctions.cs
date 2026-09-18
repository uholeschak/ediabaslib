using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.VinValidator;
using BMW.ISPI.TRIC.ISTA.VinValidator;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderHelper;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

#pragma warning disable CS0618
namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public static class VehicleFunctions
    {
        public static void AddServiceCodeAndLogsForTypeKeys(string currentValue, string propertyName)
        {
            if (Environment.StackTrace.Contains("System.Runtime.Serialization") || Environment.StackTrace.Contains("BMW.Rheingold.ISTAGUI") || Environment.StackTrace.Contains("PropertyChangedEventHandler"))
            {
                return;
            }
            IFasta2Service service = ServiceLocator.Current.GetService<IFasta2Service>();
            if (service != null)
            {
                string text = "Used typeKey: " + propertyName + ",  value: " + currentValue + ". Values returned by VinValidator. TypeKey: " + string.Join(",", Validator.TypeKeys.Select((TypeKeys t) => t.TypeKey)) + ", TypeKeyBasic: " + string.Join(",", Validator.TypeKeys.Select((TypeKeys t) => t.TypeKeyBasic)) + ", TypeKeyLead: " + string.Join(",", Validator.TypeKeys.Select((TypeKeys t) => t.TypeKeyLead));
                service.AddServiceCode(ServiceCodes.IDE12_UsageOfAllTypeKeys_nu_LF, text, LayoutGroup.D);
                if (ConfigSettings.GetFeatureEnabledStatus("VinRangeUsagesLogging").IsActive)
                {
                    Log.Info(Log.CurrentMethod(), text + Environment.NewLine + Environment.StackTrace);
                }
            }
        }

        public static List<string> PermanentSAEFehlercodesInFaultList(this Vehicle vehicle)
        {
            List<string> list = new List<string>();
            if (vehicle.FaultList == null || vehicle.FaultList.Count == 0)
            {
                return new List<string>();
            }
            foreach (Fault fault in vehicle.FaultList)
            {
                if (fault.DTC.FortAsHexString == "S 0751")
                {
                    list.Add("S 0751");
                }
                if (fault.DTC.FortAsHexString == "S 0756")
                {
                    list.Add("S 0756");
                }
            }
            return list;
        }

        public static IEnumerable<Fault> GetEnrichedFaultList(this Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver)
        {
            if (!vehicle.FaultList.Any())
            {
                return Enumerable.Empty<Fault>();
            }
            ComputeResolveLabelsForAllFaultAsync(vehicle, ffmDynamicResolver).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().GetResult();
            List<Fault> list = new List<Fault>();
            foreach (Fault fault in vehicle.FaultList)
            {
                list.Add(fault);
            }
            return list;
        }

        public static async Task ComputeResolveLabelsForAllFaultAsync(Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, string language = null)
        {
            if (!string.IsNullOrEmpty(language))
            {
                ConfigSettings.CurrentUICulture = language;
            }
            //[-] IDictionary<FaultCodeIdDtcFOrtEcuVariantKey, ICollection<decimal>> refFaultLabel = DatabaseProviderFactory.Instance.GetRefFaultLabelsLabelIdByFaultList(vehicle.FaultList.Where((Fault x) => !x.IsCheckControlMessage && !x.DTC.IsVirtual && !x.DTC.IsCombined));
            //[+] IDictionary<FaultCodeIdDtcFOrtEcuVariantKey, ICollection<decimal>> refFaultLabel = ClientContext.GetDatabase(vehicle)?.GetRefFaultLabelsLabelIdByFaultList(vehicle.FaultList.Where((Fault x) => !x.IsCheckControlMessage && !x.DTC.IsVirtual && !x.DTC.IsCombined));
            IDictionary<FaultCodeIdDtcFOrtEcuVariantKey, ICollection<decimal>> refFaultLabel = ClientContext.GetDatabase(vehicle)?.GetRefFaultLabelsLabelIdByFaultList(vehicle.FaultList.Where((Fault x) => !x.IsCheckControlMessage && !x.DTC.IsVirtual && !x.DTC.IsCombined));
            //[-] Task<IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>>> xepFaultModelLabelsTask = Task.Run(() => GetXepFaultModelLabelsByDtcFOrtEcuVariantAsync(refFaultLabel));
            //[+] Task<IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>>> xepFaultModelLabelsTask = Task.Run(() => GetXepFaultModelLabelsByDtcFOrtEcuVariantAsync(refFaultLabel, vehicle));
            Task<IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>>> xepFaultModelLabelsTask = Task.Run(() => GetXepFaultModelLabelsByDtcFOrtEcuVariantAsync(refFaultLabel, vehicle));
            Task<IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTLABELS>>> xepFaultLabelsTask = Task.Run(() => GetXepFaultLabelsByDtcFOrtEcuVariantAsync(vehicle, ffmDynamicResolver, refFaultLabel));
            await Task.WhenAll(xepFaultModelLabelsTask, xepFaultLabelsTask).ConfigureAwait(continueOnCapturedContext: false);
            foreach (Fault fault in vehicle.FaultList)
            {
                fault.ResolveLabels(vehicle, ffmDynamicResolver, xepFaultModelLabelsTask.Result, xepFaultLabelsTask.Result, language);
            }
        }

        [PreserveSource(Hint = "Vehicle added", SignatureModified = true)]
        private static async Task<IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>>> GetXepFaultModelLabelsByDtcFOrtEcuVariantAsync(IDictionary<FaultCodeIdDtcFOrtEcuVariantKey, ICollection<decimal>> refFaultLabel, Vehicle vehicle)
        {
            Collection<decimal> reffaultLabelsLabelIds = new Collection<decimal>();
            refFaultLabel.ForEach(delegate (KeyValuePair<FaultCodeIdDtcFOrtEcuVariantKey, ICollection<decimal>> x)
            {
                reffaultLabelsLabelIds.AddRange(x.Value);
            });
            IEnumerable<decimal> enumerable = reffaultLabelsLabelIds.Distinct();
            IDictionary<decimal, XEP_FAULTMODELABELS> dictionary2;
            if (!enumerable.Any())
            {
                IDictionary<decimal, XEP_FAULTMODELABELS> dictionary = new Dictionary<decimal, XEP_FAULTMODELABELS>();
                dictionary2 = dictionary;
            }
            else
            {
                //[-] dictionary2 = DatabaseProviderFactory.Instance.GetFaultModelLabelsByIds(enumerable);
                //[+] dictionary2 = ClientContext.GetDatabase(vehicle)?.GetFaultModelLabelsByIds(enumerable);
                dictionary2 = ClientContext.GetDatabase(vehicle)?.GetFaultModelLabelsByIds(enumerable);
            }
            IDictionary<decimal, XEP_FAULTMODELABELS> modelFaultLabelAll = dictionary2;
            Dictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>> faultListFault = new Dictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>>(refFaultLabel.Count);
            DtcFOrtEcuVariantKey key;
            foreach (FaultCodeIdDtcFOrtEcuVariantKey key2 in refFaultLabel.Keys)
            {
                key = key2.GetDtcFOrtEcuVariantKey();
                if (!faultListFault.ContainsKey(key))
                {
                    faultListFault.Add(key, new Collection<XEP_FAULTMODELABELS>());
                }
                refFaultLabel[key2].ForEach(delegate (decimal x)
                {
                    if (modelFaultLabelAll.ContainsKey(x) && !faultListFault[key].Contains(modelFaultLabelAll[x]))
                    {
                        faultListFault[key].Add(modelFaultLabelAll[x]);
                    }
                });
            }
            return await Task.FromResult(faultListFault);
        }

        private static async Task<IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTLABELS>>> GetXepFaultLabelsByDtcFOrtEcuVariantAsync(Vehicle vehicle, IFFMDynamicResolver ffmDynamicResolver, IDictionary<FaultCodeIdDtcFOrtEcuVariantKey, ICollection<decimal>> refFaultLabel)
        {
            Collection<FaultCodeIdDtcFOrtEcuVariantKey> collection = new Collection<FaultCodeIdDtcFOrtEcuVariantKey>();
            Collection<decimal> collection2 = new Collection<decimal>();
            foreach (FaultCodeIdDtcFOrtEcuVariantKey key2 in refFaultLabel.Keys)
            {
                //[-] if (DatabaseProviderFactory.Instance.EvaluateXepRulesById(key2.FaultId, vehicle, ffmDynamicResolver))
                //[+] if (ClientContext.GetDatabase(vehicle)?.EvaluateXepRulesById(key2.FaultId.ToString(CultureInfo.InvariantCulture), vehicle, ffmDynamicResolver) == true)
                if (ClientContext.GetDatabase(vehicle)?.EvaluateXepRulesById(key2.FaultId.ToString(CultureInfo.InvariantCulture), vehicle, ffmDynamicResolver) == true)
                {
                        collection.Add(key2);
                    collection2.AddRange(refFaultLabel[key2]);
                }
            }
            Dictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTLABELS>> xepFaultLabelsList = new Dictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTLABELS>>(collection.Count);
            if (!collection.Any() || !collection2.Any())
            {
                return await Task.FromResult(xepFaultLabelsList);
            }
            //[-] IDictionary<decimal, XEP_FAULTLABELS> xepFaultLabels = DatabaseProviderFactory.Instance.GetFaultLabelXepFaultLabelByCodesAndIds(collection.Select((FaultCodeIdDtcFOrtEcuVariantKey x) => x.DtcF_Ort), collection2.Distinct());
            //[+] IDictionary<decimal, XEP_FAULTLABELS> xepFaultLabels = ClientContext.GetDatabase(vehicle)?.GetFaultLabelXepFaultLabelByCodesAndIds(collection.Select((FaultCodeIdDtcFOrtEcuVariantKey x) => x.DtcF_Ort), collection2.Distinct());
            IDictionary<decimal, XEP_FAULTLABELS> xepFaultLabels = ClientContext.GetDatabase(vehicle)?.GetFaultLabelXepFaultLabelByCodesAndIds(collection.Select((FaultCodeIdDtcFOrtEcuVariantKey x) => x.DtcF_Ort), collection2.Distinct());
            DtcFOrtEcuVariantKey key;
            foreach (FaultCodeIdDtcFOrtEcuVariantKey item in collection)
            {
                key = item.GetDtcFOrtEcuVariantKey();
                if (!xepFaultLabelsList.ContainsKey(key))
                {
                    xepFaultLabelsList.Add(key, new Collection<XEP_FAULTLABELS>());
                }
                refFaultLabel[item].ForEach(delegate (decimal x)
                {
                    if (xepFaultLabels.ContainsKey(x) && !xepFaultLabelsList[key].Contains(xepFaultLabels[x]))
                    {
                        xepFaultLabelsList[key].Add(xepFaultLabels[x]);
                    }
                });
            }
            return await Task.FromResult(xepFaultLabelsList);
        }

        public static Vehicle Deserialize(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    Log.Warning(Log.CurrentMethod() + "()", "file doesn't exist: {0}", filename);
                    return null;
                }
                using (FileStream input = File.OpenRead(filename))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(input))
                    {
                        Vehicle obj = (Vehicle)new XmlSerializer(typeof(Vehicle)).Deserialize(xmlReader);
                        obj.CalculateFaultProperties();
                        return obj;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod() + "()", exception);
            }
            return null;
        }

        public static Vehicle DeepClone(this Vehicle vehicle)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Vehicle));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    xmlSerializer.Serialize(memoryStream, vehicle);
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    Vehicle obj = (Vehicle)xmlSerializer.Deserialize(memoryStream);
                    obj.CalculateFaultProperties();
                    return obj;
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
                Log.Info(Log.CurrentMethod(), "Trying reflection based fallback.");
                try
                {
                    return DeepCloneUtility.DeepClone(vehicle);
                }
                catch (Exception exception2)
                {
                    Log.WarningException(Log.CurrentMethod(), exception2);
                    throw;
                }
            }
        }

        public static bool IsVINLessEReihe(this Vehicle vehicle)
        {
            string ereihe = vehicle.Ereihe;
            if (ereihe != null)
            {
                int length = ereihe.Length;
                if (length != 3)
                {
                    if (length == 4)
                    {
                        char c = ereihe[3];
                        if ((uint)c <= 67u)
                        {
                            if (c != '9')
                            {
                                if (c != 'C' || !(ereihe == "259C"))
                                {
                                    goto IL_01b5;
                                }
                            }
                            else
                            {
                                switch (ereihe)
                                {
                                    case "K569":
                                    case "K589":
                                    case "K599":
                                    case "E169":
                                    case "E189":
                                        break;
                                    default:
                                        goto IL_01b5;
                                }
                            }
                            goto IL_01b3;
                        }
                        if (c != 'E')
                        {
                            if (c != 'R')
                            {
                                if (c == 'S' && ereihe == "259S")
                                {
                                    goto IL_01b3;
                                }
                            }
                            else if (ereihe == "259R")
                            {
                                goto IL_01b3;
                            }
                        }
                        else if (ereihe == "247E")
                        {
                            goto IL_01b3;
                        }
                    }
                }
                else
                {
                    switch (ereihe[2])
                    {
                        case '1':
                            break;
                        case '2':
                            goto IL_00c3;
                        case '8':
                            goto IL_00d8;
                        case '7':
                            goto IL_00fd;
                        case '9':
                            goto IL_0112;
                        case '0':
                            goto IL_0127;
                        default:
                            goto IL_01b5;
                    }
                    if (ereihe == "K41" || ereihe == "R21")
                    {
                        goto IL_01b3;
                    }
                }
            }
            goto IL_01b5;
            IL_00fd:
            if (ereihe == "247")
            {
                goto IL_01b3;
            }
            goto IL_01b5;
            IL_0127:
            if (ereihe == "K30")
            {
                goto IL_01b3;
            }
            goto IL_01b5;
            IL_0112:
            if (ereihe == "259")
            {
                goto IL_01b3;
            }
            goto IL_01b5;
            IL_01b3:
            return true;
            IL_00c3:
            if (ereihe == "R22")
            {
                goto IL_01b3;
            }
            goto IL_01b5;
            IL_00d8:
            if (ereihe == "R28" || ereihe == "248")
            {
                goto IL_01b3;
            }
            goto IL_01b5;
            IL_01b5:
            return false;
        }

        public static ECU GetECUbyDTC(this Vehicle vehicle, decimal id)
        {
            if (vehicle.ECU != null)
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.FEHLER != null)
                    {
                        foreach (DTC item2 in item.FEHLER)
                        {
                            if (id.Equals(item2.Id))
                            {
                                return item;
                            }
                        }
                    }
                    if (item.INFO == null)
                    {
                        continue;
                    }
                    foreach (DTC item3 in item.INFO)
                    {
                        if (id.Equals(item3.Id))
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

        public static DTC GetDTC(this Vehicle vehicle, decimal id)
        {
            if (vehicle.ECU != null)
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.FEHLER != null)
                    {
                        foreach (DTC item2 in item.FEHLER)
                        {
                            if (id.Equals(item2.Id))
                            {
                                return item2;
                            }
                        }
                    }
                    if (item.INFO == null)
                    {
                        continue;
                    }
                    foreach (DTC item3 in item.INFO)
                    {
                        if (id.Equals(item3.Id))
                        {
                            return item3;
                        }
                    }
                }
            }
            if (vehicle.CombinedFaults != null)
            {
                return vehicle.CombinedFaults.FirstOrDefault(delegate (DTC item)
                {
                    decimal? id2 = item.Id;
                    decimal num = id;
                    return (id2.GetValueOrDefault() == num) & id2.HasValue;
                });
            }
            return null;
        }

        public static void CalculateFaultProperties(this Vehicle vehicle, IFFMDynamicResolver ffmResolver = null)
        {
            ObservableCollection<Fault> observableCollection = CalculateFaultList(vehicle, vehicle.ECU, vehicle.CombinedFaults, vehicle.ZFS, ffmResolver);
            //[-] SessionInfoAccessor.SessionInfo.FaultCodeSum = CalculateFaultCodeSum(vehicle.ECU, observableCollection, onlyNonSignalFaultDtcs: false);
            //[-] SessionInfoAccessor.SessionInfo.NonSignalErrorFaultCodeSum = CalculateFaultCodeSum(vehicle.ECU, observableCollection, onlyNonSignalFaultDtcs: true);
            //[+] SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
            SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
            //[+] if (sessionInfo != null) sessionInfo.FaultCodeSum = CalculateFaultCodeSum(vehicle.ECU, observableCollection, onlyNonSignalFaultDtcs: false);
            if (sessionInfo != null) sessionInfo.FaultCodeSum = CalculateFaultCodeSum(vehicle.ECU, observableCollection, onlyNonSignalFaultDtcs: false);
            //[+] if (sessionInfo != null) sessionInfo.NonSignalErrorFaultCodeSum = CalculateFaultCodeSum(vehicle.ECU, observableCollection, onlyNonSignalFaultDtcs: true);
            if (sessionInfo != null) sessionInfo.NonSignalErrorFaultCodeSum = CalculateFaultCodeSum(vehicle.ECU, observableCollection, onlyNonSignalFaultDtcs: true);
            Log.Info("Vehicle.CalculateFaultProperties()", "FaultCodeSum changed from \"{0}\" to \"{1}\".", vehicle.FaultList?.Count, sessionInfo.FaultCodeSum);
            vehicle.FaultList = new List<Fault>(observableCollection);
        }

        private static int? CalculateFaultCodeSum(IEnumerable<IEcu> ecus, IEnumerable<Fault> faults, bool onlyNonSignalFaultDtcs)
        {
            int num = 0;
            num = (onlyNonSignalFaultDtcs ? faults.Count((Fault f) => f.FaultGroupNumber != 6) : faults.Count());
            if (num == 0 && (ecus == null || !ecus.Any() || ecus.Any((IEcu item) => !item.FS_SUCCESSFULLY && !item.BUS.ToString().Contains("VIRTUAL"))))
            {
                return null;
            }
            return num;
        }

        private static ObservableCollection<Fault> CalculateFaultList(Vehicle vehicle, IEnumerable<ECU> ecus, IEnumerable<DTC> combinedFaults, ObservableCollection<ZFSResult> zfs, IFFMDynamicResolver ffmFesolver = null)
        {
            bool flag = true;
            bool flag2 = true;
            if (ConfigSettings.OperationalMode != OperationalMode.ISTA)
            {
                flag = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideBogusFaults", defaultValue: true);
                flag2 = ConfigSettings.getConfigStringAsBoolean("TesterGUI.HideUnknownFaults", defaultValue: false);
            }
            ObservableCollection<Fault> observableCollection = new ObservableCollection<Fault>();
            try
            {
                if (ecus != null)
                {
                    foreach (ECU item in ecus.Where((ECU item) => item.FEHLER != null))
                    {
                        foreach (DTC item2 in item.FEHLER)
                        {
                            Fault fault = new Fault(item, item2, zfs, vehicle.Classification.IsNewFaultMemoryActive);
                            if (item2.Relevance == true)
                            {
                                if (ffmFesolver != null && ConfigSettings.getConfigStringAsBoolean("EnableRelevanceFaultCode", defaultValue: true))
                                {
                                    fault.ResolveRelevanceFaultCode(vehicle, ffmFesolver);
                                    if (fault.DTC.Relevance == true)
                                    {
                                        observableCollection.AddIfNotContains(fault);
                                    }
                                }
                                else
                                {
                                    observableCollection.AddIfNotContains(fault);
                                }
                            }
                            else if (item2.Relevance == false && !flag)
                            {
                                observableCollection.AddIfNotContains(new Fault(item, item2, zfs, vehicle.Classification.IsNewFaultMemoryActive));
                            }
                            else if (!item2.Relevance.HasValue && !flag2)
                            {
                                observableCollection.AddIfNotContains(new Fault(item, item2, zfs, vehicle.Classification.IsNewFaultMemoryActive));
                            }
                        }
                    }
                }
                if (combinedFaults == null)
                {
                    return observableCollection;
                }
                foreach (DTC combinedFault in combinedFaults)
                {
                    Fault fault2 = new Fault(null, combinedFault, null, vehicle.Classification.IsNewFaultMemoryActive);
                    fault2.ResolveLabels(vehicle, null);
                    observableCollection.AddIfNotContains(fault2);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Vehicle.CalculateFaultList()", exception);
            }
            return observableCollection;
        }

        public static typeECU_Transaction getECUTransaction(ECU transECU, string transId)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }
            if (transECU == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(transId))
            {
                return null;
            }
            try
            {
                if (transECU.TAL != null)
                {
                    foreach (typeECU_Transaction item in transECU.TAL)
                    {
                        if (string.Compare(item.transactionId, transId, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECUTransaction()", exception);
            }
            return null;
        }

        public static bool HasUnidentifiedECU(this Vehicle vehicle)
        {
            bool flag = false;
            if (vehicle.ECU != null)
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (string.IsNullOrEmpty(item.VARIANTE) || !item.COMMUNICATION_SUCCESSFULLY)
                    {
                        flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
                    }
                }
                return flag;
            }
            return true;
        }

        public static ECU getECU(this Vehicle vehicle, long? sgAdr)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }
            try
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.ID_SG_ADR == sgAdr)
                    {
                        return item;
                    }
                    if (!string.IsNullOrEmpty(item.ECU_ADR))
                    {
                        string text = string.Empty;
                        if (item.ECU_ADR.Length >= 4 && item.ECU_ADR.Substring(0, 2).ToLower() == "0x")
                        {
                            text = item.ECU_ADR.ToUpper().Substring(2);
                        }
                        if (item.ECU_ADR.Length == 2)
                        {
                            text = item.ECU_ADR.ToUpper();
                        }
                        if (text == string.Format(CultureInfo.InvariantCulture, "{0:X2}", sgAdr))
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECU()", exception);
            }
            return null;
        }

        public static ECU getECU(this Vehicle vehicle, long? sgAdr, long? subAddress)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }
            try
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.ID_SG_ADR == sgAdr && item.ID_LIN_SLAVE_ADR == subAddress)
                    {
                        return item;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehcile.getECU()", exception);
            }
            return null;
        }

        public static ECU getECUbyECU_GRUPPE(this Vehicle vehicle, string ECU_GRUPPE)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }
            if (string.IsNullOrEmpty(ECU_GRUPPE))
            {
                Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "parameter was null or empty");
                return null;
            }
            if (vehicle.ECU == null)
            {
                Log.Warning("Vehicle.getECUbyECU_GRUPPE()", "ECU was null");
                return null;
            }
            try
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (string.IsNullOrEmpty(item.ECU_GRUPPE))
                    {
                        continue;
                    }
                    string[] array = ECU_GRUPPE.Split('|');
                    string[] array2 = item.ECU_GRUPPE.Split('|');
                    foreach (string a in array2)
                    {
                        string[] array3 = array;
                        foreach (string b in array3)
                        {
                            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getECUbyECU_GRUPPE()", exception);
            }
            return null;
        }

        public static uint getDiagProtECUCount(this Vehicle vehicle, typeDiagProtocoll ecuDiag)
        {
            if (!CoreFramework.validLicense)
            {
                throw new Exception("This copy of CoreFramework.dll is not licensed !!!");
            }
            uint num = 0u;
            try
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.DiagProtocoll == ecuDiag)
                    {
                        num++;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehcile.getECU()", exception);
            }
            return num;
        }

        public static typeCBSInfo getCBSMeasurementValue(this Vehicle vehicle, typeCBSMeaurementType mType)
        {
            try
            {
                if (vehicle.CBS == null)
                {
                    return null;
                }
                foreach (typeCBSInfo cB in vehicle.CBS)
                {
                    if (cB.Type == mType)
                    {
                        return cB;
                    }
                }
                return null;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getCBSMeasurementValue()", exception);
            }
            return null;
        }

        public static bool addOrUpdateCBSMeasurementValue(this Vehicle vehicle, typeCBSInfo cbsNew)
        {
            try
            {
                if (cbsNew == null)
                {
                    return false;
                }
                if (vehicle.CBS == null)
                {
                    vehicle.CBS = new ObservableCollection<typeCBSInfo>();
                }
                foreach (typeCBSInfo cB in vehicle.CBS)
                {
                    if (cB.Type == cbsNew.Type)
                    {
                        vehicle.CBS.Remove(cB);
                        vehicle.CBS.Add(cbsNew);
                        return true;
                    }
                }
                vehicle.CBS.Add(cbsNew);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.addOrUpdateCBSMeasurementValue()", exception);
            }
            return false;
        }

        public static bool addOrUpdateCBSMeasurementValues(this Vehicle vehicle, IList<typeCBSInfo> cbsNewList)
        {
            try
            {
                if (cbsNewList == null)
                {
                    return false;
                }
                if (vehicle.CBS == null)
                {
                    vehicle.CBS = new ObservableCollection<typeCBSInfo>();
                }
                foreach (typeCBSInfo cbsNew in cbsNewList)
                {
                    bool flag = false;
                    foreach (typeCBSInfo cB in vehicle.CBS)
                    {
                        if (cB.Type == cbsNew.Type)
                        {
                            int num = vehicle.CBS.IndexOf(cB);
                            if (num >= 0 && num < vehicle.CBS.Count)
                            {
                                vehicle.CBS[num] = cbsNew;
                            }
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        vehicle.CBS.Add(cbsNew);
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.addOrUpdateCBSMeasurementValue()", exception);
            }
            return false;
        }

        public static void AddEcu(this Vehicle vehicle, ECU ecu)
        {
            vehicle.ECU.Add(ecu);
        }

        public static bool AddOrUpdateECU(this Vehicle vehicle, ECU nECU)
        {
            try
            {
                if (nECU == null)
                {
                    Log.Warning("Vehicle.AddOrUpdateECU()", "ecu was null");
                    return false;
                }
                if (vehicle.ECU == null)
                {
                    vehicle.ECU = new ObservableCollection<ECU>();
                }
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.ID_SG_ADR == nECU.ID_SG_ADR)
                    {
                        int num = vehicle.ECU.IndexOf(item);
                        if (num >= 0 && num < vehicle.ECU.Count)
                        {
                            vehicle.ECU[num] = nECU;
                            Log.Info("Vehicle.AddOrUpdateECU()", "updating ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                            return true;
                        }
                    }
                }
                vehicle.ECU.Add(nECU);
                Log.Info("Vehicle.AddOrUpdateECU()", "adding ecu: \"{0:X2}\" (hex.), slave address: \"{1:X2}\" (hex.).", nECU.ID_SG_ADR, nECU.ID_LIN_SLAVE_ADR);
                return true;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.AddOrUpdateECU()", exception);
            }
            return false;
        }

        public static bool getISTACharacteristics(this Vehicle vehicle, decimal id, out string value, long datavalueId, ValidationRuleInternalResults internalResult)
        {
            //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
            //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
            PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
            //[-] IXepCharacteristicRoots characteristicRootsById = instance.GetCharacteristicRootsById(id);
            //[+] PsdzDatabase.CharacteristicRoots characteristicRootsById = instance.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
            PsdzDatabase.CharacteristicRoots characteristicRootsById = instance.GetCharacteristicRootsById(id.ToString(CultureInfo.InvariantCulture));
            if (characteristicRootsById != null)
            {
                //[-] return new VehicleCharacteristicVehicleHelper(instance, vehicle).GetISTACharacteristics(characteristicRootsById.Nodeclass, out value, id, vehicle, datavalueId, internalResult);
                //[+] return new VehicleCharacteristicVehicleHelper(vehicle).GetISTACharacteristics(characteristicRootsById.NodeClass, out value, id, vehicle, datavalueId, internalResult);
                return new VehicleCharacteristicVehicleHelper(vehicle).GetISTACharacteristics(characteristicRootsById.NodeClass, out value, id, vehicle, datavalueId, internalResult);
            }
            Log.Warning("Vehicle.getISTACharactersitics()", "No entry found in CharacteristicRoots for id: {0}!", id);
            value = "???";
            return false;
        }

        public static void UpdateStatus(this Vehicle vehicle, string name, StateType type, double? progress)
        {
            try
            {
                //[+] SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
                SessionInfo sessionInfo = ClientContext.GetClientContext(vehicle)?.SessionInfo;
                //[+] if (sessionInfo == null) return;
                if (sessionInfo == null) return;
                //[-] string status_FunctionName = SessionInfoAccessor.SessionInfo.Status_FunctionName;
                //[+] string status_FunctionName = sessionInfo.Status_FunctionName;
                string status_FunctionName = sessionInfo.Status_FunctionName;
                StateType status_FunctionState = vehicle.Status_FunctionState;
                Log.Info("Vehicle.UpdateStatus()", "Change state from '{0}/{1}' to '{2}/{3}'", status_FunctionName, status_FunctionState, name, type);
                //[-] SessionInfoAccessor.SessionInfo.Status_FunctionName = name;
                //[+] sessionInfo.Status_FunctionName = name;
                sessionInfo.Status_FunctionName = name;
                vehicle.Status_FunctionState = type;
                if (progress.HasValue)
                {
                    //[-] SessionInfoAccessor.SessionInfo.Status_FunctionProgress = progress.Value;
                    //[+] sessionInfo.Status_FunctionProgress = progress.Value;
                    sessionInfo.Status_FunctionProgress = progress.Value;
                }
                //[-] SessionInfoAccessor.SessionInfo.IsNoVehicleCommunicationRunning = vehicle.Status_FunctionState != StateType.running;
                //[+] sessionInfo.IsNoVehicleCommunicationRunning = vehicle.Status_FunctionState != StateType.running;
                sessionInfo.IsNoVehicleCommunicationRunning = vehicle.Status_FunctionState != StateType.running;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.UpdateStatus()", exception);
            }
        }

        public static bool evalILevelExpression(this Vehicle vehicle, string iLevelExpressions)
        {
            bool flag = false;
            bool flag2 = true;
            try
            {
                if (string.IsNullOrEmpty(iLevelExpressions))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(vehicle.ILevel))
                {
                    Log.Info("Vehicle.evaILevelExpression()", "ILevel unknown; result will be true; expression was: {0}", iLevelExpressions);
                    return true;
                }
                if (iLevelExpressions.Contains("&"))
                {
                    flag2 = false;
                    flag = true;
                }
                if (CoreFramework.DebugLevel > 0)
                {
                    Log.Info("Vehicle.evalILevelExpression()", "expression:{0} vehicle iLEVEL:{1}", iLevelExpressions, vehicle.ILevel);
                }
                string[] separator = new string[2] { "&", "|" };
                string[] array = iLevelExpressions.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string text in array)
                {
                    string[] separator2 = new string[1] { "," };
                    string[] array2 = text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
                    if (array2.Length != 2)
                    {
                        continue;
                    }
                    Log.Info("Vehicle.evalILevelExpression()", "expression {0} {1}", vehicle.ILevel, text);
                    if (string.Compare(vehicle.ILevel, 0, array2[1], 0, 4, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        switch (array2[0])
                        {
                            case ">":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(vehicle.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "> was true");
                                }
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) > FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(vehicle.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "< was true");
                                }
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) < FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(vehicle.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "= was true");
                                }
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) == FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case ">=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(vehicle.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", ">= was true");
                                }
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) >= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "<=":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(vehicle.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "<= was true");
                                }
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) <= FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                            case "!=":
                            case "<>":
                                if (CoreFramework.DebugLevel > 0 && FormatConverter.ExtractNumericalILevel(vehicle.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))
                                {
                                    Log.Info("Vehicle.evalILevelExpression()", "!= was true");
                                }
                                flag = ((!flag2) ? (flag & (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))) : (flag | (FormatConverter.ExtractNumericalILevel(vehicle.ILevel) != FormatConverter.ExtractNumericalILevel(array2[1]))));
                                break;
                        }
                    }
                    else
                    {
                        Log.Warning("Vehicle.evalILevelExpression()", "iLevel main type does not match");
                    }
                }
                return flag;
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.evalILevelExpression()", exception);
                return true;
            }
        }

        public static bool isECUAlreadyScanned(this Vehicle vehicle, ECU checkSG)
        {
            try
            {
                foreach (ECU item in vehicle.ECU)
                {
                    if (item.ID_SG_ADR == checkSG.ID_SG_ADR)
                    {
                        return true;
                    }
                    if (!string.IsNullOrEmpty(item.ECU_ADR) && !string.IsNullOrEmpty(checkSG.ECU_ADR) && string.Compare(item.ECU_ADR, checkSG.ECU_ADR, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.isECUAlreadyScanned()", exception);
            }
            return false;
        }

        public static T getResultAs<T>(this Vehicle vehicle, string resultName)
        {
            try
            {
                Type typeFromHandle = typeof(T);
                if (!string.IsNullOrEmpty(resultName))
                {
                    object obj = null;
                    switch (resultName)
                    {
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/BaureihenVerbund":
                            obj = vehicle.BasisEReihe;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/IStufe":
                            obj = vehicle.ILevel;
                            break;
                        case "/VehicleConfiguration.RootNode/GetGroupListEx/Arguments/Fahrzeugauftrag":
                            obj = vehicle.FA.STANDARD_FA;
                            break;
                        case "/Result/DList":
                        case "/Result/GruppenListe":
                            {
                                string text4 = string.Empty;
                                foreach (ECU item in vehicle.ECU)
                                {
                                    text4 = text4 + item.ECU_GRUPPE + ",";
                                }
                                text4 = text4.TrimEnd(',');
                                obj = text4;
                                break;
                            }
                        case "/Result/SonderAusstattungsListe":
                            {
                                string text3 = string.Empty;
                                foreach (string item2 in vehicle.FA.SA)
                                {
                                    text3 = text3 + item2 + ",";
                                }
                                text3 = text3.TrimEnd(',');
                                obj = text3;
                                break;
                            }
                        case "/Result/EWortListe":
                            {
                                string text2 = string.Empty;
                                foreach (string item3 in vehicle.FA.E_WORT)
                                {
                                    text2 = text2 + item3 + ",";
                                }
                                text2 = text2.TrimEnd(',');
                                obj = text2;
                                break;
                            }
                        case "/Result/HOWortListe":
                            {
                                string text = string.Empty;
                                foreach (string item4 in vehicle.FA.HO_WORT)
                                {
                                    text = text + item4 + ",";
                                }
                                text = text.TrimEnd(',');
                                obj = text;
                                break;
                            }
                        case "/Result/Baustand":
                            obj = vehicle.FA.C_DATE;
                            break;
                        default:
                            Log.Error("VehicleHelper.getResultAs<T>", "Unknown resultName '{0}' found!", resultName);
                            break;
                    }
                    if (obj != null)
                    {
                        if (obj.GetType() != typeFromHandle)
                        {
                            return (T)Convert.ChangeType(obj, typeFromHandle);
                        }
                        return (T)obj;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Vehicle.getISTAResultAs(string resultName)", exception);
            }
            return default(T);
        }

        public static void AddDiagCode(this Vehicle vehicle, string diagCodeString, string diagCodeSuffixString, string originatingAblauf, IList<string> reparaturPaketList)
        {
            if (!string.IsNullOrEmpty(diagCodeString))
            {
                if (vehicle.DiagCodes == null)
                {
                    vehicle.DiagCodes = new ObservableCollection<typeDiagCode>();
                }
                typeDiagCode typeDiagCode2 = new typeDiagCode();
                typeDiagCode2.DiagnoseCode = diagCodeString;
                typeDiagCode2.DiagnoseCodeSuffix = diagCodeSuffixString;
                typeDiagCode2.Origin = ((originatingAblauf == null) ? string.Empty : originatingAblauf);
                if (reparaturPaketList != null)
                {
                    typeDiagCode2.ReparaturPaket = new ObservableCollection<string>(reparaturPaketList);
                }
                else
                {
                    typeDiagCode2.ReparaturPaket = new ObservableCollection<string>();
                }
                vehicle.DiagCodes.Add(typeDiagCode2);
                if (!string.IsNullOrEmpty(diagCodeString) && !vehicle.DiagCodesProgramming.Contains(diagCodeString))
                {
                    vehicle.DiagCodesProgramming.Add(diagCodeString);
                }
            }
        }

        public static bool? IsABSVehicle(this Vehicle vehicle)
        {
            if (vehicle.ECU != null && vehicle.ECU.Count > 0)
            {
                string[] array = new string[16]
                {
                "ASCMK20", "absmk4", "absmk4g", "abs5", "abs_uc", "asc4gus", "asc5", "asc57", "asc57r75", "asc5d",
                "ascmk20", "ascmk4.prg", "ascmk4g", "ascmk4g1", "asc_l22", "asc_t"
                };
                ECU eCU = vehicle.getECU(86L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                eCU = vehicle.getECU(41L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                eCU = vehicle.getECU(54L, null);
                if (eCU != null && eCU.IDENT_SUCCESSFULLY)
                {
                    string[] array2 = array;
                    for (int i = 0; i < array2.Length; i++)
                    {
                        if (array2[i].Equals(eCU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            return null;
        }

        public static void AddCombinedDTC(this Vehicle vehicle, DTC dtc)
        {
            if (dtc == null)
            {
                Log.Warning("Vehicle.AddCombinedDTC()", "dtc was null");
            }
            else if (dtc.IsVirtual && dtc.IsCombined && vehicle.CombinedFaults != null)
            {
                vehicle.CombinedFaults.AddIfNotContains(dtc);
            }
        }

        public static bool GetProgrammingEnabledForBn(this Vehicle vehicle, string bn)
        {
            return GetBnTypes(bn).Contains(vehicle.BNType);
        }

        private static ISet<BNType> GetBnTypes(string bnTypes)
        {
            ISet<BNType> set = new HashSet<BNType>();
            if (string.IsNullOrEmpty(bnTypes))
            {
                return set;
            }
            string[] array = bnTypes.Split(',');
            foreach (string text in array)
            {
                if (Enum.TryParse<BNType>(text, ignoreCase: false, out var result))
                {
                    set.Add(result);
                    continue;
                }
                Log.Error("Vehicle.GetBnTypes()", "Ignore BN \"{0}\", because of missconfiguration.", text);
            }
            return set;
        }

        public static int GetFaultListHashCode(this Vehicle vehicle, FaultFilter filter, IEnumerable<XEP_PERCEIVEDSYMPTOMSEX> perceivedSymptoms, List<Fault> selectedFaults = null)
        {
            int num = 37;
            int num2 = 327;
            IList<int> faultGroupNumbers = filter.FaultGroupNumbers;
            IEnumerable<(string, string)> enumerable = null;
            IEnumerable<string> enumerable2 = null;
            if (selectedFaults != null)
            {
                enumerable = from f in selectedFaults
                             where !f.DTC.IsCombined && (f.DTC.FaultGroup == 0 || faultGroupNumbers.Contains(f.DTC.FaultGroup))
                             orderby f.ECU.VARIANTE, f.DTC.FortAsHexString
                             select (VARIANTE: f.ECU.VARIANTE, FortAsHexString: f.DTC.FortAsHexString);
                enumerable2 = from f in selectedFaults
                              where f.DTC.IsCombined && (f.DTC.FaultGroup == 0 || faultGroupNumbers.Contains(f.DTC.FaultGroup))
                              orderby f.DTC.FortAsHexString
                              select f.DTC.FortAsHexString;
            }
            else
            {
                enumerable = vehicle.ECU.OrderBy((ECU e) => e.VARIANTE).SelectMany((ECU ecu) => from dtc in ecu.FEHLER
                                                                                                where dtc.Relevance == true && (dtc.FaultGroup == 0 || faultGroupNumbers.Contains(dtc.FaultGroup))
                                                                                                orderby dtc.FortAsHexString
                                                                                                select (VARIANTE: ecu.VARIANTE, FortAsHexString: dtc.FortAsHexString));
                enumerable2 = from f in vehicle.CombinedFaults
                              where f.Relevance == true && (f.FaultGroup == 0 || faultGroupNumbers.Contains(f.FaultGroup))
                              orderby f.FortAsHexString
                              select f.FortAsHexString;
            }
            foreach (var item in enumerable)
            {
                num += item.Item1?.GetHashCode() ?? 0;
                num += item.Item2.GetHashCode();
            }
            num *= num2;
            if (perceivedSymptoms != null)
            {
                foreach (XEP_PERCEIVEDSYMPTOMSEX item2 in perceivedSymptoms.OrderBy((XEP_PERCEIVEDSYMPTOMSEX s) => s.Id))
                {
                    num += item2.Id.GetHashCode();
                }
                num *= num2;
            }
            foreach (string item3 in enumerable2)
            {
                num += item3.GetHashCode();
            }
            return num * num2;
        }
    }
}
