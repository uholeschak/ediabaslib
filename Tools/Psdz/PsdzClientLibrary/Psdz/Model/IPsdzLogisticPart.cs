using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    public interface IPsdzLogisticPart
    {
        string NameTais { get; }

        string SachNrTais { get; }

        int Typ { get; }

        string ZusatzTextRef { get; }
    }
}
