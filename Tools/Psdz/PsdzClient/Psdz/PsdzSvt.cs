using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzSvt : PsdzStandardSvt, IPsdzStandardSvt, IPsdzSvt
    {
        [DataMember]
        public bool IsValid { get; set; }

        [DataMember]
        public string Vin { get; set; }

        public override bool Equals(object obj)
        {
            PsdzSvt psdzSvt = obj as PsdzSvt;
            return psdzSvt != null && base.Equals(psdzSvt) && this.IsValid.Equals(psdzSvt.IsValid) && string.Equals(this.Vin, psdzSvt.Vin);
        }

        public override int GetHashCode()
        {
            return (base.GetHashCode() * 397 ^ this.IsValid.GetHashCode()) * 397 ^ ((this.Vin != null) ? this.Vin.GetHashCode() : 0);
        }
    }
}
