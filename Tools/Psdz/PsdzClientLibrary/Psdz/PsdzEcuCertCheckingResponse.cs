using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Runtime.Serialization;

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

    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzEcuCertCheckingResponse
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier Ecu { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? CertificateStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? BindingsStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? OtherBindingsStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? OnlineCertificateStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? OnlineBindingsStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzEcuCertCheckingStatus? KeyPackStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBindingDetailsStatus[] BindingDetailStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzBindingDetailsStatus[] OnlineBindingDetailStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzOtherBindingDetailsStatus[] OtherBindingDetailStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzKeypackDetailStatus[] KeyPackDatailedStatus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string CreationTimestamp { get; set; }
    }
}
