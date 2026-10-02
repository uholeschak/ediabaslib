using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.ISPI.IstaServices.Contract.PUK.Data;
using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    public interface IProgrammingSessionExt : IProgrammingSession, INotifyPropertyChanged, IDisposable
    {
        IBackendCallsWatchDog BackendCallsWatchDogProgramming { get; set; }

        IProgrammingSessionData Data { get; }

        IEcuProgrammingInfos EcuProgrammingInfos { get; }

        DateTime ExecutionStartTime { get; }

        IAction<IObjectCalculation> FastaCurrentContext { get; set; }

        IFFMDynamicResolver FFMResolver { get; }

        string GetPathToPsdzBackupData { get; }

        bool IsActive { get; }

        bool IsProgressIndeterminate { get; }

        bool IsSuspended { get; }

        bool IsTherapyPlanExecuting { get; }

        bool IsTalExecuting { get; }

        bool UserHasCalculatedTheMpManually { get; set; }

        DateTime LastActive { get; }

        string ProgressLabel { get; }

        bool IcomIsRebooting { get; set; }

        int IcomRestartCounter { get; set; }

        double ProgressValue { get; }

        ISwt SwtAction { get; }

        IEnumerable<IProgrammingTask> Tasks { get; }

        ITherapyPlan TherapyPlan { get; }

        TimeSpan TimeLeft { get; }

        DateTime? VehicleTestStarted { get; set; }

        void Abort();
        bool CanContinueOperation();
        void CheckNote(IVehicle vehicle);
        WorkStateValues CheckOpenActions();
        void ContinueCalculatingTherapyPlan();
        void DeleteIndividualDataFromPuk();
        void DownloadIndividualDataFromPuk();
        LayoutGroup FindLayoutGroupVehicleTest();
        bool IsTargetILevelSetToBackIlevel(IProtocolBasic fastaService);
        void NotifyFinishedMessage();
        void RefreshCanExecute();
        void Reset();
        void Resume();
        void SelectCoding(long ecu);
        void SelectCodingAll(bool enable);
        void SelectEcuPostExchange(long ecu);
        void SelectEcuPreExchange(long ecu);
        void SelectProgramming(long ecu);
        void SelectSpecialAction(SwiActionType swiActionType);
        void SelectVehicleConversion(decimal swiActionId);
        string SerializeData();
        void SetFaTarget(IFa fa, IProtocolBasic fastaService);
        bool SetTargetToBackupILevel(IProtocolBasic fastaService);
        bool SetTargetToDefinedILevel(IProtocolBasic fastaService, string targetILevel);
        IVehicleUpdate SpecialPlanRequired(ISwiAction swiAction);
        void StartUsing(bool refresh = false);
        void Stop();
        void TryClosePSdZConnection();
        void UpdateAndProtocolTherapyPlanReport();
        void HandleSecureFeatureData();
    }
}