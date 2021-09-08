using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzOrderPart))]
    [DataContract]
    [KnownType(typeof(PsdzEcu))]
    [KnownType(typeof(PsdzEcuVariantInstance))]
    public class PsdzEcuVariantInstance : PsdzLogisticPart, IPsdzEcuVariantInstance, IPsdzLogisticPart
    {
        [DataMember]
        public IPsdzOrderPart OrderablePart { get; set; }

        [DataMember]
        public IPsdzEcuVariantInstance[] CombinedWith { get; set; }

        [DataMember]
        public IPsdzEcu Ecu { get; set; }
    }
}
