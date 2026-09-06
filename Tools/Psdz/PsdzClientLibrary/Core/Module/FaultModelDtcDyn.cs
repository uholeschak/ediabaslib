using BMW.Rheingold.CoreFramework.DatabaseProvider;
using PsdzClient.Core;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using PsdzClient;

namespace BMW.Rheingold.Module.ISTA
{
    public class FaultModelDtcDyn : INotifyPropertyChanged
    {
        private bool marked;
        private bool selected;
        private int index;
        private string btnNo;
        private string faultLabel;
        public DtcAnzeigeButtonModel ButtonModel => new DtcAnzeigeButtonModel(IsMarked, IsSelected, DTC?.FortAsHexString, FaultLabel, btnNo, index);

        public DTC DTC => Fault?.DTC;

        public ECU ECU => Fault?.ECU;

        public Fault Fault { get; set; }

        public string FaultLabel
        {
            get
            {
                return faultLabel;
            }

            set
            {
                faultLabel = value;
                OnPropertyChanged("FaultLabel");
            }
        }

        public bool IsMarked
        {
            get
            {
                return marked;
            }

            set
            {
                marked = value;
                OnPropertyChanged("IsMarked");
            }
        }

        public bool IsSelected
        {
            get
            {
                return selected;
            }

            set
            {
                if (selected != value)
                {
                    selected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                index = value;
                OnPropertyChanged("Index");
            }
        }

        public string BtnNo
        {
            get
            {
                return btnNo;
            }

            set
            {
                btnNo = value;
                OnPropertyChanged("BtnNo");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public FaultModelDtcDyn(Fault fault, PsdzDatabase database, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            Fault = fault;
            faultLabel = Fault.FaultLabel;
            Initialize(fault, database, vehicle, ffmResolver);
        }

        public FaultModelDtcDyn(Fault fault, string faultLabel)
        {
            Fault = fault;
            FaultLabel = faultLabel;
        }

        private void Initialize(Fault fault, PsdzDatabase database, Vehicle vehicle, IFFMDynamicResolver ffmResolver)
        {
            //[-] XEP_FAULTLABELS ecuFaultLabelByFaultCodeAndEcuVariant = database.GetEcuFaultLabelByFaultCodeAndEcuVariant(fault.DTC.F_ORT.ToString(), fault.ECU.VARIANTE, vehicle, ffmResolver);
            //[-] string value = ((ecuFaultLabelByFaultCodeAndEcuVariant != null) ? ecuFaultLabelByFaultCodeAndEcuVariant.Title : string.Empty);
            //[-] if (!string.IsNullOrEmpty(value))
            //[-] {
            //[-] FaultLabel = value;
            //[-] }
            //[-] else
            //[-] {
            //[-] FaultLabel = fault.DTC.F_ORT_TEXT;
            //[-] }
            if (!string.IsNullOrEmpty(FaultLabel))
            {
                FaultLabel = FaultLabel.Trim();
            }
        }

        public void Initialize(bool mark, int index)
        {
            IsMarked = mark;
            Index = index;
            BtnNo = (Index + 1).ToString(CultureInfo.InvariantCulture);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}