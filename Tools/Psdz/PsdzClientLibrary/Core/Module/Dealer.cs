
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public class Dealer : IDealer
    {
        public BMW.Rheingold.CoreFramework.DatabaseProvider.Outlet FirstOutlet
        {
            get
            {
                if (dealerDataLogic == null)
                {
                    return null;
                }
                return dealerDataLogic.FirstOutlet;
            }
        }

        private DealerDataLogic dealerDataLogic;

        public IDealerData DealerData => dealerDataLogic;

        public string OutletCountry => DealerData?.OutletCountry;

        public bool HasLicenseForBrand(BrandName? brandName)
        {
            if (DealerData != null)
            {
                return DealerData.HasLicenseForBrand(brandName);
            }
            return false;
        }

        public bool HasOutlet()
        {
            if (dealerDataLogic != null)
            {
                return dealerDataLogic.HasOutlet;
            }
            return false;
        }

        public bool HasProtectionVehicleService(BrandName brandName)
        {
            if (dealerDataLogic != null)
            {
                return dealerDataLogic.HasProtectionVehicleService();
            }
            return false;
        }
    }
}
