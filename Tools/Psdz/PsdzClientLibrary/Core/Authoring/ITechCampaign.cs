using BMW.Authoring;
using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public interface ITechCampaign : IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        TechCampaignType TechCampaignType { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        TechCampaignStatus TechCampaignStatus { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        ITextContent Titel { get; }

        [EditorBrowsable(EditorBrowsableState.Always)]
        string Sonderbefundnummer { get; }
    }
}
