using PsdzClient;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [PreserveSource(AttributesModified = true)]
    [DataContract]
    [KnownType(typeof(PsdzSgbmId))]
    public class PsdzStandardSvk : IPsdzStandardSvk
    {
        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte ProgDepChecked { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public IEnumerable<IPsdzSgbmId> SgbmIds { get; set; }

        [PreserveSource(KeepAttribute = true)]
        [DataMember]
        public byte SvkVersion { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PsdzStandardSvk psdzStandardSvk))
            {
                return false;
            }

            if (ProgDepChecked != psdzStandardSvk.ProgDepChecked)
            {
                return false;
            }

            if (SvkVersion != psdzStandardSvk.SvkVersion)
            {
                return false;
            }

            if (SgbmIds != null)
            {
                if (psdzStandardSvk.SgbmIds != null)
                {
                    return SgbmIds.OrderBy((IPsdzSgbmId x) => x).SequenceEqual(psdzStandardSvk.SgbmIds.OrderBy((IPsdzSgbmId x) => x));
                }

                return false;
            }

            return psdzStandardSvk.SgbmIds == null;
        }

        public override int GetHashCode()
        {
            return (((ProgDepChecked.GetHashCode() * 397) ^ ((SgbmIds != null) ? SgbmIds.OrderBy((IPsdzSgbmId x) => x).Aggregate(17, (int current, IPsdzSgbmId sgbmId) => (current * 397) ^ sgbmId.GetHashCode()) : 0)) * 397) ^ SvkVersion.GetHashCode();
        }
    }
}