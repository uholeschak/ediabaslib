using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	[Serializable]
	public class EcuConfiguration : ICloneable
	{
		public IList<long> EcuCliques
		{
			get
			{
				return this.ecuCliques;
			}
			set
			{
				this.ecuCliques = value;
			}
		}

		public IList<long> EcuGroups
		{
			get
			{
				return this.ecuGroups;
			}
			set
			{
				this.ecuGroups = value;
			}
		}

		public IList<long> EcuRepresentatives
		{
			get
			{
				return this.ecuRepresentatives;
			}
			set
			{
				this.ecuRepresentatives = value;
			}
		}

		public IList<long> EcuVariants
		{
			get
			{
				return this.ecuVariants;
			}
			set
			{
				this.ecuVariants = value;
			}
		}

		public IList<long> Equipments
		{
			get
			{
				return this.equipments;
			}
			set
			{
				this.equipments = value;
			}
		}

		public long IStufe
		{
			get
			{
				return this.iStufe;
			}
			set
			{
				this.iStufe = value;
			}
		}

		public IList<long> SaLaPas
		{
			get
			{
				return this.saLaPas;
			}
			set
			{
				this.saLaPas = value;
			}
		}

		public IList<long> UnknownEcuCliques
		{
			get
			{
				return this.unknownEcuCliques;
			}
			set
			{
				this.unknownEcuCliques = value;
			}
		}

		public IList<long> UnknownEcuGroups
		{
			get
			{
				return this.unknownEcuGroups;
			}
			set
			{
				this.unknownEcuGroups = value;
			}
		}

		public IList<long> UnknownEcuRepresentatives
		{
			get
			{
				return this.unknownEcuRepresentatives;
			}
			set
			{
				this.unknownEcuRepresentatives = value;
			}
		}

		public IList<long> UnknownEcuVariants
		{
			get
			{
				return this.unknownEcuVariants;
			}
			set
			{
				this.unknownEcuVariants = value;
			}
		}

		public IList<long> UnknownEquipments
		{
			get
			{
				return this.unknownEquipments;
			}
			set
			{
				this.unknownEquipments = value;
			}
		}

		public object Clone()
		{
			return new EcuConfiguration
			{
				EcuGroups = new List<long>(this.EcuGroups),
				EcuVariants = new List<long>(this.EcuVariants),
				EcuCliques = new List<long>(this.EcuCliques),
				EcuRepresentatives = new List<long>(this.EcuRepresentatives),
				Equipments = new List<long>(this.Equipments),
				SaLaPas = new List<long>(this.SaLaPas),
				IStufe = this.IStufe,
				UnknownEcuGroups = new List<long>(this.UnknownEcuGroups),
				UnknownEcuVariants = new List<long>(this.UnknownEcuVariants),
				UnknownEcuCliques = new List<long>(this.UnknownEcuCliques),
				UnknownEcuRepresentatives = new List<long>(this.UnknownEcuRepresentatives),
				UnknownEquipments = new List<long>(this.UnknownEquipments)
			};
		}

		private IList<long> ecuCliques = new List<long>();

		private IList<long> ecuGroups = new List<long>();

		private IList<long> ecuRepresentatives = new List<long>();

		private IList<long> ecuVariants = new List<long>();

		private IList<long> equipments = new List<long>();

		private long iStufe;

		private IList<long> saLaPas = new List<long>();

		private IList<long> unknownEcuCliques = new List<long>();

		private IList<long> unknownEcuGroups = new List<long>();

		private IList<long> unknownEcuRepresentatives = new List<long>();

		private IList<long> unknownEcuVariants = new List<long>();

		private IList<long> unknownEquipments = new List<long>();
	}
}
