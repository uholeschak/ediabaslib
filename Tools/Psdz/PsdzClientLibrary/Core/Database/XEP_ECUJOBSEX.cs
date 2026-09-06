using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_ECUJOBSEX : INotifyPropertyChanged
    {
        private IList<XEP_ECUPARAMETERSEX> parameters = new List<XEP_ECUPARAMETERSEX>();

        private ObservableCollection<XEP_ECURESULTSEX> results = new ObservableCollection<XEP_ECURESULTSEX>();

        private decimal idField;

        private string functionNameJobField;

        private string adapterConfigurationField;

        private string nameField;

        private string phaseField;

        private decimal? rankField;

        public IList<XEP_ECUPARAMETERSEX> Parameters
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        public ICollection<XEP_ECURESULTSEX> Results => results;

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

        public string FunctionNameJob
        {
            get
            {
                return functionNameJobField;
            }
            set
            {
                if (functionNameJobField != null)
                {
                    if (!functionNameJobField.Equals(value))
                    {
                        functionNameJobField = value;
                        OnPropertyChanged("FunctionNameJob");
                    }
                }
                else
                {
                    functionNameJobField = value;
                    OnPropertyChanged("FunctionNameJob");
                }
            }
        }

        public string AdapterConfiguration
        {
            get
            {
                return adapterConfigurationField;
            }
            set
            {
                if (adapterConfigurationField != null)
                {
                    if (!adapterConfigurationField.Equals(value))
                    {
                        adapterConfigurationField = value;
                        OnPropertyChanged("AdapterConfiguration");
                    }
                }
                else
                {
                    adapterConfigurationField = value;
                    OnPropertyChanged("AdapterConfiguration");
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

        public decimal? Rank
        {
            get
            {
                return rankField;
            }
            set
            {
                if (rankField.HasValue)
                {
                    if (!rankField.Equals(value))
                    {
                        rankField = value;
                        OnPropertyChanged("Rank");
                    }
                }
                else
                {
                    rankField = value;
                    OnPropertyChanged("Rank");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string GetParameterString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (Parameters != null)
            {
                List<XEP_ECUPARAMETERSEX> list = new List<XEP_ECUPARAMETERSEX>(Parameters);
                list.Sort(new XEP_ECUJOBSEX_ParameterSequenceComparer());
                foreach (XEP_ECUPARAMETERSEX item in list)
                {
                    stringBuilder.Append(item.ParamValue + ";");
                }
            }
            return stringBuilder.ToString().TrimEnd(';');
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is XEP_ECUJOBSEX xEP_ECUJOBSEX))
            {
                return false;
            }
            if (Id != xEP_ECUJOBSEX.Id)
            {
                return false;
            }
            if (string.CompareOrdinal(FunctionNameJob, xEP_ECUJOBSEX.FunctionNameJob) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(AdapterConfiguration, xEP_ECUJOBSEX.AdapterConfiguration) != 0)
            {
                return false;
            }
            if (string.CompareOrdinal(Name, xEP_ECUJOBSEX.Name) != 0)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
