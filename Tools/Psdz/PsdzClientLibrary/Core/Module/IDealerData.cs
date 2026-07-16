using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    public interface IDealerData
    {
        IDealerAddress DealerAddress { get; }

        string DistributionPartnerNumber { get; }

        string OutletNumber { get; }

        string OutletCountry { get; }

        IDictionary<BrandName, bool> DealerBrands { get; set; }

        bool HasLicenseForBrand(BrandName? brandName);

        string GetDealerNumber(BrandName? brandName);
    }
}
