using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public enum IdentificationLevel
    {
        None,
        BasicFeatures,
        ReopenedOperation,
        VINOnly,
        VINBasedFeatures,
        VINBasedOnlineUpdated,
        VINVehicleReadout,
        VINVehicleReadoutOnlineUpdated,
        VehicleTypeNotLicensed
    }
}