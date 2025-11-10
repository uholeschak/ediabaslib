using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
    [DataContract]
    public class EcuProgrammingInfoData : IEcuProgrammingInfoData, IEcuProgrammingInfo, INotifyPropertyChanged
    {
        [DataMember]
        private bool codingScheduled;
        [DataMember]
        private bool programmingScheduled;
        [DataMember]
        private bool codingDisabled;
        [DataMember]
        private bool programmingDisabled;
        [DataMember]
        private double progressValue;
        [DataMember]
        private IStandardSvk svkCurrent;
        [DataMember]
        private IStandardSvk svkTarget;
        [DataMember]
        private bool isExchangeScheduled;
        [DataMember]
        private bool isExchangeDone;
        [DataMember]
        private bool exchangeDoneDisabled;
        [DataMember]
        private bool exchangeScheduledDisabled;
        [DataMember]
        private bool programmingSelectionDisabled;
        [DataMember]
        private bool codingSelectionDisabled;
        [DataMember]
        private IEcu ecu;
        [DataMember]
        private int flashOrder = int.MaxValue;
        private string category = string.Empty;
        private string ecuTitle = string.Empty;
        private string ecuDescription = string.Empty;
        private ObservableCollection<IProgrammingActionData> programmingActions = new ObservableCollection<IProgrammingActionData>();
        [XmlIgnore]
        public IEcu Ecu
        {
            get
            {
                return ecu;
            }

            set
            {
                ecu = value;
                OnPropertyChanged("Ecu");
            }
        }

        [XmlIgnore]
        public IEnumerable<IProgrammingAction> ProgrammingActions => new List<IProgrammingAction>();

        [DataMember]
        public string Category
        {
            get
            {
                return category;
            }

            set
            {
                category = value;
                OnPropertyChanged("Category");
            }
        }

        [DataMember]
        public string EcuTitle
        {
            get
            {
                return ecuTitle;
            }

            set
            {
                ecuTitle = value;
                OnPropertyChanged("EcuTitle");
            }
        }

        [DataMember]
        public string EcuDescription
        {
            get
            {
                return ecuDescription;
            }

            set
            {
                ecuDescription = value;
                OnPropertyChanged("EcuDescription");
            }
        }

        [DataMember]
        [XmlIgnore]
        public ObservableCollection<IProgrammingActionData> ProgrammingActionData
        {
            get
            {
                return programmingActions;
            }

            set
            {
                programmingActions = value;
                OnPropertyChanged("ProgrammingActionData");
            }
        }

        public bool IsCodingDisabled
        {
            get
            {
                return codingDisabled;
            }

            set
            {
                if (codingDisabled != value)
                {
                    codingDisabled = value;
                    OnPropertyChanged("IsCodingDisabled");
                }
            }
        }

        public bool IsCodingScheduled
        {
            get
            {
                return codingScheduled;
            }

            set
            {
                if (codingScheduled != value)
                {
                    codingScheduled = value;
                    OnPropertyChanged("IsCodingScheduled");
                }
            }
        }

        public bool IsProgrammingDisabled
        {
            get
            {
                return programmingDisabled;
            }

            set
            {
                if (programmingDisabled != value)
                {
                    programmingDisabled = value;
                    OnPropertyChanged("IsProgrammingDisabled");
                }
            }
        }

        public bool IsProgrammingSelectionDisabled
        {
            get
            {
                return programmingSelectionDisabled;
            }

            set
            {
                if (programmingSelectionDisabled != value)
                {
                    programmingSelectionDisabled = value;
                    OnPropertyChanged("IsProgrammingSelectionDisabled");
                }
            }
        }

        public bool IsCodingSelectionDisabled
        {
            get
            {
                return codingSelectionDisabled;
            }

            set
            {
                if (codingSelectionDisabled != value)
                {
                    codingSelectionDisabled = value;
                    OnPropertyChanged("IsCodingSelectionDisabled");
                }
            }
        }

        public bool IsExchangeDoneDisabled
        {
            get
            {
                return exchangeDoneDisabled;
            }

            set
            {
                if (exchangeDoneDisabled != value)
                {
                    exchangeDoneDisabled = value;
                    OnPropertyChanged("IsExchangeDoneDisabled");
                }
            }
        }

        public bool IsExchangeScheduledDisabled
        {
            get
            {
                return exchangeScheduledDisabled;
            }

            set
            {
                if (exchangeScheduledDisabled != value)
                {
                    exchangeScheduledDisabled = value;
                    OnPropertyChanged("IsExchangeScheduledDisabled");
                }
            }
        }

        public bool IsExchangeDone
        {
            get
            {
                return isExchangeDone;
            }

            set
            {
                if (isExchangeDone != value)
                {
                    isExchangeDone = value;
                    OnPropertyChanged("IsExchangeDone");
                }
            }
        }

        public bool IsExchangeScheduled
        {
            get
            {
                return isExchangeScheduled;
            }

            set
            {
                if (isExchangeScheduled != value)
                {
                    isExchangeScheduled = value;
                    OnPropertyChanged("IsExchangeScheduled");
                }
            }
        }

        public double ProgressValue
        {
            get
            {
                return progressValue;
            }

            set
            {
                if (progressValue != value)
                {
                    progressValue = value;
                    OnPropertyChanged("ProgressValue");
                }
            }
        }

        public bool IsProgrammingScheduled
        {
            get
            {
                return programmingScheduled;
            }

            set
            {
                if (programmingScheduled != value)
                {
                    programmingScheduled = value;
                    OnPropertyChanged("IsProgrammingScheduled");
                }
            }
        }

        [DataMember]
        public ProgrammingActionState? State { get; set; }

        [XmlIgnore]
        public IStandardSvk SvkCurrent
        {
            get
            {
                return svkCurrent;
            }

            set
            {
                if (svkCurrent != value)
                {
                    svkCurrent = value;
                    OnPropertyChanged("SvkCurrent");
                }
            }
        }

        [XmlIgnore]
        public IStandardSvk SvkTarget
        {
            get
            {
                return svkTarget;
            }

            set
            {
                if (svkTarget != value)
                {
                    svkTarget = value;
                    OnPropertyChanged("SvkTarget");
                }
            }
        }

        public string EcuIdentifier { get; set; }

        public int FlashOrder
        {
            get
            {
                return flashOrder;
            }

            set
            {
                if (flashOrder != value)
                {
                    flashOrder = value;
                    OnPropertyChanged("FlashOrder");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public IProgrammingAction GetProgrammingAction(ProgrammingActionType type)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IProgrammingAction> GetProgrammingActions(ProgrammingActionType[] programmingActionTypeFilter)
        {
            throw new NotSupportedException();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}