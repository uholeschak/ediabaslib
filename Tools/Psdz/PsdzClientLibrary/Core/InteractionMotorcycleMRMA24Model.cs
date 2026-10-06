using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Interaction;
using BMW.Rheingold.CoreFramework.Interaction.Models;
using BMW.Rheingold.CoreFramework.Interaction.Models.Interfaces;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
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