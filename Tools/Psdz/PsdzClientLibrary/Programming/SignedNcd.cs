using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    public class SignedNcd
    {
        [DataMember(Name = "ncd")]
        public readonly string ncd;

        [DataMember(Name = "BTLD-SGBM-NO")]
        public readonly string btld;

        [DataMember(Name = "CAFD-SGBM-ID")]
        public readonly string cafd;

        public byte[] NcdConvertedFromBase64 => Convert.FromBase64String(ncd);

        public override string ToString()
        {
            return "BTLD-SGBM-NO :" + btld + " - CAFD-SGBM-ID:" + cafd + " - Ncd: " + ncd;
        }
    }
}
