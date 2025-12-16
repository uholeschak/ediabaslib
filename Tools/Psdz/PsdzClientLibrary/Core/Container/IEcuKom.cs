using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Contracts;

namespace PsdzClient.Core.Container
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuKom : IEcuKomApi
    {
        uint EdiabasHandle { get; }

        bool IsInSimulationMode { get; }

        bool IsProblemHandlingTraceRunning { get; set; }

        string VciIpAddress { get; }

        VCIDeviceType VCIDeviceType { get; }

        CommMode CommunicationMode { get; set; }

        VCIDevice VCI { get; set; }

        List<IEcuJob> JobList { get; }

        bool FromFastaConfig { get; set; }

        string APP { get; set; }

        new IEcuJob ExecuteJobOverEnetActivateDHCP(string icomAddress, string ecu, string job, string param, bool isDoIP, string resultFilter = "", int retries = 0);
        IEcuJob ApiJob(string ecu, string job, string param, string resultFilter, bool cacheAdding);
        IEcuJob DefaultApiJob(string ecu, string job, string param, string resultFilter);
        IEcuJob ApiJobWithRetries(string variant, string job, string param, string resultFilter, int retries);
        IEcuJob ApiJob(string ecu, string job, string param, string resultFilter = "", int retries = 0, IProtocolBasic fastaprotocoller = null, bool fastaActive = true);
        IEcuJob apiJob(string variant, string job, string param, string resultFilter, int retries, string sgbd = "", IProtocolBasic fastaprotocoller = null, [CallerMemberName] string callerMember = "");
        IEcuJob apiJob(string ecu, string jobName, string param, string resultFilter, int retries, int millisecondsTimeout, IProtocolBasic fastaprotocoller = null, string callerMember = "");
        IEcuJob apiJob(string ecu, string job, string param, string resultFilter, IProtocolBasic fastaprotocoller = null, string callerMember = "");
        IEcuJob apiJob(string ecu, string jobName, string param, string resultFilter, bool cacheAdding, IProtocolBasic fastaprotocoller = null, string callerMember = "");
        IEcuJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter, int retries);
        IEcuJob apiJobData(string ecu, string job, byte[] param, int paramlen, string resultFilter, string callerMember);
        void End();
        string GetEdiabasIniFilePath(string iniFilename);
        BoolResultObject InitVCI(IVciDevice vciDevice, bool isDoIP);
        bool Refresh(bool isDoIP);
        void SetLogLevelToMax();
        int GetCacheHitCounter();
        int GetCacheMissCounter();
        int GetCacheListNumberOfJobs();
        int GetCacheListNumberOfJobsToBeRetrieved();
        string GetLogPath();
        bool ApiInitExt(string ifh, string unit, string app, string reserved);
        void SetEcuPath(bool logging);
        void RemoveTraceLevel(string callerMember);
        void SetTraceLevelToMax(string callerMember);
        bool setConfig(string cfgName, string cfgValue);
        int getErrorCode();
        string getErrorText();
        SpecialSecurityCases DetectedSpecialSecurityCase();
    }
}