using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    public interface IPsdzTaCategory
    {
        bool IsEmpty { get; }

        IEnumerable<IPsdzTa> Tas { get; }

        PsdzTaExecutionState? ExecutionState { get; }
    }
}
