using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [DataContract]
    public class PsdzLogisticPart : IPsdzLogisticPart
    {
        [DataMember]
        public string NameTais { get; set; }

        [DataMember]
        public string SachNrTais { get; set; }

        [DataMember]
        public int Typ { get; set; }

        [DataMember]
        public string ZusatzTextRef { get; set; }
    }
}
