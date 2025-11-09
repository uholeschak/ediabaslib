using PsdzClientLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    [DataContract]
    [KnownType(typeof(PsdzEcuVariantInstance))]
    public class PsdzOrderList : IPsdzOrderList
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public int NumberOfUnits { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuVariantInstance[] BntnVariantInstances { get; set; }
    }
}