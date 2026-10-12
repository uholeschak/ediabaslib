using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class KeypackDetailStatus : IKeypackDetailStatus
    {
        public EcuCertCheckingStatus? KeyPackStatus { get; internal set; }

        public string KeyId { get; internal set; }
    }
}