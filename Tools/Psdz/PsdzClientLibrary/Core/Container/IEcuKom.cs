// BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication.IEcuKom
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;

namespace PsdzClient.Core.Container
{
    // ToDo: Check on update
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuKom : IEcuKomApi
    {
        //uint EdiabasHandle { get; }

        bool IsInSimulationMode { get; }

        bool IsProblemHandlingTraceRunning { get; set; }

        bool IpbWithoutCertificates { get; }

        string VciIpAddress { get; }

        VCIDeviceType VCIDeviceType { get; }

        new IEcuJob ExecuteJobOverEnetActivateDHCP(string icomAddress, string ecu, string job, string param, bool isDoIP, string resultFilter = "", int retries = 0);

        IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding);

        IEcuJob DefaultApiJob(string ecu, string job, string param, string resultFilter);

        IEcuJob ApiJobWithRetries(string variant, string job, string param, string resultFilter, int retries);

        void End();

        string GetEdiabasIniFilePath(string iniFilename);

        BoolResultObject InitVCI(IVciDevice vciDevice, bool isDoIP);

        BoolResultObject InitVCI(IVciDevice vciDevice, bool isDoIP, bool firstInitialisation);

        bool Refresh(bool isDoIP);

        void SetLogLevelToMax();

        int GetCacheHitCounter();

        int GetCacheMissCounter();

        int GetCacheListNumberOfJobs();

        int GetCacheListNumberOfJobsToBeRetrieved();

        bool ApiInitExt(string ifh, string unit, string app, string reserved);

        void SetEcuPath(bool logging);

        void RemoveTraceLevel(string callerMember);

        void SetTraceLevelToMax(string callerMember);
    }
}
