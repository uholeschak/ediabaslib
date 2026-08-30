using BMW.Rheingold.CoreFramework.Contracts.FASTA;
using PsdzClient.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class QuestionSelectButtonModel : INotifyPropertyChanged
    {
        [DataMember]
        private readonly string plainText;

        [DataMember]
        private readonly int result;

        [DataMember]
        private readonly string buttonText;

        private readonly ISelectableEntry fastaEntry;

        [DataMember]
        private string selectedbrand;

        [DataMember]
        private bool useMultiSelect;

        [DataMember]
        private bool isChecked;

        [DataMember]
        private bool isEnabled;

        [DataMember]
        private bool isMarked;

        [DataMember]
        private string label;

        [DataMember]
        private bool isVisible;

        public string SelectedBrand
        {
            get
            {
                return selectedbrand;
            }
            set
            {
                if (selectedbrand != value)
                {
                    selectedbrand = value;
                    OnPropertyChanged("SelectedBrand");
                }
            }
        }

        public bool IsMultiSelect
        {
            get
            {
                return useMultiSelect;
            }
            set
            {
                if (useMultiSelect != value)
                {
                    useMultiSelect = value;
                    OnPropertyChanged("IsMultiSelect");
                }
            }
        }

        public int SelectionState
        {
            get
            {
                if (!isVisible)
                {
                    return 0;
                }
                if (IsMultiSelect)
                {
                    if (isEnabled)
                    {
                        if (!isChecked)
                        {
                            return 1;
                        }
                        return 2;
                    }
                    if (!isChecked)
                    {
                        return -1;
                    }
                    return -2;
                }
                if (isEnabled)
                {
                    if (!isChecked)
                    {
                        return -1;
                    }
                    return 1;
                }
                if (!isChecked)
                {
                    return -2;
                }
                return 2;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        public string Label
        {
            get
            {
                return label;
            }
            private set
            {
                label = value;
                OnPropertyChanged("Label");
            }
        }

        public bool IsMarked
        {
            get
            {
                return isMarked;
            }
            set
            {
                if (isMarked != value)
                {
                    isMarked = value;
                    OnPropertyChanged("IsMarked");
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public int Result => result;

        public string PlainText => plainText;

        public ISelectableEntry FastaEntry => fastaEntry;

        public string ButtonText => buttonText;

        public event PropertyChangedEventHandler PropertyChanged;

        public QuestionSelectButtonModel(bool isMarked, int selectionState, string buttonText, string plainText, string label, int result, ISelectableEntry fastaEntry, bool useMultiSelect = false, string selectedBrand = "BMWPKW")
        {
            this.isMarked = isMarked;
            isEnabled = true;
            isChecked = true;
            isVisible = true;
            this.buttonText = buttonText;
            this.plainText = plainText;
            Label = label;
            this.result = result;
            this.fastaEntry = fastaEntry;
            IsMultiSelect = useMultiSelect;
            selectedbrand = selectedBrand;
            if (IsMultiSelect)
            {
                switch (selectionState)
                {
                    case 1:
                        isEnabled = true;
                        isChecked = false;
                        break;
                    case 2:
                        isEnabled = true;
                        isChecked = true;
                        break;
                    case -1:
                        isEnabled = false;
                        isChecked = false;
                        break;
                    case -2:
                        isEnabled = false;
                        isChecked = true;
                        break;
                    case 0:
                        isVisible = false;
                        isEnabled = true;
                        isChecked = false;
                        break;
                    default:
                        Log.Warning("QuestionSelectButtonModel.QuestionSelectButtonModel()", "Selectionstate {0} is unknown", selectionState);
                        isEnabled = true;
                        isChecked = false;
                        break;
                }
            }
            else
            {
                switch (selectionState)
                {
                    case -1:
                        isEnabled = true;
                        isChecked = false;
                        break;
                    case 1:
                        isEnabled = true;
                        isChecked = true;
                        break;
                    case -2:
                        isEnabled = false;
                        isChecked = false;
                        break;
                    case 2:
                        isEnabled = false;
                        isChecked = true;
                        break;
                    case 0:
                        isVisible = false;
                        isEnabled = true;
                        isChecked = false;
                        break;
                    default:
                        Log.Warning("QuestionSelectButtonModel.QuestionSelectButtonModel()", "Selectionstate {0} is unknown", selectionState);
                        break;
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
