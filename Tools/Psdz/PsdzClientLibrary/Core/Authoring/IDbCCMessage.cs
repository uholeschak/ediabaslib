using BMW.Authoring;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Database
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public interface IDbCCMessage : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        long IsarID { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        int ID { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Langtext { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        List<IDbEcuVariant> EcuVariantList { get; }
    }
}
