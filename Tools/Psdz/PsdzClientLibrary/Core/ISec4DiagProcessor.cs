using BMW.ISPI.TRIC.ISTA.Contracts.Enums;
using BMW.Rheingold.CoreFramework.Sec4Diag;
using BMW.Rheingold.InfoProvider.Sec4Diag.Models;
using PsdzClient.Core;

namespace BMW.Rheingold.InfoProvider.Sec4Diag
{
    public interface ISec4DiagProcessor
    {
        WebCallResponse<Sec4DiagResponseData> SendDataToBackend(Sec4DiagRequestData data, BackendServiceType backendServiceType, string accessToken);

        WebCallResponse<bool> GetCertReqProfil(BackendServiceType backendServiceType, string accessToken);
    }
}