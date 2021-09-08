using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzFetchEcuCertCheckingResult
    {
        [DataMember]
        public IEnumerable<PsdzEcuFailureResponse> FailedEcus { get; set; }

        [DataMember]
        public IEnumerable<PsdzEcuCertCheckingResponse> Results { get; set; }
    }
}
