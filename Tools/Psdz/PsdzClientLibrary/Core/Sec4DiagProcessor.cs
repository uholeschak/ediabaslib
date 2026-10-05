using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.Rheingold.CoreFramework.Sec4Diag;
using BMW.Rheingold.InfoProvider;
using BMW.Rheingold.InfoProvider.Sec4Diag;
using BMW.Rheingold.InfoProvider.Sec4Diag.Models;

namespace BMW.Rheingold.InfoProvider.Sec4Diag
{
    public class Sec4DiagProcessor : ISec4DiagProcessor
    {
        private readonly ISec4DiagProcessorImpl sec4DiagProcessImpl;
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