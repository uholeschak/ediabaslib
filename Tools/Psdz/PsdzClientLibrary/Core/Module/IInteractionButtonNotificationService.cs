using PsdzClient.Core;
using System;

namespace BMW.Rheingold.CoreFramework.Interaction
{
    public interface IInteractionButtonNotificationService
    {
        bool NotifyResponse(Guid modelId, InteractionResponse response);

        string GetInteractionModelType(Guid modelId);

        void NotifyClosing(Guid modelId);
    }
}
