using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;

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

        [PreserveSource(Cleaned = true)]
        IPsdzStandardSvt GetVehicleSvtUsingPsdz();

        [PreserveSource(Cleaned = true)]
        bool ExecuteEarlyEcuValidationUsingPsdz();

        [PreserveSource(Cleaned = true)]
        bool ImportSecureTokenForSec4CnSp21();

        [PreserveSource(Cleaned = true)]
        ISvt GetCurrentSvtFromPsdzSvt();
    }
}