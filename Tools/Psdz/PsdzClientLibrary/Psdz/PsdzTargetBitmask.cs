using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
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