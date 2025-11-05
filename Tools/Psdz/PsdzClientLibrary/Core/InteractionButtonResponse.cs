using System.Runtime.Serialization;

namespace PsdzClient.Core
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