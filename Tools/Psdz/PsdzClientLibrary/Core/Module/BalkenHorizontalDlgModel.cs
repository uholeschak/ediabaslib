using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class BalkenHorizontalDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string txtObereTextbox;

        [DataMember]
        private string txtUntereTextbox;

        [DataMember]
        private ObservableCollection<BalkenHorizontalControlModel> balken;

        public ObservableCollection<BalkenHorizontalControlModel> Balken
        {
            get
            {
                return balken;
            }
            set
            {
                if (!object.Equals(balken, value))
                {
                    balken = value;
                    OnPropertyChanged("Balken");
                }
            }
        }

        public string TxtObereTextbox
        {
            get
            {
                return txtObereTextbox;
            }
            set
            {
                if (!object.Equals(txtObereTextbox, value))
                {
                    txtObereTextbox = value;
                    OnPropertyChanged("TxtObereTextbox");
                }
            }
        }

        public string TxtUntereTextbox
        {
            get
            {
                return txtUntereTextbox;
            }
            set
            {
                if (!object.Equals(txtUntereTextbox, value))
                {
                    txtUntereTextbox = value;
                    OnPropertyChanged("TxtUntereTextbox");
                }
            }
        }

        public BalkenHorizontalDlgModel()
        {
            TxtObereTextbox = string.Empty;
            TxtUntereTextbox = string.Empty;
            Balken = new ObservableCollection<BalkenHorizontalControlModel>();
        }

        internal void SetValues(IList<string> lang, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam)
        {
            ITextLocator textLocator = inParam.getParameter("i_ObereTextbox", null) as ITextLocator;
            ITextLocator textLocator2 = inParam.getParameter("i_UntereTextbox", null) as ITextLocator;
            TxtObereTextbox = textLocator?.TextContent.GetTextForUI(lang)[0].TextItem;
            TxtUntereTextbox = textLocator2?.TextContent.GetTextForUI(lang)[0].TextItem;
            if (Balken.Count.Equals(1))
            {
                Balken.First().SetValues(lang, inParam, outParam, inoutParam, "");
                return;
            }
            for (int i = 0; i < Balken.Count; i++)
            {
                Balken[i].SetValues(lang, inParam, outParam, inoutParam, (i + 1).ToString());
            }
        }
    }
}
