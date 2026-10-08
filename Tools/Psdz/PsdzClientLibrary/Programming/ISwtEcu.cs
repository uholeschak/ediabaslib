using System.Collections.Generic;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISwtEcu
    {
        IEcuIdentifier EcuIdentifier { get; }

        RootCertificateState RootCertificateState { get; }

        SoftwareSigState SoftwareSigState { get; }

        IEnumerable<ISwtApplication> SwtApplications { get; }
    }
}
