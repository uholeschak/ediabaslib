using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.FeatureStatusTo;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzDiscoverFeatureStatusResultCto
    {
        string ErrorMessage { get; set; }

        IList<IPsdzFeatureStatusTo> FeatureStatus { get; set; }
    }
}