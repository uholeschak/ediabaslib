using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.CoreFramework.Programming.Data.Ecu
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISmartActuatorEcu : IEcuObj
    {
        int? SmacMasterDiagAddressAsInt { get; }

        string SmacID { get; }
    }
}