// BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication.IEcuKom
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core.Container
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuKom : IEcuKomApi
    {
        uint EdiabasHandle { get; }

        bool IsInSimulationMode { get; }

        bool IsProblemHandlingTraceRunning { get; }

        IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding);

        IEcuJob DefaultApiJob(string ecu, string job, string param, string resultFilter);

        IEcuJob ApiJobWithRetries(string variant, string job, string param, string resultFilter, int retries);

        void End();

        string GetEdiabasIniFilePath(string iniFilename);

        bool InitVCI(IVciDevice vciDevice);

        bool Refresh();

        void SetLogLevelToMax();

        void SetLogLevelToNormal();

        int GetCacheHitCounter();

        int GetCacheMissCounter();

        int GetCacheListNumberOfJobs();

        int GetCacheListNumberOfJobsToBeRetrieved();

        bool ApiInitExt(string ifh, string unit, string app, string reserved);

        void SetEcuPath(bool logging);
    }
}
