using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzDetailedStatusCto
    {
        IPsdzDiagAddress DiagAddressCto { get; }

        IPsdzFeatureIdCto FeatureIdCto { get; }

        PsdzTokenDetailedStatusEtoEnum TokenDetailedStatusEto { get; }
    }
}
