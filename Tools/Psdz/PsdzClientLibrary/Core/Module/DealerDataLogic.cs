using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using PsdzClient;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public class DealerDataLogic : IDealerData
    {
        public const string FallbackBuNo = "XXXXX";

        private DateTime expirationDate;

        private string distributionPartnerNumber;

        private string distributionPartnerName;

        private bool hasDistributionPartner;

        private IDictionary<BrandName, bool> dealerBrands;

        private IDealerAddress dealerAddress;

        internal DateTime ExpirationDate => expirationDate;

        public string DistributionPartnerNumber => distributionPartnerNumber;

        internal string DistributionPartnerName => distributionPartnerName;

        internal bool HasDistributionPartner => hasDistributionPartner;

        internal bool HasOutlet
        {
            get
            {
                return false;
            }
        }

        public string OutletNumber => string.Empty;

        public string OutletCountry => string.Empty;

        public IDictionary<BrandName, bool> DealerBrands
        {
            get
            {
                if (dealerBrands == null)
                {
                    dealerBrands = new Dictionary<BrandName, bool>();
                }
                return dealerBrands;
            }
            set
            {
                dealerBrands = value;
            }
        }

        public UiBrand AvailableCombinedBrand { get; private set; }

        public IDealerAddress DealerAddress
        {
            get
            {
                return dealerAddress;
            }
        }

        internal DealerDataLogic(string dealerMasterDataString)
        {
        }

        public string GetDealerNumber(BrandName? vehicleBrandName)
        {
            Log.Warning(Log.CurrentMethod(), "Default (XXXXX) dealer number will be used.");
            return "XXXXX";
        }

        internal bool HasProtectionVehicleService()
        {
            return false;
        }

        internal bool HasProtectionVehicleService(BrandName brandName, string salesBranch)
        {
            return false;
        }

        private bool HasLicenseForBrandInternal(BrandName? brandName)
        {
            return false;
        }

        bool IDealerData.HasLicenseForBrand(BrandName? brandName)
        {
            if (!brandName.HasValue)
            {
                return false;
            }
            return DealerBrands[brandName.Value];
        }

        internal ISet<string> RetrieveInternationalDealerNumbers()
        {
            ISet<string> set = new HashSet<string>();
            return set;
        }

        private void AddAddressPartIfExists(IList<string> address, string part)
        {
            if (!string.IsNullOrEmpty(part))
            {
                address.Add(part);
            }
        }

        private void CheckIfCombinedBrandOptionIsAvailable()
        {
            if (DealerBrands[BrandName.BMWPKW] && DealerBrands[BrandName.MINIPKW] && DealerBrands[BrandName.BMWi])
            {
                AvailableCombinedBrand = UiBrand.BMWBMWiMINI;
            }
            else if (DealerBrands[BrandName.BMWPKW] && DealerBrands[BrandName.MINIPKW])
            {
                AvailableCombinedBrand = UiBrand.BMWMINI;
            }
            else if (DealerBrands[BrandName.BMWPKW] && DealerBrands[BrandName.BMWi])
            {
                AvailableCombinedBrand = UiBrand.BMWBMWi;
            }
            else if (DealerBrands[BrandName.BMWi] && DealerBrands[BrandName.MINIPKW])
            {
                AvailableCombinedBrand = UiBrand.BMWiMINI;
            }
            else
            {
                AvailableCombinedBrand = UiBrand.Unknown;
            }
        }
    }
}
