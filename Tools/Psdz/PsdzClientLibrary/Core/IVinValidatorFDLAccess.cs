using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.ISPI.TRIC.ISTA.Contracts.Implementations.Models;
using BMW.ISPI.TRIC.ISTA.Contracts.Models;

namespace BMW.ISPI.ISTA.Contracts.Interfaces.VinValidator
{
    public interface IVinValidatorFDLAccess
    {
        BackendData<VinValidationTypeKeyResult> GetTypeKeys(string vin17, Language language);
    }
}
