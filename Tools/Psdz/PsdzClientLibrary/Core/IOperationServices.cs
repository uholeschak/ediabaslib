using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework
{
    public interface IOperationServices
    {
        INavigationService NavigationService { get; }

        IInteractionService InteractionService { get; }
    }
}
