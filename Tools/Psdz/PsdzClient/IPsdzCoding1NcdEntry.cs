using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzCoding1NcdEntry
    {
        int BlockAdress { get; }

        byte[] UserData { get; }

        bool IsWriteable { get; }
    }
}
