using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    public interface IPsdzTargetBitmask
    {
        IList<IPsdzEcuFailureResponseCto> FailedEcus { get; }

        byte[] TargetBitmask { get; }
    }
}