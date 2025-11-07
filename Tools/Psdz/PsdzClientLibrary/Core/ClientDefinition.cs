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
        private bool accessSiFa;
        private string brandAuthorization;
        private long[] brandIds;
        private DateTime clientDate;
        private string country;
        private long countryId;
        private string productType;
        public bool AccessSiFa
        {
            get
            {
                return accessSiFa;
            }

            set
            {
                accessSiFa = value;
            }
        }

        public string BrandAuthorization
        {
            get
            {
                return brandAuthorization;
            }

            set
            {
                brandAuthorization = value;
            }
        }

        public long[] BrandIds
        {
            get
            {
                return brandIds;
            }

            set
            {
                brandIds = value;
            }
        }

        public DateTime ClientDate
        {
            get
            {
                return clientDate;
            }

            set
            {
                clientDate = value;
            }
        }

        public string Country
        {
            get
            {
                return country;
            }

            set
            {
                country = value;
            }
        }

        public long CountryId
        {
            get
            {
                return countryId;
            }

            set
            {
                countryId = value;
            }
        }

        public string ProductType
        {
            get
            {
                return productType;
            }

            set
            {
                productType = value;
            }
        }

        public object Clone()
        {
            ClientDefinition clientDefinition = new ClientDefinition();
            if (country != null)
            {
                clientDefinition.country = string.Copy(country);
            }

            clientDefinition.countryId = countryId;
            clientDefinition.clientDate = clientDate;
            clientDefinition.accessSiFa = accessSiFa;
            if (brandAuthorization != null)
            {
                clientDefinition.brandAuthorization = string.Copy(brandAuthorization);
            }

            if (brandIds != null)
            {
                clientDefinition.brandIds = (long[])brandIds.Clone();
            }

            if (productType != null)
            {
                clientDefinition.productType = string.Copy(productType);
            }

            return clientDefinition;
        }
    }
}