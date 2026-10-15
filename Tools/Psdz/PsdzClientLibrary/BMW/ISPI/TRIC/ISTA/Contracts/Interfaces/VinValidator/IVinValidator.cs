using System.Collections.Generic;
using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.ISPI.TRIC.ISTA.Contracts.Models.VinValidator;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.VinValidator
{
    public interface IVinValidator
    {
        List<TypeKeys> PossibleTypeKeys { get; set; }

        VINResolverResult ValidateVINAndSetTypeKeys(string vin, bool isVehicleConnected, Language language);
    }
}
