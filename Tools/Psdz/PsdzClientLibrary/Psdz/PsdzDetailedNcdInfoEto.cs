using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [KnownType(typeof(PsdzSgbmId))]
    [DataContract]
    [KnownType(typeof(PsdzDiagAddressCto))]
    public class PsdzDetailedNcdInfoEto : IPsdzDetailedNcdInfoEto
    {
        [DataMember]
        public IPsdzSgbmId Btld { get; set; }

        [DataMember]
        public IPsdzSgbmId Cafd { get; set; }

        [DataMember]
        public string CodingVersion { get; set; }

        [DataMember]
        public IList<IPsdzDiagAddressCto> DiagAdresses { get; set; }

        [DataMember]
        public PsdzNcdStatusEtoEnum NcdStatus { get; set; }
    }
}
