using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Contracts;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using PsdzClient;
using static PsdzClient.PsdzDatabase;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public interface ITherapyPlan : IDisposable
    {
        //ITherapyPlanData Data { get; }

        string CurrentContextId { get; set; }

        string TargetContextId { get; set; }

        bool IsFirstCalculation { get; set; }

        bool IsZgwRepairExecuted { get; set; }

        bool IsNative { get; }

        //IList<ITherapyPlanEntry> Entries { get; }

        //ITherapyPlanReport Report { get; }

        //TypeOfTherapyPlan? Type { get; }

        IList<string> FaElementAdded { get; }

        IList<string> FaElementRemoved { get; }

        IEnumerable<ISwiAction> SwiActionsRegister { get; }

        IList<ISwiAction> SwiActionsRegisterCommon { get; }

        bool IsMarkedForReset { get; set; }

        bool IsCodingConversionPlanned { get; set; }

        bool IsVehicleOrderImportWithoutProgramming { get; }

        void Reset(bool native, bool deselectSwiActions);

        bool ContainsHwTransactions(ProgrammingActionType? type, List<IEcuProgrammingInfo> listEcuProgrammingInfos);

        bool ContainsHwOrSoftwareAction();

        bool ContainsNoVehicleCodingConversion();

        bool ContainsMountTransactions();

        void AddSuspiciousInfoObject(InfoObject infoObject);

        void CodeAll(bool select);

        //void MoveServiceFunctionToReport(ITherapyPlanEntry serviceFunctionEntry);

        void AddSwiAction(ISwiAction swiAction);

        IBoolResultObject AddSwiActions(List<ISwiAction> swiActions, bool planned);

        void RemoveSwiAction(ISwiAction swiAction);

        //IList<IServiceProgramProgramming> GetSortedServiceProgramListTherapyPlan(SwiActionLinkType linkType, IComparer<IServiceProgramProgramming> cmp);

        //void AddHddAction(ISwiAction swiAction, ICollection<HddCard> hddcards, string hddServerDownloadUrl, int huDiagAddr, string huVariante);

        void AddProgrammingAction(IEcuProgrammingInfo ecu, ProgrammingActionType type);

        void AddProgrammingAction(IEcuProgrammingInfo ecu, ProgrammingActionType type, TherapyPlanItemOrigin origin);

        void RemoveProgrammingAction(IEcuProgrammingInfo ecu, ProgrammingActionType type);

        bool IsFirstCodingConversionSelection(List<ISwiAction> swiAction);

        //void ChangeProgrammingStateTo(ProgrammingExecutionState state);

        bool AnySoftwareBesidesSFAActionFailed();

        bool NoActionSucceededOrExecuted();

        void SetProgrammingActionStateForTherapyPlanEntries(string tpEntry, ProgrammingActionState state);
    }
}
