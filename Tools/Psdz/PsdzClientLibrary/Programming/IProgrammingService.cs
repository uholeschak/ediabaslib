using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
    public interface IProgrammingService
    {
        IPsdz Psdz { get; }

        [PreserveSource(Hint = "IEnumerable<IProgrammingTask>", Placeholder = true)]
        PlaceholderType RetrieveAvailableProgrammingTasks(IVehicle vehicle);

        [PreserveSource(Hint = "IProgrammingSessionExt", Placeholder = true)]
        PlaceholderType Start(PlaceholderType programmingParam);

        [PreserveSource(Hint = "IProgrammingSessionExt", Placeholder = true)]
        PlaceholderType Start(PlaceholderType programmingParam, bool avoidTlsConnection);

        void SetLogLevelToMax();

        void SetLogLevelToNormal();

        bool CollectPsdzLog(string targetLogFilePath);

        [PreserveSource(Hint = "FcFnActivationResult", Placeholder = true)]
        PlaceholderType StoreAndActivateFcFn(IVehicle vehicle, int appNo, int upgradeIdx, byte[] fsc);

        [PreserveSource(Hint = "force added")]
        void CloseConnectionsToPsdz(bool force);

        //IProgrammingCallbackHandler CreateCallbackHandler(ILogic logic, IProgressMonitor progressMonitor);

        string GetPsdzWebServiceLogFilePath();

        string GetPsdzLogFilePath();

        [PreserveSource(Hint = "Changed to Boolean")]
        bool StartPsdzService(IVehicle vehicle);

        //IPsdzStandardSvt GetVehicleSvtUsingPsdz(ILogic logic);

        //bool ExecuteEarlyEcuValidationUsingPsdz(ILogic logic, IDatabaseProvider database, string mainseries, bool avoidTlsConnection);

        //ISvt GetCurrentSvtFromPsdzSvt(IPsdzStandardSvt psdzStandardSvt, IDatabaseProvider database, Vehicle vehicle, IFFMDynamicResolver fFMDynamicResolver);
    }
}