using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzReadStatusResultCto
    {
        IList<IPsdzFeatureLongStatusCto> FeatureStatusSet { get; set; }

        IList<IPsdzEcuFailureResponseCto> Failures { get; set; }
    }
}
