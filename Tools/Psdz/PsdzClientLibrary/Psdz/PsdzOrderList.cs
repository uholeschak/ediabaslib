using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [KnownType(typeof(PsdzEcuVariantInstance))]
    [DataContract]
    public class PsdzOrderList : IPsdzOrderList
    {
        // [UH] Keep data members for backward compatibility
        [DataMember]
        public int NumberOfUnits { get; set; }

        [DataMember]
        public IPsdzEcuVariantInstance[] BntnVariantInstances { get; set; }
    }
}
