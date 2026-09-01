using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionDoIpCheckModel : InteractionRequestModel<InteractionButtonResponse>, IInteractionDoIpCheckModel, IInteractionModel, INotifyPropertyChanged
    {
        [DataMember]
        public int SourceOperationPid { get; set; }

        public InteractionDoIpCheckModel(int pid)
        {
            SourceOperationPid = pid;
        }

        public override void OnResponseReceived(InteractionButtonResponse response)
        {
            Log.Info("InteractionQuestionModel.OnResponseRecived()", "InteractionButtonResponse was set to the model. Parameter: Button:'{0}'.", response?.Action);
            NotifyAboutResponseReceived();
            Dispose();
        }
    }
}
