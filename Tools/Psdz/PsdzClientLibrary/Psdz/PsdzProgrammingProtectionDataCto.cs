using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzProgrammingProtectionDataCto : IPsdzProgrammingProtectionDataCto
    {
        [DataMember]
        public IList<IPsdzEcuIdentifier> ProgrammingProtectionEcus { get; set; }

        [DataMember]
        public IList<IPsdzSgbmId> SWEList { get; set; }

        [DataMember]
        public byte[] SWEData { get; set; }
    }
}