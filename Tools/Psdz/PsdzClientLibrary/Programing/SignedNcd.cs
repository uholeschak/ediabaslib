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
        public byte[] NcdConvertedFromBase64
        {
            get
            {
                return Convert.FromBase64String(this.ncd);
            }
        }

        internal new string ToString
        {
            get
            {
                return string.Concat(new string[]
                {
                    "BTLD-SGBM-NO :",
                    this.btld,
                    " - CAFD-SGBM-ID:",
                    this.cafd,
                    " - Ncd: ",
                    string.Join("/", new string[]
                    {
                        this.ncd
                    })
                });
            }
        }

        [DataMember(Name = "ncd")]
        public readonly string ncd;

        [DataMember(Name = "BTLD-SGBM-NO")]
        public readonly string btld;

        [DataMember(Name = "CAFD-SGBM-ID")]
        public readonly string cafd;
    }
}
