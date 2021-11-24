using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Sfa
{
    [DataContract]
    public class PsdzFeatureIdCto : IPsdzFeatureIdCto
    {
        [DataMember]
        public long Value { get; set; }
    }
}
