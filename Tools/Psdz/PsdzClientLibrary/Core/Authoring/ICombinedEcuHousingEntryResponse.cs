using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Programming.API.Models.Interfaces
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ICombinedEcuHousingEntryResponse
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        int[] RequiredEcuAddresses { get; }
    }
}
