using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class BindingDetailsStatus : IBindingDetailsStatus
    {
        public EcuCertCheckingStatus? BindingStatus { get; internal set; }

        public EcuCertCheckingStatus? CertificateStatus { get; internal set; }

        public string RollenName { get; internal set; }
    }
}
