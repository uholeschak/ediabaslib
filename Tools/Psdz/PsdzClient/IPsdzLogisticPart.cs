using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzLogisticPart
    {
        string NameTais { get; }

        string SachNrTais { get; }

        int Typ { get; }

        string ZusatzTextRef { get; }
    }
}
