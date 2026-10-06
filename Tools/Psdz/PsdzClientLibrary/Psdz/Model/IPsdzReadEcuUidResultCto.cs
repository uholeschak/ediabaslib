using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement
{
    public interface IPsdzReadEcuUidResultCto
    {
        IDictionary<IPsdzEcuIdentifier, IPsdzEcuUidCto> EcuUids { get; }

        IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; }
    }
}
