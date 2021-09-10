using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [KnownType(typeof(PsdzOrderPart))]
    [KnownType(typeof(PsdzEcuVariantInstance))]
    [DataContract]
    [KnownType(typeof(PsdzLogisticPart))]
    [KnownType(typeof(PsdzReplacementPart))]
    public class PsdzOrderPart : PsdzLogisticPart, IPsdzLogisticPart, IPsdzOrderPart
    {
        [DataMember]
        public IPsdzLogisticPart[] Deliverables { get; set; }

        [DataMember]
        public IPsdzLogisticPart[] Pattern { get; set; }
    }
}
