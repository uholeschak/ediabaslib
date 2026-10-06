using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSecureTokenRequestCto
    {
        IPsdzVin VIN { get; }

        IDictionary<IPsdzEcuIdentifier, IEnumerable<IPsdzFeatureRequestCto>> EcuFeatureRequests { get; }
    }
}
