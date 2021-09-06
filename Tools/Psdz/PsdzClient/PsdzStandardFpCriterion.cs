using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzStandardFpCriterion : IPsdzStandardFpCriterion
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string NameEn { get; set; }

        [DataMember]
        public int Value { get; set; }
    }
}
