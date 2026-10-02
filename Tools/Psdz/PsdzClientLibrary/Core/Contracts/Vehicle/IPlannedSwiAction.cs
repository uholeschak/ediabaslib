using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Contracts.Vehicle
{
    [AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IPlannedSwiAction
    {
        string SwiActionName { get; set; }

        bool IsDisabled { get; set; }
    }
}
