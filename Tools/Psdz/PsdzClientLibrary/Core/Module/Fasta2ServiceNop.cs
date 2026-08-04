using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.FASTA
{
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public class Fasta2ServiceNop : IFasta2Service, IProtocolBasic, IProtocolBasicBase, IFastaGroupingBase, IFastaGrouping
    {
        //public IAction<IObjectCalculation> CurrentTestPlan { get; set; }

        public object CyclicalJournalizingDifferentiator => GetType();

        public DateTime EndTime { get; set; }

        public bool HasFeedback { get; set; }

        public string Identifier { get; set; }

        public bool IsValid { get; set; }

        public IList<string> Lang { get; private set; }

        public bool Pannenfall { get; set; }

        //public FbmPingData PingData { get; set; }

        public IProtocolBasic ProtocolingInstance => this;

        public DateTime StartTime { get; set; }

        public string Title { get; set; }

        //internal ActionNop<IObjectCalculation> CurrentTherapyPlanCalculation { get; private set; }

        //internal ActionNop<BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan> CurrentTherapyPlanExecution { get; private set; }

        public string MacAddressForRequests
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Fasta2ServiceNop()
        {
            Lang = new string[1] { "de-DE" };
        }

        public void AddAddedEcu(IEcu ecu)
        {
            Log.Debug("Fasta2ServiceNop.AddAddedEcu()", "Not implemented.");
        }

        public void AddButtonPressedEvent(IList<LocalizedText> buttonText, string maskName, string maskIdent, LayoutGroup layoutGroup)
        {
            Log.Debug("Fasta2ServiceNop.AddButtonPressedEvent()", "Not implemented.");
        }

        public void AddDocument(DateTime start, DateTime end, IList<InfoObject> documentInfos, IList<string> lang, LayoutGroup layoutGroup, bool addContent = false)
        {
            Log.Debug("Fasta2ServiceNop.AddDocument()", "Not operation executed.");
        }

        public void AddECUValidationAction(object ecuValidationAction)
        {
            Log.Debug(string.Format("{0}.{1}()", GetType().Name, "AddECUValidationAction"), "Not implemented.");
        }

        public void AddException(string description, string errorCode, string errorLocation, string snapshot, LayoutGroup layoutGroup)
        {
            Log.Debug("Fasta2ServiceNop.AddException()", "Not operation executed.");
        }

        public void AddFeedback(string message, string user, string eMail, IDictionary<string, string> informationObjectConcernToMessage)
        {
            Log.Debug("Fasta2ServiceNop.AddFeedback()", "Not operation executed.");
        }

        public void AddNewFeedback(string message, string user, string eMail, KeyValuePair<string, string> errorLocation, string typeOfMessage)
        {
            Log.Debug("Fasta2ServiceNop.AddFeedback()", "Not operation executed");
        }

        public void AddLogStatement(string headlineValue, Dictionary<string, string> logStatementEntries, DateTime startTime)
        {
            Log.Debug("Fasta2ServiceNop.AddLogStatement()", "Not operation executed.");
        }

        public void AddInfoLogStatementWhithTitle(bool removePreviousInfoLog, string infoTitle, Dictionary<string, string> logInfoEntries)
        {
            Log.Debug("Fasta2ServiceNop.AddInfoLogStatementWhithTitle()", "Not operation executed.");
        }

        public void AddMaskChangedEvent(string maskName, int maskId)
        {
            Log.Debug(Log.CurrentMethod(), "Not implemented");
        }

        public void AddMaskChangedEvent(string maskName, string maskId)
        {
            Log.Debug(Log.CurrentMethod(), "Not implemented");
        }

        public IMethodCall AddMethodCall(string name, IDictionary<string, string> parameter = null, DateTime? startTime = null)
        {
            throw new NotImplementedException();
        }

        public void AddPowerSupplyEvent(string actualPowerSupply)
        {
            Log.Debug("Fasta2ServiceNop.AddPowerSupplyEvent()", "Not implemented.");
        }

        public bool AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false, bool bufferIfSessionNotStarted = false, DateTime? timeStamp = null, bool? isSystemTime = null)
        {
            Log.Debug("Fasta2ServiceNop.AddServiceCode()", "Not operation executed.");
            return false;
        }

        public string AddTestPlanFaultPattern(string actionId, string structureNodeName, string code, string vfcLocationType, string vfcLocationCode, string layoutGroup)
        {
            Log.Debug("Fasta2ServiceNop.AddTestPlanFaultPattern()", "Not operation executed.");
            return string.Empty;
        }

        public void AddUiObjectDisplay(string objectType, string id, string selectedElement, string selectedElementType, LayoutGroup layoutGroup)
        {
            Log.Debug("Fasta2ServiceNop.AddUiObjectDisplay()", "Not operation executed.");
        }

        public void Connect(IVciDevice device)
        {
            Log.Debug("Fasta2ServiceNop.Connect()", "Not operation executed.");
        }

        public void CreateAndAddEcuCommunications(IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup)
        {
            Log.Debug("Fasta2ServiceNop.CreateAndAddEcuCommunications()", "Not implemented.");
        }


        public IFastaGrouping CreateSubGroup(BMW.Rheingold.CoreFramework.Contracts.FASTA.GroupingType groupingType, IList<LocalizedText> subgroupTitleList)
        {
            throw new NotImplementedException();
        }

        public int CyclicJournalizeHashCode()
        {
            return GetHashCode();
        }

        public void Disconnect(IVciDevice device, string reasonDisconnect)
        {
            Log.Debug("Fasta2ServiceNop.Disconnect()", "Not operation executed.");
        }

        public string EndSession(Vehicle vehicle, string istaCaseId)
        {
            Log.Debug("Fasta2ServiceNop.EndSession()", "Not operation executed.");
            return string.Empty;
        }

        public void HandOverData()
        {
            Log.Debug("Fasta2ServiceNop.HandOverData()", "Not operation executed.");
        }

        public void InitializeOnlinePatchIfNeeded()
        {
            Log.Debug("Fasta2ServiceNop.InitializeOnlinePatchIfNeeded()", "Not operation executed.");
        }

        public void ProtocolDialog(DateTime start, string dialog, IList<LocalizedText> titleTextList, IList<LocalizedText> messageTextList, string[] buttonId, string buttonPressed, LayoutGroup layoutGroup, IList<LocalizedText> answerList = null, bool display = true)
        {
            Log.Debug("Fasta2ServiceNop.ProtocolDialog()", "Not operation executed.");
        }

        public void ProtocolNetworkchange()
        {
            Log.Debug("Fasta2ServiceNop.ProtocolNetworkchange()", "Not implemented.");
        }

        public void ProtocolOnlinePatch(string patchType, string patchVersion, List<OnlinePatchDownloadStatus> onlinePatchFiles, int countServiceType)
        {
            throw new NotImplementedException();
        }

        public void ProtocolRsuStartFailed(string identifier, string title, string language)
        {
            Log.Debug("Fasta2ServiceNop.ProtocolRsuStartFailed()", "Not operation executed.");
        }

        public void ResetCyclicJournalize()
        {
            Log.Debug("Fasta2ServiceNop.ResetCyclicCounter()", "Not implemented.");
        }

        public string SaveBehdat(string fileName, bool sendFastaDataForbidden)
        {
            Log.Debug("Fasta2ServiceNop.SaveBehdat()", "Not implemented.");
            return null;
        }

        public void SetStatus(IList<LocalizedText> statusNameList, IList<LocalizedText> statusValueList)
        {
            Log.Debug("Fasta2ServiceNop.SetStatus()", "Not implemented.");
        }

        public void SetTitle(IList<LocalizedText> messageTextList)
        {
            Log.Debug("Fasta2ServiceNop.SetTitle()", "Not implemented.");
        }

        public void UpdateSessionHeader(Vehicle vecInfo)
        {
            Log.Debug("Fasta2ServiceNop.UpdateSessionHeader()", "Not implemented.");
        }

        public void UpdateSessionHeader(string istaCaseId)
        {
            Log.Debug("Fasta2ServiceNop.UpdateSessionHeader()", "Not implemented.");
        }

        public void WriteLoopEntriesToLog(bool verboseLoop)
        {
            Log.Debug("Fasta2ServiceNop.WriteLoopEntriesToLog()", "Not implemented.");
        }
    }
}
