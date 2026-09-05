using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class MeldungNeuModel : ServiceDialogModelBase
    {
        private string txtParamFlow;

        private string wertFeldFlow;

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

        public string WertFeldFlow
        {
            get
            {
                return wertFeldFlow;
            }
            set
            {
                if (!object.Equals(wertFeldFlow, value))
                {
                    wertFeldFlow = value;
                    OnPropertyChanged("WertFeldFlow");
                }
            }
        }
    }
}
