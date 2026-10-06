using BMW.Authoring;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IFscList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<IFsc> GetEnumerator();
    }
}
