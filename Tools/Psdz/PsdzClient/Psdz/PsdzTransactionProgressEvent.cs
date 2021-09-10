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
    public class PsdzTransactionProgressEvent : PsdzTransactionEvent, ILocalizableMessage, IPsdzEvent, IPsdzTransactionEvent, IPsdzTransactionProgressEvent
    {
        [DataMember]
        public int Progress { get; set; }

        [DataMember]
        public int TaProgress { get; set; }
    }
}
