using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	[DataContract]
	public class PsdzStandardFa : IPsdzStandardFa
	{
		[DataMember]
		public string AsString { get; set; }

		[DataMember]
		public IEnumerable<string> EWords { get; set; }

		[DataMember]
		public string Entwicklungsbaureihe { get; set; }

		[DataMember]
		public int FaVersion { get; set; }

		[DataMember]
		public IEnumerable<string> HOWords { get; set; }

		[DataMember]
		public bool IsValid { get; set; }

		[DataMember]
		public string Lackcode { get; set; }

		[DataMember]
		public string Polstercode { get; set; }

		[DataMember]
		public IEnumerable<string> Salapas { get; set; }

		[DataMember]
		public string Type { get; set; }

		[DataMember]
		public string Zeitkriterium { get; set; }

		public override bool Equals(object obj)
		{
			PsdzStandardFa psdzStandardFa = obj as PsdzStandardFa;
			return psdzStandardFa != null && (this.FaVersion == psdzStandardFa.FaVersion && string.Equals(this.Entwicklungsbaureihe, psdzStandardFa.Entwicklungsbaureihe) && string.Equals(this.Type, psdzStandardFa.Type) && string.Equals(this.Zeitkriterium, psdzStandardFa.Zeitkriterium) && this.IsValid.Equals(psdzStandardFa.IsValid) && string.Equals(this.Lackcode, psdzStandardFa.Lackcode) && string.Equals(this.Polstercode, psdzStandardFa.Polstercode) && PsdzStandardFa.StringSequenceEqual(this.EWords, psdzStandardFa.EWords) && PsdzStandardFa.StringSequenceEqual(this.HOWords, psdzStandardFa.HOWords)) && PsdzStandardFa.StringSequenceEqual(this.Salapas, psdzStandardFa.Salapas);
		}

		public override int GetHashCode()
		{
			int num = ((((((this.FaVersion.GetHashCode() * 397 ^ ((this.Entwicklungsbaureihe != null) ? this.Entwicklungsbaureihe.GetHashCode() : 0)) * 397 ^ ((this.Type != null) ? this.Type.GetHashCode() : 0)) * 397 ^ ((this.Zeitkriterium != null) ? this.Zeitkriterium.GetHashCode() : 0)) * 397 ^ this.IsValid.GetHashCode()) * 397 ^ ((this.Lackcode != null) ? this.Lackcode.GetHashCode() : 0)) * 397 ^ ((this.Polstercode != null) ? this.Polstercode.GetHashCode() : 0)) * 397;
			int num2;
			if (this.EWords == null)
			{
				num2 = 0;
			}
			else
			{
				num2 = (from x in this.EWords
						orderby x
						select x).Aggregate(17, (int current, string sgbmId) => current * 397 ^ sgbmId.ToLowerInvariant().GetHashCode());
			}
			int num3 = (num ^ num2) * 397;
			int num4;
			if (this.HOWords == null)
			{
				num4 = 0;
			}
			else
			{
				num4 = (from x in this.HOWords
						orderby x
						select x).Aggregate(17, (int current, string sgbmId) => current * 397 ^ sgbmId.ToLowerInvariant().GetHashCode());
			}
			int num5 = (num3 ^ num4) * 397;
			int num6;
			if (this.Salapas == null)
			{
				num6 = 0;
			}
			else
			{
				num6 = (from x in this.Salapas
						orderby x
						select x).Aggregate(17, (int current, string sgbmId) => current * 397 ^ sgbmId.ToLowerInvariant().GetHashCode());
			}
			return num5 ^ num6;
		}

		private static bool StringSequenceEqual(IEnumerable<string> first, IEnumerable<string> second)
		{
			if (first == null)
			{
				return second == null;
			}
			if (second != null)
			{
				return (from x in first
						orderby x
						select x).SequenceEqual(from x in second
												orderby x
												select x, StringComparer.OrdinalIgnoreCase);
			}
			return false;
		}
	}
}
