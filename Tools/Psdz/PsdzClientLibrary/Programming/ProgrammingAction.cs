using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    public enum typeDiagObjectState
    {
        NotCalled,
        Minimized,
        Suspected,
        Canceled,
        Performed,
        Running
    }

    public enum SwiActionCategory
    {
        AL,
        UR,
        RR,
        CU,
        NR,
        FCA,
        UNM,
        FCD,
        IDR,
        PRG,
        HDD,
        COD,
        MNT,
        SFA
    }

	public class ProgrammingAction : IComparable<IProgrammingAction>, INotifyPropertyChanged, IProgrammingAction
	{
        private ProgrammingActionData data;

        private string assemblyNumberSetPoint;

        //private string pn;

        private IList<ISgbmIdChange> sgbmIds;

        private typeDiagObjectState stateDiag;

        protected string titleTextId;

        internal ProgrammingAction(IEcu parentEcu, ProgrammingActionType type, bool isEditable, int order)
        {
            data = new ProgrammingActionData();
            ParentEcu = parentEcu;
            data.ParentEcu = parentEcu;
            data.Type = type;
            data.IsEditable = isEditable;
            data.Order = order;
            data.StateProgramming = ProgrammingActionState.ActionPlanned;
            SgbmIds = new List<ISgbmIdChange>();
            //EscalationSteps = new List<IEscalationStep>();
            Title = BuildTitle(Type, ParentEcu, ConfigSettings.CurrentUICulture);
            data.Channel = string.Empty;
            data.Note = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTime StartExecution { get; internal set; }

        public DateTime EndExecution { get; internal set; }

        public IList<ISgbmIdChange> SgbmIds
        {
            get
            {
                return sgbmIds;
            }
            internal set
            {
                this.PropertyChanged.NotifyPropertyChanged(this, () => SgbmIds, ref sgbmIds, value);
                if (ActionData != null)
                {
                    ActionData.SetSgbmIds(value);
                }
            }
        }

        public string Title { get; internal set; }

        public typeDiagObjectState State
        {
            get
            {
                return stateDiag;
            }
            internal set
            {
                this.PropertyChanged.NotifyPropertyChanged(this, () => State, ref stateDiag, value);
                if (ActionData != null)
                {
                    ActionData.SetState(value);
                }
            }
        }

        public string AssemblyNumberSetPoint
        {
            get
            {
                return assemblyNumberSetPoint;
            }
            internal set
            {
                this.PropertyChanged.NotifyPropertyChanged(this, () => AssemblyNumberSetPoint, ref assemblyNumberSetPoint, value);
            }
        }

        public string Channel
        {
            get
            {
                return data.Channel;
            }
            internal set
            {
                data.Channel = value;
            }
        }

        public bool IsEditable => data.IsEditable;

        public bool IsFlashAction => data.IsFlashAction;

        public bool IsSelected
        {
            get
            {
                return data.IsSelected;
            }
            private set
            {
                data.IsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public string Note
        {
            get
            {
                return data.Note;
            }
            internal set
            {
                data.Note = value;
            }
        }

        public int Order => data.Order;

        public IEcu ParentEcu { get; private set; }

        public IList<int> AffectedEcuDiagAddr { get; internal set; }

        public string PartNumber
        {
            get
            {
                if (ParentEcu == null)
                {
                    return null;
                }
                return ParentEcu.ID_BMW_NR;
            }
        }

        public ProgrammingActionState StateProgramming
        {
            get
            {
                return data.StateProgramming;
            }
            internal set
            {
                data.StateProgramming = value;
                if (ActionData != null)
                {
                    ActionData.SetStateProgramming(value);
                }
                SetStateDiag();
            }
        }

        public ProgrammingActionType Type => data.Type;

        public string InfoType => BuildTherapyPlanType(Type);

        //public ICollection<IEscalationStep> EscalationSteps { get; private set; }

        internal IList<LocalizedText> TitleExtension { get; set; }

        public ITherapyPlanActionData ActionData { get; set; }

        internal static string BuildTherapyPlanType(ProgrammingActionType type)
        {
            switch (type)
            {
                case ProgrammingActionType.Programming:
                case ProgrammingActionType.BootloaderProgramming:
                    return SwiActionCategory.PRG.ToString();
                case ProgrammingActionType.Coding:
                    return SwiActionCategory.COD.ToString();
                case ProgrammingActionType.FscActivate:
                    return SwiActionCategory.FCA.ToString();
                case ProgrammingActionType.FscBakup:
                    return "FCB";
                case ProgrammingActionType.FscDeactivate:
                    return SwiActionCategory.FCD.ToString();
                case ProgrammingActionType.FscStore:
                    return "FCS";
                case ProgrammingActionType.HddUpdate:
                    return SwiActionCategory.HDD.ToString();
                case ProgrammingActionType.HDDUpdateAndroid:
                    return "HDA";
                case ProgrammingActionType.IbaDeploy:
                    return "IBD";
                case ProgrammingActionType.IdRestore:
                    return SwiActionCategory.IDR.ToString();
                case ProgrammingActionType.IdSave:
                    return "IDS";
                case ProgrammingActionType.Mounting:
                    return SwiActionCategory.MNT.ToString();
                case ProgrammingActionType.Unmounting:
                    return SwiActionCategory.UNM.ToString();
                case ProgrammingActionType.Replacement:
                    return "HWA";
                case ProgrammingActionType.SFAWrite:
                    return "SFW";
                case ProgrammingActionType.SFADelete:
                    return "SFD";
                case ProgrammingActionType.SFAVerfy:
                    return "SFV";
                default:
                    Log.Warning("TherapyPlanActionProgrammingManual.BuildTherapyPlanType()", "Unsupported programming action type \"{0}\". Using \"{1}\" instead.", type, "---");
                    return "---";
            }
        }

        internal bool IsFailureIgnored
		{
			get
			{
				return this.Type == ProgrammingActionType.FscActivate || this.Type == ProgrammingActionType.FscBakup || this.Type == ProgrammingActionType.FscDeactivate || this.Type == ProgrammingActionType.FscStore || this.Type == ProgrammingActionType.IdRestore || this.Type == ProgrammingActionType.IdSave;
			}
		}

		public int CompareTo(IProgrammingAction other)
		{
			if (this.Order < other.Order)
			{
				return -1;
			}
			if (this.Order > other.Order)
			{
				return 1;
			}
			return 0;
		}

		public bool RequiresEscalation()
		{
			return this.StateProgramming != ProgrammingActionState.ActionSuccessful && this.data.IsEscalationActionType;
		}

		public string GetShortType()
		{
			return this.Type.ToString().Substring(0, 1);
		}

		public bool Select(bool value)
		{
			this.IsSelected = value;
			return true;
		}

        public IList<LocalizedText> GetLocalizedObjectTitle(IList<string> lang)
        {
            IList<LocalizedText> list = BuildTitle(Type, ParentEcu, lang, titleTextId);
            if (TitleExtension != null && TitleExtension.Any())
            {
                IList<LocalizedText> list2 = new List<LocalizedText>();
                {
                    foreach (string l in lang)
                    {
                        LocalizedText localizedText = list.Single((LocalizedText t) => l.Equals(t.Language));
                        LocalizedText localizedText2 = TitleExtension.Single((LocalizedText t) => l.Equals(t.Language));
                        localizedText.TextItem += localizedText2.TextItem;
                        list2.Add(localizedText);
                    }
                    return list2;
                }
            }
            return list;
        }

        public static IList<LocalizedText> BuildTitle(ProgrammingActionType type, IEcu ecu, IList<string> lang, string textIdentifierOverride = null)
        {
            List<LocalizedText> list = new List<LocalizedText>();
            list.AddRange(lang.Select((string x) => new LocalizedText(BuildTitle(type, ecu, x, textIdentifierOverride), x)));
            return list;
        }

        protected static string BuildTitle(ProgrammingActionType type, IEcu ecu, string lang, string textIdentifierOverride = null)
        {
            string arg = new FormatedData(textIdentifierOverride ?? ("#ProgrammingActionType." + type)).Localize(new CultureInfo(lang));
            return string.Format(CultureInfo.InvariantCulture, "{0} {1}", arg, ecu.TITLE_ECUTREE);
        }

        internal void UpdateState(ProgrammingActionState state, bool executed)
		{
			if (this.IsFailureIgnored && executed && state != ProgrammingActionState.ActionSuccessful)
			{
				this.StateProgramming = ProgrammingActionState.ActionWarning;
				return;
			}
			if (this.IsFailureIgnored && !executed && (state == ProgrammingActionState.ActionFailed || state == ProgrammingActionState.MissingPrerequisitesForAction))
			{
				this.StateProgramming = ProgrammingActionState.ActionWarning;
				return;
			}
			this.StateProgramming = state;
		}

		internal void UpdateState(IEnumerable<IPsdzTalLine> talLines)
		{
			ProgrammingActionState? programmingActionState = null;
			foreach (IPsdzTalLine talLine in talLines)
			{
				ProgrammingActionState value = this.CalculateProgrammingState(talLine);
				if (programmingActionState == null || value.CompareTo(programmingActionState) > 0)
				{
					programmingActionState = new ProgrammingActionState?(value);
				}
			}
			if (programmingActionState != null)
			{
				this.UpdateState(programmingActionState.Value, false);
			}
		}

		internal void Update(IEnumerable<IPsdzTalLine> talLines, int escalationSteps)
		{
			DateTime? dateTime = null;
			DateTime? dateTime2 = null;
			ProgrammingActionState? programmingActionState = null;
			foreach (IPsdzTalLine psdzTalLine in talLines)
			{
				ProgrammingActionState value = this.CalculateProgrammingState(psdzTalLine);
				if (programmingActionState == null || value.CompareTo(programmingActionState) > 0)
				{
					programmingActionState = new ProgrammingActionState?(value);
				}
				if (dateTime == null || psdzTalLine.StartTime < dateTime)
				{
					dateTime = new DateTime?(psdzTalLine.StartTime);
				}
				if (dateTime2 == null || psdzTalLine.EndTime > dateTime2)
				{
					dateTime2 = new DateTime?(psdzTalLine.EndTime);
				}
			}
		}

		private ProgrammingActionState CalculateProgrammingState(IPsdzTalLine talLine)
		{
			ProgrammingActionState programmingActionState;
			if (talLine.TaCategories != PsdzTaCategories.FscDeploy)
			{
				programmingActionState = this.MapState(talLine.ExecutionState);
			}
			else
			{
				programmingActionState = ProgrammingActionState.ActionSuccessful;
				foreach (IPsdzTa psdzTa in this.GetFscTas(talLine.FscDeploy))
				{
					ProgrammingActionState programmingActionState2 = this.MapState(psdzTa.ExecutionState);
					if (programmingActionState2.CompareTo(programmingActionState) > 0)
					{
						programmingActionState = programmingActionState2;
					}
				}
			}
			return programmingActionState;
		}

		private ProgrammingActionState MapState(PsdzTaExecutionState? executionStateInput)
		{
			if (executionStateInput != null)
			{
				PsdzTaExecutionState value = executionStateInput.Value;
				switch (value)
				{
					case PsdzTaExecutionState.Executable:
					case PsdzTaExecutionState.Inactive:
						break;
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
						throw new ArgumentException(string.Format("Unsupported TA execution state: {0}", value));
				}
			}
			else
			{
				Log.Warning("ProgrammingAction.MapState", "input is null. 'TaExecutionState.Inactive' will be used.", Array.Empty<object>());
			}
			return ProgrammingActionState.ActionPlanned;
		}

		private void SetStateDiag()
		{
			ProgrammingActionState stateProgramming = this.data.StateProgramming;
			if (stateProgramming <= ProgrammingActionState.ActionPlanned)
			{
				if (stateProgramming - ProgrammingActionState.ActionSuccessful <= 1)
				{
					this.State = typeDiagObjectState.Performed;
                    return;
				}
				if (stateProgramming == ProgrammingActionState.ActionPlanned)
				{
                    this.State = typeDiagObjectState.NotCalled;
                    return;
				}
			}
			else
			{
				if (stateProgramming == ProgrammingActionState.MissingPrerequisitesForAction)
				{
                    this.State = typeDiagObjectState.NotCalled;
                    return;
				}
				if (stateProgramming == ProgrammingActionState.ActionFailed)
				{
					this.State = typeDiagObjectState.Canceled;
                    return;
				}
				if (stateProgramming == ProgrammingActionState.ActionInProcess)
				{
					this.State = typeDiagObjectState.Running;
					return;
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unsupported programming state {0}.", this.data.StateProgramming));
		}

		private ICollection<IPsdzTa> GetFscTas(PsdzFscDeploy fscDeploy)
		{
			IEnumerable<PsdzFscDeployTa> source = fscDeploy.Tas.OfType<PsdzFscDeployTa>();
			ICollection<IPsdzTa> collection = new List<IPsdzTa>();
			ProgrammingActionType type = this.data.Type;
			if (type != ProgrammingActionType.FscStore)
			{
				if (type != ProgrammingActionType.FscActivate)
				{
					if (type != ProgrammingActionType.FscDeactivate)
					{
					}
					else
					{
						collection.AddRange(source.Where(delegate (PsdzFscDeployTa ta)
						{
							PsdzSwtActionType? action = ta.Action;
							return action.GetValueOrDefault() == PsdzSwtActionType.Deactivate & action != null;
						}));
					}
				}
				else
				{
					collection.AddRange(source.Where(delegate (PsdzFscDeployTa ta)
					{
						PsdzSwtActionType? action = ta.Action;
						if (!(action.GetValueOrDefault() == PsdzSwtActionType.ActivateUpdate & action != null))
						{
							action = ta.Action;
							return action.GetValueOrDefault() == PsdzSwtActionType.ActivateUpgrade & action != null;
						}
						return true;
					}));
				}
			}
			else
			{
				collection.AddRange(source.Where(delegate (PsdzFscDeployTa ta)
				{
					PsdzSwtActionType? action = ta.Action;
					return action.GetValueOrDefault() == PsdzSwtActionType.ActivateStore & action != null;
				}));
			}
			return collection;
		}

		private void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public IProgrammingActionData DataContext
		{
			get
			{
				return this.data;
			}
		}
    }
}
