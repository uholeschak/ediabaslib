using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [DataContract]
    public class PsdzScbResultStatusCto : IPsdzScbResultStatusCto
    {
        [DataMember]
        public string AppErrorId { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
