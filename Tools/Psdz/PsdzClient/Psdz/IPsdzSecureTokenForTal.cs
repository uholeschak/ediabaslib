using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzSecureTokenForTal
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        long FeatureId { get; }

        string SerializedSecureToken { get; }

        string TokenId { get; }
    }
}
