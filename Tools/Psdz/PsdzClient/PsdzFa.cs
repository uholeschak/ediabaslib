using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    public class PsdzFa : PsdzStandardFa, IPsdzFa, IPsdzStandardFa
    {
        [DataMember]
        public string AsXml { get; set; }

        [DataMember]
        public string Vin { get; set; }

        public override bool Equals(object obj)
        {
            PsdzFa psdzFa = obj as PsdzFa;
            return psdzFa != null && base.Equals(psdzFa) && string.Equals(this.Vin, psdzFa.Vin);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ ((this.Vin != null) ? this.Vin.GetHashCode() : 0);
        }
    }
}
