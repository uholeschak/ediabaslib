using BMW.ISPI.TRIC.ISTA.Contracts.Implementations.Models;
using BMW.ISPI.TRIC.ISTA.Contracts.Models;

namespace BMW.ISPI.TRIC.ISTA.Contracts.Interfaces.VinValidator
{
    public interface IVinValidatorSVMDAccess
    {
        BackendData<VinValidatorSVMDRequestVINResponse> GetPossibleVIN17AndTypeKeys(string vin7);
    }
}
