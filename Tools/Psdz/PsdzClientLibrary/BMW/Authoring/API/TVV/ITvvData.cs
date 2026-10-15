using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.TVV
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ITvvData : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        IApiResult ApiResult { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string TvvType { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string TvvVersion { get; }
    }
}
