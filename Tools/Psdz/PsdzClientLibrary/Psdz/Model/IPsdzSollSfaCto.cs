using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    public interface IPsdzSollSfaCto
    {
        IEnumerable<IPsdzEcuFeatureTokenRelationCto> SollFeatures { get; }
    }
}
