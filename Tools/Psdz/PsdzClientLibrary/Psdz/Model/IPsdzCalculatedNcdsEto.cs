namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    public interface IPsdzCalculatedNcdsEto
    {
        string Btld { get; }

        IPsdzSgbmId CafdId { get; }

        IPsdzNcd Ncd { get; }
    }
}
