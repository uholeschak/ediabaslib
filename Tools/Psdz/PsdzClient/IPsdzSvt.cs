using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzSvt : IPsdzStandardSvt
    {
        bool IsValid { get; }

        string Vin { get; }
    }
}
