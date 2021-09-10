using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    public interface IPsdzEcuVariantInstance : IPsdzLogisticPart
    {
        IPsdzOrderPart OrderablePart { get; }

        IPsdzEcuVariantInstance[] CombinedWith { get; }

        IPsdzEcu Ecu { get; set; }
    }
}
