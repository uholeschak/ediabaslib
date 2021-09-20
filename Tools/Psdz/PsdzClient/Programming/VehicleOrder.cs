using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
	public class VehicleOrder : IFa
	{
		public IList<string> EWords { get; set; }

		public string Entwicklungsbaureihe { get; set; }

		public int FaVersion { get; set; }

		public IList<string> HOWords { get; set; }

		public string Lackcode { get; set; }

		public string Polstercode { get; set; }

		public IList<string> Salapas { get; set; }

		public string Type { get; set; }

		public string Zeitkriterium { get; set; }

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}#{1}*{2}%{3}&{4}{5}{6}{7}", new object[]
			{
				this.Entwicklungsbaureihe,
				this.Zeitkriterium,
				this.Type,
				this.Lackcode,
				this.Polstercode,
				ConcatStrElems(this.Salapas, "$"),
				ConcatStrElems(this.EWords, "-"),
				ConcatStrElems(this.HOWords, "+")
			});
		}

		public bool AreEqual(BMW.Rheingold.CoreFramework.Contracts.Vehicle.IFa vehicleOrder)
		{
			return this.Equals(this.Entwicklungsbaureihe, vehicleOrder.BR) && this.Equals(this.Lackcode, vehicleOrder.LACK) && this.Equals(this.Polstercode, vehicleOrder.POLSTER) && this.Equals(this.Type, vehicleOrder.TYPE) && this.Equals(this.Zeitkriterium, vehicleOrder.C_DATE) && this.Equals(this.Salapas, vehicleOrder.SA) && this.Equals(this.EWords, vehicleOrder.E_WORT) && this.Equals(this.HOWords, vehicleOrder.HO_WORT);
		}

		public IFa Clone()
		{
			return new VehicleOrder
			{
				Entwicklungsbaureihe = this.Entwicklungsbaureihe,
				EWords = new List<string>(this.EWords),
				FaVersion = this.FaVersion,
				HOWords = new List<string>(this.HOWords),
				Lackcode = this.Lackcode,
				Polstercode = this.Polstercode,
				Salapas = new List<string>(this.Salapas),
				Type = this.Type,
				Zeitkriterium = this.Zeitkriterium
			};
		}

		private bool Equals(string a, string b)
		{
			if (a == null)
			{
				return b == null;
			}
			return a.Equals(b);
		}

		private bool Equals(IEnumerable<string> a, IEnumerable<string> b)
		{
			if (a == null)
			{
				return b == null;
			}
			if (b == null)
			{
				return false;
			}
			List<string> list = new List<string>(a);
			int num = 0;
			foreach (string item in b)
			{
				if (!list.Contains(item))
				{
					return false;
				}
				num++;
			}
			return num == list.Count;
		}

        public static string ConcatStrElems(IEnumerable<string> elems, string sep)
        {
            if (elems != null && elems.Any<string>())
            {
                string text = new List<string>(elems).Aggregate((string intermediate, string elem) => intermediate + sep + elem);
                if (!string.IsNullOrEmpty(text))
                {
                    text = sep + text;
                }
                return text;
            }
            return string.Empty;
        }
    }
}
