using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    public interface IPsdzCheckNcdAvailabilityResultCto
    {
        IDictionary<IPsdzSgbmId, PsdzNcdStatusEtoEnum> DetailedNcdStatus { get; }

        bool IsEachNcdSigned { get; }
    }
}
