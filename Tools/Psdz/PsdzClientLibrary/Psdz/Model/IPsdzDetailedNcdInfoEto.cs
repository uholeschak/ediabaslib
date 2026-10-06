using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
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
