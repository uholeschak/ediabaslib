using PsdzClient.Core;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.API.TVV
{
    [AuthorAPI(SelectableTypeDeclaration = false)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ITvvDataHandler : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        ITvvData GetTvvDataFromBackend();
    }
}
