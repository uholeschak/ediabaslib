using PsdzClient.Core;
using System.ComponentModel;
using BMW.Authoring.Programming.API.Models.Interfaces;
using BMW.Rheingold.CoreFramework;

namespace BMW.Authoring.Programming.API.Interface
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ICombinedEcuHousing
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ICombinedEcuHousingEntryResponse[] GetAllCombinedEcuHousingAddresses();

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        ICombinedEcuHousingEntryResponse GetCombinedEcuHousingAddresses(int ecuAddress);
    }
}
