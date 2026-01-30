using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzDetailedNcdInfoEto
    {
        IPsdzSgbmId Btld { get; }

        IPsdzSgbmId Cafd { get; }

        string CodingVersion { get; }

        IList<IPsdzDiagAddressCto> DiagAdresses { get; }

        PsdzNcdStatusEtoEnum NcdStatus { get; }
    }
}
