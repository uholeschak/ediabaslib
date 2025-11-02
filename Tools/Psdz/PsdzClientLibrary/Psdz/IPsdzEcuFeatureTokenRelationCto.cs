using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    public interface IPsdzEcuFeatureTokenRelationCto
    {
        IPsdzEcuIdentifier ECUIdentifier { get; }

        PsdzFeatureGroupEtoEnum FeatureGroup { get; }

        IPsdzFeatureIdCto FeatureId { get; }

        string TokenId { get; }
    }
}
