using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
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
