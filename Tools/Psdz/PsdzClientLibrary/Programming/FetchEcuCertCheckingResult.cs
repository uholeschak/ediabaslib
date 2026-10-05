using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.API
{
    internal class FetchEcuCertCheckingResult : IFetchEcuCertCheckingResult
    {
        public IEnumerable<IEcuFailureResponse> FailedEcus { get; internal set; }

        public IEnumerable<IEcuCertCheckingResponse> Results { get; internal set; }
    }
}
