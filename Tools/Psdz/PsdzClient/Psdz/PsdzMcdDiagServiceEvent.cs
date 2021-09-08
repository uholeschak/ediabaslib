using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzMcdDiagServiceEvent : PsdzEvent, ILocalizableMessage, IPsdzMcdDiagServiceEvent, IPsdzEvent
    {
        [DataMember]
        public int ErrorId { get; set; }

        [DataMember]
        public string ErrorName { get; set; }

        [DataMember]
        public string JobName { get; set; }

        [DataMember]
        public string LinkName { get; set; }

        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public string ResponseType { get; set; }

        [DataMember]
        public bool IsTimingEvent { get; set; }
    }
}
