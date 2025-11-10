using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    internal class FetchEcuCertCheckingResult : IFetchEcuCertCheckingResult
    {
        public IEnumerable<IEcuFailureResponse> FailedEcus { get; internal set; }

        public IEnumerable<IEcuCertCheckingResponse> Results { get; internal set; }
    }
}
