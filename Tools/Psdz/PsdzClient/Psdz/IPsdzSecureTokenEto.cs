using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzSecureTokenEto
    {
        IPsdzEcuIdentifier EcuIdentifier { get; }

        IPsdzFeatureIdCto FeatureIdCto { get; }

        string SerializedSecureToken { get; }

        string TokenId { get; }
    }
}
