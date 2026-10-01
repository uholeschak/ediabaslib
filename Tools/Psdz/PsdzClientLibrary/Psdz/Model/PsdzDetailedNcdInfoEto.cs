using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [PreserveSource(AttributesModified = true)]
    [KnownType(typeof(PsdzSgbmId))]
    [DataContract]
    [KnownType(typeof(PsdzDiagAddressCto))]
    public class PsdzDetailedNcdInfoEto : IPsdzDetailedNcdInfoEto
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Btld { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string CodingVersion { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IList<IPsdzDiagAddressCto> DiagAdresses { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzNcdStatusEtoEnum NcdStatus { get; set; }
    }
}
