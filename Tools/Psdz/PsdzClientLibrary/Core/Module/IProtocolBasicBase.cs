using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public interface IProtocolBasicBase : IFastaGroupingBase
    {
        bool HasFeedback { get; set; }

        //IAction<IObjectCalculation> CurrentTestPlan { get; }

        void AddAddedEcu(IEcu ecu);

        //bool AddMeasuringAction(DateTime start, DateTime end, ActionResult result, string measuredVariable, string actualValue, string actualValueUnit, string desiredValue, string desiredValueUnit, string adaption, string signalName, string deviceIdentification, IList<string> lang);

        void AddFeedback(string message, string user, string eMail, IDictionary<string, string> informationObjectConcernToMessage);

        void AddNewFeedback(string message, string user, string eMail, KeyValuePair<string, string> errorLocation, string typeOfMessage);

        //IAction<IUiDialog> CreateAndAddUiDialogFromServiceProgram(string type, string methodName);

        //IAction<IUiDialog> CreateAndAddUiDialog(string type, LayoutGroup layoutGroup);

        void ProtocolDialog(DateTime start, string dialog, IList<LocalizedText> titleTextList, IList<LocalizedText> messageTextList, string[] buttonId, string buttonPressed, LayoutGroup layoutGroup, IList<LocalizedText> answerList = null, bool display = true);

        //IAction<IUiDialog> CreateUiDialog(string type, LayoutGroup layoutGroup, string methodName = null);

        //IAction<IEcuCommunication> CreateAndAddEcuCommunication(string ecuName, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        //IAction<IEcuCommunication> CreateAndAddEcuCommunication(IEcu ecu, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        //IAction<IEcuCommunication> CreateEcuCommunication(string ecuName, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        //IAction<IEcuCommunication> CreateEcuCommunication(IEcu ecu, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        //void ProtocolEcuTest(DateTime startTime, IEnumerable<JobResultData> formatedJobResults, IEcu ecu, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        void ResetCyclicJournalize();

        //void AddIfIsNotInLoopOrDoLoopHandling(IAction<object> action, bool verboseLoopLog, bool doLoopHandling);

        void WriteLoopEntriesToLog(bool verboseLoop);

        void CreateAndAddEcuCommunications(IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        void AddDocument(DateTime start, DateTime end, IList<InfoObject> documentInfos, IList<string> lang, LayoutGroup layoutGroup, bool addContent = false);

        //IAction<IUiDiagnosisCode> CreateAndAddUiDiagnosis(LayoutGroup layoutGroup);

        void AddUiObjectDisplay(string objectType, string id, string selectedElement, string selectedElementType, LayoutGroup layoutGroup);

        void AddException(string description, string errorCode, string errorLocation, string snapshot, LayoutGroup layoutGroup);

        //void AddConnectionEvent(DeviceType deviceType, string deviceIdent, string statusChange, string newStatus, LayoutGroup layoutGroup);

        void AddButtonPressedEvent(IList<LocalizedText> buttonText, string maskName, string maskIdent, LayoutGroup layoutGroup);

        void AddMaskChangedEvent(string maskName, int maskId);

        void AddMaskChangedEvent(string maskName, string maskId);

        //void AddBatteryAlertEvent(string upperThreshold, string actualLevel, string lowerThreshold, BatteryAlertSeverity? severity);

        void AddPowerSupplyEvent(string actualPowerSupply);

        bool AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false, bool bufferIfSessionNotStarted = false, DateTime? timeStamp = null, bool? isSystemTime = null);

        //void AddVehicleTest(DateTime startTime, DateTime endTime, KindOfVehicleTestType type, IVehicle vehicle, bool filterRelevantOnly, LayoutGroup layoutGroup);

        //IAction<IObjectCalculation> CreateAndAddObjectCalculation(ObjectCalculationObjectType objectType, LayoutGroup layoutGroup);

        //IAction<BMW.Rheingold.CoreFramework.Contracts.FASTA.ITherapyPlan> CreateAndAddTherapyPlanExecution(DateTime startTimeTP, DateTime endTimeTP, string reason, ITherapyPlanReport report);

        //void FillTherapyPlanExecution(string therapyPlanId, StateTypeOfTherapyPlan? stateType, TypeOfTherapyPlan? planType, string basedOnTherapyPlanId, ActionResult result, IEnumerable<ITherapyPlanEntry> therapyPlanEntries, ITherapyPlanReport report, IList<string> lang, IList<SecureFeatureData> sfaTarget = null, IList<SecureFeatureData> sfaCurrent = null, IEnumerable<EnablingCodeData> enablingCodes = null);

        //void CreateAndAddVoChange(DateTime startTime, ITherapyPlanReport therapyPlanReport, string faCurrent, string faTarget, LayoutGroup layoutGroup);

        IMethodCall AddMethodCall(string name, IDictionary<string, string> parameter = null, DateTime? startTime = null);

        //void AddSwtDeactivation(IEnumerable<SwtDeactivation> swtDeactivationData, LayoutGroup layoutGroup);

        void AddECUValidationAction(object ecuValidationAction);

        //void JournalizeTestplan(DateTime start, DateTime end, TestPlanType testplan);

        void AddLogStatement(string headlineValue, Dictionary<string, string> logStatementEntries, DateTime startTime);

        void AddInfoLogStatementWhithTitle(bool removePreviousInfoLog, string infoTitle, Dictionary<string, string> logInfoEntries);
    }
}
