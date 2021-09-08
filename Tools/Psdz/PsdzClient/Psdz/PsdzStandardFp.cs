using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzStandardFpCriterion))]
    public class PsdzStandardFp : IPsdzStandardFp
    {
        [DataMember(IsRequired = true)]
        public string AsString { get; set; }

        [DataMember]
        public IDictionary<int, IList<IPsdzStandardFpCriterion>> Category2Criteria { get; set; }

        [DataMember]
        public IDictionary<int, string> CategoryId2CategoryName { get; set; }

        [DataMember]
        public bool IsValid { get; set; }
    }
}
