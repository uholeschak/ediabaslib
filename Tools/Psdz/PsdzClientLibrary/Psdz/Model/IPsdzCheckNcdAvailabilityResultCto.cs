using System.Collections.Generic;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzCheckNcdAvailabilityResultCto
    {
        IDictionary<IPsdzSgbmId, PsdzNcdStatusEtoEnum> DetailedNcdStatus { get; }

        bool IsEachNcdSigned { get; }
    }
}
