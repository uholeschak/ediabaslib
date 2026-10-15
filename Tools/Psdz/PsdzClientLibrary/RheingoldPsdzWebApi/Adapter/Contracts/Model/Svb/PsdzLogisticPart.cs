using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Svb
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
