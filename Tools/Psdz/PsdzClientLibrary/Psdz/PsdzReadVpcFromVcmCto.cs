using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    public class PsdzReadVpcFromVcmCto : IPsdzReadVpcFromVcmCto
    {
        [DataMember]
        public bool IsSuccessful { get; set; }

        [DataMember]
        public byte[] VpcCrc { get; set; }

        [DataMember]
        public long VpcVersion { get; set; }

        [DataMember]
        public IList<IPsdzEcuFailureResponseCto> FailedEcus { get; set; }
    }
}
