using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzDiagAddressCto : IPsdzDiagAddressCto
    {
        [DataMember]
        public int INVALID_OFFSET { get; set; }

        [DataMember]
        public int MAX_OFFSETT { get; set; }

        [DataMember]
        public int MIN_OFFSET { get; set; }

        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public string OffsetSetAsHex { get; set; }

        [DataMember]
        public int OffsetSetAsInt { get; set; }

        [DataMember]
        public string OffsetSetAsString { get; set; }
    }
}
