using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
	[Serializable]
	public class ClientDefinition : ICloneable
	{
		public bool AccessSiFa
		{
			get
			{
				return this.accessSiFa;
			}
			set
			{
				this.accessSiFa = value;
			}
		}

		public string BrandAuthorization
		{
			get
			{
				return this.brandAuthorization;
			}
			set
			{
				this.brandAuthorization = value;
			}
		}

		public long[] BrandIds
		{
			get
			{
				return this.brandIds;
			}
			set
			{
				this.brandIds = value;
			}
		}

		public DateTime ClientDate
		{
			get
			{
				return this.clientDate;
			}
			set
			{
				this.clientDate = value;
			}
		}

		public string Country
		{
			get
			{
				return this.country;
			}
			set
			{
				this.country = value;
			}
		}

		public long CountryId
		{
			get
			{
				return this.countryId;
			}
			set
			{
				this.countryId = value;
			}
		}

		public string ProductType
		{
			get
			{
				return this.productType;
			}
			set
			{
				this.productType = value;
			}
		}

		public object Clone()
		{
			ClientDefinition clientDefinition = new ClientDefinition();
			if (this.country != null)
			{
				clientDefinition.country = string.Copy(this.country);
			}
			clientDefinition.countryId = this.countryId;
			clientDefinition.clientDate = this.clientDate;
			clientDefinition.accessSiFa = this.accessSiFa;
			if (this.brandAuthorization != null)
			{
				clientDefinition.brandAuthorization = string.Copy(this.brandAuthorization);
			}
			if (this.brandIds != null)
			{
				clientDefinition.brandIds = (long[])this.brandIds.Clone();
			}
			if (this.productType != null)
			{
				clientDefinition.productType = string.Copy(this.productType);
			}
			return clientDefinition;
		}

		private bool accessSiFa;

		private string brandAuthorization;

		private long[] brandIds;

		private DateTime clientDate;

		private string country;

		private long countryId;

		private string productType;
	}
}
