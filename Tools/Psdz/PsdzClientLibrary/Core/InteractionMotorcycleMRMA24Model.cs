using System.Runtime.Serialization;

namespace PsdzClientLibrary.Core
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
