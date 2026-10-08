using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    public interface IProtocolEnrichment : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void ProtocolLockingConfigurationSwitches();
    }
}
