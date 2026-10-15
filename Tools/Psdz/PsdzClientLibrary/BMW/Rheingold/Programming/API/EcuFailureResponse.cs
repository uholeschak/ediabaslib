using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class EcuFailureResponse : IEcuFailureResponse
    {
        public IEcuIdentifier Ecu { get; internal set; }

        public string Reason { get; internal set; }
    }
}
