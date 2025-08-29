using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Tal
{
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzSmacTransferStartTA : PsdzTa
    {
        [DataMember]
        public IDictionary<string, IList<IPsdzSgbmId>> SmartActuatorData { get; set; }

        public PsdzSmacTransferStartTA()
        {
            SmartActuatorData = new Dictionary<string, IList<IPsdzSgbmId>>();
        }
    }
}
