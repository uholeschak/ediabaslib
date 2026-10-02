using BMW.Rheingold.CoreFramework.AutomotiveSecurity;
using BMW.Rheingold.CoreFramework.Contracts;
using PsdzClient.Contracts;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ISecManagementService
    {
        IBoolResultObject<IIPsecLinkDeactivationResult> ExecuteIPsecLinkDeactivation(bool rollback);
    }
}
