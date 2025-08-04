using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzFa : PsdzStandardFa, IPsdzFa, IPsdzStandardFa
    {
        [DataMember]
        public string AsXml { get; set; }

#if OLD_PSDZ_FA
        [DataMember]
        public string Vin { get; set; }
#endif
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
