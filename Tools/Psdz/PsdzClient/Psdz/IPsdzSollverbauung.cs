using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    public interface IPsdzSollverbauung
    {
        string AsXml { get; }

        IPsdzSvt Svt { get; }

        IPsdzOrderList PsdzOrderList { get; }
    }
}
