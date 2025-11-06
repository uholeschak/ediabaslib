namespace PsdzClient.Core
{
    public interface ISec4DiagProcessorImpl
    {
        WebCallResponse<Sec4DiagResponseData> SendDataToBackend(Sec4DiagRequestData data, BackendServiceType backendServiceType, string accessToken);

        WebCallResponse<bool> GetCertReqProfil(BackendServiceType backendServiceType, string accessToken);
    }
}