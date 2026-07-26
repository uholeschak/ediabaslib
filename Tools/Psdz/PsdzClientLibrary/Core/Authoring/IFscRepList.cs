using BMW.Authoring;
using BMW.Authoring.Vehicle;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IFscRepList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<IFsc> GetEnumerator();
    }
}
