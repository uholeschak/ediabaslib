using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzSFADeleteTA : PsdzTa, IPsdzFsaTa, IPsdzTa, IPsdzTalElement
    {
        [DataMember]
        public long EstimatedExecutionTime { get; set; }

        [DataMember]
        public long FeatureId { get; set; }
    }
}
