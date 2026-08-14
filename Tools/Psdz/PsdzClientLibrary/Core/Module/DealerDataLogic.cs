using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer
{
    public class DealerDataLogic : IDealerData
    {
        public const string FallbackBuNo = "XXXXX";
        private readonly DealerMasterData dealerMasterData;
        private DateTime expirationDate;
        private string distributionPartnerNumber;
        private string distributionPartnerName;
        private bool hasDistributionPartner;
        private IList<Outlet> outlets;
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
                if (outlets != null)
                {
                    return outlets.Count > 0;
                }

                return false;
            }
        }

        internal Outlet FirstOutlet
        {
            get
            {
                if (!HasOutlet)
                {
                    return null;
                }

                return outlets[0];
            }
        }

        public string OutletNumber => FormatConverterBase.FillWithZeros(FirstOutlet?.outletNumber, 2, new NugetLogger());
        public string OutletCountry => FirstOutlet?.address?.country;

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
                if (dealerAddress == null)
                {
                //[-] dealerAddress = new DealerAddress(FirstOutlet);
                }

                return dealerAddress;
            }
        }

        internal DealerDataLogic(string dealerMasterDataString) : this(dealerMasterDataString, null, overwrite: false)
        {
        }

        internal DealerDataLogic(DealerMasterData dealerData, IDealerData dealer)
        {
            dealerMasterData = dealerData;
            dealerBrands = dealer.DealerBrands;
            Initialize();
        }

        internal DealerDataLogic(string dealerMasterDataString, string licenseDealerConractDataString, bool overwrite)
        {
            if (string.IsNullOrEmpty(dealerMasterDataString))
            {
                throw new ArgumentException("Param 'dealerMasterDataString' must not be null or empty!");
            }

            this.dealerMasterData = DealerMasterData.Deserialize(dealerMasterDataString);
            try
            {
                if (!string.IsNullOrEmpty(licenseDealerConractDataString))
                {
                    DealerMasterData dealerMasterData = DealerMasterData.Deserialize(licenseDealerConractDataString);
                    if (dealerMasterData != null && dealerMasterData.distributionPartner != null && dealerMasterData.expirationDate > DateTime.Now && dealerMasterData.distributionPartner.outlet != null && dealerMasterData.distributionPartner.outlet.Count > 0 && this.dealerMasterData != null && this.dealerMasterData.distributionPartner != null && this.dealerMasterData.distributionPartner.outlet != null && this.dealerMasterData.distributionPartner.outlet.Count > 0 && this.dealerMasterData.distributionPartner.outlet[0].contract != null && dealerMasterData.distributionPartner.outlet[0].contract != null)
                    {
                        if (overwrite)
                        {
                            this.dealerMasterData.distributionPartner.outlet[0].contract.Clear();
                        }

                        this.dealerMasterData.distributionPartner.outlet[0].contract.AddRange(dealerMasterData.distributionPartner.outlet[0].contract);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("DealerDataLogic.DealerDataLogic()", exception);
            }

            Initialize();
            PreLoadValidBrands();
            CheckIfCombinedBrandOptionIsAvailable();
        }

        public string GetDealerNumber(BrandName? vehicleBrandName)
        {
            string method = "DealerDataLogic.GetDealerNumber()";
            try
            {
                Contract contract = (vehicleBrandName.HasValue ? GetValidContract(OutletNumber, vehicleBrandName.Value, null) : null);
                if (contract != null)
                {
                    if (!string.IsNullOrEmpty(contract.internationalDealerNumber))
                    {
                        Log.Info(method, "International Dealer Number is {0}", contract.internationalDealerNumber);
                        return FormatConverterBase.FillWithZeros(contract.internationalDealerNumber.TrimStart('0'), 5, new NugetLogger());
                    }

                    if (!string.IsNullOrEmpty(contract.nationalDealerNumber))
                    {
                        Log.Info(method, "National Dealer Number is {0}", contract.nationalDealerNumber);
                        return FormatConverterBase.FillWithZeros(contract.nationalDealerNumber.TrimStart('0'), 5, new NugetLogger());
                    }
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException("Dealer.GetDealerNumber()", exception);
            }

            Log.Warning(Log.CurrentMethod(), "Default (XXXXX) dealer number will be used.");
            return "XXXXX";
        }

        private void Initialize()
        {
            expirationDate = dealerMasterData.expirationDate;
            if (dealerMasterData.distributionPartner != null)
            {
                hasDistributionPartner = true;
                DistributionPartner distributionPartner = dealerMasterData.distributionPartner;
                distributionPartnerNumber = distributionPartner.distributionPartnerNumber;
                distributionPartnerName = distributionPartner.name;
                outlets = distributionPartner.outlet;
            }
            else
            {
                hasDistributionPartner = false;
                Log.Warning("DealerDataLogic.Initialize()", "No element 'distributionPartner' in dealer master data found!");
            }
        //[-] if (IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA") && (string.IsNullOrEmpty(distributionPartnerNumber) || distributionPartnerNumber == "0"))
        //[-] {
        //[-] distributionPartnerNumber = "-1";
        //[-] }
        }

        private bool IsValidServiceContract(Contract contract)
        {
            if (contract.businessLine == BusinessLine.Service)
            {
                bool num = contract.startDate < DateTime.Now;
                bool flag = !contract.endServiceDateSpecified || contract.endServiceDate > DateTime.Now;
                bool flag2 = !contract.endContractDateSpecified || contract.endContractDate > DateTime.Now;
                return num & flag & flag2;
            }

            return false;
        }

        internal bool HasProtectionVehicleService()
        {
            if (HasOutlet)
            {
                foreach (Outlet outlet in outlets)
                {
                    if (outlet.protectionVehicleService)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool HasProtectionVehicleService(BrandName brandName, string salesBranch)
        {
            foreach (Outlet outlet in outlets)
            {
                if (outlet.protectionVehicleService && GetValidContract(outlet.outletNumber, brandName, salesBranch) != null)
                {
                    return true;
                }
            }

            return false;
        }

        [PreserveSource(SignatureModified = true)]
        internal Contract GetValidContract(string outletNumber, BrandName brandName, string salesBranch)
        {
            //[-] return GetValidContract(outletNumber, brandName, !CoreFramework.OSSModeActive, salesBranch);
            //[+] return GetValidContract(outletNumber, brandName, true, salesBranch);
            return GetValidContract(outletNumber, brandName, true, salesBranch);
        }

        [PreserveSource(SignatureModified = true)]
        internal Contract GetValidContract(string outletNumber, BrandName brandName, bool isBmwDealer, string salesBranch)
        {
            BrandName baseBrandName = BrandMapping.GetBaseBrandName(brandName);
            Brand brand = BrandMapping.ConvertToBrand(baseBrandName);
            Product product = BrandMapping.ConvertToProduct(baseBrandName);
            Contract validServiceContract = GetValidServiceContract(outletNumber, brand, product, salesBranch);
            if (validServiceContract != null || !isBmwDealer)
            {
                return validServiceContract;
            }

            return null;
        }

        private Contract GetValidServiceContract(string outletNumber, Brand brand, Product product, string salesBranch)
        {
            try
            {
                Outlet outlet = GetOutlet(outletNumber);
                if (outlet != null)
                {
                    foreach (Contract item in outlet.contract)
                    {
                        if (item.brand == brand && item.product == product && IsValidServiceContract(item) && (string.IsNullOrEmpty(salesBranch) || item.salesBranch == null || item.salesBranch.Equals(salesBranch)))
                        {
                            return item;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("Dealer.getValidContract()", exception);
            }

            return null;
        }

        private Outlet GetOutlet(string outletNumber)
        {
            if (HasOutlet)
            {
                foreach (Outlet outlet in outlets)
                {
                    if (string.Compare(FormatConverterBase.FillWithZeros(outlet.outletNumber, 2, new NugetLogger()), FormatConverterBase.FillWithZeros(outletNumber, 2, new NugetLogger()), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return outlet;
                    }
                }
            }

            return null;
        }

        private bool HasLicenseForBrandInternal(BrandName? brandName)
        {
            //[-] if (brandName.HasValue && HasOutlet)
            //[-] {
            //[-] return GetValidContract(OutletNumber, brandName.Value, !CoreFramework.OSSModeActive, null) != null;
            //[-] }
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
            if (outlets == null)
            {
                return set;
            }

            foreach (Outlet outlet in outlets)
            {
                foreach (Contract item in outlet.contract)
                {
                    if (item.businessLine == BusinessLine.Service && !string.IsNullOrEmpty(item.internationalDealerNumber))
                    {
                        set.Add(FormatConverterBase.FillWithZeros(item.internationalDealerNumber, 6, new NugetLogger()));
                    }
                }
            }

            return set;
        }

        private void AddAddressPartIfExists(IList<string> address, string part)
        {
            if (!string.IsNullOrEmpty(part))
            {
                address.Add(part);
            }
        }

        internal IAddress OutletAddress(string outletNumber)
        {
            return GetOutlet(outletNumber)?.address;
        }

        internal string GetOutletName(string outletNumber)
        {
            return GetOutlet(outletNumber)?.name;
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

        private void PreLoadValidBrands()
        {
        //[-] foreach (BrandName baseBrandName in BrandMapping.BaseBrandNames)
        //[-] {
        //[-] DealerBrands.Add(baseBrandName, HasLicenseForBrandInternal(baseBrandName));
        //[-] }
        }
    }
}