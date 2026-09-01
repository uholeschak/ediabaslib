using BMW.Rheingold.CoreFramework.Contracts;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework
{
    public interface IOperationServices
    {
        INavigationService NavigationService { get; }

        IInteractionService InteractionService { get; }
    }
}
