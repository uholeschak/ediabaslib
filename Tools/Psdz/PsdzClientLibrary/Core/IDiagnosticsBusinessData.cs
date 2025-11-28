using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;
using System;
using PsdzClient.Core.Container;

namespace PsdzClient.Core
{
    // ToDo: Check on update
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

        bool IsSp2021Enabled(IVehicle vecInfo);

        bool IsSp2025Enabled(IVehicle vecInfo);

        bool IsNewFaultMemoryEnabled(IVehicle vecInfo);

        string GetMainSeriesSgbd(IIdentVehicle vecInfo);

        string GetMainSeriesSgbdAdditional(IIdentVehicle vecInfo);

        List<int> GetGatewayEcuAdresses(IVehicle vecInfo);

        void ShowIsarPopup(IVehicle vecInfo, IFFMDynamicResolver FFMResolver, IInteractionService services);

        BNType GetBNType(IVehicle vehicle);

        bool IsEPMEnabled(IVehicle vehicle);

        IEcuJob ExecuteFSLesenExpert(IEcuKom ecuKom, string variant, int retries);

        void BN2000HandleKMMFixes(IVehicle vecInfo, IEcuKom ecuKom, bool resetMOSTDone, IProgressMonitor monitor, int retryCount, DoECUIdentDelegate doECUIdentDelegate);

        void HandleECUGroups(IVehicle vecInfo, IEcuKom ecuKom, List<IEcu> ecusToRemoveKMM);

        void SetVehicleLifeStartDate(IVehicle vehicle, IEcuKom ecuKom);

        void MaskResultsFromFSLesenExpertForFSLesenDetail(IEcuJob ecuJob);

        bool CheckForSpecificModelPopUpForElectricalChecks(string ereihe);

        void ReadILevelBn2020(IVehicle vecInfo, IEcuKom ecuKom, int retryCount);

        bool ProcessILevelJobResults(Reactor reactor, IVehicle vecInfo, IEcuJob iJob);

        bool ProcessILevelJobResultsEES25(Reactor reactor, IVehicle vecInfo, IEcuJob iJob);

        bool IsSp2021Gateway(IVehicle vecInfo, IEcuKom ecuKom, int retryCount);

        IEcuJob ClampShutdownManagement(IVehicle vecInfo, IEcuKom ecuKom, int retryCount = 2, int i_geschw_schwelle = 30);

        string ReadVinForGroupCars(BNType bNType, IEcuKom ecuKom);

        string ReadVinForGroupCarsNcar(BNType bNType, IEcuKom ecuKom);

        string ReadVinForMotorcycles(BNType bNType, IEcuKom ecuKom);

        decimal? ReadGwszForGroupCars(IVehicle vecInfo, IEcuKom ecuKom);

        decimal? ReadGwszForGroupMotorbike(IVehicle vehicle, IEcuKom ecuKom, int retryCount, Action<string> protocolUnit, Action<IVehicle, string, string> logIfEcuMissing);

        string GetFourCharEreihe(string ereihe);

        //string GetServiceProgramName(TestModuleName testmodulename);

        //IEcuJob SendJobToKombiOrMmi(IVehicle vecInfo, IEcuKom ecuKom, string job, string param, string resultFilter, int retries);

        //IEcuJob SendStatusLesenCcmJobToKombiOrMmi(IVehicle vecInfo, IEcuKom ecuKom);

        //List<string> RemoveFirstDigitOfSalapaIfLengthIs4(List<string> salapa);

        //void Add14DigitFakeSerialNumberToFstdat(IVehicle vecInfo, IEnumerable<IEcuJob> jobList);

        //void CheckEcusFor14DigitSerialNumber(IEcuKom ecuKom, IEnumerable<IEcu> ecus);

        string[] GetMaxEcuList(BrandName brand, string ereihe);

        //bool ShouldNotValidateFAForOldCars(string ereihe, DateTime constructionDate);

        bool IsEES25Vehicle(IVehicle vecInfo);

        bool IsPreE65Vehicle(string ereihe);

        bool IsPreDS2Vehicle(string ereihe, DateTime? c_DateTime);

        bool? HasMSAButton(FA fa, DateTime? c_DateTime, string productLine);
    }
}
