using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSmacSwDeployOnMasterTA : PsdzSwDeployTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<string> SmacIds { get; set; }
    }
}