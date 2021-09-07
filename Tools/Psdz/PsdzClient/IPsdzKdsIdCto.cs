using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzKdsIdCto
    {
        string IdAsHex { get; }

        int Id { get; }
    }
}
