using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzReadStatusResultCto
    {
        IList<IPsdzFeatureLongStatusCto> FeatureStatusSet { get; set; }

        IList<IPsdzEcuFailureResponseCto> Failures { get; set; }
    }
}
