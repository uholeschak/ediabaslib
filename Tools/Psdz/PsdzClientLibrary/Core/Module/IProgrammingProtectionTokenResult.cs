using PsdzClient.Core;
using PsdzClient.Programming;
using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.AutomotiveSecurity
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IProgrammingProtectionTokenResult
    {
        ICollection<IEcuIdentifier> TalProgrammingProtectionEcus { get; }

        ICollection<string> ErrorCauses { get; }

        ICollection<IEcuFailureResponse> FailureEcus { get; }
    }
}
