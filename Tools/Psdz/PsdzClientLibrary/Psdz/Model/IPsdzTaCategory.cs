using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    public interface IPsdzTaCategory
    {
        bool IsEmpty { get; }

        IEnumerable<IPsdzTa> Tas { get; }

        PsdzTaExecutionState? ExecutionState { get; }
    }
}
