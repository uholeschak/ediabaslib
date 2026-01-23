using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
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