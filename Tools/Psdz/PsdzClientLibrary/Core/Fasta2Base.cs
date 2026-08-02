using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Contracts;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

#pragma warning disable CS0649
namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Class cleaned", SuppressWarning = true)]
    public abstract class Fasta2Base : IDisposable
    {
        private ILogic logic;

        internal int maxAmountOfCyclicEntries;

        // internal Dictionary<object, IAction<object>> firstEntries;

        // internal Dictionary<object, IAction<object>> lastEntries;

        // internal Dictionary<ICyclicJournalizeGroupable, IAction<object>> smallestEntries;

        // internal Dictionary<ICyclicJournalizeGroupable, IAction<object>> greatestEntries;

        internal Dictionary<object, int> currentAmountOfCyclicEntries;

        // private Dictionary<ICyclicJournalizeGroupable, IList<FastaSubGroup>> cyclicProtocoledGroups;

        // private TherapyPlanTypeTAList taList;

        private Dictionary<string, List<string>> cachedServiceCodes = new Dictionary<string, List<string>>();

        public ICollection<object> protocolTypeItems;

        public bool HasFeedback { get; set; }

        protected object SycronisationVariable { get; set; }

        // protected IList<IFastaSubGroup> FastaGroups { get; set; }

        // internal IList<IAction<object>> CurrentActions { get; set; }

        protected bool ProtocolAdditionalInformation { get; set; }

        // private protected Queue<ActionType> BufferedServiceCodes { get; } = new Queue<ActionType>();

        internal ICollection<object> ProtocolTypeItems
        {
            get
            {
                return protocolTypeItems;
            }
            private protected set
            {
                protocolTypeItems = value;
                if (protocolTypeItems != null)
                {
                    //[-] while (BufferedServiceCodes.Count > 0)
                    //[-] {
                    //[-] ProtocolTypeItems.Add(BufferedServiceCodes.Dequeue());
                    //[-] }
                }
            }
        }

        // public IAction<IObjectCalculation> CurrentTestPlan { get; set; }

        // private IAction<BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan> CurrentTherapyPlanExecution { get; set; }

        // protected HeaderType Header { get; set; }

        public ILogic Logic
        {
            get
            {
                return logic;
            }
            set
            {
                logic = value;
            }
        }

        // public ActionResult Result { get; set; }

        protected Fasta2Base()
        {
            Initialize();
        }

        private void Initialize()
        {
            //[-] CurrentActions = new List<IAction<object>>();
            SycronisationVariable = new object();
            //[-] FastaGroups = new List<IFastaSubGroup>();
            maxAmountOfCyclicEntries = ConfigSettings.getConfigint("BMW.Rheingold.ISTAGUI.CommandECUFunctionsReadStatus.MaximalFASTAJobs", 100);
            //[-] firstEntries = new Dictionary<object, IAction<object>>(new CyclicJournalizeGroupableCommonObjEqualityComparer());
            //[-] lastEntries = new Dictionary<object, IAction<object>>(new CyclicJournalizeGroupableCommonObjEqualityComparer());
            //[-] smallestEntries = new Dictionary<ICyclicJournalizeGroupable, IAction<object>>(new EqualityComparerCyclicJournalizeGroupable());
            //[-] greatestEntries = new Dictionary<ICyclicJournalizeGroupable, IAction<object>>(new EqualityComparerCyclicJournalizeGroupable());
            //[-] currentAmountOfCyclicEntries = new Dictionary<object, int>(new CyclicJournalizeGroupableCommonObjEqualityComparer());
            //[-] cyclicProtocoledGroups = new Dictionary<ICyclicJournalizeGroupable, IList<FastaSubGroup>>(10, new EqualityComparerCyclicJournalizeGroupable());
        }

        public void AddAddedEcu(IEcu ecu)
        {
        }

        public void AddFeedback(string message, string user, string eMail, IDictionary<string, string> informationObjectConcernToMessage)
        {
        }

        public void AddNewFeedback(string message, string user, string eMail, KeyValuePair<string, string> errorLocation, string typeOfMessage)
        {
        }

        protected abstract void CheckProtocolTime();


        public void WriteLoopEntriesToLog(bool verboseLoop)
        {
        }

        private void FillFirstMinMaxLastEntriesInAnLoopToProtocoll()
        {
        }

        public void CreateAndAddEcuCommunications(IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup)
        {
        }

        public void AddDocument(DateTime start, DateTime end, IList<InfoObject> documentInfos, IList<string> lang, LayoutGroup layoutGroup, bool addContent = false)
        {
        }

        public void AddUiObjectDisplay(string objectType, string id, string selectedElement, string selectedElementType, LayoutGroup layoutGroup)
        {
        }

        public virtual void HandOverData()
        {
        }

        public void AddException(string description, string errorCode, string errorLocation, string snapshot, LayoutGroup layouGroup)
        {
        }

        public void AddButtonPressedEvent(IList<LocalizedText> buttonText, string maskName, string maskIdent, LayoutGroup layouGroup)
        {
        }

        public void AddMaskChangedEvent(string maskName, int maskId)
        {
        }

        public void AddMaskChangedEvent(string maskName, string maskId)
        {
        }

        public void AddPowerSupplyEvent(string actualPowerSupply)
        {
        }

        public bool AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false, bool bufferIfSessionNotStarted = false, DateTime? timeStamp = null, bool? isSystemTime = null)
        {
            return true;
        }

        private string CleanupServiceCodeValues(string value)
        {
            foreach (string item in new List<string> { "<(\\S+)>" })
            {
                value = Regex.Replace(value, item, "|$1|");
            }
            return value;
        }

        public void ProtocolDialog(DateTime start, string dialog, IList<LocalizedText> titleTextList, IList<LocalizedText> messageTextList, string[] buttonId, string buttonPressed, LayoutGroup layoutGroup, IList<LocalizedText> answerList = null, bool display = true)
        {
            FillCommonFastaAction(start, dialog, titleTextList, messageTextList, buttonId, buttonPressed, answerList, display, layoutGroup);
        }

        private void FillCommonFastaAction(DateTime start, string dialog, IList<LocalizedText> titleTextList, IList<LocalizedText> messageTextList, string[] buttonId, string buttonPressed, IList<LocalizedText> answerList, bool display, LayoutGroup layoutGroup)
        {
        }

        public IBoolResultObject AddTechnicalActionResultToProtocoll(string taNummer, string taBezeichnung, IList<string> mindestIStufens, bool abArbeitungsstatus, string diagnosisCodeTitle, string diagnoseCodes)
        {
            return new BoolResultObject();
        }

        public IMethodCall AddMethodCall(string name, IDictionary<string, string> parameter, DateTime? startTime = null)
        {
            return null;
        }

        public void AddECUValidationAction(object pEcuValidationAction)
        {
        }

        public void ResetCyclicJournalize()
        {
            currentAmountOfCyclicEntries.Clear();
        }

        public void AddLogStatement(string headlineValue, Dictionary<string, string> logStatementEntries, DateTime startTime)
        {
        }

        public void AddInfoLogStatementWhithTitle(bool removePreviousInfoLog, string infoTitle, Dictionary<string, string> logInfoEntries)
        {
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
    }
}
