using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzCalculatedNcdsEto
    {
        string Btld { get; }

        IPsdzSgbmId CafdId { get; }

        IPsdzNcd Ncd { get; }
    }
}
