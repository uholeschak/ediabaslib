using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming
{
    [DataContract]
    public class ProgrammingActionData : IProgrammingActionData, INotifyPropertyChanged
    {
        private static readonly ProgrammingActionType[] FlashActions = new ProgrammingActionType[3]
        {
            ProgrammingActionType.BootloaderProgramming,
            ProgrammingActionType.Programming,
            ProgrammingActionType.DataProgramming
        };
        private static readonly ProgrammingActionType[] EscalationActions = new ProgrammingActionType[5]
        {
            ProgrammingActionType.BootloaderProgramming,
            ProgrammingActionType.Programming,
            ProgrammingActionType.DataProgramming,
            ProgrammingActionType.Coding,
            ProgrammingActionType.IbaDeploy
        };
        private ProgrammingActionState stateProgramming;
        private IEcu parentEcu;
        private ProgrammingActionType type;
        private string channel = string.Empty;
        private string note = string.Empty;
        private bool isEditable;
        private bool isSelected;
        private int order;
        [DataMember]
        public ProgrammingActionState StateProgramming
        {
            get
            {
                return stateProgramming;
            }

            set
            {
                if (stateProgramming != value)
                {
                    stateProgramming = value;
                    OnPropertyChanged("StateProgramming");
                }
            }
        }

        [DataMember]
        public IEcu ParentEcu
        {
            get
            {
                return parentEcu;
            }

            set
            {
                if (parentEcu != value)
                {
                    parentEcu = value;
                    OnPropertyChanged("ParentEcu");
                }
            }
        }

        [DataMember]
        public ProgrammingActionType Type
        {
            get
            {
                return type;
            }

            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        [DataMember]
        public string Channel
        {
            get
            {
                return channel;
            }

            set
            {
                if (channel != value)
                {
                    channel = value;
                    OnPropertyChanged("Channel");
                }
            }
        }

        [DataMember]
        public string Note
        {
            get
            {
                return note;
            }

            set
            {
                if (note != value)
                {
                    note = value;
                    OnPropertyChanged("Note");
                }
            }
        }

        [DataMember]
        public bool IsEditable
        {
            get
            {
                return isEditable;
            }

            set
            {
                if (isEditable != value)
                {
                    isEditable = value;
                    OnPropertyChanged("IsEditable");
                }
            }
        }

        [DataMember]
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        [DataMember]
        public int Order
        {
            get
            {
                return order;
            }

            internal set
            {
                order = value;
                OnPropertyChanged("Order");
            }
        }

        public bool IsFlashAction => FlashActions.Contains(Type);
        public bool IsEscalationActionType => EscalationActions.Contains(Type);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}