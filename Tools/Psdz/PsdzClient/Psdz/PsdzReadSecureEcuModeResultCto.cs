using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzEcuIdentifier))]
    [KnownType(typeof(PsdzEcuFailureResponseCto))]
    [DataContract]
    public class PsdzReadSecureEcuModeResultCto : IPsdzReadSecureEcuModeResultCto
    {
        [DataMember]
        public IDictionary<IPsdzEcuIdentifier, PsdzSecureEcuModeEtoEnum> SecureEcuModes { get; set; }

        [DataMember]
        public IEnumerable<IPsdzEcuFailureResponseCto> FailureResponse { get; set; }
    }
}
