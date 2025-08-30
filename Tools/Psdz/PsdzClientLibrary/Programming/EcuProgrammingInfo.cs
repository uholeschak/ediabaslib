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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        public ProgrammingActionState? State
        {
            get
            {
                ProgrammingActionState? programmingActionState = null;
#if false
                foreach (IProgrammingAction programmingAction in ProgrammingActions)
                {
                    if (programmingAction.IsSelected)
                    {
                        programmingActionState = programmingAction.StateProgramming;
                        if (programmingActionState.HasValue && programmingActionState != ProgrammingActionState.ActionSuccessful)
                        {
                            break;
                        }
                    }
                }
#endif
                return programmingActionState;
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

        private void Init()
        {
            scheduled = EcuScheduledState.NothingScheduled;
            //XepSwiActionList = new Collection<XEP_SWIACTION>();
            programmingActionList = new ObservableCollectionEx<ProgrammingAction>();
            programmingActionList.CollectionChanged += OnProgrammingActionsCollectionChanged;
        }

        protected void InitData(IEcu ecu, ProgrammingObjectBuilder programmingObjectBuilder)
        {
            if (data == null)
            {
                data = new EcuProgrammingInfoData();
            }
            Ecu = ecu;
            data.EcuTitle = Ecu.TITLE_ECUTREE;
#if false
            if (Ecu is ECU eCU && eCU.XepEcuClique != null && eCU.XepEcuClique.TITLEID.HasValue)
            {
                data.EcuDescription = eCU.XepEcuClique.Title;
            }
            else
#endif
            {
                data.EcuDescription = data.EcuTitle;
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

		public int FlashOrder
		{
			get
			{
				return this.Data.FlashOrder;
			}
			set
			{
				this.Data.FlashOrder = value;
			}
		}

		public bool IsExchangeDoneDisabled
		{
			get
			{
				return this.data.IsExchangeDoneDisabled;
			}
			set
			{
				this.data.IsExchangeDoneDisabled = value;
			}
		}

		public bool IsExchangeScheduledDisabled
		{
			get
			{
				return this.data.IsExchangeScheduledDisabled;
			}
			set
			{
				this.data.IsExchangeScheduledDisabled = value;
			}
		}

		public bool IsExchangeDone
		{
			get
			{
				return this.data.IsExchangeDone;
			}
			set
			{
				this.data.IsExchangeDone = value;
				this.data.IsExchangeScheduledDisabled = value;
			}
		}

		public bool IsExchangeScheduled
		{
			get
			{
				return this.data.IsExchangeScheduled;
			}
			set
			{
				this.data.IsExchangeScheduled = value;
				this.data.IsExchangeDoneDisabled = value;
			}
		}

		public virtual string EcuIdentifier
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}_0x{1:X2}", this.Ecu.TITLE_ECUTREE, this.Ecu.ID_SG_ADR);
			}
		}

		//internal ICollection<XEP_SWIACTION> XepSwiActionList { get; private set; }

		public IProgrammingAction GetProgrammingAction(ProgrammingActionType type)
		{
			using (IEnumerator<ProgrammingAction> enumerator = this.programmingActionList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					IProgrammingAction programmingAction = enumerator.Current;
					if (programmingAction.Type == type)
					{
						return programmingAction;
					}
				}
			}
			return null;
		}

		public IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter)
		{
			IList<IProgrammingAction> list = new List<IProgrammingAction>();
			if (programmingActionTypeFilter == null)
			{
				list.AddRange(this.ProgrammingActions);
				return list;
			}
			foreach (IProgrammingAction programmingAction in this.ProgrammingActions)
			{
				if (programmingActionTypeFilter.Contains(programmingAction.Type))
				{
					list.Add(programmingAction);
				}
			}
			return list;
		}

		internal void UpdateProgrammingActions(IEnumerable<IPsdzTalLine> talLines, bool isTalExecuted, int escalationSteps = 0)
		{
			IDictionary<ProgrammingActionType, ICollection<IPsdzTalLine>> dictionary = new Dictionary<ProgrammingActionType, ICollection<IPsdzTalLine>>();
			foreach (IPsdzTalLine psdzTalLine in talLines)
			{
				foreach (ProgrammingActionType key in EcuProgrammingInfo.MapProgrammingActionType(psdzTalLine))
				{
					if (dictionary.ContainsKey(key))
					{
						dictionary[key].Add(psdzTalLine);
					}
					else
					{
						dictionary.Add(key, new List<IPsdzTalLine>
						{
							psdzTalLine
						});
					}
				}
			}
			foreach (ProgrammingActionType programmingActionType in dictionary.Keys)
			{
				ProgrammingAction programmingAction = this.GetProgrammingAction(programmingActionType) as ProgrammingAction;
				if (programmingAction != null)
				{
					if (isTalExecuted)
					{
						programmingAction.Update(dictionary[programmingActionType], escalationSteps);
					}
					else
					{
						programmingAction.UpdateState(dictionary[programmingActionType]);
					}
				}
			}
		}

		internal void AddProgrammingActions(IEnumerable<IPsdzTalLine> talLines, int order, bool preselectAction, bool codingAfterProgramming = false)
		{
			new List<IProgrammingAction>();
			if (talLines == null)
			{
				Log.Error("ProgrammingUtils.AddProgrammingActionsToEcu()", "Param 'talLines' is missing!", Array.Empty<object>());
			}
			IDictionary<ProgrammingActionType, IList<SgbmIdChange>> dictionary = this.CalculateActionStates(talLines);
			if (dictionary.ContainsKey(ProgrammingActionType.Unmounting) && dictionary.ContainsKey(ProgrammingActionType.Mounting))
			{
				IList<SgbmIdChange> list = new List<SgbmIdChange>();
				list.AddRange(dictionary[ProgrammingActionType.Unmounting]);
				list.AddRange(dictionary[ProgrammingActionType.Mounting]);
				this.AddSingleProgrammingAction(ProgrammingActionType.Replacement, preselectAction, order, null, null, list, null);
				return;
			}
			foreach (ProgrammingActionType programmingActionType in dictionary.Keys)
			{
				if (codingAfterProgramming && programmingActionType == ProgrammingActionType.Coding)
				{
					order *= 100;
				}
				string additionalTitleInfoForSfaActions = this.GetAdditionalTitleInfoForSfaActions(programmingActionType, talLines);
				this.AddSingleProgrammingAction(programmingActionType, preselectAction, order, null, null, dictionary[programmingActionType], additionalTitleInfoForSfaActions);
			}
		}

		private string GetAdditionalTitleInfoForSfaActions(ProgrammingActionType actionType, IEnumerable<IPsdzTalLine> talLines)
		{
			if (actionType == ProgrammingActionType.SFAWrite || actionType == ProgrammingActionType.SFADelete)
			{
				foreach (IPsdzTalLine psdzTalLine in (from x in talLines
													  where x.TaCategories == PsdzTaCategories.SFADeploy
													  select x).ToList<IPsdzTalLine>())
				{
					if (actionType == ProgrammingActionType.SFAWrite)
					{
						List<IPsdzFsaTa> list = psdzTalLine.SFADeploy.Tas.OfType<PsdzSFAWriteTA>().ToList<IPsdzFsaTa>();
						if (list != null && talLines.Any<IPsdzTalLine>())
						{
							return this.GetAdditionalTitle(list);
						}
					}
					else if (actionType == ProgrammingActionType.SFADelete)
					{
						List<IPsdzFsaTa> list2 = psdzTalLine.SFADeploy.Tas.OfType<PsdzSFADeleteTA>().ToList<IPsdzFsaTa>();
						if (list2 != null && talLines.Any<IPsdzTalLine>())
						{
							return this.GetAdditionalTitle(list2);
						}
					}
				}
			}
			return null;
		}

		private string GetAdditionalTitle(List<IPsdzFsaTa> tas)
		{
			return string.Join(Environment.NewLine ?? "", from x in tas
														  select string.Concat(new string[]
														  {
				" ",
				x.FeatureId.ToString(),
				" (",
				string.Format(CultureInfo.InvariantCulture, "0x{0:X6}", x.FeatureId),
				")"
														  }));
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
