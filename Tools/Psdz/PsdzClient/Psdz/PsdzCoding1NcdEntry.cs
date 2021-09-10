using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.SecureCoding
{
    [DataContract]
    public class PsdzCoding1NcdEntry : IPsdzCoding1NcdEntry
    {
        [DataMember]
        public int BlockAdress { get; set; }

        [DataMember]
        public byte[] UserData { get; set; }

        [DataMember]
        public bool IsWriteable { get; set; }
    }
}
