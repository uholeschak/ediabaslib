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
using PsdzClientLibrary.Core;

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
		internal ProgrammingAction(IEcu parentEcu, ProgrammingActionType type, bool isEditable, int order)
		{
			this.data = new ProgrammingActionData();
			this.ParentEcu = parentEcu;
			this.data.ParentEcu = parentEcu;
			this.data.Type = type;
			this.data.IsEditable = isEditable;
			this.data.Order = order;
			this.data.StateProgramming = ProgrammingActionState.ActionPlanned;
			this.SgbmIds = new List<ISgbmIdChange>();
			//this.EscalationSteps = new List<IEscalationStep>();
			//this.Title = ProgrammingAction.BuildTitle(this.Type, this.ParentEcu, ConfigSettings.CurrentUICulture);
			this.data.Channel = string.Empty;
			this.data.Note = string.Empty;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public DateTime StartExecution { get; internal set; }

		public DateTime EndExecution { get; internal set; }

		public IList<ISgbmIdChange> SgbmIds
		{
			get
			{
				return this.sgbmIds;
			}
			internal set
			{
				this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Property(Expression.Constant(this, typeof(ProgrammingAction)), "SgbmIds"), Array.Empty<ParameterExpression>()), ref this.sgbmIds, value);
			}
		}

		public string Title { get; internal set; }

		public typeDiagObjectState State
		{
			get
			{
				return this.stateDiag;
			}
			internal set
			{
				this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Convert(Expression.Property(Expression.Constant(this, typeof(ProgrammingAction)), "State"), typeof(object)), Array.Empty<ParameterExpression>()), ref this.stateDiag, value);
			}
		}

		public string AssemblyNumberSetPoint
		{
			get
			{
				return this.assemblyNumberSetPoint;
			}
			internal set
            {
				this.PropertyChanged.NotifyPropertyChanged(this, Expression.Lambda<Func<object>>(Expression.Property(Expression.Constant(this, typeof(ProgrammingAction)), "AssemblyNumberSetPoint"), Array.Empty<ParameterExpression>()), ref this.assemblyNumberSetPoint, value);
			}
		}

		public string Channel
		{
			get
			{
				return this.data.Channel;
			}
			internal set
			{
				this.data.Channel = value;
			}
		}

		public bool IsEditable
		{
			get
			{
				return this.data.IsEditable;
			}
		}

		public bool IsFlashAction
		{
			get
			{
				return this.data.IsFlashAction;
			}
		}

		public bool IsSelected
		{
			get
			{
				return this.data.IsSelected;
			}
			private set
			{
				this.data.IsSelected = value;
				this.OnPropertyChanged("IsSelected");
			}
		}

		public string Note
		{
			get
			{
				return this.data.Note;
			}
			internal set
			{
				this.data.Note = value;
			}
		}

		public int Order
		{
			get
			{
				return this.data.Order;
			}
		}

		public IEcu ParentEcu { get; private set; }

		public string PartNumber
		{
			get
			{
				if (this.ParentEcu == null)
				{
					return null;
				}
				return this.ParentEcu.ID_BMW_NR;
			}
		}

		public ProgrammingActionState StateProgramming
		{
			get
			{
				return this.data.StateProgramming;
			}
			internal set
			{
				this.data.StateProgramming = value;
				this.SetStateDiag();
			}
		}

		public ProgrammingActionType Type
		{
			get
			{
				return this.data.Type;
			}
		}

		public string InfoType
		{
			get
			{
				return ProgrammingAction.BuildTherapyPlanType(this.Type);
			}
		}

		//public ICollection<IEscalationStep> EscalationSteps { get; private set; }

		//internal IList<LocalizedText> TitleExtension { get; set; }

		//public ITherapyPlanActionData ActionData { get; set; }

		internal static string BuildTherapyPlanType(ProgrammingActionType type)
		{
			if (type <= ProgrammingActionType.FscStore)
			{
				if (type <= ProgrammingActionType.Unmounting)
				{
					if (type <= ProgrammingActionType.BootloaderProgramming)
					{
						if (type == ProgrammingActionType.Programming || type == ProgrammingActionType.BootloaderProgramming)
						{
							return SwiActionCategory.PRG.ToString();
						}
					}
					else
					{
						if (type == ProgrammingActionType.Coding)
						{
							return SwiActionCategory.COD.ToString();
						}
						if (type == ProgrammingActionType.Unmounting)
						{
							return SwiActionCategory.UNM.ToString();
						}
					}
				}
				else if (type <= ProgrammingActionType.Replacement)
				{
					if (type == ProgrammingActionType.Mounting)
					{
						return SwiActionCategory.MNT.ToString();
					}
					if (type == ProgrammingActionType.Replacement)
					{
						return "HWA";
					}
				}
				else
				{
					if (type == ProgrammingActionType.FscBakup)
					{
						return "FCB";
					}
					if (type == ProgrammingActionType.FscStore)
					{
						return "FCS";
					}
				}
			}
			else if (type <= ProgrammingActionType.IdSave)
			{
				if (type <= ProgrammingActionType.FscDeactivate)
				{
					if (type == ProgrammingActionType.FscActivate)
					{
						return SwiActionCategory.FCA.ToString();
					}
					if (type == ProgrammingActionType.FscDeactivate)
					{
						return SwiActionCategory.FCD.ToString();
					}
				}
				else
				{
					if (type == ProgrammingActionType.HddUpdate)
					{
						return SwiActionCategory.HDD.ToString();
					}
					if (type == ProgrammingActionType.IdSave)
					{
						return "IDS";
					}
				}
			}
			else if (type <= ProgrammingActionType.IbaDeploy)
			{
				if (type == ProgrammingActionType.IdRestore)
				{
					return SwiActionCategory.IDR.ToString();
				}
				if (type == ProgrammingActionType.IbaDeploy)
				{
					return "IBD";
				}
			}
			else
			{
				if (type == ProgrammingActionType.SFAWrite)
				{
					return "SFW";
				}
				if (type == ProgrammingActionType.SFADelete)
				{
					return "SFD";
				}
				if (type == ProgrammingActionType.SFAVerfy)
				{
					return "SFV";
				}
			}
			return "---";
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

		private ProgrammingActionData data;

		private string assemblyNumberSetPoint;

		//private string pn;

		private IList<ISgbmIdChange> sgbmIds;

		private typeDiagObjectState stateDiag;
	}
}
