using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using System;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider.Dealer
{
    public abstract class BrandMapping
    {
        public static ISet<BrandName> BaseBrandNames
        {
            get
            {
                ISet<BrandName> set = new HashSet<BrandName>();
                foreach (BrandName value in Enum.GetValues(typeof(BrandName)))
                {
                    set.Add(GetBaseBrandName(value));
                }
                return set;
            }
        }

        public static UiBrand ConvertToEnumBrand(BrandName brandName)
        {
            switch (brandName)
            {
                case BrandName.BMWPKW:
                case BrandName.BMWMGmbHPKW:
                case BrandName.BMWUSAPKW:
                    return UiBrand.BMWPKW;
                case BrandName.MINIPKW:
                    return UiBrand.Mini;
                case BrandName.ROLLSROYCEPKW:
                    return UiBrand.RollsRoyce;
                case BrandName.BMWMOTORRAD:
                    return UiBrand.BMWMotorrad;
                case BrandName.BMWi:
                    return UiBrand.BMWi;
                case BrandName.TOYOTA:
                    return UiBrand.TOYOTA;
                default:
                    return UiBrand.Unknown;
            }
        }

        public static Brand ConvertToBrand(BrandName? brandName)
        {
            switch (brandName)
            {
                case BrandName.BMWi:
                    return Brand.BMWi;
                case BrandName.BMWPKW:
                case BrandName.BMWMOTORRAD:
                case BrandName.BMWMGmbHPKW:
                case BrandName.BMWUSAPKW:
                    return Brand.BMW;
                case BrandName.MINIPKW:
                    return Brand.Mini;
                case BrandName.ROLLSROYCEPKW:
                    return Brand.RollsRoyce;
                case BrandName.TOYOTA:
                    return Brand.TOYOTA;
                default:
                    return Brand.BMW;
            }
        }

        public static Product ConvertToProduct(BrandName? brandName)
        {
            if (brandName.HasValue && brandName == BrandName.BMWMOTORRAD)
            {
                return Product.Motorcycle;
            }
            return Product.Vehicle;
        }

        public static BrandName? ConvertToBrandName(Brand brand, Product product)
        {
            switch (brand)
            {
                case Brand.BMW:
                    switch (product)
                    {
                        case Product.Motorcycle:
                            return BrandName.BMWMOTORRAD;
                        case Product.Vehicle:
                            return BrandName.BMWPKW;
                    }
                    break;
                case Brand.BMWi:
                    return BrandName.BMWi;
                case Brand.Mini:
                    return BrandName.MINIPKW;
                case Brand.RollsRoyce:
                    return BrandName.ROLLSROYCEPKW;
                case Brand.TOYOTA:
                    return BrandName.TOYOTA;
                default:
                    Log.Warning("Dealer.ConvertToBrandName()", "Unknown brand: {0}", brand.ToString());
                    break;
            }
            return null;
        }

        public static BrandName GetBaseBrandName(BrandName brandName)
        {
            if ((uint)(brandName - 4) <= 1u)
            {
                return BrandName.BMWPKW;
            }
            return brandName;
        }

        public static bool IsVehicleInRange(UiBrand brand, Vehicle vecInfo)
        {
            bool flag = true;
            if (vecInfo == null || !vecInfo.BrandName.HasValue)
            {
                return !flag;
            }
            switch (brand)
            {
                case UiBrand.BMWMINI:
                    if (vecInfo.BrandName != BrandName.BMWPKW && vecInfo.BrandName != BrandName.MINIPKW)
                    {
                        flag = false;
                    }
                    break;
                case UiBrand.BMWBMWiMINI:
                    if (vecInfo.BrandName != BrandName.BMWPKW && vecInfo.BrandName != BrandName.BMWi && vecInfo.BrandName != BrandName.MINIPKW)
                    {
                        flag = false;
                    }
                    break;
                case UiBrand.BMWBMWi:
                    if (vecInfo.BrandName != BrandName.BMWPKW && vecInfo.BrandName != BrandName.BMWi)
                    {
                        flag = false;
                    }
                    break;
                case UiBrand.BMWiMINI:
                    if (vecInfo.BrandName != BrandName.BMWi && vecInfo.BrandName != BrandName.MINIPKW)
                    {
                        flag = false;
                    }
                    break;
                default:
                    if (ConvertToEnumBrand(vecInfo.BrandName.Value) != brand)
                    {
                        flag = false;
                    }
                    break;
            }
            return flag;
        }
    }
}
