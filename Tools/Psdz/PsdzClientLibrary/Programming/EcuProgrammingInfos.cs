using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;

#pragma warning disable CS0169
namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "Changed to public", AccessModified = true)]
    public class EcuProgrammingInfos : IEnumerable<IEcuProgrammingInfo>, IEcuProgrammingInfos, IEnumerable
	{
		public event PropertyChangedEventHandler PropertyChanged;

        public IEcuProgrammingInfosData DataContext
        {
            get
            {
                return dataContext;
            }
            set
            {
                dataContext = value;
                OnPropertyChanged("DataContext");
            }
        }

        [PreserveSource(Hint = "Changed to public")]
        public ProgrammingObjectBuilder ProgrammingObjectBuilder
        {
            get
            {
                return programmingObjectBuilder;
            }
            set
            {
                programmingObjectBuilder = value;
            }
        }

        [PreserveSource(Hint = "db removed")]
        public EcuProgrammingInfos(IVehicle vehicle, IFFMDynamicResolver ffmResolver, bool standard = true)
		{
            if (vehicle == null)
            {
                throw new ArgumentNullException("vehicle");
            }
            // [IGNORE] this.db = db;
            this.vehicle = vehicle;
            this.ffmResolver = ffmResolver;
            ecuProgrammingInfos = new List<EcuProgrammingInfo>();
            if (standard)
            {
                dataContext = new EcuProgrammingInfosData();
                ResetProgrammingInfos(unregister: false);
            }
		}

        ~EcuProgrammingInfos()
        {
            UnregisterEventHandler();
        }

        public virtual void EstablishSelection()
		{
		}

        public IEnumerator<IEcuProgrammingInfo> GetEnumerator()
        {
            return ecuProgrammingInfos.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ecuProgrammingInfos.GetEnumerator();
        }

        IEcuProgrammingInfo IEcuProgrammingInfos.GetItem(IEcu ecu)
        {
            return GetItem(ecu)?.Data;
        }

        public virtual IEcuProgrammingInfo GetItem(IEcu ecu, string category)
        {
            return GetItem(ecu);
        }

        public void SelectCodingForIndustrialCustomer(IEcu ecu, string category, bool value)
        {
            IEcuProgrammingInfo item = GetItem(ecu, category);
            if (item == null)
            {
                throw new ArgumentNullException("ecu");
            }
            IProgrammingAction programmingAction = item.GetProgrammingAction(ProgrammingActionType.Coding);
            programmingAction?.Select(value);
            if (programmingAction != null)
            {
                item.IsCodingScheduled = value;
            }
        }

        public void SelectProgrammingForIndustrialCustomer(IEcu ecu, string category, bool value)
        {
            IEcuProgrammingInfo item = GetItem(ecu, category);
            if (item == null)
            {
                throw new ArgumentNullException("ecu");
            }
            foreach (IProgrammingAction programmingAction in item.GetProgrammingActions(null))
            {
                programmingAction?.Select(value);
                if (programmingAction != null && programmingAction.Type == ProgrammingActionType.Coding)
                {
                    item.IsCodingScheduled = value;
                }
            }
            item.IsProgrammingScheduled = value;
        }

        public void SelectReplacementForIndustrialCustomer(IEcu ecu, string category, bool value)
        {
            (GetItem(ecu, category) ?? throw new ArgumentNullException("ecu")).GetProgrammingAction(ProgrammingActionType.Replacement)?.Select(value);
        }

        internal EcuProgrammingInfo AddEcuProgrammingInfo(IEcu ecu)
        {
            if (GetItemFromProgrammingInfos(ecu.ID_SG_ADR) != null)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Diagnosis address 0x{0:X2} is already added.", ecu.ID_SG_ADR));
            }
            EcuProgrammingInfo ecuProgrammingInfo = new EcuProgrammingInfo(ecu, programmingObjectBuilder);
            RegisterEventHandler(ecuProgrammingInfo);
            ecuProgrammingInfos.Add(ecuProgrammingInfo);
            DataContext.List.Add(ecuProgrammingInfo.Data);
            AddProgrammingInfoBeforeReplace(ecuProgrammingInfo.Data);
            ecuProgrammingInfosMap.Add(ecuProgrammingInfo.Ecu, ecuProgrammingInfo);
            return ecuProgrammingInfo;
        }

        internal void AddProgrammingInfoBeforeReplace(IEcuProgrammingInfoData ecuProgrammingInfoData)
        {
            if (ProgrammingInfoCanBeAdded(ecuProgrammingInfoData))
            {
                DataContext.ECUsWithIndividualData.Add(ecuProgrammingInfoData);
            }
        }

        internal EcuProgrammingInfo GetItem(IEcu ecu)
        {
            if (ecuProgrammingInfosMap.ContainsKey(ecu))
            {
                return ecuProgrammingInfosMap[ecu];
            }
            return null;
        }

        internal EcuProgrammingInfo GetItemFromProgrammingInfos(long diagAddress)
        {
            foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
            {
                if (ecuProgrammingInfo.Ecu.ID_SG_ADR == diagAddress)
                {
                    return ecuProgrammingInfo;
                }
            }
            return null;
        }

        internal IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter, typeDiagProtocoll[] diagProtocolFilter)
        {
            IList<IProgrammingAction> list = new List<IProgrammingAction>();
            foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
            {
                if (diagProtocolFilter == null || diagProtocolFilter.Contains(((IEcuProgrammingInfo)ecuProgrammingInfo).Ecu.DiagProtocoll))
                {
                    list.AddRange(((IEcuProgrammingInfo)ecuProgrammingInfo).GetProgrammingActions(programmingActionTypeFilter));
                }
            }
            return list;
        }

        internal void RefreshProgrammingInfoBeforeReplace(IEnumerable<IEcuProgrammingInfoData> ecuProgrammingInfoData)
        {
            DataContext.ECUsWithIndividualData.Clear();
            foreach (IEcuProgrammingInfoData ecuProgrammingInfoDatum in ecuProgrammingInfoData)
            {
                AddProgrammingInfoBeforeReplace(ecuProgrammingInfoDatum);
            }
        }

        [PreserveSource(Hint = "db removed")]
        internal void ResetProgrammingInfos(bool unregister = true, bool resetAll = true)
        {
            if (resetAll)
            {
                if (unregister)
                {
                    UnregisterEventHandler();
                }
                programmingObjectBuilder = new ProgrammingObjectBuilder((Vehicle)vehicle, ffmResolver);
                CreateEcuProgrammingInfos(vehicle.ECU);
                ecuProgrammingInfosMap = new Dictionary<IEcu, EcuProgrammingInfo>();
                ecuProgrammingInfos.ForEach(delegate (EcuProgrammingInfo info)
                {
                    ecuProgrammingInfosMap.Add(info.Ecu, info);
                });
                return;
            }
            foreach (IEcu item in (IEnumerable<IEcu>)new List<IEcu>(ecuProgrammingInfosMap.Keys.Where((IEcu ecu) => !vehicle.ECU.Contains(ecu))))
            {
                EcuProgrammingInfo ecuProgrammingInfo = ecuProgrammingInfosMap[item];
                ecuProgrammingInfos.Remove(ecuProgrammingInfo);
                DataContext.List.Remove(ecuProgrammingInfo.Data);
                DataContext.ECUsWithIndividualData.Remove(ecuProgrammingInfo.Data);
                ecuProgrammingInfosMap.Remove(item);
            }
        }


        internal void SetSvkCurrentForEachEcu(ISvt svt)
        {
            if (svt == null)
            {
                SetSvkCurrentToNull();
                return;
            }
            UpdateProgrammingInfo(svt);
            RefreshProgrammingInfoBeforeReplace(DataContext.List.ToList());
        }

        internal void SetSvkTargetForEachEcu(ISvt svt)
        {
            if (svt == null)
            {
                foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
                {
                    ecuProgrammingInfo.SvkTarget = null;
                }
                return;
            }
            foreach (IEcuObj ecu in svt.Ecus)
            {
                EcuProgrammingInfo itemFromProgrammingInfos = GetItemFromProgrammingInfos(ecu.EcuIdentifier.DiagAddrAsInt);
                if (itemFromProgrammingInfos != null)
                {
                    itemFromProgrammingInfos.SvkTarget = ecu.StandardSvk;
                }
                else if (ecu is ECU ecuFromTargetSvt)
                {
                    itemFromProgrammingInfos = GetEcuProgrammingInfo(ecuFromTargetSvt);
                }
            }
        }

        internal void UpdateProgrammingActions(IPsdzTal tal, int escalationStep)
        {
            foreach (IPsdzEcuIdentifier affectedEcu in tal.AffectedEcus)
            {
                EcuProgrammingInfo itemFromProgrammingInfos = GetItemFromProgrammingInfos(affectedEcu.DiagAddrAsInt);
                if (itemFromProgrammingInfos != null)
                {
                    IPsdzEcuIdentifier id = affectedEcu;
                    IEnumerable<IPsdzTalLine> talLines = tal.TalLines.Where((IPsdzTalLine talLine) => talLine.EcuIdentifier.Equals(id));
                    itemFromProgrammingInfos.UpdateProgrammingActions(talLines, isTalExecuted: true, escalationStep);
                }
                else
                {
                    Log.Warning("EcuProgrammingInfos.UpdateProgrammingActions", "Could not find ecu programming object for 0x{0:X2}", affectedEcu.DiagAddrAsInt);
                }
            }
            UpdateSmartActuators(tal);
        }

        private void UpdateSmartActuators(IPsdzTal tal)
        {
            try
            {
                IEnumerable<IPsdzTalLine> enumerable = tal.TalLines.Where((IPsdzTalLine x) => !x.SmacTransferStart.IsEmpty || !x.SmacTransferStatus.IsEmpty);
                TalLineHelper talLineHelper = new TalLineHelper(tal);
                foreach (IPsdzTalLine item in enumerable)
                {
                    foreach (IPsdzTa ta in item.TaCategory.Tas)
                    {
                        List<string> list = new List<string>();
                        if (ta is PsdzSmacTransferStartTA psdzSmacTransferStartTA)
                        {
                            list.AddRange(psdzSmacTransferStartTA.SmartActuatorData.Keys);
                        }
                        if (ta is PsdzSmacTransferStatusTA psdzSmacTransferStatusTA)
                        {
                            list.AddRange(psdzSmacTransferStatusTA.SmartActuatorIDs);
                        }
                        foreach (string item2 in list)
                        {
                            PsdzDiagAddress psdzDiagAddress = talLineHelper.CalculateSmacDiagAddress(item.EcuIdentifier.DiagnosisAddress, item2);
                            GetItemFromProgrammingInfos(psdzDiagAddress.Offset).UpdateSingleProgrammingAction(ProgrammingActionType.Programming, MapState(ta.ExecutionState), executed: false);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        private ProgrammingActionState MapState(PsdzTaExecutionState? executionStateInput)
        {
            PsdzTaExecutionState psdzTaExecutionState;
            if (executionStateInput.HasValue)
            {
                psdzTaExecutionState = executionStateInput.Value;
            }
            else
            {
                Log.Warning("ProgrammingAction.MapState", "input is null. 'TaExecutionState.Inactive' will be used.");
                psdzTaExecutionState = PsdzTaExecutionState.Inactive;
            }
            switch (psdzTaExecutionState)
            {
                case PsdzTaExecutionState.Executable:
                case PsdzTaExecutionState.Inactive:
                    return ProgrammingActionState.ActionPlanned;
                case PsdzTaExecutionState.NotExecutable:
                    return ProgrammingActionState.MissingPrerequisitesForAction;
                case PsdzTaExecutionState.AbortedByError:
                case PsdzTaExecutionState.AbortedByUser:
                case PsdzTaExecutionState.FinishedWithError:
                    return ProgrammingActionState.ActionFailed;
                case PsdzTaExecutionState.Finished:
                case PsdzTaExecutionState.FinishedWithWarnings:
                    return ProgrammingActionState.ActionSuccessful;
                case PsdzTaExecutionState.Running:
                    return ProgrammingActionState.ActionInProcess;
                default:
                    throw new ArgumentException($"Unsupported TA execution state: {psdzTaExecutionState}");
            }
        }

        protected virtual void OnActionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsSelected") || !(sender is IProgrammingAction programmingAction))
            {
                return;
            }
            lock (threadLock)
            {
                if (!programmingAction.IsSelected && dataContext.SelectedActionData.Contains(programmingAction.DataContext))
                {
                    dataContext.SelectedActionData.Remove(programmingAction.DataContext);
                }
                else if (programmingAction.IsSelected && !dataContext.SelectedActionData.Contains(programmingAction.DataContext))
                {
                    AddItem(programmingAction);
                }
            }
        }

        protected virtual bool OnlyAddECUsWithIndividualData()
        {
            return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.DisplayOnlyECUsWithIndividualData", defaultValue: true);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RegisterEventHandler(IEcuProgrammingInfo ecuProgrammingInfo)
        {
            if (ecuProgrammingInfo.ProgrammingActions is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += OnEcuProgrammingActionsChanged;
            }
        }

        protected void UnregisterEventHandler()
        {
            foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
            {
                if (((IEcuProgrammingInfo)ecuProgrammingInfo).ProgrammingActions is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged -= OnEcuProgrammingActionsChanged;
                }
                foreach (IProgrammingAction programmingAction in ((IEcuProgrammingInfo)ecuProgrammingInfo).ProgrammingActions)
                {
                    programmingAction.PropertyChanged -= OnActionPropertyChanged;
                }
            }
        }

        private void AddItem(IProgrammingAction itemToAdd)
        {
            for (int i = 0; i < dataContext.SelectedActionData.Count; i++)
            {
                IProgrammingActionData programmingActionData = dataContext.SelectedActionData[i];
                if (itemToAdd.DataContext.Order < programmingActionData.Order)
                {
                    dataContext.SelectedActionData.Insert(i, itemToAdd.DataContext);
                    OnPropertyChanged("SelectedActions");
                    return;
                }
            }
            dataContext.SelectedActionData.Add(itemToAdd.DataContext);
            OnPropertyChanged("SelectedActions");
        }

        private void CreateEcuProgrammingInfos(IEnumerable<IEcu> ecus)
        {
            if (ecus == null)
            {
                throw new ArgumentNullException();
            }
            ecuProgrammingInfos.Clear();
            IList<EcuProgrammingInfoData> list = new List<EcuProgrammingInfoData>();
            foreach (IEcu ecu in ecus)
            {
                EcuProgrammingInfo ecuProgrammingInfo = new EcuProgrammingInfo(ecu, programmingObjectBuilder);
                if (ecuProgrammingInfo?.Ecu != null && ecu != null)
                {
                    IDictionary<IEcu, EcuProgrammingInfo> dictionary = ecuProgrammingInfosMap;
                    if (dictionary != null && dictionary.ContainsKey(ecu) && !string.IsNullOrEmpty(ecuProgrammingInfosMap[ecu]?.Ecu?.ProgrammingVariantName))
                    {
                        ecuProgrammingInfo.Ecu.ProgrammingVariantName = ecuProgrammingInfosMap[ecu].Ecu.ProgrammingVariantName;
                    }
                }
                RegisterEventHandler(ecuProgrammingInfo);
                ecuProgrammingInfos.Add(ecuProgrammingInfo);
                list.Add(ecuProgrammingInfo.Data);
            }
            RefreshProgrammingInfoBeforeReplace(list);
            RefreshProgrammingInfo(list);
        }

        private EcuProgrammingInfo GetEcuProgrammingInfo(ECU ecuFromTargetSvt)
        {
            Log.Info("EcuProgrammingInfos.SetSvkTargetForEachEcu", "Try to fill ECU name from database for ECU: {0}", ecuFromTargetSvt.LogEcu());
            EcuProgrammingInfo ecuProgrammingInfo = new EcuProgrammingInfo(ecuFromTargetSvt, programmingObjectBuilder);
            ecuProgrammingInfos.Add(ecuProgrammingInfo);
            DataContext.List.Add(ecuProgrammingInfo.Data);
            AddProgrammingInfoBeforeReplace(ecuProgrammingInfo.Data);
            ecuProgrammingInfosMap.Add(ecuFromTargetSvt, ecuProgrammingInfo);
            Log.Info("EcuProgrammingInfos.SetSvkTargetForEachEcu", "Added EcuProgrammingInfo {0} from SVT target.", ecuProgrammingInfo.EcuIdentifier);
            return ecuProgrammingInfo;
        }

        private void OnEcuProgrammingActionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedAction action = e.Action;
            if ((uint)action > 1u)
            {
                return;
            }
            foreach (object newItem in e.NewItems)
            {
                if (!(newItem is IProgrammingAction programmingAction))
                {
                    continue;
                }
                lock (threadLock)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        programmingAction.PropertyChanged += OnActionPropertyChanged;
                        if (programmingAction.IsSelected && !dataContext.SelectedActionData.Contains(programmingAction.DataContext))
                        {
                            AddItem(programmingAction);
                        }
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        programmingAction.PropertyChanged -= OnActionPropertyChanged;
                        if (dataContext.SelectedActionData.Contains(programmingAction.DataContext))
                        {
                            dataContext.SelectedActionData.Remove(programmingAction.DataContext);
                        }
                    }
                }
            }
        }


        private bool ProgrammingInfoCanBeAdded(IEcuProgrammingInfoData ecuProgrammingInfoData)
        {
            if (!OnlyAddECUsWithIndividualData() || vehicle.Classification.IsMotorcycle())
            {
                return true;
            }
            if (ecuProgrammingInfoData.Ecu != null && ecuProgrammingInfoData.Ecu.StatusInfo != null)
            {
                return ecuProgrammingInfoData.Ecu.StatusInfo.HasIndividualData;
            }
            return false;
        }

        private void RefreshProgrammingInfo(IList<EcuProgrammingInfoData> ecuProgrammingInfoDataList)
        {
            DataContext.List.Clear();
            DataContext.List.AddRange(ecuProgrammingInfoDataList);
        }

        private void SetSvkCurrentToNull()
        {
            foreach (EcuProgrammingInfo ecuProgrammingInfo in ecuProgrammingInfos)
            {
                ecuProgrammingInfo.SvkCurrent = null;
            }
        }

        private void UpdateProgrammingInfo(ISvt svt)
        {
            foreach (IEcuObj ecu in svt.Ecus)
            {
                EcuProgrammingInfo itemFromProgrammingInfos = GetItemFromProgrammingInfos(ecu.EcuIdentifier.DiagAddrAsInt);
                if (itemFromProgrammingInfos != null)
                {
                    if (ecu.BnTnName == null && itemFromProgrammingInfos.Ecu.ProgrammingVariantName != null)
                    {
                        Log.Info("EcuProgrammingInfos.SetSvkCurrentForEachEcu()", "Do not replace \"{0}\" with null for ECU: {1}.", itemFromProgrammingInfos.Ecu.ProgrammingVariantName, itemFromProgrammingInfos.Ecu.LogEcu());
                    }
                    else
                    {
                        itemFromProgrammingInfos.Ecu.ProgrammingVariantName = ecu.BnTnName;
                    }
                    itemFromProgrammingInfos.Ecu.StatusInfo = ecu.EcuStatusInfo;
                    itemFromProgrammingInfos.SvkCurrent = ecu.StandardSvk;
                }
            }
        }

        protected readonly IList<EcuProgrammingInfo> ecuProgrammingInfos;

		protected IDictionary<IEcu, EcuProgrammingInfo> ecuProgrammingInfosMap;

		protected IVehicle vehicle;

        [PreserveSource(Hint = "Added", Placeholder = true)]
		private readonly PlaceholderType db;

		private readonly IFFMDynamicResolver ffmResolver;

		private readonly object threadLock = new object();

		private IEcuProgrammingInfosData dataContext;

		private ProgrammingObjectBuilder programmingObjectBuilder;
	}
}
