using System.Collections.Generic;
using System.Runtime.Serialization;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa
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