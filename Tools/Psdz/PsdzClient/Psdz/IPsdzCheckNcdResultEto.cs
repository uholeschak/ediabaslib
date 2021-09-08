using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzCheckNcdResultEto
    {
        IList<IPsdzDetailedNcdInfoEto> DetailedNcdStatus { get; }

        bool isEachNcdSigned { get; }
    }
}
