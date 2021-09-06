using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzStandardSvk : IPsdzStandardSvk
    {
        [DataMember]
        public byte ProgDepChecked { get; set; }

        [DataMember]
        public IEnumerable<IPsdzSgbmId> SgbmIds { get; set; }

        [DataMember]
        public byte SvkVersion { get; set; }

        public override bool Equals(object obj)
        {
            PsdzStandardSvk psdzStandardSvk = obj as PsdzStandardSvk;
            if (psdzStandardSvk == null)
            {
                return false;
            }
            if (this.ProgDepChecked != psdzStandardSvk.ProgDepChecked)
            {
                return false;
            }
            if (this.SvkVersion != psdzStandardSvk.SvkVersion)
            {
                return false;
            }
            if (this.SgbmIds == null)
            {
                return psdzStandardSvk.SgbmIds == null;
            }
            if (psdzStandardSvk.SgbmIds != null)
            {
                return (from x in this.SgbmIds
                    orderby x
                    select x).SequenceEqual(from x in psdzStandardSvk.SgbmIds
                    orderby x
                    select x);
            }
            return false;
        }

        public override int GetHashCode()
        {
            int num = this.ProgDepChecked.GetHashCode() * 397;
            int num2;
            if (this.SgbmIds != null)
            {
                num2 = (from x in this.SgbmIds
                    orderby x
                    select x).Aggregate(17, (int current, IPsdzSgbmId sgbmId) => current * 397 ^ sgbmId.GetHashCode());
            }
            else
            {
                num2 = 0;
            }
            return (num ^ num2) * 397 ^ this.SvkVersion.GetHashCode();
        }
    }
}
