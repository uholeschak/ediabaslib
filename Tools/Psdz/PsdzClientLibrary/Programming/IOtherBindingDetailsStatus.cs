using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Programming;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Programming
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IOtherBindingDetailsStatus
    {
        EcuCertCheckingStatus? OtherBindingStatus { get; }

        string RollenName { get; }

        string EcuName { get; }
    }
}