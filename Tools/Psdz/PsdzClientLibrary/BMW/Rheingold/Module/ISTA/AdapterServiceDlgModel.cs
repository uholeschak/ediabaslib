using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class AdapterServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string text;

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (!object.Equals(text, value))
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }
    }
}
