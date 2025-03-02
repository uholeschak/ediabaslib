using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class EcuCertCheckingResponse : IEcuCertCheckingResponse
    {
        public IEcuIdentifier Ecu { get; internal set; }

        public EcuCertCheckingStatus? CertificateStatus { get; internal set; }

        public EcuCertCheckingStatus? BindingsStatus { get; internal set; }

        public EcuCertCheckingStatus? OtherBindingsStatus { get; internal set; }

        public IBindingDetailsStatus[] BindingDetailStatus { get; internal set; }

        public IOtherBindingDetailsStatus[] OtherBindingDetailStatus { get; internal set; }

        public EcuCertCheckingStatus? KeypackStatus { get; internal set; }

        public IKeypackDetailStatus[] KeyPackDetailedStatus { get; internal set; }

        public EcuCertCheckingStatus? OnlineBindingsStatus { get; internal set; }

        public IBindingDetailsStatus[] OnlineBindingDetailStatus { get; internal set; }

        public EcuCertCheckingStatus? OnlineCertificateStatus { get; internal set; }

        public string CreationTimestamp { get; internal set; }

        public IEcuPdxInfo PdxInfo { get; set; }
    }
}
