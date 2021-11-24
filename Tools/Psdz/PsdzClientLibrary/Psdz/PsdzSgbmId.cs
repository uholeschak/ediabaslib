using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model
{
	[DataContract]
	public class PsdzSgbmId : IComparable<IPsdzSgbmId>, IPsdzSgbmId
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

		public int CompareTo(IPsdzSgbmId other)
		{
			if (other == null)
			{
				return 1;
			}
			int num = string.Compare(this.HexString, other.HexString, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			num = string.Compare(this.ProcessClass, other.ProcessClass, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			num = string.Compare(this.Id, other.Id, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			num = this.IdAsLong.CompareTo(other.IdAsLong);
			if (num != 0)
			{
				return num;
			}
			num = this.MainVersion.CompareTo(other.MainVersion);
			if (num != 0)
			{
				return num;
			}
			num = this.SubVersion.CompareTo(other.SubVersion);
			if (num != 0)
			{
				return num;
			}
			return this.PatchVersion.CompareTo(other.PatchVersion);
		}

		public override bool Equals(object obj)
		{
			PsdzSgbmId other = obj as PsdzSgbmId;
			return this.CompareTo(other) == 0;
		}

		public override int GetHashCode()
		{
			return (((((((this.HexString != null) ? this.HexString.ToLowerInvariant().GetHashCode() : 0) * 397 ^ ((this.Id != null) ? this.Id.ToLowerInvariant().GetHashCode() : 0)) * 397 ^ this.IdAsLong.GetHashCode()) * 397 ^ this.MainVersion) * 397 ^ this.PatchVersion) * 397 ^ ((this.ProcessClass != null) ? this.ProcessClass.ToLowerInvariant().GetHashCode() : 0)) * 397 ^ this.SubVersion;
		}
	}
}
