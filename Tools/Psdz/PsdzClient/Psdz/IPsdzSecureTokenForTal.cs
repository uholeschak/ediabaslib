using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzSecureTokenForTal
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        long FeatureId { get; }

        string SerializedSecureToken { get; }

        string TokenId { get; }
    }
}
