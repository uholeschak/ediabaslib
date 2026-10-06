using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Vcm
{
    public interface IPsdzReadVpcFromVcmCto
    {
        bool IsSuccessful { get; }

        byte[] VpcCrc { get; }

        long VpcVersion { get; }

        IList<IPsdzEcuFailureResponseCto> FailedEcus { get; }
    }
}
