using BMW.Authoring;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IVerbraucherList : IHideObjectMembers
    {
        [Obsolete("Please use the overload that uses IEnumerable<string> as input.")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        IList<IVerbraucher> Verbraucher_GetByEFuseID(IEnumerable<int> eFuseIds);

        [EditorBrowsable(EditorBrowsableState.Always)]
        IList<IVerbraucher> Verbraucher_GetByEFuseID(IEnumerable<string> eFuseIds);
    }
}
