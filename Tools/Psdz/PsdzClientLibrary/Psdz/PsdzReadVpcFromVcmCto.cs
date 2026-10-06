using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Vcm
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
