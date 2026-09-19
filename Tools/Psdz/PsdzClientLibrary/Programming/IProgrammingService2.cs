using BMW.Rheingold.CoreFramework.Contracts.Programming;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework;
using PsdzClient.Core.Container;

namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "IProgrammingService renamed", InheritanceModified = true)]
    public interface IProgrammingService2
    {
        IPsdz Psdz { get; }

        IEnumerable<IProgrammingTask> RetrieveAvailableProgrammingTasks(IVehicle vehicle);
        [PreserveSource(Hint = "ProgrammingParam", Placeholder = true)]
        IProgrammingSessionExt Start(PlaceholderType programmingParam);
        [PreserveSource(Hint = "ProgrammingParam", Placeholder = true)]
        IProgrammingSessionExt Start(PlaceholderType programmingParam, bool avoidTlsConnection);
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
        [PreserveSource(Hint = "Arguments removed", Cleaned = true)]
        bool ExecuteIPBEcuValidation();
        [PreserveSource(Cleaned = true)]
        bool ImportSecureTokenForSec4CnSp21();
        [PreserveSource(Hint = "database changed")]
        bool ImportSecureTokenForSec4CnSp18(ILogic logic, PsdzDatabase database, string seriesGroup, bool avoidTlsConnection);
        bool RevokeSec4CnAutorizationForSp21(IProgrammingSession session);
        bool RevokeSec4CnAutorizationForSp18(IEcuKom ecuKom);
        [PreserveSource(Cleaned = true)]
        ISvt GetCurrentSvtFromPsdzSvt();
        [PreserveSource(Cleaned = true)]
        bool ExecuteEarlyEcuValidationUsingPsdz();
    }
}