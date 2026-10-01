using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzSmacTransferStartTA : PsdzTa
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IDictionary<string, IList<IPsdzSgbmId>> SmartActuatorData { get; set; }

        public PsdzSmacTransferStartTA()
        {
            SmartActuatorData = new Dictionary<string, IList<IPsdzSgbmId>>();
        }
    }
}
