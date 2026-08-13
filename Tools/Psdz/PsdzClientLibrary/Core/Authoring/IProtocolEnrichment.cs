using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IProtocolEnrichment : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void ProtocolLockingConfigurationSwitches();
    }
}
