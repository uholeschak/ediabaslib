using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzIstufe : IComparable<IPsdzIstufe>
    {
        bool IsValid { get; }

        string Value { get; }
    }
}
