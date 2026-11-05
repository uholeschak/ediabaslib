using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.ComponentModel;

namespace BMW.Authoring.Session.Interface
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDiagCodeList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        bool DiagCode_IsSet(string code);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IDiagCode GetByCode(string code);
    }
}
