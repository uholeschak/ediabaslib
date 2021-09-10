using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecurityManagement
{
    [DataContract]
    public class PsdzEcuUidCto : IPsdzEcuUidCto
    {
        [DataMember]
        public string EcuUid { get; set; }
    }
}
