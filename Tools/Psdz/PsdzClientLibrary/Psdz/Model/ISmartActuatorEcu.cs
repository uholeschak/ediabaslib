using PsdzClient.Core;
using PsdzClient.Programming;

namespace BMW.Rheingold.Psdz.Model.Ecu
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISmartActuatorEcu : IEcuObj
    {
        int? SmacMasterDiagAddressAsInt { get; }

        string SmacID { get; }
    }
}