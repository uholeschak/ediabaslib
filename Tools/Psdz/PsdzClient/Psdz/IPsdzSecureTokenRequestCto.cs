using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzSecureTokenRequestCto
    {
        IPsdzVin VIN { get; }

        IDictionary<IPsdzEcuIdentifier, IEnumerable<IPsdzFeatureRequestCto>> EcuFeatureRequests { get; }
    }
}
