using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzFa : IPsdzStandardFa
    {
        string AsXml { get; }

        string Vin { get; }
    }
}
