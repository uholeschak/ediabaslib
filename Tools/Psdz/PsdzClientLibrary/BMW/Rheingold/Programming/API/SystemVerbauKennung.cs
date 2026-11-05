using System.Collections.Generic;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

#pragma warning disable CS0618
namespace BMW.Rheingold.Programming.API
{
    [DataContract]
    public class SystemVerbauKennung : IStandardSvk
    {
        [DataMember]
        public byte ProgDepChecked { get; set; }

        [DataMember]
        public IEnumerable<ISgbmId> SgbmIds { get; set; }

        [DataMember]
        public byte SvkVersion { get; set; }
    }
}
