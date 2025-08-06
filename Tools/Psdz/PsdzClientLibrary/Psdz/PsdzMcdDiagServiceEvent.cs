using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Localization;

namespace BMW.Rheingold.Psdz.Model.Events
{
    [DataContract]
    public class PsdzMcdDiagServiceEvent : PsdzEvent, IPsdzMcdDiagServiceEvent, IPsdzEvent, ILocalizableMessage
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
