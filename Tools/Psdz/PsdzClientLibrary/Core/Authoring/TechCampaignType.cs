using PsdzClient.Core;
using System.ComponentModel;

namespace BMW.Authoring.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    public enum TechCampaignType
    {
        SACC,
        INFO,
        TA
    }

}
