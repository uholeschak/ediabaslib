using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface ISvk : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        DateTime Programmierdatum { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int ProgTest { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<ISvkEinheit> GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Always)]
        Version Svk_getVersion(SvkProzessklasse Prozessklasse, string Identifier);
    }
}
