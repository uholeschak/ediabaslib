using BMW.Rheingold.Psdz.Model.SecureCoding.SignatureResultCto;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Sfa.RequestNcdSignatureResponseCto
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSignatureResultCto))]
    [KnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
    public class PsdzRequestNcdSignatureResponseCto : IPsdzRequestNcdSignatureResponseCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzSignatureResultCto> SignatureResultCtoList { get; internal set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int DurationOfLastRequest { get; internal set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzSecurityBackendRequestFailureCto> Failures { get; internal set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSecurityBackendRequestProgressStatusToEnum ProgressStatus { get; internal set; }
    }
}
