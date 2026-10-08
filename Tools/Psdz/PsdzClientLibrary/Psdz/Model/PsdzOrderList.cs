using PsdzClient;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
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