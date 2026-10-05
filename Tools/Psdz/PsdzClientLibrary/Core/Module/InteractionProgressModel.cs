using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.Serialization;
using BMW.Rheingold.CoreFramework.Localization;

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
                ref double reference = ref processProgress;
                double num;
                if (value > 1.0)
                {
                    num = 0.0;
                }
                else
                {
                    num = ((value < 0.0) ? 0.0 : value);
                }

                reference = num;
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
            Title = FormatedData.Localize("#BackgroundProcessOngoing");
            description = FormatedData.Localize("#PleaseBePatient");
            isIndeterminate = true;
            processProgress = 0.0;
            IsPrintButtonVisible = false;
            DialogSize = -1;
        }
    }
}