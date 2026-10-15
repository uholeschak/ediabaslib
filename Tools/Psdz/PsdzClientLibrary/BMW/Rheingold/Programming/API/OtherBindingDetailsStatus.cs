using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class OtherBindingDetailsStatus : IOtherBindingDetailsStatus
    {
        public EcuCertCheckingStatus? OtherBindingStatus { get; internal set; }

        public string RollenName { get; internal set; }

        public string EcuName { get; internal set; }
    }
}
