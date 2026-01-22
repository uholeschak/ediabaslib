using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    public class PsdzReadVpcFromVcmCto : IPsdzReadVpcFromVcmCto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public bool IsSuccessful { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] VpcCrc { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public long VpcVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzEcuFailureResponseCto> FailedEcus { get; set; }
    }
}
