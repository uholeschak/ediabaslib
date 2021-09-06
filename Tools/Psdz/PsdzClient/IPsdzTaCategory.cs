using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzTaCategory
    {
        bool IsEmpty { get; }

        IEnumerable<IPsdzTa> Tas { get; }
    }
}
