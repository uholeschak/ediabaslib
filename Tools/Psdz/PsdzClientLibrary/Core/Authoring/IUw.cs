using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface IUw : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string UW_EINH { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        long UW_NR { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string UW_NAME { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        UwTyp UW_TYP { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        string UW_TEXT { get; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        T UW_WERT_getAs<T>();
    }

}
