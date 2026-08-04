using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.Contracts.FASTA
{
    public interface IEcuFunction : IJournalizeManager, IProtocolTransaction
    {
        bool HasOrder { get; }

        void AddArgument(string name, string value);

        void AddArgument(string jobParam);

        [PreserveSource(Hint = "IEnumerable<DTC> ", Placeholder = true)]
        void AddJobResults(IEcuJob ecuJob, IEnumerable<PlaceholderType> dtcs, bool filterRelevantOnly);

        [PreserveSource(Hint = "IEnumerable<DTC> ", Placeholder = true)]
        void AddJobResults(IEcuJob ecuJob, Predicate<IEcuResult> ecuResultSetFilter, IEnumerable<PlaceholderType> dtcs, bool reduceZeroKmEntries);
    }
}
