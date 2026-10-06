using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Interaction;
using PsdzClient.Core;

namespace BMW.Rheingold.CoreFramework.Interaction
{
    [DataContract]
    public class InteractionButtonResponse : InteractionResponse
    {
        [DataMember]
        public InteractionButton Action { get; protected set; }

        public InteractionButtonResponse()
        {
            Action = InteractionButton.NoAction;
        }

        public InteractionButtonResponse(InteractionButton action)
        {
            Action = action;
        }
    }
}