using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

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
