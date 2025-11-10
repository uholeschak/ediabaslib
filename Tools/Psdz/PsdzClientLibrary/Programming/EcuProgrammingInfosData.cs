using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
    [DataContract]
    internal class EcuProgrammingInfosData : IEcuProgrammingInfosData, INotifyPropertyChanged
    {
        private ObservableCollection<IEcuProgrammingInfoData> ecuProgrammingInfosList;
        private ObservableCollection<IProgrammingActionData> selectedActionData;
        private ObservableCollection<IEcuProgrammingInfoData> ecusWithIndividualData;
        [DataMember]
        public ObservableCollection<IEcuProgrammingInfoData> ECUsWithIndividualData
        {
            get
            {
                return ecusWithIndividualData;
            }

            set
            {
                ecusWithIndividualData = value;
                OnPropertyChanged("ECUsWithIndividualData");
            }
        }

        [DataMember]
        public ObservableCollection<IEcuProgrammingInfoData> List
        {
            get
            {
                return ecuProgrammingInfosList;
            }

            set
            {
                ecuProgrammingInfosList = value;
                OnPropertyChanged("List");
            }
        }

        [DataMember]
        public ObservableCollection<IProgrammingActionData> SelectedActionData
        {
            get
            {
                return selectedActionData;
            }

            set
            {
                selectedActionData = value;
                OnPropertyChanged("SelectedActionData");
            }
        }

        [DataMember]
        public virtual bool SelectionEstablished
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal EcuProgrammingInfosData()
        {
            ecuProgrammingInfosList = new ObservableCollection<IEcuProgrammingInfoData>();
            selectedActionData = new ObservableCollection<IProgrammingActionData>();
            ecusWithIndividualData = new ObservableCollection<IEcuProgrammingInfoData>();
        }

        internal void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}