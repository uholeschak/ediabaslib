
namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding
{
    public interface IPsdzCalculatedNcdsEto
    {
        string Btld { get; }

        IPsdzSgbmId CafdId { get; }

        IPsdzNcd Ncd { get; }
    }
}
