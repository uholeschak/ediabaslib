using BMW.ISPI.TRIC.ISTA.Contracts.Interfaces;
using System.Collections.Generic;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.VinValidator
{
    public interface IVinValidatorDataAccess
    {
        IEnumerable<string> GetAllMatchingTypSchluessels(string vin);

        IList<IXepCharacteristics> GetVehicleIdentByTypeKey(string typeKey, bool isAlpina = false);
    }
}
