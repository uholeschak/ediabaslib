using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
    [DataContract]
    public class PsdzSgbmId : IPsdzSgbmId, IComparable<IPsdzSgbmId>
    {
        [DataMember]
        public string HexString { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public long IdAsLong { get; set; }

        [DataMember]
        public int MainVersion { get; set; }

        [DataMember]
        public int PatchVersion { get; set; }

        [DataMember]
        public string ProcessClass { get; set; }

        [DataMember]
        public int SubVersion { get; set; }

        [DataMember]
        public string SGBMIDVersion { get; set; }

        public int CompareTo(IPsdzSgbmId other)
        {
            if (other == null)
            {
                return 1;
            }
            int num = string.Compare(HexString, other.HexString, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            num = string.Compare(ProcessClass, other.ProcessClass, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            num = string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            num = IdAsLong.CompareTo(other.IdAsLong);
            if (num != 0)
            {
                return num;
            }
            num = MainVersion.CompareTo(other.MainVersion);
            if (num != 0)
            {
                return num;
            }
            num = SubVersion.CompareTo(other.SubVersion);
            if (num != 0)
            {
                return num;
            }
            num = string.Compare(SGBMIDVersion, other.SGBMIDVersion, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            return PatchVersion.CompareTo(other.PatchVersion);
        }

        public override bool Equals(object obj)
        {
            PsdzSgbmId other = obj as PsdzSgbmId;
            return CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            return (((((((((((((((HexString != null) ? HexString.ToLowerInvariant().GetHashCode() : 0) * 397) ^ ((Id != null) ? Id.ToLowerInvariant().GetHashCode() : 0)) * 397) ^ IdAsLong.GetHashCode()) * 397) ^ MainVersion) * 397) ^ PatchVersion) * 397) ^ ((ProcessClass != null) ? ProcessClass.ToLowerInvariant().GetHashCode() : 0)) * 397) ^ SubVersion) * 397) ^ (SGBMIDVersion?.ToLowerInvariant().GetHashCode() ?? 0);
        }
    }
}
