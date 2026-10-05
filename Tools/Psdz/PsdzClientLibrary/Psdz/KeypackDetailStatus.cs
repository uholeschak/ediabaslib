using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Programming;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Certificate
{
    internal class KeypackDetailStatus : IKeypackDetailStatus
    {
        public EcuCertCheckingStatus? KeyPackStatus { get; internal set; }

        public string KeyId { get; internal set; }
    }
}