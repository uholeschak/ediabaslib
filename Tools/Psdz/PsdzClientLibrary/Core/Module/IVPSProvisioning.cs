using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IVPSProvisioning
    {
        string VPSGetProvisioningDataForVehicle(string VIN17, string CURRENT_PROV_ID, string SMCC, string SMNC, string ECU, string HW_PU, string SW_PU, string SW_VERSION, string CAUSE, string EUICC_ID, string ICC_ID, int HMI_VERSION, string IMEI, string SERIAL_NUMBER, out string PATH, out bool STATUS, out string[] errors);
    }
}
