using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzIdLightBasisTa : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<string> Ids { get; set; }
    }
}
