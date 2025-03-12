using System.Runtime.Serialization;

namespace PsdzClient.Core
{
    [DataContract]
    public class InteractionMotorcycleMRMA24Model : InteractionModel, IInteractionMotorcycleMRMA24Model
    {
        public InteractionMotorcycleMRMA24Model()
        {
            base.DialogSize = 1;
        }
    }
}
