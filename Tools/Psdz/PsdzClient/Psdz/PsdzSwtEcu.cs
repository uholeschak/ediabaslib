using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    [KnownType(typeof(PsdzSwtApplication))]
    [KnownType(typeof(PsdzEcuIdentifier))]
    public class PsdzSwtEcu : IPsdzSwtEcu
    {
        [DataMember]
        public IPsdzEcuIdentifier EcuIdentifier { get; set; }

        [DataMember]
        public PsdzRootCertificateState RootCertState { get; set; }

        [DataMember]
        public PsdzSoftwareSigState SoftwareSigState { get; set; }

        [DataMember]
        public IEnumerable<IPsdzSwtApplication> SwtApplications { get; set; }

        [DataMember]
        public string Vin { get; set; }
    }
}
