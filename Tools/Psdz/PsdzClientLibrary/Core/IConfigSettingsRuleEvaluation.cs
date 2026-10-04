using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces
{
    public interface IConfigSettingsRuleEvaluation
    {
        IEnumerable<BrandName> SelectedBrand { get; }
    }
}