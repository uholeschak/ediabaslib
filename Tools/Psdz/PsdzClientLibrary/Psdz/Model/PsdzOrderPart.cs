using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
{
    [DataContract]
    [KnownType(typeof(PsdzLogisticPart))]
    [KnownType(typeof(PsdzOrderPart))]
    [KnownType(typeof(PsdzEcuVariantInstance))]
    [KnownType(typeof(PsdzReplacementPart))]
    public class PsdzOrderPart : PsdzLogisticPart, IPsdzOrderPart, IPsdzLogisticPart
    {
        [DataMember]
        public IPsdzLogisticPart[] Deliverables { get; set; }

        [DataMember]
        public IPsdzLogisticPart[] Pattern { get; set; }
    }
}