using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class VehicleStateServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string mainText;

        [DataMember]
        private string valueField;

        public string MainText
        {
            get
            {
                return mainText;
            }
            set
            {
                if (!object.Equals(mainText, value))
                {
                    mainText = value;
                    OnPropertyChanged("MainText");
                }
            }
        }

        public string ValueField
        {
            get
            {
                return valueField;
            }
            set
            {
                if (!object.Equals(valueField, value))
                {
                    valueField = value;
                    OnPropertyChanged("ValueField");
                }
            }
        }
    }
}
