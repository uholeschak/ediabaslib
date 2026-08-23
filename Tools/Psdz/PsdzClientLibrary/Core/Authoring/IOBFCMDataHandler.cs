using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API.OBFCM
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IOBFCMDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        void SendObfcmDataToBackend(LastTransmissionStatus StatusMessage, bool PrivacyConsentOBFCM, double FuelSystem_Overall_Fuel, double FuelSystem_Overall_ReferenceDistance, double FuelSystem_InChargeDepleting_Fuel, double FuelSystem_InChargeDepleting_EngineOff_ReferenceDistance, double FuelSystem_InChargeDepleting_EngineOn_ReferenceDistance, double FuelSystem_InChargeIncreasing_Fuel, double FuelSystem_InChargeIncreasing_ReferenceDistance, double ElectricEngine_Overall_GridEnergy, double ElectricEngine_Overall_ReferenceDistance, double ElectricEngine_EngineOff_GridEnergy, double ElectricEngine_EngineOff_ReferenceDistance, double EletricEngine_EngineOn_GridEnergy, double ElectricEngine_EngineOn_ReferenceDistance);

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool OBFCMDataProtection();

        [EditorBrowsable(EditorBrowsableState.Always)]
        bool IsAOSModeActive();
    }
}
