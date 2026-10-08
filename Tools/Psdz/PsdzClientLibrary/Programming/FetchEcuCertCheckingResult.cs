using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class FetchEcuCertCheckingResult : IFetchEcuCertCheckingResult
    {
        public IEnumerable<IEcuFailureResponse> FailedEcus { get; internal set; }

        public IEnumerable<IEcuCertCheckingResponse> Results { get; internal set; }
    }
}
