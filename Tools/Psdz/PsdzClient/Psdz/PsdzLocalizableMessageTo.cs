using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzLocalizableMessageTo : ILocalizableMessage, ILocalizableMessageTo
    {
        [DataMember]
        public int MessageId { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
