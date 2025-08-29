using BMW.Rheingold.Psdz.Model.Tal;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    public class PsdzSmacTransferStatusTA : PsdzTa
    {
        [DataMember]
        public IList<string> SmartActuatorIDs { get; set; }

        public PsdzSmacTransferStatusTA()
        {
            SmartActuatorIDs = new List<string>();
        }
    }
}