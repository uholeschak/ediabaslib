using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [DataContract]
    [KnownType(typeof(PsdzLogisticPart))]
    [KnownType(typeof(PsdzOrderPart))]
    [KnownType(typeof(PsdzEcuVariantInstance))]
    [KnownType(typeof(PsdzReplacementPart))]
    public class PsdzReplacementPart : PsdzLogisticPart, IPsdzReplacementPart, IPsdzLogisticPart
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzLogisticPart[] Deliverables { get; set; }
    }
}
