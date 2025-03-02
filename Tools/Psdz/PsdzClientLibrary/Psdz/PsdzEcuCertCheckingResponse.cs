using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Ecu;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    public enum PsdzEcuCertCheckingStatus
    {
        CheckStillRunning,
        Decryption_Error,
        Empty,
        Incomplete,
        IssuerCertError,
        Malformed,
        Ok,
        Other,
        Outdated,
        OwnCertNotPresent,
        SecurityError,
        Unchecked,
        Undefined,
        WrongEcuUid,
        WrongVin17,
        KeyError,
        NotUsed
    }

    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzEcuCertCheckingResponse
    {
        [DataMember]
        public IPsdzEcuIdentifier Ecu { get; set; }

        [DataMember]
        public PsdzEcuCertCheckingStatus? CertificateStatus { get; set; }

        [DataMember]
        public PsdzEcuCertCheckingStatus? BindingsStatus { get; set; }

        [DataMember]
        public PsdzEcuCertCheckingStatus? OtherBindingsStatus { get; set; }

        [DataMember]
        public PsdzEcuCertCheckingStatus? OnlineCertificateStatus { get; set; }

        [DataMember]
        public PsdzEcuCertCheckingStatus? OnlineBindingsStatus { get; set; }

        [DataMember]
        public PsdzEcuCertCheckingStatus? KeyPackStatus { get; set; }

        [DataMember]
        public PsdzBindingDetailsStatus[] BindingDetailStatus { get; set; }

        [DataMember]
        public PsdzBindingDetailsStatus[] OnlineBindingDetailStatus { get; set; }

        [DataMember]
        public PsdzOtherBindingDetailsStatus[] OtherBindingDetailStatus { get; set; }

        [DataMember]
        public PsdzKeypackDetailStatus[] KeyPackDatailedStatus { get; set; }

        [DataMember]
        public string CreationTimestamp { get; set; }
    }
}
