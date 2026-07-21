using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface ITechnicalAction
    {
        string SpecialDefectCode { get; }

        string Description { get; }

        TechnicalActionState TechnicalActionState { get; }

        TechnicalActionRecallType TechnicalActionRecallType { get; }

        bool IsSalesStop { get; }

        bool IsSoftwareCampaign { get; }

        string[] SoftwareVersions { get; }
    }
}
