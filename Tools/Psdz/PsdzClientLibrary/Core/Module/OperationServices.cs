using PsdzClient.Core;
using System;
using BMW.Rheingold.CoreFramework.Contracts;

namespace BMW.Rheingold.CoreFramework
{
    public class OperationServices : IOperationServices
    {
        public INavigationService NavigationService { get; }

        public IInteractionService InteractionService { get; protected set; }

        public IInteractionController InteractionController { get; }

        public OperationServices(INavigationService navigationService, IInteractionController interactionController)
        {
            if (navigationService == null)
            {
                throw new ArgumentNullException("navigationService");
            }
            if (interactionController == null)
            {
                throw new ArgumentNullException("interactionController");
            }
            NavigationService = navigationService;
            InteractionService = new InteractionService(interactionController);
            InteractionController = interactionController;
        }
    }
}
