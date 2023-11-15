namespace PsdzClientLibrary.Core
{
    public interface IOperationServices
    {
        INavigationService NavigationService { get; }

        IInteractionService InteractionService { get; }
    }
}
