using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    [DataContract]
    public class InteractionMotorcycleMRMA24Model : InteractionRequestModel<InteractionButtonResponse>, IInteractionMotorcycleMRMA24Model
    {
        public override void OnResponseReceived(InteractionButtonResponse response)
        {
            Dispose();
        }
    }
}