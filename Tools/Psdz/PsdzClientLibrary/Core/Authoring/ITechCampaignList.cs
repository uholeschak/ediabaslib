using BMW.Authoring;
using PsdzClient.Core;
using System.Collections.Generic;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ITechCampaignList : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        int Count { get; }

        [AuthorAPIHidden]
        [EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerator<ITechCampaign> GetEnumerator();

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITechCampaign TechCampaign_GetByNr(string Sonderbefundnummer);
    }
}
