using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUPARAMETERSEX : INotifyPropertyChanged
    {
        private decimal idField;

        private string paramValueField;

        private string functionNameParameterField;

        private string adapterPathField;

        private string nameField;

        private decimal? ecuJobIdField;

        private string phaseField;

        public decimal Id
        {
            get
            {
                return idField;
            }
            set
            {
                _ = idField;
                if (!idField.Equals(value))
                {
                    idField = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string ParamValue
        {
            get
            {
                return paramValueField;
            }
            set
            {
                if (paramValueField != null)
                {
                    if (!paramValueField.Equals(value))
                    {
                        paramValueField = value;
                        OnPropertyChanged("ParamValue");
                    }
                }
                else
                {
                    paramValueField = value;
                    OnPropertyChanged("ParamValue");
                }
            }
        }

        public string FunctionNameParameter
        {
            get
            {
                return functionNameParameterField;
            }
            set
            {
                if (functionNameParameterField != null)
                {
                    if (!functionNameParameterField.Equals(value))
                    {
                        functionNameParameterField = value;
                        OnPropertyChanged("FunctionNameParameter");
                    }
                }
                else
                {
                    functionNameParameterField = value;
                    OnPropertyChanged("FunctionNameParameter");
                }
            }
        }

        public string AdapterPath
        {
            get
            {
                return adapterPathField;
            }
            set
            {
                if (adapterPathField != null)
                {
                    if (!adapterPathField.Equals(value))
                    {
                        adapterPathField = value;
                        OnPropertyChanged("AdapterPath");
                    }
                }
                else
                {
                    adapterPathField = value;
                    OnPropertyChanged("AdapterPath");
                }
            }
        }

        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                if (nameField != null)
                {
                    if (!nameField.Equals(value))
                    {
                        nameField = value;
                        OnPropertyChanged("Name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public decimal? EcuJobId
        {
            get
            {
                return ecuJobIdField;
            }
            set
            {
                if (ecuJobIdField.HasValue)
                {
                    if (!ecuJobIdField.Equals(value))
                    {
                        ecuJobIdField = value;
                        OnPropertyChanged("EcuJobId");
                    }
                }
                else
                {
                    ecuJobIdField = value;
                    OnPropertyChanged("EcuJobId");
                }
            }
        }

        public string Phase
        {
            get
            {
                return phaseField;
            }
            set
            {
                if (phaseField != null)
                {
                    if (!phaseField.Equals(value))
                    {
                        phaseField = value;
                        OnPropertyChanged("Phase");
                    }
                }
                else
                {
                    phaseField = value;
                    OnPropertyChanged("Phase");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_ECUPARAMETERSEX xEP_ECUPARAMETERSEX))
            {
                return false;
            }
            if (Id != xEP_ECUPARAMETERSEX.Id)
            {
                return false;
            }
            if (string.CompareOrdinal(ParamValue, xEP_ECUPARAMETERSEX.ParamValue) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(FunctionNameParameter, xEP_ECUPARAMETERSEX.FunctionNameParameter) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(AdapterPath, xEP_ECUPARAMETERSEX.AdapterPath) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Name, xEP_ECUPARAMETERSEX.Name) != 0)
            {
                return false;
            }
            if (!(EcuJobId == xEP_ECUPARAMETERSEX.EcuJobId))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
