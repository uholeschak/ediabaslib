using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzReadLcsResultCto
    {
        IEnumerable<IPsdzEcuLcsValueCto> EcuLcsValues { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> Failures { get; }
    }
}
