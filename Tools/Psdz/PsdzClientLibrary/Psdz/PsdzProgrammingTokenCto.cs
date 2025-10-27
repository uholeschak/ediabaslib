using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzVin))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzEcuUidCto))]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzProgrammingTokenCto : IPsdzProgrammingTokenCto
    {
        [DataMember]
        public int TokenVersion { get; set; }

        [DataMember]
        public IPsdzVin Vin { get; set; }

        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [DataMember]
        public IPsdzEcuUidCto EcuUidCto { get; set; }

        [DataMember]
        public IEnumerable<IPsdzSgbmId> ActiveSGBMIDs { get; set; }

        [DataMember]
        public IEnumerable<IPsdzSgbmId> NewSGBMIDs { get; set; }

        [DataMember]
        public byte[] ActiveSGBMIDsHash { get; set; }

        [DataMember]
        public byte[] ValidityStartTime { get; set; }

        [DataMember]
        public byte[] ValidityEndTime { get; set; }

        [DataMember]
        public bool IsSigned { get; set; }

        [DataMember]
        public byte[] ProgrammingTokenAsBytes { get; set; }
    }
}