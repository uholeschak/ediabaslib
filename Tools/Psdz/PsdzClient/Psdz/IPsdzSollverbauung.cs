using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzSollverbauung
    {
        string AsXml { get; }

        IPsdzSvt Svt { get; }

        IPsdzOrderList PsdzOrderList { get; }
    }
}
