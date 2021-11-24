using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.SecureCoding.SignatureResultCto;

namespace BMW.Rheingold.Psdz.Model.Sfa.RequestNcdSignatureResponseCto
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
