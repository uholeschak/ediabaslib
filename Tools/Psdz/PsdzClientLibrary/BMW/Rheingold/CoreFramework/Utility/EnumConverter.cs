using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;

namespace BMW.Rheingold.CoreFramework.Utility
{
    public static class EnumConverter
    {
        public static BrandName ConvertBrandNameToContractsBrandName(BrandName? brandName)
        {
            if (brandName.HasValue)
            {
                return (BrandName)Enum.Parse(typeof(BrandName), brandName.Value.ToString());
            }
            return BrandName.BMWPKW;
        }
    }
}
