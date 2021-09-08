using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
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
