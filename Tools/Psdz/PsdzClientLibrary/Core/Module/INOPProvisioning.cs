using PsdzClient.Core;

namespace BMW.Rheingold.ISTA.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface INOPProvisioning
    {
        string[] AcknowledgeNOPProvisioningDownload(string s_DATA_VIN, string s_DATA_DOWNLOAD_ID, string s_DATA_RESULT_TEXT);

        string GetNOPProvisioningDataForVehicle(string s_PARAMETER_VIN, string s_PARAMETER_VERSION, string s_PARAMETER_SMNC, string s_PARAMETER_SMCC, string s_PARAMETER_NMNC, string s_PARAMETER_NMCC, string s_PARAMETER_OTA_ID, string s_PARAMETER_DAS_ID, string s_PARAMETER_CAUSE, bool b_PARAMETER_DPAS, bool b_PARAMETER_DPAS_Specified, string s_PARAMETER_ICC_ID, string s_PARAMETER_EU_ICC_ID, string s_PARAMETER_IMEI, string s_PARAMETER_SERIAL_NUMBER, bool signature, out string[] errors);
    }
}
