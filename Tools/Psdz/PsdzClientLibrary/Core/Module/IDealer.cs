using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IDealer
    {
        string OutletCountry { get; }

        bool HasLicenseForBrand(BrandName? brandName);

        bool HasProtectionVehicleService(BrandName brandName);
    }
}
