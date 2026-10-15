using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class EnterServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string txtParamFlow;

        [DataMember]
        private string textInput;

        [DataMember]
        private int textInputMaxLegth;

        [DataMember]
        private string inputTypeHint;

        public string TxtParamFlow
        {
            get
            {
                return txtParamFlow;
            }
            set
            {
                if (!object.Equals(txtParamFlow, value))
                {
                    txtParamFlow = value;
                    OnPropertyChanged("TxtParamFlow");
                }
            }
        }

        public string TextInput
        {
            get
            {
                return textInput;
            }
            set
            {
                if (!object.Equals(textInput, value))
                {
                    textInput = value;
                    OnPropertyChanged("TextInput");
                }
            }
        }

        public int TextInputMaxLength
        {
            get
            {
                return textInputMaxLegth;
            }
            set
            {
                if (!object.Equals(textInputMaxLegth, value))
                {
                    textInputMaxLegth = value;
                    OnPropertyChanged("TextInputMaxLength");
                }
            }
        }

        public string InputTypeHint
        {
            get
            {
                return inputTypeHint;
            }
            set
            {
                if (!object.Equals(inputTypeHint, value))
                {
                    inputTypeHint = value;
                    OnPropertyChanged("InputTypeHint");
                }
            }
        }
    }
}
