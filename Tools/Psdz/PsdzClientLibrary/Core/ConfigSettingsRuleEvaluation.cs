using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    public class ConfigSettingsRuleEvaluation : IConfigSettingsRuleEvaluation
    {
        public IEnumerable<BrandName> SelectedBrand { get; set; }

        public ConfigSettingsRuleEvaluation(UiBrand selectedBrand)
        {
            List<BrandName> list = new List<BrandName>();
            switch (selectedBrand)
            {
                case UiBrand.BMWBMWiMINI:
                    list.Add(BrandName.BMWPKW);
                    list.Add(BrandName.BMWi);
                    list.Add(BrandName.MINIPKW);
                    break;
                case UiBrand.BMWBMWi:
                    list.Add(BrandName.BMWPKW);
                    list.Add(BrandName.BMWi);
                    break;
                case UiBrand.BMWiMINI:
                    list.Add(BrandName.BMWi);
                    list.Add(BrandName.MINIPKW);
                    break;
                case UiBrand.BMWMINI:
                    list.Add(BrandName.MINIPKW);
                    break;
                case UiBrand.BMWPKW:
                    list.Add(BrandName.BMWPKW);
                    break;
                case UiBrand.Mini:
                    list.Add(BrandName.MINIPKW);
                    break;
                case UiBrand.RollsRoyce:
                    list.Add(BrandName.ROLLSROYCEPKW);
                    break;
                case UiBrand.BMWMotorrad:
                    list.Add(BrandName.BMWMOTORRAD);
                    break;
                case UiBrand.TOYOTA:
                    list.Add(BrandName.TOYOTA);
                    break;
                case UiBrand.BMWi:
                    list.Add(BrandName.BMWi);
                    break;
            }
            SelectedBrand = list;
        }
    }
}