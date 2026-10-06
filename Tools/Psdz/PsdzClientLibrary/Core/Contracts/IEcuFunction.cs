using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient;
using System;
using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.VehicleCommunication;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IEcuFunction : IJournalizeManager, IProtocolTransaction
    {
        bool HasOrder { get; }

        void AddArgument(string name, string value);

        void AddArgument(string jobParam);

        void AddJobResults(IEcuJob ecuJob, IEnumerable<DTC> dtcs, bool filterRelevantOnly);

        void AddJobResults(IEcuJob ecuJob, Predicate<IEcuResult> ecuResultSetFilter, IEnumerable<DTC> dtcs, bool reduceZeroKmEntries);
    }
}
