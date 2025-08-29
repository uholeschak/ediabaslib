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

        public EcuProgrammingInfo(IEcu ecu, ProgrammingObjectBuilder programmingObjectBuilder, bool withInitData = true)
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
				this.InitData(ecu, programmingObjectBuilder);
			}
			this.Init();
		}

		internal EcuProgrammingInfo()
		{
			this.Ecu = null;
			this.Init();
			this.SvkCurrent = null;
		}

		private void Init()
		{
			this.scheduled = EcuScheduledState.NothingScheduled;
			//this.XepSwiActionList = new Collection<XEP_SWIACTION>();
			this.programmingActionList = new ObservableCollectionEx<ProgrammingAction>();
			this.programmingActionList.CollectionChanged += this.OnProgrammingActionsCollectionChanged;
		}

		protected void InitData(IEcu ecu, ProgrammingObjectBuilder programmingObjectBuilder)
		{
			if (this.data == null)
			{
				this.data = new EcuProgrammingInfoData();
			}
			this.Ecu = ecu;
			this.data.EcuTitle = this.Ecu.TITLE_ECUTREE;
			try
			{
				this.SvkCurrent = programmingObjectBuilder.Build(ecu.SVK);
			}
			catch (Exception exception)
			{
				Log.WarningException("EcuProgrammingInfo.EcuProgrammingInfo()", exception);
				this.SvkCurrent = null;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public IEcu Ecu
		{
			get
			{
				return this.data.Ecu;
			}
			private set
			{
				this.data.Ecu = value;
			}
		}

		public bool IsCodingDisabled
		{
			get
			{
				return this.data.IsCodingDisabled;
			}
			set
			{
				this.data.IsCodingDisabled = value;
			}
		}

		public bool IsCodingScheduled
		{
			get
			{
				return this.data.IsCodingScheduled;
			}
			set
			{
				if (value && !this.Scheduled.HasFlag(EcuScheduledState.CodingScheduledByUser))
				{
					this.Scheduled |= EcuScheduledState.CodingScheduledByUser;
					return;
				}
				if (!value && this.Scheduled.HasFlag(EcuScheduledState.CodingScheduledByUser))
				{
					this.Scheduled &= ~EcuScheduledState.CodingScheduledByUser;
				}
			}
		}

		public bool IsProgrammingDisabled
		{
			get
			{
				return this.data.IsProgrammingDisabled;
			}
			set
			{
				this.data.IsProgrammingDisabled = value;
			}
		}

		public bool IsProgrammingSelectionDisabled
		{
			get
			{
				return this.data.IsProgrammingSelectionDisabled;
			}
			set
			{
				this.data.IsProgrammingSelectionDisabled = value;
			}
		}

		public bool IsCodingSelectionDisabled
		{
			get
			{
				return this.data.IsCodingSelectionDisabled;
			}
			set
			{
				this.data.IsCodingSelectionDisabled = value;
			}
		}

		public bool IsProgrammingScheduled
		{
			get
			{
				return this.data.IsProgrammingScheduled;
			}
			set
			{
				if (value && !this.Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByUser))
				{
					this.Scheduled |= EcuScheduledState.ProgrammingScheduledByUser;
					return;
				}
				if (!value && this.Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByUser))
				{
					this.Scheduled &= ~EcuScheduledState.ProgrammingScheduledByUser;
				}
			}
		}

		public IEnumerable<IProgrammingAction> ProgrammingActions
		{
			get
			{
				return this.programmingActionList;
			}
		}

		public double ProgressValue
		{
			get
			{
				return this.data.ProgressValue;
			}
			set
			{
				this.data.ProgressValue = value;
				this.OnPropertyChanged("ProgressValue");
			}
		}

		public EcuScheduledState Scheduled
		{
			get
			{
				return this.scheduled;
			}
			private set
			{
				if (this.scheduled != value)
				{
					this.scheduled = value;
					this.data.IsProgrammingScheduled = (this.Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByUser) || this.Scheduled.HasFlag(EcuScheduledState.ProgrammingScheduledByLogistic));
					this.data.IsCodingScheduled = (this.Scheduled.HasFlag(EcuScheduledState.CodingScheduledByUser) || this.Scheduled.HasFlag(EcuScheduledState.CodingScheduledByLogistic));
				}
			}
		}

		public ProgrammingActionState? State
		{
			get
			{
				ProgrammingActionState? programmingActionState = null;
#if false
				foreach (IProgrammingAction programmingAction in this.ProgrammingActions)
				{
					if (programmingAction.IsSelected)
					{
						programmingActionState = new ProgrammingActionState?(programmingAction.StateProgramming);
						if (programmingActionState != null)
						{
							ProgrammingActionState? programmingActionState2 = programmingActionState;
							if (!(programmingActionState2.GetValueOrDefault() == ProgrammingActionState.ActionSuccessful & programmingActionState2 != null))
							{
								break;
							}
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
				return this.data.SvkCurrent;
			}
			internal set
			{
				this.data.SvkCurrent = value;
				this.OnPropertyChanged("SvkCurrent");
			}
		}

		public IStandardSvk SvkTarget
		{
			get
			{
				return this.data.SvkTarget;
			}
			internal set
			{
				this.data.SvkTarget = value;
				this.OnPropertyChanged("SvkTarget");
			}
		}

		public string Category
		{
			get
			{
				return this.data.Category;
			}
			set
			{
				this.data.Category = value;
			}
		}

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

		internal ProgrammingAction AddSingleProgrammingAction(ProgrammingActionType type, bool isSelected, int order, string channel, string assemblyNumberSetPoint, IList<SgbmIdChange> sgbmIdList = null, string extraTitle = null)
		{
			bool isExpertModeEnabled = true;
			ProgrammingAction programmingAction = new ProgrammingAction(this.Ecu, type, isExpertModeEnabled, order);
			programmingAction.Select(isSelected);
			programmingAction.Channel = channel;
			programmingAction.Note = ((this.Category != null) ? this.Category : string.Empty);
			if (sgbmIdList != null)
			{
				programmingAction.SgbmIds.AddRange(sgbmIdList);
			}
			if (!string.IsNullOrEmpty(extraTitle))
			{
				ProgrammingAction programmingAction2 = programmingAction;
				programmingAction2.Title = programmingAction2.Title + Environment.NewLine + extraTitle;
			}
			this.programmingActionList.Add(programmingAction);
			this.data.ProgrammingActionData.Add(programmingAction.DataContext);
			return programmingAction;
		}

		internal void Reset(EcuScheduledState remove, IList<IEcuProgrammingInfo> exchangeScheduled)
		{
			this.programmingActionList.Clear();
			if (remove == EcuScheduledState.NothingScheduled)
			{
				this.Scheduled = EcuScheduledState.NothingScheduled;
				if (!exchangeScheduled.Any((IEcuProgrammingInfo ecu) => ecu.Ecu.ID_SG_ADR == this.Ecu.ID_SG_ADR))
				{
					this.IsExchangeScheduled = false;
				}
				this.IsExchangeDone = false;
			}
			else
			{
				this.Scheduled &= ~remove;
			}
			this.IsCodingDisabled = this.IsCodingSelectionDisabled;
			this.IsProgrammingDisabled = this.IsProgrammingSelectionDisabled;
			this.IsExchangeScheduledDisabled = false;
			this.IsExchangeDoneDisabled = false;
		}

		internal void SetCodingScheduled()
		{
			this.IsCodingDisabled = true;
			this.Scheduled |= EcuScheduledState.CodingScheduledByLogistic;
		}

		internal void SetProgrammingScheduled()
		{
			this.IsProgrammingDisabled = true;
			this.Scheduled |= EcuScheduledState.ProgrammingScheduledByLogistic;
		}

		internal bool UpdateSingleProgrammingAction(ProgrammingActionType type, ProgrammingActionState state)
		{
			using (IEnumerator<ProgrammingAction> enumerator = this.programmingActionList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ProgrammingAction programmingAction = enumerator.Current;
					if (programmingAction.Type == type)
					{
						programmingAction.StateProgramming = state;
						return true;
					}
				}
				return false;
			}
		}

		internal bool UpdateSingleProgrammingAction(ProgrammingActionType type, ProgrammingActionState state, bool executed)
		{
			using (IEnumerator<ProgrammingAction> enumerator = this.programmingActionList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ProgrammingAction programmingAction = enumerator.Current;
					if (programmingAction.Type == type)
					{
						programmingAction.UpdateState(state, executed);
						return true;
					}
				}
				return false;
			}
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
				foreach (ProgrammingActionType key in EcuProgrammingInfo.MapProgrammingActionType(talLine))
				{
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, new List<SgbmIdChange>());
					}
					IList<SgbmIdChange> sgbmIds = this.GetSgbmIds(talLine);
					dictionary[key].AddRange(sgbmIds);
				}
			}
			return dictionary;
		}

		private void OnProgrammingActionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedAction action = e.Action;
			if (action == NotifyCollectionChangedAction.Add)
			{
				foreach (object obj in e.NewItems)
				{
					INotifyPropertyChanged notifyPropertyChanged = obj as INotifyPropertyChanged;
					if (notifyPropertyChanged != null)
					{
						notifyPropertyChanged.PropertyChanged += this.OnPropertyChanged;
					}
				}
				this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Convert(Expression.Property(Expression.Constant(this, typeof(EcuProgrammingInfo)), "State"), typeof(object)), Array.Empty<ParameterExpression>()));
				this.OnPropertyChanged("Item[]");
				return;
			}
			if (action != NotifyCollectionChangedAction.Remove)
			{
				return;
			}
			foreach (object obj2 in e.NewItems)
			{
				INotifyPropertyChanged notifyPropertyChanged2 = obj2 as INotifyPropertyChanged;
				if (notifyPropertyChanged2 != null)
				{
					notifyPropertyChanged2.PropertyChanged -= this.OnPropertyChanged;
				}
			}
			this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Convert(Expression.Property(Expression.Constant(this, typeof(EcuProgrammingInfo)), "State"), typeof(object)), Array.Empty<ParameterExpression>()));
			this.OnPropertyChanged("Item[]");
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
				this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Convert(Expression.Property(Expression.Constant(this, typeof(EcuProgrammingInfo)), "State"), typeof(object)), Array.Empty<ParameterExpression>()));
			}
		}
	}
}
