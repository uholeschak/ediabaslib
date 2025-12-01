using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Core;

namespace PsdzClient.Programming
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
