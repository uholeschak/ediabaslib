using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
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