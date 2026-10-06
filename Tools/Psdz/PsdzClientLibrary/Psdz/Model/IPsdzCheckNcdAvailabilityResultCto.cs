using System.Collections.Generic;
using BMW.Rheingold.Psdz.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCheckNcdAvailabilityResultCto
    {
        IDictionary<IPsdzSgbmId, PsdzNcdStatusEtoEnum> DetailedNcdStatus { get; }

        bool IsEachNcdSigned { get; }
    }
}
