using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ILcSwitch : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        string Number { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string NumberText { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Value { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string ValueText { get; }
    }
}
