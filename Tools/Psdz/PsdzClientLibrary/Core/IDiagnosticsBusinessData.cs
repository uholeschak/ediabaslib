using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;
using System;
using PsdzClient.Core.Container;
using PsdzClientLibrary.Core;

namespace PsdzClient.Core
{
    public interface IDiagnosticsBusinessData
    {
        List<string> ProductLinesEpmBlacklist { get; }

        DateTime DTimeF01Lci { get; }

        DateTime DTimeRR_S2 { get; }

        DateTime DTimeF25Lci { get; }

        DateTime DTimeF01BN2020MostDomain { get; }

        DateTime DTime2022_07 { get; }

        DateTime DTime2023_03 { get; }

        DateTime DTime2023_07 { get; }

        void SetSp2021Enabled(IVehicle vecInfo);

        string GetMainSeriesSgbd(IVehicle vecInfo);

        void UpdateCharacteristics(IVehicle vecInfo, string typsnr, IEcuKom ecuKom, bool isVorSerie, IProgressMonitor monitor, int retryCount, Func<BNType, int, IProgressMonitor, IEcuJob> doECUReadFA);

        string GetMainSeriesSgbdAdditional(IVehicle vecInfo);

        void SpecialTreatmentBasedOnEreihe(string typsnr, IVehicle vecInfo);

        List<int> GetGatewayEcuAdresses(IVehicle vecInfo);

        BNType GetBNType(IVehicle vehicle);

        bool IsEPMEnabled(IVehicle vehicle);

        //IEcuJob ExecuteFSLesenExpert(IEcuKom ecuKom, string variant, int retries);

        void BN2000HandleKMMFixes(IVehicle vecInfo, IEcuKom ecuKom, bool resetMOSTDone, IProgressMonitor monitor, int retryCount, DoECUIdentDelegate doECUIdentDelegate);

        void HandleECUGroups(IVehicle vecInfo, IEcuKom ecuKom, List<IEcu> ecusToRemoveKMM);

        void AddServiceCode(string methodName, int identifier);

        void SetVehicleLifeStartDate(IVehicle vehicle, IEcuKom ecuKom);
#if false
        void MaskResultsFromFSLesenExpertForFSLesenDetail(IEcuJob ecuJob);

        bool CheckForSpecificModelPopUpForElectricalChecks(string ereihe);
#endif
        void ReadILevelBn2020(IVehicle vecInfo, IEcuKom ecuKom, int retryCount);

        bool ProcessILevelJobResults(Reactor reactor, IVehicle vecInfo, IEcuJob iJob);

        bool IsSp2021Gateway(IVehicle vecInfo, IEcuKom ecuKom, int retryCount);
#if false
        IEcuJob ClampShutdownManagement(IVehicle vecInfo, IEcuKom ecuKom, int retryCount = 2, int i_geschw_schwelle = 30);
#endif
        string ReadVinForGroupCars(BNType bNType, IEcuKom ecuKom);

        string ReadVinForMotorcycles(BNType bNType, IEcuKom ecuKom);

        string GetFourCharEreihe(string ereihe);

        //void ShowAdapterHintMotorCycle(IProgressMonitor monitor, IOperationServices services, string eReihe, string basicType);
    }
}
