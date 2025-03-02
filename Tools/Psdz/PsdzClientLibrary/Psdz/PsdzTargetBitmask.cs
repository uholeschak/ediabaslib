using BMW.Rheingold.Psdz.Model.Sfa;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    public class PsdzTargetBitmask : IPsdzTargetBitmask
    {
        [DataMember]
        public IList<IPsdzEcuFailureResponseCto> FailedEcus { get; set; }

        [DataMember]
        public byte[] TargetBitmask { get; set; }
    }
}