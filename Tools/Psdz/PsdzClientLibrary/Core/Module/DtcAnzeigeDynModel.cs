using BMW.Authoring.Vehicle;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using PsdzClient;
using PsdzClient.Core;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class DtcAnzeigeDynModel : ServiceDialogModelBase
    {
        [DataMember]
        private string priorText;

        [DataMember]
        private string pastText;

        [DataMember]
        private ObservableCollection<DtcAnzeigeButtonModel> buttons;

        [DataMember]
        private int selectedIndex;

        public Fault Fault { get; private set; }

        public string PriorText
        {
            get
            {
                return priorText;
            }
            set
            {
                if (!object.Equals(priorText, value))
                {
                    priorText = value;
                    OnPropertyChanged("PriorText");
                }
            }
        }

        public string PastText
        {
            get
            {
                return pastText;
            }
            set
            {
                if (!object.Equals(pastText, value))
                {
                    pastText = value;
                    OnPropertyChanged("PastText");
                }
            }
        }

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if (!object.Equals(selectedIndex, value))
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                }
            }
        }

        public ObservableCollection<DtcAnzeigeButtonModel> Buttons
        {
            get
            {
                return buttons;
            }
            set
            {
                buttons.Clear();
                buttons.AddRange(value);
                OnPropertyChanged("Buttons");
            }
        }

        public event EventHandler<Fault> FaultChanged;

        public DtcAnzeigeDynModel()
        {
            buttons = new ObservableCollection<DtcAnzeigeButtonModel>();
        }

        internal void SelectedButton(int index)
        {
            DtcAnzeigeButtonModel dtcAnzeigeButtonModel = buttons.FirstOrDefault((DtcAnzeigeButtonModel x) => x.Index == index);
            if (dtcAnzeigeButtonModel != null)
            {
                dtcAnzeigeButtonModel.IsSelected = true;
                SelectedIndex = index;
            }
            else
            {
                Log.Error("DtcAnzeigeDynModel.SelectedButton()", "No buttondata found with index {0}", index);
            }
        }

        internal void SelectedFault(Fault fault)
        {
            Fault = fault;
            this.FaultChanged?.Invoke(this, Fault);
        }
    }
}

