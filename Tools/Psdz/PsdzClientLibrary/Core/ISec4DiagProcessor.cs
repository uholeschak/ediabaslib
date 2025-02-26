namespace PsdzClientLibrary.Core
{
    public interface ISec4DiagProcessor
    {
        WebCallResponse<Sec4DiagResponseData> SendDataToBackend(Sec4DiagRequestData data, BackendServiceType backendServiceType, string accessToken);
    }
}