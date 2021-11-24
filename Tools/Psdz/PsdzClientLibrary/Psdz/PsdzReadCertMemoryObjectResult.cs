using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Certificate
{
    [DataContract]
    public class PsdzReadCertMemoryObjectResult
    {
        [DataMember]
        public PsdzCertMemoryObject[] MemoryObjects { get; set; }

        [DataMember]
        public PsdzEcuFailureResponse[] FailedEcus { get; set; }
    }
}
