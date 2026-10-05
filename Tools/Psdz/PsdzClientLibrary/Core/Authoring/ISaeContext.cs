using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ISaeContext : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string SaeCodeString { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ITextContent SaeCodeTitel { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        SaeCodeStatus SaeCodeStatus { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void _Write_SaeCodeStatus(SaeCodeStatus newValue);

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        void ReplaceSaeCode(string newValue);
    }
}
