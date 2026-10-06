using BMW.Authoring;
using System.Collections.Generic;
using System.ComponentModel;
using BMW.Rheingold.CoreFramework;

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
