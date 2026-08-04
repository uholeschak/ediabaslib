using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.ISPI.IstaServices.Contract.PUK.Data
{
    [DataContract]
    [KnownType(typeof(List<string>))]
    public class FastaPukVfc : PukVfc
    {
        [DataMember]
        public bool HasIstaVfc { get; set; }

        [DataMember]
        public ICollection<string> FaultConditionList { get; set; }

        [DataMember]
        public ICollection<string> FaultPositionComponentList { get; set; }

        [DataMember]
        public string FaultPositionLocation { get; set; }

        [DataMember]
        public List<PukLocalizedText> LocalizedFaultPatternNames { get; set; }

        public FastaPukVfc()
        {
            FaultConditionList = new List<string>();
            FaultPositionComponentList = new List<string>();
            LocalizedFaultPatternNames = new List<PukLocalizedText>();
        }
    }
}
