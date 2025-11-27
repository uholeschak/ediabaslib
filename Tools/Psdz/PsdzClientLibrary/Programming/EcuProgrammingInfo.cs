using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Programming.API;
using BMW.Rheingold.Programming.Common;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace PsdzClient.Programming
{
    [Flags]
    public enum EcuScheduledState
    {
        NothingScheduled = 0,
        ProgrammingScheduledByLogistic = 1,
        ProgrammingScheduledByUser = 2,
        CodingScheduledByLogistic = 4,
        CodingScheduledByUser = 8
    }

	public class EcuProgrammingInfo : INotifyPropertyChanged, IEcuProgrammingInfo
	{
        protected EcuProgrammingInfoData data;

        private ObservableCollectionEx<ProgrammingAction> programmingActionList;

        private EcuScheduledState scheduled;

        public IProgrammingAction this[ProgrammingActionType type] => GetProgrammingAction(type);

        internal EcuProgrammingInfoData Data => data;

        public IEcu Ecu
        {
            get
            {
                return data.Ecu;
            }
            private set
            {
                data.Ecu = value;
            }
        }

        public bool IsCodingDisabled
        {
            get
            {
                return data.IsCodingDisabled;
            }
            set
            {
                data.IsCodingDisabled = value;
            }
        }

        public bool IsCodingScheduled
        {
            get
            {
                return data.IsCodingScheduled;
            }
            set
            {
                if (value && !Scheduled.HasFlag(EcuScheduledState.CodingScheduledByUser))
                {
                    Scheduled |= EcuScheduledState.CodingScheduledByUser;
                }
                else if (!value && Scheduled.HasFlag(EcuScheduledState.CodingScheduledByUser))
                {
                    Scheduled &= ~EcuScheduledState.CodingScheduledByUser;
                }
            }
        }

        public bool IsProgrammingDisabled
        {
            get
            {
                return data.IsProgrammingDisabled;
            }
            set
            {
                data.IsProgrammingDisabled = value;
            }
        }

        public bool IsProgrammingSelectionDisabled
        {
            get
            {
                return data.IsProgrammingSelectionDisabled;
            }
            set
            {
                data.IsProgrammingSelectionDisabled = value;
            }
        }

        public bool IsCodingSelectionDisabled
        {
            get
            {
                return data.IsCodingSelectionDisabled;
            }
            set
            {
                data.IsCodingSelectionDisabled = value;
            }
        }

        public bool IsProgrammingScheduled
        {
            get
            {
                return data.IsProgrammingScheduled;
            }
            set
            {
                if (value && !Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByUser))
                {
                    Scheduled |= EcuScheduledState.ProgrammingScheduledByUser;
                }
                else if (!value && Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByUser))
                {
                    Scheduled &= ~EcuScheduledState.ProgrammingScheduledByUser;
                }
            }
        }

        public IEnumerable<IProgrammingAction> ProgrammingActions => programmingActionList;

        public double ProgressValue
        {
            get
            {
                return data.ProgressValue;
            }
            set
            {
                data.ProgressValue = value;
                OnPropertyChanged("ProgressValue");
            }
        }

        public EcuScheduledState Scheduled
        {
            get
            {
                return scheduled;
            }
            private set
            {
                if (scheduled != value)
                {
                    scheduled = value;
                    data.IsProgrammingScheduled = Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByUser) || Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByLogistic);
                    data.IsCodingScheduled = Scheduled.HasFlag(EcuScheduledState.CodingScheduledByUser) || Scheduled.HasFlag(EcuScheduledState.CodingScheduledByLogistic);
                }
            }
        }

        [PreserveSource(Hint = "Modified")]
        public ProgrammingActionState? State
        {
            get
            {
                return null;
            }
        }

        public IStandardSvk SvkCurrent
        {
            get
            {
                return data.SvkCurrent;
            }
            internal set
            {
                data.SvkCurrent = value;
                OnPropertyChanged("SvkCurrent");
            }
        }

        public IStandardSvk SvkTarget
        {
            get
            {
                return data.SvkTarget;
            }
            internal set
            {
                data.SvkTarget = value;
                OnPropertyChanged("SvkTarget");
            }
        }

        public string Category
        {
            get
            {
                return data.Category;
            }
            set
            {
                data.Category = value;
            }
        }

        public int FlashOrder
        {
            get
            {
                return Data.FlashOrder;
            }
            set
            {
                Data.FlashOrder = value;
            }
        }

        public bool IsExchangeDoneDisabled
        {
            get
            {
                return data.IsExchangeDoneDisabled;
            }
            set
            {
                data.IsExchangeDoneDisabled = value;
            }
        }

        public bool IsExchangeScheduledDisabled
        {
            get
            {
                return data.IsExchangeScheduledDisabled;
            }
            set
            {
                data.IsExchangeScheduledDisabled = value;
            }
        }

        public bool IsExchangeDone
        {
            get
            {
                return data.IsExchangeDone;
            }
            set
            {
                data.IsExchangeDone = value;
                data.IsExchangeScheduledDisabled = value;
            }
        }

        public bool IsExchangeScheduled
        {
            get
            {
                return data.IsExchangeScheduled;
            }
            set
            {
                data.IsExchangeScheduled = value;
                data.IsExchangeDoneDisabled = value;
            }
        }

        public virtual string EcuIdentifier => string.Format(CultureInfo.InvariantCulture, "{0}_0x{1:X2}", Ecu.TITLE_ECUTREE, Ecu.ID_SG_ADR);

        //internal ICollection<XEP_SWIACTION> XepSwiActionList { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        internal EcuProgrammingInfo(IEcu ecu, ProgrammingObjectBuilder programmingObjectBuilder, bool withInitData = true)
        {
            if (ecu == null)
            {
                throw new ArgumentNullException("ecu");
            }
            if (programmingObjectBuilder == null)
            {
                throw new ArgumentNullException("programmingObjectBuilder");
            }
            if (withInitData)
            {
                InitData(ecu, programmingObjectBuilder);
            }
            Init();
        }

        internal EcuProgrammingInfo()
        {
            Ecu = null;
            Init();
            SvkCurrent = null;
        }

        [PreserveSource(Hint = "Modified")]
        private void Init()
        {
            scheduled = EcuScheduledState.NothingScheduled;
            // [IGNORE] XepSwiActionList = new Collection<XEP_SWIACTION>();
            programmingActionList = new ObservableCollectionEx<ProgrammingAction>();
            programmingActionList.CollectionChanged += OnProgrammingActionsCollectionChanged;
        }

        [PreserveSource(Hint = "Modified")]
        protected void InitData(IEcu ecu, ProgrammingObjectBuilder programmingObjectBuilder)
        {
            if (data == null)
            {
                data = new EcuProgrammingInfoData();
            }
            Ecu = ecu;
            data.EcuTitle = Ecu.TITLE_ECUTREE;
            data.EcuDescription = data.EcuTitle;
            try
            {
                SvkCurrent = programmingObjectBuilder.Build(ecu.SVK);
            }
            catch (Exception exception)
            {
                Log.WarningException("EcuProgrammingInfo.EcuProgrammingInfo()", exception);
                SvkCurrent = null;
            }
        }

        public IProgrammingAction GetProgrammingAction(ProgrammingActionType type)
        {
            foreach (ProgrammingAction programmingAction in programmingActionList)
            {
                if (((IProgrammingAction)programmingAction).Type == type)
                {
                    return programmingAction;
                }
            }
            return null;
        }

        public IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter)
        {
            IList<IProgrammingAction> list = new List<IProgrammingAction>();
            if (programmingActionTypeFilter == null)
            {
                list.AddRange(ProgrammingActions);
                return list;
            }
            foreach (IProgrammingAction programmingAction in ProgrammingActions)
            {
                if (programmingActionTypeFilter.Contains(programmingAction.Type))
                {
                    list.Add(programmingAction);
                }
            }
            return list;
        }

        public List<ProgrammingAction> GetAllProgrammingActions(ProgrammingActionType type)
        {
            List<ProgrammingAction> list = new List<ProgrammingAction>();
            foreach (ProgrammingAction programmingAction in programmingActionList)
            {
                if (programmingAction.Type == type)
                {
                    list.Add(programmingAction);
                }
            }
            return list;
        }

        internal void UpdateProgrammingActions(IEnumerable<IPsdzTalLine> talLines, bool isTalExecuted, int escalationSteps = 0)
        {
            IDictionary<ProgrammingActionType, ICollection<IPsdzTalLine>> dictionary = new Dictionary<ProgrammingActionType, ICollection<IPsdzTalLine>>();
            foreach (IPsdzTalLine talLine in talLines)
            {
                foreach (ProgrammingActionType item in MapProgrammingActionType(talLine))
                {
                    if (dictionary.ContainsKey(item))
                    {
                        dictionary[item].Add(talLine);
                        continue;
                    }
                    dictionary.Add(item, new List<IPsdzTalLine> { talLine });
                }
            }
            foreach (ProgrammingActionType key in dictionary.Keys)
            {
                List<ProgrammingAction> list;
                if (key == ProgrammingActionType.SFAWrite || key == ProgrammingActionType.SFADelete)
                {
                    list = GetAllProgrammingActions(key);
                }
                else
                {
                    list = new List<ProgrammingAction>();
                    list.Add(GetProgrammingAction(key) as ProgrammingAction);
                }
                foreach (ProgrammingAction item2 in list)
                {
                    if (item2 != null)
                    {
                        if (isTalExecuted)
                        {
                            item2.Update(dictionary[key], escalationSteps);
                        }
                        else
                        {
                            item2.UpdateState(dictionary[key]);
                        }
                    }
                }
            }
        }

        internal void AddProgrammingActions(IEnumerable<IPsdzTalLine> talLines, int order, bool preselectAction, ProgrammingSession session, bool codingAfterProgramming = false)
        {
            if (talLines == null)
            {
                Log.Error("ProgrammingUtils.AddProgrammingActionsToEcu()", "Param 'talLines' is missing!");
            }
            IDictionary<ProgrammingActionType, IList<SgbmIdChange>> dictionary = CalculateActionStates(talLines);
            if (dictionary.ContainsKey(ProgrammingActionType.Unmounting) && dictionary.ContainsKey(ProgrammingActionType.Mounting))
            {
                IList<SgbmIdChange> list = new List<SgbmIdChange>();
                list.AddRange(dictionary[ProgrammingActionType.Unmounting]);
                list.AddRange(dictionary[ProgrammingActionType.Mounting]);
                AddSingleProgrammingAction(ProgrammingActionType.Replacement, preselectAction, order, null, null, list);
                return;
            }
            IList<int> affectedEcuDiagAddressesFromTalLines = GetAffectedEcuDiagAddressesFromTalLines(talLines);
            foreach (ProgrammingActionType key in dictionary.Keys)
            {
                if (codingAfterProgramming && key == ProgrammingActionType.Coding)
                {
                    order *= 100;
                }
                if (key == ProgrammingActionType.SFAWrite || key == ProgrammingActionType.SFADelete)
                {
                    AddSingleSfaProgrammingAction(talLines, key, preselectAction, order);
                    continue;
                }
                string additionalTitleInfoForFscActions = GetAdditionalTitleInfoForFscActions(key, talLines, session);
                AddSingleProgrammingAction(key, preselectAction, order, null, null, dictionary[key], additionalTitleInfoForFscActions, affectedEcuDiagAddressesFromTalLines);
            }
        }

        private static List<int> GetAffectedEcuDiagAddressesFromTalLines(IEnumerable<IPsdzTalLine> talLines)
        {
            return (from talLine in talLines
                where talLine.EcuIdentifier != null
                select talLine.EcuIdentifier.DiagAddrAsInt).Distinct().ToList();
        }

        private void AddSingleSfaProgrammingAction(IEnumerable<IPsdzTalLine> talLines, ProgrammingActionType actionType, bool preselectAction, int order)
        {
            var (sgbmIdList, sgbmIdList2) = GetDividedSgbmIdsForSfa(talLines, actionType);
            var (enumerable, enumerable2) = GroupTalLinesForSfa(actionType, talLines);
            if (enumerable.Any())
            {
                string additionalTitleInfoForSfaActions = GetAdditionalTitleInfoForSfaActions(actionType, enumerable, systemFunction: true);
                List<int> affectedEcuDiagAddressesFromTalLines = GetAffectedEcuDiagAddressesFromTalLines(enumerable);
                AddSingleProgrammingAction(actionType, preselectAction, order, null, null, sgbmIdList, additionalTitleInfoForSfaActions, affectedEcuDiagAddressesFromTalLines, isSystemSfaAction: true);
            }
            if (enumerable2.Any())
            {
                string additionalTitleInfoForSfaActions2 = GetAdditionalTitleInfoForSfaActions(actionType, enumerable2, systemFunction: false);
                List<int> affectedEcuDiagAddressesFromTalLines2 = GetAffectedEcuDiagAddressesFromTalLines(enumerable2);
                AddSingleProgrammingAction(actionType, preselectAction, order, null, null, sgbmIdList2, additionalTitleInfoForSfaActions2, affectedEcuDiagAddressesFromTalLines2);
            }
        }

        private (IList<SgbmIdChange> systemTokenSgbmIds, IList<SgbmIdChange> customerTokenSgbmIds) GetDividedSgbmIdsForSfa(IEnumerable<IPsdzTalLine> talLines, ProgrammingActionType actionType)
        {
            IList<SgbmIdChange> list = new List<SgbmIdChange>();
            IList<SgbmIdChange> list2 = new List<SgbmIdChange>();
            foreach (IPsdzTalLine talLine in talLines)
            {
                foreach (IPsdzTa ta in talLine.TaCategory.Tas)
                {
                    IPsdzSgbmId id = ta.SgbmId;
                    if (id == null)
                    {
                        continue;
                    }
                    IPsdzFsaTa psdzFsaTa = GetSfaTasForActionType(talLine, actionType).FirstOrDefault((IPsdzFsaTa x) => x.SgbmId == id);
                    if (psdzFsaTa != null)
                    {
                        SgbmIdentifier sgbmIdentifier = new SgbmIdentifier();
                        sgbmIdentifier.ProcessClass = id.ProcessClass;
                        sgbmIdentifier.Id = id.IdAsLong;
                        sgbmIdentifier.MainVersion = id.MainVersion;
                        sgbmIdentifier.SubVersion = id.SubVersion;
                        sgbmIdentifier.PatchVersion = id.PatchVersion;
                        if (SecureFeatureData.IsSystemFeature(psdzFsaTa.FeatureId))
                        {
                            list.Add(new SgbmIdChange(GetSgbmIdActual(id), sgbmIdentifier.ToString()));
                        }
                        else
                        {
                            list2.Add(new SgbmIdChange(GetSgbmIdActual(id), sgbmIdentifier.ToString()));
                        }
                    }
                }
            }
            return (systemTokenSgbmIds: list, customerTokenSgbmIds: list2);
        }

        private static (IEnumerable<IPsdzTalLine> systemTalLines, IEnumerable<IPsdzTalLine> customerTalLines) GroupTalLinesForSfa(ProgrammingActionType actionType, IEnumerable<IPsdzTalLine> talLines)
        {
            if (actionType == ProgrammingActionType.SFAWrite || actionType == ProgrammingActionType.SFADelete)
            {
                IEnumerable<IPsdzTalLine> enumerable = talLines.Where((IPsdzTalLine x) => x.TaCategories == PsdzTaCategories.SFADeploy);
                List<IPsdzTalLine> list = new List<IPsdzTalLine>();
                List<IPsdzTalLine> list2 = new List<IPsdzTalLine>();
                foreach (IPsdzTalLine item in enumerable)
                {
                    if (GetSfaTasForActionType(item, actionType).Any((IPsdzFsaTa x) => x != null && SecureFeatureData.IsSystemFeature(x.FeatureId)))
                    {
                        list.Add(item);
                    }
                    if (GetSfaTasForActionType(item, actionType).Any((IPsdzFsaTa x) => x == null || !SecureFeatureData.IsSystemFeature(x.FeatureId)))
                    {
                        list2.Add(item);
                    }
                }
                return (systemTalLines: list, customerTalLines: list2);
            }
            return (systemTalLines: null, customerTalLines: null);
        }

        private static IEnumerable<IPsdzFsaTa> GetSfaTasForActionType(IPsdzTalLine sfaLine, ProgrammingActionType actionType)
        {
            switch (actionType)
            {
                case ProgrammingActionType.SFAWrite:
                    return sfaLine.SFADeploy.Tas.OfType<PsdzSFAWriteTA>();
                case ProgrammingActionType.SFADelete:
                    return sfaLine.SFADeploy.Tas.OfType<PsdzSFADeleteTA>();
                default:
                    return null;
            }
        }

        private string GetAdditionalTitleInfoForSfaActions(ProgrammingActionType actionType, IEnumerable<IPsdzTalLine> talLines, bool systemFunction)
        {
            if (actionType == ProgrammingActionType.SFAWrite || actionType == ProgrammingActionType.SFADelete)
            {
                foreach (IPsdzTalLine item in talLines.Where((IPsdzTalLine x) => x.TaCategories == PsdzTaCategories.SFADeploy).ToList())
                {
                    IEnumerable<IPsdzFsaTa> tas = from x in GetSfaTasForActionType(item, actionType)
                                                  where SecureFeatureData.IsSystemFeature(x.FeatureId) == systemFunction
                                                  select x;
                    if (talLines.Any())
                    {
                        return GetDetailedDescriptionLineFromSfaDeployTa(tas, systemFunction);
                    }
                }
            }
            return null;
        }

        private static string GetDetailedDescriptionLineFromSfaDeployTa(IEnumerable<IPsdzFsaTa> tas, bool systemFunction)
        {
            if (systemFunction)
            {
                return string.Join(Environment.NewLine, tas.Select((IPsdzFsaTa x) => string.Format(CultureInfo.InvariantCulture, " 0x{0:X6}", x.FeatureId)));
            }
            return string.Join(Environment.NewLine, tas.Select((IPsdzFsaTa x) => " " + SecureFeatureData.GetSecureFeatureName(x.FeatureId) + " (" + string.Format(CultureInfo.InvariantCulture, "0x{0:X6}", x.FeatureId) + ")"));
        }

        private string GetAdditionalTitleInfoForFscActions(ProgrammingActionType actionType, IEnumerable<IPsdzTalLine> talLines, ProgrammingSession session)
        {
            if (actionType == ProgrammingActionType.FscActivate || actionType == ProgrammingActionType.FscDeactivate)
            {
                if (session == null)
                {
                    Log.Warning(Log.CurrentMethod(), "session is null, so the additional FSC title lines can't be calculated (The title of each ta requires data from the session to be determined).");
                    return null;
                }
                foreach (IPsdzTalLine item in talLines.Where((IPsdzTalLine x) => x.TaCategories == PsdzTaCategories.FscDeploy).ToList())
                {
                    List<IPsdzTa> list = FilterTalsByProgrammingActionType(item, actionType).ToList();
                    if (list != null)
                    {
                        IEnumerable<string> values = list.Select((IPsdzTa psdzTa) => GetDetailedDescriptionLineFromFscDeployTa((PsdzFscDeployTa)psdzTa, session));
                        return string.Join(Environment.NewLine, values);
                    }
                }
            }
            return null;
        }

        private string GetDetailedDescriptionLineFromFscDeployTa(PsdzFscDeployTa psdzFscTa, ProgrammingSession session)
        {
            if (psdzFscTa.ApplicationId == null)
            {
                Log.Warning(Log.CurrentMethod(), "psdzTa.ApplicationId is null, preventing us from displaying a proper description.");
                return " " + FormatedData.Localize("#TherapyPlanEntryOrigin.unknown");
            }
            string text = string.Format(CultureInfo.InvariantCulture, $"0x{psdzFscTa.ApplicationId.ApplicationNumber:X4}{psdzFscTa.ApplicationId.UpgradeIndex:X4}");
            string enablingCodeName = EnablingCodeData.GetEnablingCodeName(psdzFscTa.ApplicationId.ApplicationNumber, psdzFscTa.ApplicationId.UpgradeIndex, (Vehicle)session.Vehicle, session.FFMResolver);
            return " " + enablingCodeName + " (" + text + ")";
        }

        internal ProgrammingAction AddSingleProgrammingAction(ProgrammingActionType type, bool isSelected, int order, string channel, string assemblyNumberSetPoint, IList<SgbmIdChange> sgbmIdList = null, string extraTitle = null, IList<int> ecuDiagAddr = null, bool isSystemSfaAction = false)
        {
            bool isExpertModeEnabled = ProgrammingUtils.IsExpertModeEnabled;
            ProgrammingAction programmingAction = (isSystemSfaAction ? new SystemSfaProgrammingAction(Ecu, type, isExpertModeEnabled, order) : new ProgrammingAction(Ecu, type, isExpertModeEnabled, order));
            programmingAction.Select(isSelected);
            programmingAction.Channel = channel;
            programmingAction.Note = ((Category != null) ? Category : string.Empty);
            programmingAction.AffectedEcuDiagAddr = ecuDiagAddr;
            if (sgbmIdList != null)
            {
                programmingAction.SgbmIds.AddRange(sgbmIdList);
            }
            if (!string.IsNullOrEmpty(extraTitle))
            {
                programmingAction.Title = programmingAction.Title + Environment.NewLine + extraTitle;
            }
            programmingActionList.Add(programmingAction);
            data.ProgrammingActionData.Add(programmingAction.DataContext);
            Log.Info("EcuProgrammingInfo.AddSingleProgrammingAction()", "ECU: 0x{0:X2} - Action: {1} - State: {2}", Ecu.ID_SG_ADR, programmingAction.Type, programmingAction.StateProgramming);
            return programmingAction;
        }

        internal void Reset(EcuScheduledState remove, IList<IEcuProgrammingInfo> exchangeScheduled)
        {
            programmingActionList.Clear();
            if (remove == EcuScheduledState.NothingScheduled)
            {
                Scheduled = EcuScheduledState.NothingScheduled;
                if (!exchangeScheduled.Any((IEcuProgrammingInfo ecu) => ecu.Ecu.ID_SG_ADR == Ecu.ID_SG_ADR))
                {
                    IsExchangeScheduled = false;
                }
                IsExchangeDone = false;
            }
            else
            {
                Scheduled &= ~remove;
            }
            IsCodingDisabled = IsCodingSelectionDisabled;
            IsProgrammingDisabled = IsProgrammingSelectionDisabled;
            IsExchangeScheduledDisabled = false;
            IsExchangeDoneDisabled = false;
        }

        internal void SetCodingScheduled()
        {
            IsCodingDisabled = true;
            Scheduled |= EcuScheduledState.CodingScheduledByLogistic;
        }

        internal void SetProgrammingScheduled()
        {
            IsProgrammingDisabled = true;
            Scheduled |= EcuScheduledState.ProgrammingScheduledByLogistic;
        }

        internal bool UpdateSingleProgrammingAction(ProgrammingActionType type, ProgrammingActionState state)
        {
            foreach (ProgrammingAction programmingAction in programmingActionList)
            {
                if (programmingAction.Type == type)
                {
                    Log.Info("EcuProgrammingInfo.UpdateSingleProgrammingAction", "ECU: 0x{0:X2} - Action: {1} - State: {2}", Ecu.ID_SG_ADR, type, state);
                    programmingAction.StateProgramming = state;
                    return true;
                }
            }
            return false;
        }

        internal bool UpdateSingleProgrammingAction(ProgrammingActionType type, ProgrammingActionState state, bool executed)
        {
            foreach (ProgrammingAction programmingAction in programmingActionList)
            {
                if (programmingAction.Type == type)
                {
                    programmingAction.UpdateState(state, executed);
                    Log.Info("EcuProgrammingInfo.UpdateSingleProgrammingAction", "ECU: 0x{0:X2} - Action: {1} - State: {2}", Ecu.ID_SG_ADR, type, programmingAction.StateProgramming);
                    return true;
                }
            }
            return false;
        }

        internal static ISet<ProgrammingActionType> MapProgrammingActionType(IPsdzTalLine talLine)
        {
            ISet<ProgrammingActionType> set = new HashSet<ProgrammingActionType>();
            switch (talLine.TaCategories)
            {
                case PsdzTaCategories.BlFlash:
                case PsdzTaCategories.GatewayTableDeploy:
                case PsdzTaCategories.SwDeploy:
                case PsdzTaCategories.EcuActivate:
                case PsdzTaCategories.EcuPoll:
                case PsdzTaCategories.EcuMirrorDeploy:
                case PsdzTaCategories.SmacTransferStart:
                case PsdzTaCategories.SmacTransferStatus:
                    set.Add(ProgrammingActionType.Programming);
                    break;
                case PsdzTaCategories.CdDeploy:
                    set.Add(ProgrammingActionType.Coding);
                    break;
                case PsdzTaCategories.FscBackup:
                    set.Add(ProgrammingActionType.FscBakup);
                    break;
                case PsdzTaCategories.FscDeploy:
                    foreach (PsdzSwtActionType? item in from ta in talLine.FscDeploy.Tas.OfType<PsdzFscDeployTa>()
                                                        select ta.Action)
                    {
                        switch (item)
                        {
                            case PsdzSwtActionType.ActivateStore:
                                set.Add(ProgrammingActionType.FscStore);
                                break;
                            case PsdzSwtActionType.ActivateUpdate:
                            case PsdzSwtActionType.ActivateUpgrade:
                            case PsdzSwtActionType.WriteVin:
                                set.Add(ProgrammingActionType.FscActivate);
                                break;
                            case PsdzSwtActionType.Deactivate:
                                set.Add(ProgrammingActionType.FscDeactivate);
                                break;
                            default:
                                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unsupported FSC action {0}.", item));
                        }
                    }
                    break;
                case PsdzTaCategories.HddUpdate:
                    set.Add(ProgrammingActionType.HddUpdate);
                    break;
                case PsdzTaCategories.HwDeinstall:
                    set.Add(ProgrammingActionType.Unmounting);
                    break;
                case PsdzTaCategories.HwInstall:
                    set.Add(ProgrammingActionType.Mounting);
                    break;
                case PsdzTaCategories.IbaDeploy:
                    set.Add(ProgrammingActionType.IbaDeploy);
                    break;
                case PsdzTaCategories.IdBackup:
                    set.Add(ProgrammingActionType.IdSave);
                    break;
                case PsdzTaCategories.IdRestore:
                    set.Add(ProgrammingActionType.IdRestore);
                    break;
                case PsdzTaCategories.SFADeploy:
                    if (talLine.SFADeploy.Tas.OfType<PsdzSFAWriteTA>().Any())
                    {
                        set.Add(ProgrammingActionType.SFAWrite);
                    }
                    if (talLine.SFADeploy.Tas.OfType<PsdzSFADeleteTA>().Any())
                    {
                        set.Add(ProgrammingActionType.SFADelete);
                    }
                    if (talLine.SFADeploy.Tas.OfType<PsdzSFAVerifyTA>().Any())
                    {
                        set.Add(ProgrammingActionType.SFAVerfy);
                    }
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unsupported TA category type {0}.", talLine.TaCategories));
                case PsdzTaCategories.Unknown:
                    break;
            }
            return set;
        }

        internal static IEnumerable<IPsdzTa> FilterTalsByProgrammingActionType(IPsdzTalLine talLine, ProgrammingActionType programmingActionType)
        {
            switch (programmingActionType)
            {
                case ProgrammingActionType.FscStore:
                    if (talLine.TaCategories != PsdzTaCategories.FscDeploy)
                    {
                        return new List<PsdzFscDeployTa>();
                    }
                    return from ta in talLine.TaCategory.Tas.OfType<PsdzFscDeployTa>()
                           where ta.Action == PsdzSwtActionType.ActivateStore
                           select ta;
                case ProgrammingActionType.FscActivate:
                    if (talLine.TaCategories != PsdzTaCategories.FscDeploy)
                    {
                        return new List<PsdzFscDeployTa>();
                    }
                    return from ta in talLine.TaCategory.Tas.OfType<PsdzFscDeployTa>()
                           where ta.Action == PsdzSwtActionType.ActivateUpdate || ta.Action == PsdzSwtActionType.ActivateUpgrade || ta.Action == PsdzSwtActionType.WriteVin
                           select ta;
                case ProgrammingActionType.FscDeactivate:
                    if (talLine.TaCategories != PsdzTaCategories.FscDeploy)
                    {
                        return new List<PsdzFscDeployTa>();
                    }
                    return from ta in talLine.TaCategory.Tas.OfType<PsdzFscDeployTa>()
                           where ta.Action == PsdzSwtActionType.Deactivate
                           select ta;
                case ProgrammingActionType.SFAWrite:
                    if (talLine.TaCategories != PsdzTaCategories.SFADeploy)
                    {
                        return new List<PsdzSFAWriteTA>();
                    }
                    return talLine.TaCategory.Tas.OfType<PsdzSFAWriteTA>();
                case ProgrammingActionType.SFADelete:
                    if (talLine.TaCategories != PsdzTaCategories.SFADeploy)
                    {
                        return new List<PsdzSFADeleteTA>();
                    }
                    return talLine.TaCategory.Tas.OfType<PsdzSFADeleteTA>();
                case ProgrammingActionType.SFAVerfy:
                    if (talLine.TaCategories != PsdzTaCategories.SFADeploy)
                    {
                        return new List<PsdzSFAVerifyTA>();
                    }
                    return talLine.TaCategory.Tas.OfType<PsdzSFAVerifyTA>();
                default:
                    {
                        ISet<ProgrammingActionType> set = MapProgrammingActionType(talLine);
                        if (set.Count == 0)
                        {
                            Log.Warning(Log.CurrentMethod(), "talLineActionTypes was empty. Assuming all tas are of the correct type.");
                            return talLine.TaCategory.Tas;
                        }
                        if (set.Count > 1)
                        {
                            Log.Warning(Log.CurrentMethod(), "talLineActionTypes had multiple values yet fell through to the default handling. This likely means that MapProgrammingActionType has been changed with more special cases, so updating this method as well would probably not be amiss.");
                        }
                        if (!set.Contains(programmingActionType))
                        {
                            return new List<IPsdzTa>();
                        }
                        return talLine.TaCategory.Tas;
                    }
            }
        }

        private IList<SgbmIdChange> GetSgbmIds(IPsdzTalLine talLine)
        {
            IList<SgbmIdChange> list = new List<SgbmIdChange>();
            foreach (IPsdzTa ta in talLine.TaCategory.Tas)
            {
                if (talLine.TaCategories == PsdzTaCategories.SmacTransferStart || talLine.TaCategories == PsdzTaCategories.SmacTransferStatus)
                {
                    if (!(Ecu is SmartActuatorECU smacEcu))
                    {
                        Log.Warning(Log.CurrentMethod(), "'" + Ecu.EcuUid + "' seems to be Master and not Smac");
                        continue;
                    }
                    Log.Info(Log.CurrentMethod(), $"Found SmacTransfer TalLine: '{ta.Id}'");
                    if (talLine.TaCategories == PsdzTaCategories.SmacTransferStart)
                    {
                        list.AddRange(GetSmacTransferStartSgbmIds(ta, smacEcu));
                    }
                }
                else
                {
                    IPsdzSgbmId sgbmId = ta.SgbmId;
                    if (sgbmId != null)
                    {
                        SgbmIdentifier sgbmIdentifier = BuildSgmbIdentifier(sgbmId);
                        list.Add(new SgbmIdChange(GetSgbmIdActual(sgbmId), sgbmIdentifier.ToString()));
                    }
                }
            }
            return list;
        }

        private List<SgbmIdChange> GetSmacTransferStartSgbmIds(IPsdzTa ta, SmartActuatorECU smacEcu)
        {
            List<SgbmIdChange> list = new List<SgbmIdChange>();
            IList<IPsdzSgbmId> value;
            if (!(ta is PsdzSmacTransferStartTA psdzSmacTransferStartTA))
            {
                Log.Error(Log.CurrentMethod(), $"TA '{ta}' is not of type PsdzSmacTransferStartTA");
            }
            else if (psdzSmacTransferStartTA.SmartActuatorData.TryGetValue(smacEcu.SmacID, out value))
            {
                foreach (IPsdzSgbmId item in value)
                {
                    SgbmIdentifier sgbmIdentifier = BuildSgmbIdentifier(item);
                    list.Add(new SgbmIdChange(GetSgbmIdActual(item), sgbmIdentifier.ToString()));
                }
            }
            return list;
        }

        private SgbmIdentifier BuildSgmbIdentifier(IPsdzSgbmId id)
        {
            return new SgbmIdentifier
            {
                ProcessClass = id.ProcessClass,
                Id = id.IdAsLong,
                MainVersion = id.MainVersion,
                SubVersion = id.SubVersion,
                PatchVersion = id.PatchVersion
            };
        }

        private string GetSgbmIdActual(IPsdzSgbmId target)
        {
            if (SvkCurrent != null && SvkCurrent.SgbmIds != null)
            {
                foreach (ISgbmId sgbmId in data.SvkCurrent.SgbmIds)
                {
                    if (sgbmId != null && target.ProcessClass == sgbmId.ProcessClass && target.IdAsLong == sgbmId.Id)
                    {
                        return sgbmId.ToString();
                    }
                }
            }
            return "--";
        }

        private IDictionary<ProgrammingActionType, IList<SgbmIdChange>> CalculateActionStates(IEnumerable<IPsdzTalLine> talLines)
        {
            Dictionary<ProgrammingActionType, IList<SgbmIdChange>> dictionary = new Dictionary<ProgrammingActionType, IList<SgbmIdChange>>();
            foreach (IPsdzTalLine talLine in talLines)
            {
                foreach (ProgrammingActionType item in MapProgrammingActionType(talLine))
                {
                    if (!dictionary.ContainsKey(item))
                    {
                        dictionary.Add(item, new List<SgbmIdChange>());
                    }
                    IList<SgbmIdChange> sgbmIds = GetSgbmIds(talLine);
                    dictionary[item].AddRange(sgbmIds);
                }
            }
            return dictionary;
        }

        private void OnProgrammingActionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object newItem in e.NewItems)
                    {
                        if (newItem is INotifyPropertyChanged notifyPropertyChanged2)
                        {
                            notifyPropertyChanged2.PropertyChanged += OnPropertyChanged;
                        }
                    }
                    this.PropertyChanged.NotifyPropertyChanged(this, () => State);
                    OnPropertyChanged("Item[]");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object newItem2 in e.NewItems)
                    {
                        if (newItem2 is INotifyPropertyChanged notifyPropertyChanged)
                        {
                            notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
                        }
                    }
                    this.PropertyChanged.NotifyPropertyChanged(this, () => State);
                    OnPropertyChanged("Item[]");
                    break;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ("StateProgramming".Equals(e.PropertyName) || "IsSelected".Equals(e.PropertyName))
            {
                this.PropertyChanged.NotifyPropertyChanged(this, () => State);
            }
        }
    }
}
