using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    public class PsdzSmacTransferStatusTA : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<string> SmartActuatorIDs { get; set; }

        public PsdzSmacTransferStatusTA()
        {
            SmartActuatorIDs = new List<string>();
        }
    }
}