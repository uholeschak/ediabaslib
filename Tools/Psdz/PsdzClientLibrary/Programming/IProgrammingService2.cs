using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;

namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "IProgrammingService renamed", InheritanceModified = true)]
    public interface IProgrammingService2
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

        [PreserveSource(Hint = "IProgrammingCallbackHandler", Placeholder = true)]
        PlaceholderType CreateCallbackHandler();

        string GetPsdzWebServiceLogFilePath();

        string GetPsdzLogFilePath();

        [PreserveSource(Hint = "Changed to Boolean")]
        bool StartPsdzService(IVehicle vehicle);

        [PreserveSource(Hint = "Removed")]
        IPsdzStandardSvt GetVehicleSvtUsingPsdz();

        [PreserveSource(Hint = "Removed")]
        bool ExecuteEarlyEcuValidationUsingPsdz();

        [PreserveSource(Hint = "Removed")]
        ISvt GetCurrentSvtFromPsdzSvt();
    }
}