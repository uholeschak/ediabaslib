using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    public interface IFetchEcuCertCheckingResult
    {
        IEnumerable<IEcuFailureResponse> FailedEcus { get; }

        IEnumerable<IEcuCertCheckingResponse> Results { get; }
    }
}
