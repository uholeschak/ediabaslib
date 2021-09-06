using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [KnownType(typeof(PsdzSwtApplicationId))]
    [DataContract]
    public class PsdzFscDeployTa : PsdzTa
    {
        [DataMember]
        public PsdzSwtActionType? Action { get; set; }

        [DataMember]
        public IPsdzSwtApplicationId ApplicationId { get; set; }

        [DataMember]
        public byte[] Fsc { get; set; }

        [DataMember]
        public byte[] FscCert { get; set; }
    }
}
