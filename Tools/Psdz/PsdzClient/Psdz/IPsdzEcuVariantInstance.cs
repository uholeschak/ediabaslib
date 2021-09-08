using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzEcuVariantInstance : IPsdzLogisticPart
    {
        IPsdzOrderPart OrderablePart { get; }

        IPsdzEcuVariantInstance[] CombinedWith { get; }

        IPsdzEcu Ecu { get; set; }
    }
}
