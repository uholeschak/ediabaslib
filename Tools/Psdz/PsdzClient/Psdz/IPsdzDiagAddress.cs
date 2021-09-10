using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    public interface IPsdzDiagAddress
    {
        int Offset { get; }
    }
}
