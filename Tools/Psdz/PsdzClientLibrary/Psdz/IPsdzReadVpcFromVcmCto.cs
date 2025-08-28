using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzReadVpcFromVcmCto
    {
        bool IsSuccessful { get; }

        byte[] VpcCrc { get; }

        long VpcVersion { get; }

        IList<IPsdzEcuFailureResponseCto> FailedEcus { get; }
    }
}
