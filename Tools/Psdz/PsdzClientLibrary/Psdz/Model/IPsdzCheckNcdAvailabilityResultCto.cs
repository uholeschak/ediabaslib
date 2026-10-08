using System.Collections.Generic;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCheckNcdAvailabilityResultCto
    {
        IDictionary<IPsdzSgbmId, PsdzNcdStatusEtoEnum> DetailedNcdStatus { get; }

        bool IsEachNcdSigned { get; }
    }
}
