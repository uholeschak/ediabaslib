namespace PsdzClient.Core
{
    public interface IOperationServices
    {
        INavigationService NavigationService { get; }

        IInteractionService InteractionService { get; }
    }
}
