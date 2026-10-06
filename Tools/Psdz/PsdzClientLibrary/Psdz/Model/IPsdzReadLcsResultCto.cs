using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzReadLcsResultCto
    {
        IEnumerable<IPsdzEcuLcsValueCto> EcuLcsValues { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; }
    }
}
