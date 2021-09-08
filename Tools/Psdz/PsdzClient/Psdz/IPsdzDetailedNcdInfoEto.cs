using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzNcdStatusEtoEnum
    {
        NO_NCD,
        SIGNED,
        UNSIGNED,
        CPS_INVALID
    }

    public interface IPsdzDetailedNcdInfoEto
    {
        IPsdzSgbmId Btld { get; }

        IPsdzSgbmId Cafd { get; }

        string CodingVersion { get; }

        IList<IPsdzDiagAddressCto> DiagAdresses { get; }

        PsdzNcdStatusEtoEnum NcdStatus { get; }
    }
}
