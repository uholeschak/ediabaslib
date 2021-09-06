using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzFeatureConditionCto : IPsdzFeatureConditionCto
    {
        [DataMember]
        public PsdzConditionTypeEtoEnum ConditionType { get; set; }

        [DataMember]
        public string CurrentValidityValue { get; set; }

        [DataMember]
        public int Length { get; set; }

        [DataMember]
        public string ValidityValue { get; set; }
    }
}
