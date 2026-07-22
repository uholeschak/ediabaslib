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

        void AddAddedEcu(IEcu ecu);

        void AddFeedback(string message, string user, string eMail, IDictionary<string, string> informationObjectConcernToMessage);

        void AddNewFeedback(string message, string user, string eMail, KeyValuePair<string, string> errorLocation, string typeOfMessage);

        void ProtocolDialog(DateTime start, string dialog, IList<LocalizedText> titleTextList, IList<LocalizedText> messageTextList, string[] buttonId, string buttonPressed, LayoutGroup layoutGroup, IList<LocalizedText> answerList = null, bool display = true);

        void ResetCyclicJournalize();

        void WriteLoopEntriesToLog(bool verboseLoop);

        void CreateAndAddEcuCommunications(IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering, LayoutGroup layoutGroup);

        void AddDocument(DateTime start, DateTime end, IList<InfoObject> documentInfos, IList<string> lang, LayoutGroup layoutGroup, bool addContent = false);

        void AddUiObjectDisplay(string objectType, string id, string selectedElement, string selectedElementType, LayoutGroup layoutGroup);

        void AddException(string description, string errorCode, string errorLocation, string snapshot, LayoutGroup layoutGroup);


        void AddButtonPressedEvent(IList<LocalizedText> buttonText, string maskName, string maskIdent, LayoutGroup layoutGroup);

        void AddMaskChangedEvent(string maskName, int maskId);

        void AddMaskChangedEvent(string maskName, string maskId);

        void AddPowerSupplyEvent(string actualPowerSupply);

        bool AddServiceCode(string name, string value, LayoutGroup layoutGroup, bool allowMultipleEntries = false, bool bufferIfSessionNotStarted = false, DateTime? timeStamp = null, bool? isSystemTime = null);

        void AddLogStatement(string headlineValue, Dictionary<string, string> logStatementEntries, DateTime startTime);

        void AddInfoLogStatementWhithTitle(bool removePreviousInfoLog, string infoTitle, Dictionary<string, string> logInfoEntries);
    }
}
