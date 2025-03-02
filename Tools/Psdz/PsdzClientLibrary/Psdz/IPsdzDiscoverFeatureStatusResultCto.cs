using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzDiscoverFeatureStatusResultCto
    {
        string ErrorMessage { get; set; }

        IList<IPsdzFeatureStatusTo> FeatureStatus { get; set; }
    }
}