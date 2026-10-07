using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
