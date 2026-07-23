using PsdzClient.Core;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IRxSwinObject
    {
        string HexEcuAdress { get; }

        List<string> RxSwinList { get; }

        bool IsMasterEcu { get; set; }

        bool IsNegativeJobStatus { get; }

        string NegativeJobResult { get; }
    }
}
