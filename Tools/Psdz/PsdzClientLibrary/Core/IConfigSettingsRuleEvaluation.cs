using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;

namespace PsdzClientLibrary.Core
{
    public interface IConfigSettingsRuleEvaluation
    {
        IEnumerable<BrandName> SelectedBrand { get; }
    }
}