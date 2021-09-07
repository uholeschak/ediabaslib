using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzSignatureResultCto))]
    [KnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
    internal class PsdzRequestNcdSignatureResponseCto : IPsdzRequestNcdSignatureResponseCto
    {
        [DataMember]
        public IList<IPsdzSignatureResultCto> SignatureResultCtoList { get; internal set; }

        [DataMember]
        public int DurationOfLastRequest { get; internal set; }

        [DataMember]
        public IList<IPsdzSecurityBackendRequestFailureCto> Failures { get; internal set; }

        [DataMember]
        public PsdzSecurityBackendRequestProgressStatusToEnum ProgressStatus { get; internal set; }
    }
}
