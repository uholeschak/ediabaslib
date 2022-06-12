using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Programming.Controller.SecureCoding.Model
{
    [DataContract]
    public class EcuData
    {
        public List<string> CafdId
        {
            get
            {
                return (from a in this.cafd
                    select a.Split(new char[]
                    {
                        '.'
                    })[0].Replace("cafd_", "").ToUpper()).ToList<string>();
            }
        }

        internal new string ToString
        {
            get
            {
                return "BTLD-SGBM-NO :" + this.btld + " - CAFD-SGBM-ID:" + string.Join("/", this.cafd);
            }
        }

        [DataMember(Name = "BTLD-SGBM-NO")]
        public readonly string btld;

        [DataMember(Name = "CAFD-SGBM-ID")]
        public readonly string[] cafd;
    }
}
