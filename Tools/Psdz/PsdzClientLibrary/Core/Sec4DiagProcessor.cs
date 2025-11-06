namespace PsdzClient.Core
{
    public class Sec4DiagProcessor : ISec4DiagProcessor
    {
        private ISec4DiagProcessorImpl sec4DiagProcessImpl;
        public Sec4DiagProcessor(ISec4DiagProcessorImpl sec4DiagProcessorImpl)
        {
            sec4DiagProcessImpl = sec4DiagProcessorImpl;
        }

        public WebCallResponse<Sec4DiagResponseData> SendDataToBackend(Sec4DiagRequestData data, BackendServiceType backendServiceType, string accessToken)
        {
            return sec4DiagProcessImpl.SendDataToBackend(data, backendServiceType, accessToken);
        }

        public WebCallResponse<bool> GetCertReqProfil(BackendServiceType backendServiceType, string accessToken)
        {
            return sec4DiagProcessImpl.GetCertReqProfil(backendServiceType, accessToken);
        }
    }
}