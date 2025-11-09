using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
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