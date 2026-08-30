using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BMW.Rheingold.CoreFramework.Interaction.Models
{
    [DataContract]
    public class InteractionProgressModel : InteractionModel, IInteractionProgressModel, IInteractionModel, INotifyPropertyChanged
    {
        private string description;

        private bool isIndeterminate;

        private double processProgress;

        [DataMember]
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        [DataMember]
        public double ProcessProgress
        {
            get
            {
                return processProgress;
            }
            set
            {
                processProgress = ((value > 1.0) ? 0.0 : ((value < 0.0) ? 0.0 : value));
                OnPropertyChanged("ProcessProgress");
            }
        }

        [DataMember]
        public bool IsIndeterminate
        {
            get
            {
                return isIndeterminate;
            }
            set
            {
                isIndeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }

        public InteractionProgressModel()
        {
            base.Title = FormatedData.Localize("#BackgroundProcessOngoing");
            description = FormatedData.Localize("#PleaseBePatient");
            isIndeterminate = true;
            processProgress = 0.0;
            base.IsPrintButtonVisible = false;
            base.DialogSize = -1;
        }
    }
}
