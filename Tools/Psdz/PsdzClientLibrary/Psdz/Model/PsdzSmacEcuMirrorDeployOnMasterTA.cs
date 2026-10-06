using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSmacEcuMirrorDeployOnMasterTA : PsdzEcuMirrorDeployTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<string> SmacIds { get; set; }
    }
}