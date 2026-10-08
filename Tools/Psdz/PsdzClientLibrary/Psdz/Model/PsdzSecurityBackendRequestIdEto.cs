using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
{
    [DataContract]
    public class PsdzSecurityBackendRequestIdEto : IPsdzSecurityBackendRequestIdEto
    {
        [DataMember]
        public int Value { get; set; }
    }
}
