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

        void SetLogLevelToMax();

        void SetLogLevelToNormal();

        bool CollectPsdzLog(string targetLogFilePath);

        //FcFnActivationResult StoreAndActivateFcFn(IVehicle vehicle, int appNo, int upgradeIdx, byte[] fsc, IEcuKom ecuKom, IProtocolBasic protocoller, IICOMHandler icomHandler);

        void CloseConnectionsToPsdz(bool force);

        //IProgrammingCallbackHandler CreateCallbackHandler(ILogic logic, IProgressMonitor progressMonitor);

        string GetPsdzServiceLogFilePath();

        string GetPsdzLogFilePath();

        // [UH] Boolean
        bool StartPsdzService(IVehicle vehicle);

        //IPsdzStandardSvt GetVehicleSvtUsingPsdz(ILogic logic);

        //ISvt GetCurrentSvtFromPsdzSvt(IPsdzStandardSvt psdzStandardSvt, IDatabaseProvider database, Vehicle vehicle, IFFMDynamicResolver fFMDynamicResolver);
    }
}