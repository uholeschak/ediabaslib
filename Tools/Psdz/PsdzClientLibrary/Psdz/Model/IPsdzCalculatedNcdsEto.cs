using BMW.Rheingold.Psdz.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCalculatedNcdsEto
    {
        string Btld { get; }

        IPsdzSgbmId CafdId { get; }

        IPsdzNcd Ncd { get; }
    }
}
