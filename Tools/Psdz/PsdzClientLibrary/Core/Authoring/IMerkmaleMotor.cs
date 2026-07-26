using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IMerkmaleMotor : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Nr8stellig { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Nr3stellig { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Einbaulage { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Hubraum { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Kraftstoffart { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string KraftstoffartEinbaulage { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Leistungsklasse { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string Überarbeitung { get; }
    }
}
