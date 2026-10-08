using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding.SignatureResultCto;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding.SignatureResultCtos;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.RequestNcdSignatureResponseCto
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
