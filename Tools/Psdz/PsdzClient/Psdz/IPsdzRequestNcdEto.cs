using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzRequestNcdEto
    {
        IPsdzSgbmId Btld { get; }

        IPsdzSgbmId Cafd { get; }
    }
}
