namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding.SignatureResultCtos
{
    public interface IPsdzSignatureResultCto
    {
        IPsdzSgbmId Cafd { get; }

        string BootloaderSgbmNumber { get; }

        byte[] Signature { get; }

        int[] CodingProofStamp { get; }

        string KeyAlgorithm { get; }

        string Digest { get; }
    }
}
