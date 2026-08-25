using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using PsdzClient.Core.Container;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IEcuCommunication : IJournalizeManager, IProtocolTransaction
    {
        bool HasOrder { get; }

        IEcuFunction CreateAndAddFunction(string jobName, JobStatus jobStatus);

        void AddEcuJob(IEcuJob ecuJob, IEnumerable<DTC> dtcs, bool doFastaRelevantFiltering);

        void Initialize(IEcu ecu, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering);

        void Initialize(string ecuName, IEnumerable<IEcuJob> ecuJobs, bool doFastaRelevantFiltering);

        void Initialize(IEcu ecu, IEnumerable<IEcuJob> ecuJobs, IEnumerable<JobResultData> jobFormatedResults, bool doFastaRelevantFiltering);
    }
}
