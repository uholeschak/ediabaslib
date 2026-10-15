using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    public class PsdzTargetBitmask : IPsdzTargetBitmask
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzEcuFailureResponseCto> FailedEcus { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte[] TargetBitmask { get; set; }
    }
}