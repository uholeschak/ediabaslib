using BMW.Authoring.Vehicle;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.DatabaseProvider.DatabaseProviderHelper;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class Fault : INotifyPropertyChanged
    {
        public const string OptimizedFaultMemoryDefaultMileageDisplayFormat = "0.0";
        public const string MileageSortName = "DTC.Mileage";
        private DTC dtc;
        private ECU ecu;
        private string faultLabel;
        private ILocalizedTitle xepFaultLabel;
        private string warningLabel;
        private string existingLabel;
        private string symptomLabel;
        private string classLabel;
        private string pKode;
        private ObservableCollection<string> artLabel;
        private XEP_FAULTMODELABELS dtcFVorhandenNr;
        private string existing;
        private string faultGroupLabel;
        private bool isNewFaultMemoryActive;
        protected string faultClass;
        public bool IsCheckControlMessage => FaultGroupNumber == 7;

        public double Mileage
        {
            get
            {
                if (RelevantDtcContextIndex.HasValue)
                {
                    return DTC.DTCContext[RelevantDtcContextIndex.Value].Mileage;
                }

                return DTC.Mileage;
            }
        }

        public double? Timestamp
        {
            get
            {
                if (RelevantDtcContextIndex.HasValue)
                {
                    double? f_UW_ZEIT_SUPREME = DTC.DTCContext[RelevantDtcContextIndex.Value].F_UW_ZEIT_SUPREME;
                    if (!f_UW_ZEIT_SUPREME.HasValue || !(f_UW_ZEIT_SUPREME > 0.0))
                    {
                        return DTC.DTCContext[RelevantDtcContextIndex.Value].F_UW_ZEIT;
                    }

                    return f_UW_ZEIT_SUPREME;
                }

                if (!DTC.F_UW_ZEIT_SUPREME.HasValue || !(DTC.F_UW_ZEIT_SUPREME > 0.0))
                {
                    return DTC.F_UW_ZEIT;
                }

                return DTC.F_UW_ZEIT_SUPREME;
            }
        }

        public Fault Parent { get; set; }
        public int? RelevantDtcContextIndex { get; set; }
        public bool ShowDivider { get; set; }
        public bool IsUsedForFaultPatternLocalization { get; set; }
        public bool IsUsedForIndividualTestPlanCalculation { get; set; }

        public DTC DTC
        {
            get
            {
                return dtc;
            }

            set
            {
                if (dtc != value)
                {
                    dtc = value;
                    OnPropertyChanged("DTC");
                }
            }
        }

        public bool IsNewFaultMemoryActive
        {
            get
            {
                return isNewFaultMemoryActive;
            }

            set
            {
                if (isNewFaultMemoryActive != value)
                {
                    isNewFaultMemoryActive = value;
                    OnPropertyChanged("IsNewFaultMemoryActive");
                }
            }
        }

        public ECU ECU
        {
            get
            {
                return ecu;
            }

            set
            {
                if (ecu != value)
                {
                    ecu = value;
                    OnPropertyChanged("ECU");
                }
            }
        }

        public string FaultClass => faultClass;

        public string FaultLabel
        {
            get
            {
                return faultLabel;
            }

            set
            {
                if (faultLabel != value)
                {
                    faultLabel = value;
                    OnPropertyChanged("FaultLabel");
                }
            }
        }

        public int Fault_Class => dtc.FaultClass;

        public string FaultGroupLabel
        {
            get
            {
                return faultGroupLabel;
            }

            set
            {
                if (faultGroupLabel != value)
                {
                    faultGroupLabel = value;
                    OnPropertyChanged("FaultGroupLabel");
                }
            }
        }

        public int FaultGroupNumber => dtc.FaultGroup;

        public bool FaultGroupHighlight
        {
            get
            {
                if (IsNewFaultMemoryActive)
                {
                    return FaultGroupNumber == 1;
                }

                return false;
            }
        }

        [XmlIgnore]
        public ILocalizedTitle XepFaultLabel
        {
            get
            {
                return xepFaultLabel;
            }

            set
            {
                xepFaultLabel = value;
                if (xepFaultLabel != null)
                {
                    FaultLabel = xepFaultLabel.Title;
                }
            }
        }

        public string WarningLabel
        {
            get
            {
                return warningLabel;
            }

            set
            {
                if (warningLabel != value)
                {
                    warningLabel = value;
                    OnPropertyChanged("WarningLabel");
                }
            }
        }

        public string SymptomLabel
        {
            get
            {
                return symptomLabel;
            }

            set
            {
                if (symptomLabel != value)
                {
                    symptomLabel = value;
                    OnPropertyChanged("SymptomLabel");
                }
            }
        }

        public string ExistingLabel
        {
            get
            {
                return existingLabel;
            }

            set
            {
                if (existingLabel != value)
                {
                    existingLabel = value;
                    OnPropertyChanged("ExistingLabel");
                }
            }
        }

        public string ClassLabel
        {
            get
            {
                return classLabel;
            }

            set
            {
                if (classLabel != value)
                {
                    classLabel = value;
                    OnPropertyChanged("ClassLabel");
                }
            }
        }

        public string PKode
        {
            get
            {
                return pKode;
            }

            set
            {
                if (pKode != value)
                {
                    pKode = value;
                    OnPropertyChanged("PKode");
                }
            }
        }

        public ObservableCollection<string> ArtLabel
        {
            get
            {
                return artLabel;
            }

            set
            {
                if (artLabel != value)
                {
                    artLabel = value;
                    OnPropertyChanged("ArtLabel");
                }
            }
        }

        public XEP_FAULTMODELABELS DtcFVorhandenNr
        {
            get
            {
                return dtcFVorhandenNr;
            }

            set
            {
                if (dtcFVorhandenNr != value)
                {
                    dtcFVorhandenNr = value;
                    OnPropertyChanged("DtcFVorhandenNr");
                }
            }
        }

        public string Existing
        {
            get
            {
                return existing;
            }

            set
            {
                existing = value;
                OnPropertyChanged("Existing");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public Fault()
        {
        }

        public Fault(ECU ecu, DTC dtc, ObservableCollection<ZFSResult> zfs, bool isNewFaultMemoryActive) : this()
        {
            this.ecu = ecu;
            this.dtc = dtc;
            IsNewFaultMemoryActive = isNewFaultMemoryActive;
            if (isNewFaultMemoryActive)
            {
                FaultGroupLabel = FormatedData.Localize($"#FaultGroup{FaultGroupNumber}");
            }

            faultClass = FaultCodeConverters.GetFaultClass(dtc, zfs);
        }

        public Fault(ECU ecu, DTC dtc, ObservableCollection<ZFSResult> zfs, string pKode, bool isNewFaultMemoryActive) : this(ecu, dtc, zfs, isNewFaultMemoryActive)
        {
            this.pKode = pKode;
        }

        private void SetExisting()
        {
            Existing = FormatedData.Localize("#UnknownSmall");
            if (DTC == null)
            {
                return;
            }

            if (DTC.IsVirtual)
            {
                Existing = FormatedData.Localize("#YesSmall");
                return;
            }

            switch (ECU.DiagProtocoll)
            {
                case typeDiagProtocoll.KWP:
                {
                    int? f_VORHANDEN_NR = DTC.F_VORHANDEN_NR;
                    if (f_VORHANDEN_NR.HasValue)
                    {
                        switch (f_VORHANDEN_NR.GetValueOrDefault())
                        {
                            case 32:
                            case 33:
                                Existing = FormatedData.Localize("#NoSmall");
                                break;
                            case 34:
                            case 35:
                                Existing = FormatedData.Localize("#YesSmall");
                                break;
                        }
                    }

                    break;
                }

                case typeDiagProtocoll.UDS:
                {
                    int? f_VORHANDEN_NR = DTC.F_VORHANDEN_NR;
                    if (f_VORHANDEN_NR.HasValue)
                    {
                        switch (f_VORHANDEN_NR.GetValueOrDefault())
                        {
                            case 5:
                            case 9:
                            case 13:
                            case 33:
                            case 37:
                            case 41:
                            case 45:
                                Existing = FormatedData.Localize("#YesSmall");
                                break;
                            case 4:
                            case 8:
                            case 12:
                            case 32:
                            case 36:
                            case 40:
                            case 44:
                                Existing = FormatedData.Localize("#NoSmall");
                                break;
                        }
                    }

                    break;
                }

                case typeDiagProtocoll.DS2:
                    break;
            }
        }

        public void ResolveRelevanceFaultCode(Vehicle vehicle, IFFMDynamicResolver resolver)
        {
            try
            {
                if (IsCheckControlMessage)
                {
                    return;
                }

                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                //[-] if (instance == null || instance.DatabaseAccessType == DatabaseType.None)
                //[+] if (instance == null)
                if (instance == null)
                {
                    return;
                }

                if (DTC.IsVirtual)
                {
                    DTC.RelevanceFaultCode = null;
                    Log.Info("Fault.ResolveRelevanceFaultCode()", "RelevanceFaultCode set to null for Virtual Fault");
                }
                else if (DTC.IsCombined)
                {
                    DTC.RelevanceFaultCode = null;
                    Log.Info("Fault.ResolveRelevanceFaultCode()", "RelevanceFaultCode set to null for Combined Fault");
                }
                else if (ConfigSettings.getConfigStringAsBoolean("EnableRelevanceFaultCode", defaultValue: true) && DTC.F_VORHANDEN_NR.HasValue)
                {
                    //[-] DTC.RelevanceFaultCode = instance.GetRelevanceFaultCodeByFaultCodeAndEcuVariant(DTC.F_ORT.ToString(), ECU.VARIANTE, vehicle, resolver);
                    if (DTC.RelevanceFaultCode != null && !DTC.RelevanceFaultCode.Contains(DTC.F_VORHANDEN_NR.Value))
                    {
                        Log.Info("Fault.ResolveRelevanceFaultCode", $"Relevance set to false for ecu variant: {ECU.VARIANTE} and f_ort: {DTC.F_ORT} due to fault code relevance rules from database.");
                        DTC.Relevance = false;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Fault.ResolveRelevanceFaultCode()", exception);
            }
        }

        public void ResolveLabels(Vehicle vehicle, IFFMDynamicResolver resolver, IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTMODELABELS>> xepFaultModelLabelsCollection = null, IDictionary<DtcFOrtEcuVariantKey, ICollection<XEP_FAULTLABELS>> xepFaultLabelsCollection = null, string language = null)
        {
            if (!string.IsNullOrEmpty(language))
            {
                ConfigSettings.CurrentUICulture = language;
            }

            try
            {
                if (IsCheckControlMessage)
                {
                    return;
                }

                //[-] IDatabaseProvider instance = DatabaseProviderFactory.Instance;
                //[+] PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                PsdzDatabase instance = ClientContext.GetDatabase(vehicle);
                SetExisting();
                //[-] if (instance == null || instance.DatabaseAccessType == DatabaseType.None)
                //[+] if (instance == null)
                if (instance == null)
                {
                    return;
                }

                if (DTC.IsVirtual || DTC.IsCombined)
                {
                    Log.Info("Fault.ResolveLabels()", "found unidentified ecu variant or virtual/combined fault: {0} {1:X}", DTC.Id, DTC.F_ORT);
                    if (DTC.IsVirtual && !DTC.IsCombined && ECU != null)
                    {
                        XEP_VIRTUALFAULTCODES xEP_VIRTUALFAULTCODES = null;
                        if (DTC.Id.HasValue)
                        {
                        //[-] xEP_VIRTUALFAULTCODES = DatabaseProviderFactory.Instance.GetVirtualFaultCodeById(DTC.Id.Value, vehicle, resolver);
                        }

                        if (xEP_VIRTUALFAULTCODES == null && !string.IsNullOrEmpty(ECU.ECU_GRUPPE))
                        {
                            string[] array = ECU.ECU_GRUPPE.Split('|');
                            string text = string.Format(CultureInfo.InvariantCulture, "S {0:X4}", DTC.F_ORT);
                            string[] array2 = array;
                            foreach (string ecuGroup in array2)
                            {
                                if (xEP_VIRTUALFAULTCODES == null)
                                {
                                    //[-] xEP_VIRTUALFAULTCODES = DatabaseProviderFactory.Instance.GetVirtualFaultCodeByCodeAndEcuGroup(text, ecuGroup, vehicle, resolver);
                                    //[+] xEP_VIRTUALFAULTCODES = null;
                                    xEP_VIRTUALFAULTCODES = null;
                                    if (xEP_VIRTUALFAULTCODES != null)
                                    {
                                        break;
                                    }
                                }
                            }

                            if (xEP_VIRTUALFAULTCODES == null)
                            {
                                Log.Warning("Fault.ResolveLabels()", "Unable to resolve label for: {0} ECU: {1}", text, ECU);
                            }
                        }

                        if (xEP_VIRTUALFAULTCODES != null)
                        {
                            //[-] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = DatabaseProviderFactory.Instance.GetXepVirtualFaultLabelsByVirtualFaultCodeId(xEP_VIRTUALFAULTCODES.ID);
                            //[+] XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                            XEP_VIRTUALFAULTLABELS xepVirtualFaultLabelsByVirtualFaultCodeId = null;
                            if (xepVirtualFaultLabelsByVirtualFaultCodeId != null)
                            {
                                XepFaultLabel = xepVirtualFaultLabelsByVirtualFaultCodeId;
                                if (!string.IsNullOrEmpty(FaultLabel))
                                {
                                    FaultLabel = FaultLabel.Trim();
                                }
                            }
                        }
                        else
                        {
                            Log.Warning("Fault.ResolveLabels()", "Unable to resolve label for: {0} ECU: {1}", DTC.Id, ECU);
                        }

                        return;
                    }

                    Log.Info("Fault.ResolveLabels()", "found combined fault: {0} Code: {1}", DTC.Id, DTC.FortAsHexString);
                    //[-] XEP_COMBINEDFAULTS xepCombinedFaultById = DatabaseProviderFactory.Instance.GetXepCombinedFaultById(DTC.Id.Value, vehicle, resolver);
                    //[+] XEP_COMBINEDFAULTS xepCombinedFaultById = null;
                    XEP_COMBINEDFAULTS xepCombinedFaultById = null;
                    if (xepCombinedFaultById != null)
                    {
                        //[-] XEP_COMBIFAULTLABELS xepCombiFaultLabelByCode = DatabaseProviderFactory.Instance.GetXepCombiFaultLabelByCode(xepCombinedFaultById.CODE, vehicle, resolver);
                        //[+] XEP_COMBIFAULTLABELS xepCombiFaultLabelByCode = null;
                        XEP_COMBIFAULTLABELS xepCombiFaultLabelByCode = null;
                        if (xepCombiFaultLabelByCode == null)
                        {
                            FaultLabel = xepCombinedFaultById.CODE;
                        }
                        else
                        {
                            XepFaultLabel = xepCombiFaultLabelByCode;
                        }

                        if (!string.IsNullOrEmpty(FaultLabel))
                        {
                            FaultLabel = FaultLabel.Trim();
                        }
                    }

                    return;
                }

                DtcFOrtEcuVariantKey key = new DtcFOrtEcuVariantKey(DTC.F_ORT.ToString(), ECU.VARIANTE.ToLowerInvariant());
                XEP_FAULTLABELS xEP_FAULTLABELS = null;
                //[-] xEP_FAULTLABELS = ((xepFaultLabelsCollection != null) ? (xepFaultLabelsCollection.ContainsKey(key) ? xepFaultLabelsCollection[key].LastOrDefault() : null) : DatabaseProviderFactory.Instance.GetEcuFaultLabelByFaultCodeAndEcuVariant(DTC.F_ORT.ToString(), (ECU.VARIANTE != null) ? ECU.VARIANTE.ToLowerInvariant() : string.Empty, vehicle, resolver));
                //[+] xEP_FAULTLABELS = ((xepFaultLabelsCollection != null) ? (xepFaultLabelsCollection.ContainsKey(key) ? xepFaultLabelsCollection[key].LastOrDefault() : null) : null);
                xEP_FAULTLABELS = ((xepFaultLabelsCollection != null) ? (xepFaultLabelsCollection.ContainsKey(key) ? xepFaultLabelsCollection[key].LastOrDefault() : null) : null);
                string value;
                if (xEP_FAULTLABELS == null)
                {
                    value = string.Empty;
                    Log.Info("Fault.ResolveLabels()", "The Fault label for fault {0} could not be set.", DTC.F_ORT.ToString());
                }
                else
                {
                    value = xEP_FAULTLABELS.Title;
                    XepFaultLabel = xEP_FAULTLABELS;
                }

                IEnumerable<XEP_FAULTMODELABELS> enumerable;
                if (xepFaultModelLabelsCollection == null)
                {
                    //[-] enumerable = DatabaseProviderFactory.Instance.GetEcuFaultAdditionalLabel(DTC.F_ORT.ToString(), ECU.VARIANTE);
                    //[+] enumerable = new List<XEP_FAULTMODELABELS>();
                    enumerable = new List<XEP_FAULTMODELABELS>();
                }
                else
                {
                    IEnumerable<XEP_FAULTMODELABELS> enumerable2;
                    if (!xepFaultModelLabelsCollection.ContainsKey(key))
                    {
                        enumerable2 = Enumerable.Empty<XEP_FAULTMODELABELS>();
                    }
                    else
                    {
                        IEnumerable<XEP_FAULTMODELABELS> enumerable3 = xepFaultModelLabelsCollection[key];
                        enumerable2 = enumerable3;
                    }

                    enumerable = enumerable2;
                }

                if (!string.IsNullOrEmpty(value))
                {
                    FaultLabel = value;
                }
                else
                {
                    FaultLabel = DTC.F_ORT_TEXT;
                    Log.Info("Fault.ResolveLabels()", "The Fault label for fault {0} was replaced by the F_ORT_TEXT.", DTC.F_ORT.ToString(), DTC.F_ORT_TEXT);
                }

                if (!string.IsNullOrEmpty(FaultLabel))
                {
                    FaultLabel = FaultLabel.Trim();
                }

                ArtLabel = new ObservableCollection<string>();
                foreach (XEP_FAULTMODELABELS item in enumerable)
                {
                    //[-] if (!DatabaseProviderFactory.Instance.EvaluateXepRulesById(item.ID, vehicle, resolver) || string.IsNullOrEmpty(item.Code))
                    //[+] if (ClientContext.GetDatabase(vehicle)?.EvaluateXepRulesById(item.ID.ToString(CultureInfo.InvariantCulture), vehicle, resolver) != true || string.IsNullOrEmpty(item.Code))
                    if (ClientContext.GetDatabase(vehicle)?.EvaluateXepRulesById(item.ID.ToString(CultureInfo.InvariantCulture), vehicle, resolver) != true || string.IsNullOrEmpty(item.Code))
                    {
                        continue;
                    }

                    long num = Convert.ToInt64(item.Code, CultureInfo.InvariantCulture);
                    if (DTC.F_VORHANDEN_NR.HasValue && num == DTC.F_VORHANDEN_NR.Value)
                    {
                        ExistingLabel = item.Title;
                        DtcFVorhandenNr = item;
                    }

                    if (DTC.F_SYMPTOM_NR.HasValue && num == DTC.F_SYMPTOM_NR.Value)
                    {
                        SymptomLabel = item.Title;
                    }

                    if (DTC.F_FEHLERKLASSE_NR.HasValue && num == DTC.F_FEHLERKLASSE_NR.Value && DTC.F_FEHLERKLASSE_NR.Value != 0)
                    {
                        ClassLabel = item.Title;
                    }

                    if (DTC.F_WARNUNG_NR.HasValue && num == DTC.F_WARNUNG_NR.Value && DTC.F_WARNUNG_NR.Value != 0)
                    {
                        WarningLabel = item.Title;
                    }

                    if ((DTC.F_ART.HasValue && num == DTC.F_ART.Value) || DTC.F_ART_EXT == null || DTC.F_ART_EXT.Count <= 0)
                    {
                        continue;
                    }

                    foreach (typeFArtExt item2 in DTC.F_ART_EXT)
                    {
                        if (item2.F_ART_NR == num)
                        {
                            ArtLabel.Add(item.Title);
                        }
                    }
                }

                if (!ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.CoreFramework.DatabaseProvider.Fault.UseOnlyModelledLabels", defaultValue: true))
                {
                    if (string.IsNullOrEmpty(ExistingLabel))
                    {
                        ExistingLabel = DTC.F_VORHANDEN_TEXT;
                    }

                    if (string.IsNullOrEmpty(ClassLabel))
                    {
                        ClassLabel = DTC.F_FEHLERKLASSE_TEXT;
                    }

                    if (!string.IsNullOrEmpty(ExistingLabel))
                    {
                        ExistingLabel = ExistingLabel.Trim();
                    }

                    if (!string.IsNullOrEmpty(ClassLabel))
                    {
                        ClassLabel = ClassLabel.Trim();
                    }
                }
                else
                {
                    Log.Debug("Fault.ResolveLabels()", "No fallback values for not modelled fault labels were set for Fault: {0}", DTC.F_ORT);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Fault.ResolveLabels()", exception);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is FaultId faultId)
            {
                return faultId.Equals(new FaultId(this));
            }

            if (!(obj is Fault))
            {
                return false;
            }

            Fault fault = (Fault)obj;
            if (DTC != null)
            {
                if (fault.DTC == null)
                {
                    return false;
                }

                if (DTC.Id.HasValue)
                {
                    if (fault.DTC != null && DTC.Id == fault.DTC.Id)
                    {
                        return RelevantDtcContextIndex == fault.RelevantDtcContextIndex;
                    }

                    return false;
                }

                if (DTC.F_ORT == fault.DTC.F_ORT)
                {
                    if (ECU != null || fault.ECU != null)
                    {
                        if (ECU != null && fault.ECU != null && string.Equals(ECU.VARIANTE, fault.ECU.VARIANTE, StringComparison.OrdinalIgnoreCase))
                        {
                            return RelevantDtcContextIndex == fault.RelevantDtcContextIndex;
                        }

                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            decimal num = 0m;
            if (DTC != null)
            {
                if (DTC.Id.HasValue)
                {
                    num += DTC.Id.Value;
                }

                if (DTC.F_ORT.HasValue)
                {
                    num += (decimal)DTC.F_ORT.GetHashCode();
                }
            }

            if (ECU != null && ECU.VARIANTE != null)
            {
                num += (decimal)ECU.VARIANTE.GetHashCode();
            }

            if (RelevantDtcContextIndex.HasValue)
            {
                num += (decimal)RelevantDtcContextIndex.Value;
            }

            return num.GetHashCode();
        }

        public string CompareChar(char readValue, string stateValue, string stateTitle)
        {
            try
            {
                int num = Convert.ToInt16(stateValue, CultureInfo.InvariantCulture);
                if (readValue == num)
                {
                    return stateTitle;
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("FaultCodeTabs.CompareChar()", exception);
            }

            return null;
        }

        private string CompareString(string readValue, XEP_STATEVALUES stateValue)
        {
            if (string.Compare(readValue, stateValue.Statevalue, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return stateValue.Title;
            }

            return null;
        }

        private string CompareDouble(double readValue, XEP_STATEVALUES stateValue)
        {
            try
            {
                double num = Convert.ToDouble(stateValue.Statevalue, CultureInfo.InvariantCulture);
                if (Math.Abs(readValue - num) < 0.0001)
                {
                    return stateValue.Title;
                }
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }

            return null;
        }

        private string ConvertToString(object resultValue)
        {
            if (resultValue is int num)
            {
                return num.ToString(CultureInfo.InvariantCulture);
            }

            if (resultValue is double num2)
            {
                return num2.ToString("0.00", new CultureInfo(ConfigSettings.CurrentUICulture));
            }

            if (resultValue is float num3)
            {
                return num3.ToString("0.00", new CultureInfo(ConfigSettings.CurrentUICulture));
            }

            return resultValue.ToString();
        }

        private string ConvertResultValue(object resultValue, IList<XEP_STATEVALUES> stateValues)
        {
            string text;
            if (resultValue == null)
            {
                text = FormatedData.Localize("#ECUFunctionNoResult");
            }
            else if (stateValues == null || stateValues.Count == 0)
            {
                text = ConvertToString(resultValue);
            }
            else
            {
                text = null;
                foreach (XEP_STATEVALUES stateValue in stateValues)
                {
                    string text2 = ((resultValue is char) ? CompareChar((char)resultValue, stateValue.Statevalue, stateValue.Title) : ((resultValue is double) ? CompareDouble((double)resultValue, stateValue) : ((!(resultValue is float)) ? CompareString(resultValue.ToString(), stateValue) : CompareDouble((float)resultValue, stateValue))));
                    if (text2 != null)
                    {
                        return text2;
                    }
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = ConvertToString(resultValue);
                }
            }

            return text;
        }

        public void UpdateUwDisplay(FaultCode faultCode)
        {
            if (DTC.Current == null || faultCode == null)
            {
                return;
            }

            int count = DTC.First.F_UW.Count;
            for (int i = 0; i < DTC.Current.F_UW.Count && i < count; i++)
            {
                XEP_ENVCONDSLABELS envCondsByUwNrName = faultCode.GetEnvCondsByUwNrName(DTC.Current.F_UW[i].F_UW_NR, DTC.Current.F_UW[i].F_UW_NAME);
                if (envCondsByUwNrName == null)
                {
                    continue;
                }

                object obj = DTC.Current.F_UW[i].F_UW_WERT;
                object obj2 = DTC.First.F_UW[i].F_UW_WERT;
                object obj3 = DTC.Second?.F_UW[i].F_UW_WERT;
                switch (DTC.Current.F_UW[i].F_UW_TYP)
                {
                    case UwType.DATA:
                        obj = string.Join(" ", DTC.Current.F_UW[i].F_UW_DATA.Select((byte x) => x.ToString("X2")));
                        obj2 = string.Join(" ", DTC.First.F_UW[i].F_UW_DATA.Select((byte x) => x.ToString("X2")));
                        obj3 = ((DTC.Second != null) ? string.Join(" ", DTC.Second?.F_UW[i].F_UW_DATA.Select((byte x) => x.ToString("X2"))) : null);
                        break;
                    case UwType.Discrete:
                        obj = DTC.Current.F_UW[i].F_UW_RAW;
                        obj2 = DTC.First.F_UW[i].F_UW_RAW;
                        obj3 = DTC.Second?.F_UW[i].F_UW_RAW;
                        break;
                    case UwType.TEXT:
                        obj = DTC.Current.F_UW[i].F_UW_TEXT;
                        obj2 = DTC.First.F_UW[i].F_UW_TEXT;
                        obj3 = DTC.Second?.F_UW[i].F_UW_TEXT;
                        break;
                }

                bool flag = false;
                decimal? nodeClass = envCondsByUwNrName.NodeClass;
                decimal num = 5658114;
                if ((nodeClass.GetValueOrDefault() == num) & nodeClass.HasValue)
                {
                    //[-] IList<XEP_STATEVALUES> ecuResultStateValues = DatabaseProviderFactory.Instance.GetEcuResultStateValues(envCondsByUwNrName.Id);
                    //[+] IList<XEP_STATEVALUES> ecuResultStateValues = null;
                    IList<XEP_STATEVALUES> ecuResultStateValues = null;
                    if (ecuResultStateValues != null && ecuResultStateValues.Any())
                    {
                        flag = true;
                        string current_F_UW_WERT = ConvertResultValue(obj, ecuResultStateValues);
                        string first_F_UW_WERT = ConvertResultValue(obj2, ecuResultStateValues);
                        string second_F_UW_WERT = ConvertResultValue(obj3, ecuResultStateValues);
                        string text = (string.IsNullOrEmpty(envCondsByUwNrName.Unit) ? string.Empty : envCondsByUwNrName.Unit);
                        DTC.F_UW_Display.Add(new F_UW_Display(envCondsByUwNrName, current_F_UW_WERT, text, first_F_UW_WERT, text, second_F_UW_WERT, text));
                    }
                }

                if (!flag)
                {
                    DTC.F_UW_Display.Add(new F_UW_Display(envCondsByUwNrName, obj, string.IsNullOrEmpty(envCondsByUwNrName.Unit) ? DTC.Current.F_UW[i].F_UW_EINH : envCondsByUwNrName.Unit, obj2, string.IsNullOrEmpty(envCondsByUwNrName.Unit) ? DTC.First.F_UW[i].F_UW_EINH : envCondsByUwNrName.Unit, obj3, (!string.IsNullOrEmpty(envCondsByUwNrName.Unit)) ? envCondsByUwNrName.Unit : DTC.Second?.F_UW[i].F_UW_EINH));
                }
            }
        }

        public void UpdateUwDisplayDetail()
        {
            if (!DTC.F_UW_Display.Any())
            {
                int count = DTC.First.F_UW.Count;
                for (int i = 0; i < DTC.Current.F_UW.Count && i < count; i++)
                {
                    DTC.F_UW_Display.Add(new F_UW_Display(DTC.Current.F_UW[i].F_UW_TEXT, DTC.Current.F_UW[i].F_UW_WERT, DTC.Current.F_UW[i].F_UW_EINH, DTC.First.F_UW[i].F_UW_WERT, DTC.First.F_UW[i].F_UW_EINH, DTC.Second?.F_UW[i].F_UW_WERT, DTC.Second?.F_UW[i].F_UW_EINH));
                }
            }
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Fault Clone(int relevantDtcContextIndex)
        {
            return new Fault
            {
                ArtLabel = ArtLabel,
                ClassLabel = ClassLabel,
                DTC = DTC,
                DtcFVorhandenNr = DtcFVorhandenNr,
                ECU = ECU,
                ExistingLabel = ExistingLabel,
                FaultGroupLabel = FaultGroupLabel,
                FaultLabel = FaultLabel,
                IsNewFaultMemoryActive = IsNewFaultMemoryActive,
                Parent = this,
                PKode = PKode,
                RelevantDtcContextIndex = relevantDtcContextIndex,
                SymptomLabel = SymptomLabel,
                WarningLabel = WarningLabel,
                XepFaultLabel = XepFaultLabel
            };
        }
    }
}