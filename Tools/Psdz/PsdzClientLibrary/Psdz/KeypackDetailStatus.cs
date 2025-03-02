using PsdzClient.Programming;

namespace PsdzClient.Programming
{
    internal class KeypackDetailStatus : IKeypackDetailStatus
    {
        public EcuCertCheckingStatus? KeyPackStatus { get; internal set; }

        public string KeyId { get; internal set; }
    }
}