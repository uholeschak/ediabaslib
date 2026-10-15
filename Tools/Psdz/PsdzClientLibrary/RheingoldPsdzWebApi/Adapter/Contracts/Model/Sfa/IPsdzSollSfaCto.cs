using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSollSfaCto
    {
        IEnumerable<IPsdzEcuFeatureTokenRelationCto> SollFeatures { get; }
    }
}
