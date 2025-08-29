using BMW.Rheingold.Psdz.Model.Tal;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzSmacEcuMirrorDeployOnMasterTA : PsdzEcuMirrorDeployTa
    {
        [DataMember]
        public IList<string> SmacIds { get; set; }
    }
}