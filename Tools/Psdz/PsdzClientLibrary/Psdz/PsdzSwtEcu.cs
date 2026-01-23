using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Swt
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSwtApplication))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzSwtEcu : IPsdzSwtEcu
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzRootCertificateState RootCertState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public PsdzSoftwareSigState SoftwareSigState { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzSwtApplication> SwtApplications { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public string Vin { get; set; }
    }
}
