using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Programming;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;

#pragma warning disable CS0169
namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "Changed to public", AccessModified = true)]
    public class ProgrammingAction : IProgrammingAction, INotifyPropertyChanged, IComparable<IProgrammingAction>, ITherapyPlanAction2, ITherapyPlanAction
    {
        private ProgrammingActionData data;
        private string assemblyNumberSetPoint;
        private string pn;
        private IList<ISgbmIdChange> sgbmIds;
        private typeDiagObjectState stateDiag;
        protected string titleTextId;
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
        public ICollection<IEscalationStep> EscalationSteps { get; private set; }
        internal IList<LocalizedText> TitleExtension { get; set; }
        public ITherapyPlanActionData ActionData { get; set; }

        internal bool IsFailureIgnored
        {
            get
            {
                if (Type != ProgrammingActionType.FscActivate && Type != ProgrammingActionType.FscBakup && Type != ProgrammingActionType.FscDeactivate && Type != ProgrammingActionType.FscStore && Type != ProgrammingActionType.IdRestore)
                {
                    return Type == ProgrammingActionType.IdSave;
                }

                return true;
            }
        }

        public IProgrammingActionData DataContext => data;

        public event PropertyChangedEventHandler PropertyChanged;
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
            EscalationSteps = new List<IEscalationStep>();
            Title = BuildTitle(Type, ParentEcu, ConfigSettings.CurrentUICulture);
            data.Channel = string.Empty;
            data.Note = string.Empty;
        }

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

        public int CompareTo(IProgrammingAction other)
        {
            if (Order < other.Order)
            {
                return -1;
            }

            if (Order > other.Order)
            {
                return 1;
            }

            return 0;
        }

        public bool RequiresEscalation()
        {
            if (StateProgramming != ProgrammingActionState.ActionSuccessful)
            {
                return data.IsEscalationActionType;
            }

            return false;
        }

        public string GetShortType()
        {
            return Type.ToString().Substring(0, 1);
        }

        public bool Select(bool value)
        {
            IsSelected = value;
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
            if (IsFailureIgnored && executed && state != ProgrammingActionState.ActionSuccessful)
            {
                StateProgramming = ProgrammingActionState.ActionWarning;
            }
            else if (IsFailureIgnored && !executed && (state == ProgrammingActionState.ActionFailed || state == ProgrammingActionState.MissingPrerequisitesForAction))
            {
                StateProgramming = ProgrammingActionState.ActionWarning;
            }
            else
            {
                StateProgramming = state;
            }
        }

        internal void UpdateState(IEnumerable<IPsdzTalLine> talLines)
        {
            ProgrammingActionState? programmingActionState = null;
            foreach (IPsdzTalLine talLine in talLines)
            {
                ProgrammingActionState value = CalculateProgrammingState(talLine);
                if (!programmingActionState.HasValue || value.CompareTo(programmingActionState) > 0)
                {
                    programmingActionState = value;
                }
            }

            if (programmingActionState.HasValue)
            {
                UpdateState(programmingActionState.Value, executed: false);
            }
        }

        internal void Update(IEnumerable<IPsdzTalLine> talLines, int escalationSteps)
        {
            DateTime? dateTime = null;
            DateTime? dateTime2 = null;
            ProgrammingActionState? programmingActionState = null;
            foreach (IPsdzTalLine talLine in talLines)
            {
                ProgrammingActionState value = CalculateProgrammingState(talLine);
                if (!programmingActionState.HasValue || value.CompareTo(programmingActionState) > 0)
                {
                    programmingActionState = value;
                }

                if (dateTime.HasValue)
                {
                    DateTime startTime = talLine.StartTime;
                    DateTime? dateTime3 = dateTime;
                    if (!(startTime < dateTime3))
                    {
                        goto IL_009b;
                    }
                }

                dateTime = talLine.StartTime;
                goto IL_009b;
                IL_009b:
                    if (dateTime2.HasValue)
                    {
                        DateTime startTime = talLine.EndTime;
                        DateTime? dateTime3 = dateTime2;
                        if (!(startTime > dateTime3))
                        {
                            continue;
                        }
                    }

                dateTime2 = talLine.EndTime;
            }

            if (!programmingActionState.HasValue || !dateTime.HasValue || !dateTime2.HasValue)
            {
                return;
            }

            if (escalationSteps == 0)
            {
                UpdateState(programmingActionState.Value, executed: true);
                StartExecution = dateTime.Value;
                EndExecution = dateTime2.Value;
                if (RequiresEscalation())
                {
                    EscalationStep escalationStep = new EscalationStep
                    {
                        StartTime = StartExecution,
                        EndTime = EndExecution,
                        Step = 1,
                        State = StateProgramming
                    };
                    escalationStep.AddErrorList(talLines);
                    EscalationSteps.Add(escalationStep);
                }
            }
            else
            {
                EscalationStep escalationStep2 = new EscalationStep
                {
                    StartTime = dateTime.Value,
                    EndTime = dateTime2.Value,
                    Step = escalationSteps + 1,
                    State = programmingActionState.Value
                };
                StateProgramming = escalationStep2.State;
                if (escalationStep2.EndTime > EndExecution)
                {
                    EndExecution = escalationStep2.EndTime;
                }

                escalationStep2.AddErrorList(talLines);
                EscalationSteps.Add(escalationStep2);
            }
        }

        private ProgrammingActionState CalculateProgrammingState(IPsdzTalLine talLine)
        {
            ProgrammingActionState programmingActionState;
            if (talLine.TaCategories != PsdzTaCategories.FscDeploy)
            {
                programmingActionState = MapState(talLine.ExecutionState);
            }
            else
            {
                programmingActionState = ProgrammingActionState.ActionSuccessful;
                foreach (IPsdzTa fscTa in GetFscTas(talLine.FscDeploy))
                {
                    ProgrammingActionState programmingActionState2 = MapState(fscTa.ExecutionState);
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

        private void SetStateDiag()
        {
            switch (data.StateProgramming)
            {
                case ProgrammingActionState.ActionFailed:
                    State = typeDiagObjectState.Canceled;
                    break;
                case ProgrammingActionState.ActionInProcess:
                    State = typeDiagObjectState.Running;
                    break;
                case ProgrammingActionState.ActionSuccessful:
                case ProgrammingActionState.ActionWarning:
                    State = typeDiagObjectState.Performed;
                    break;
                case ProgrammingActionState.ActionPlanned:
                case ProgrammingActionState.MissingPrerequisitesForAction:
                    State = typeDiagObjectState.NotCalled;
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unsupported programming state {0}.", data.StateProgramming));
            }

            if (ActionData != null)
            {
                ActionData.SetState(State);
            }
        }

        private ICollection<IPsdzTa> GetFscTas(PsdzFscDeploy fscDeploy)
        {
            IEnumerable<PsdzFscDeployTa> source = fscDeploy.Tas.OfType<PsdzFscDeployTa>();
            ICollection<IPsdzTa> collection = new List<IPsdzTa>();
            switch (data.Type)
            {
                case ProgrammingActionType.FscActivate:
                    collection.AddRange(source.Where((PsdzFscDeployTa ta) => ta.Action == PsdzSwtActionType.ActivateUpdate || ta.Action == PsdzSwtActionType.ActivateUpgrade));
                    break;
                case ProgrammingActionType.FscDeactivate:
                    collection.AddRange(source.Where((PsdzFscDeployTa ta) => ta.Action == PsdzSwtActionType.Deactivate));
                    break;
                case ProgrammingActionType.FscStore:
                    collection.AddRange(source.Where((PsdzFscDeployTa ta) => ta.Action == PsdzSwtActionType.ActivateStore));
                    break;
                default:
                    Log.Warning("ProgrammingAction.GetFscTas", "Could not get TAs for FscDeploy since programming type {0} not supported", data.Type);
                    break;
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
    }
}