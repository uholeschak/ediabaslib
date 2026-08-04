using System.Runtime.Serialization;

namespace BMW.ISPI.IstaServices.Contract.PUK.Data
{
    [DataContract]
    public class PukVfc
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string FaultPositionVehicle { get; set; }

        [DataMember]
        public string FaultType { get; set; }

        [DataMember]
        public string FaultLocationFunctional { get; set; }

        [DataMember]
        public string FaultLocationComponent { get; set; }
    }
}
