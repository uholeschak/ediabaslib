using System.Collections.Generic;
using BMW.Rheingold.CoreFramework.Contracts.Programming;

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
