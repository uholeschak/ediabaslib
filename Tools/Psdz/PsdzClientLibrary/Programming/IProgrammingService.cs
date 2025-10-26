using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core.Container;
using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;

namespace PsdzClient.Programming
{
    public interface IProgrammingService
    {
        IPsdz Psdz { get; }

        //IEnumerable<IProgrammingTask> RetrieveAvailableProgrammingTasks(IVehicle vehicle);

        //IProgrammingSessionExt Start(ProgrammingParam programmingParam);

        //IProgrammingSessionExt Start(ProgrammingParam programmingParam, bool avoidTlsConnection);

        void SetLogLevelToMax();

        void SetLogLevelToNormal();

        bool CollectPsdzLog(string targetLogFilePath);

        //FcFnActivationResult StoreAndActivateFcFn(IVehicle vehicle, int appNo, int upgradeIdx, byte[] fsc, IEcuKom ecuKom, IProtocolBasic protocoller, IICOMHandler icomHandler);

        // [UH] force added
        void CloseConnectionsToPsdz(bool force);

        //IProgrammingCallbackHandler CreateCallbackHandler(ILogic logic, IProgressMonitor progressMonitor);

        string GetPsdzWebServiceLogFilePath();

        string GetPsdzLogFilePath();

        // [UH] Boolean
        bool StartPsdzService(IVehicle vehicle);

        //IPsdzStandardSvt GetVehicleSvtUsingPsdz(ILogic logic);

        //bool ExecuteEarlyEcuValidationUsingPsdz(ILogic logic, IDatabaseProvider database, string mainseries, bool avoidTlsConnection);

        //ISvt GetCurrentSvtFromPsdzSvt(IPsdzStandardSvt psdzStandardSvt, IDatabaseProvider database, Vehicle vehicle, IFFMDynamicResolver fFMDynamicResolver);
    }
}