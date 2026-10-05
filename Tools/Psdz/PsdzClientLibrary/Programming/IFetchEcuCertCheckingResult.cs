using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IFetchEcuCertCheckingResult
    {
        IEnumerable<IEcuFailureResponse> FailedEcus { get; }

        IEnumerable<IEcuCertCheckingResponse> Results { get; }
    }
}