
using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer
{
    [PreserveSource(Hint = "No update", SuppressWarning = true)]
    public class Dealer : IDealer
    {
        private DealerDataLogic dealerDataLogic;

        public IDealerData DealerData => dealerDataLogic;

        public string OutletCountry => DealerData?.OutletCountry;
        public bool HasLicenseForBrand(BrandName? brandName)
        {
            return false;
        }
        public bool HasProtectionVehicleService(BrandName brandName)
        {
            return false;
        }
    }
}
