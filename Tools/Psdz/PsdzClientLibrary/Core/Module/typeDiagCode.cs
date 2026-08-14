using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class typeDiagCode : INotifyPropertyChanged, IDiagCode
    {
        private string diagnoseCodeField;
        private string diagnoseCodeSuffixField;
        private ObservableCollection<string> reparaturPaketField;
        private string originField;
        public string DiagnoseCode
        {
            get
            {
                return diagnoseCodeField;
            }

            set
            {
                if (diagnoseCodeField != null)
                {
                    if (!diagnoseCodeField.Equals(value))
                    {
                        diagnoseCodeField = value;
                        OnPropertyChanged("DiagnoseCode");
                    }
                }
                else
                {
                    diagnoseCodeField = value;
                    OnPropertyChanged("DiagnoseCode");
                }
            }
        }

        public string DiagnoseCodeSuffix
        {
            get
            {
                return diagnoseCodeSuffixField;
            }

            set
            {
                if (diagnoseCodeSuffixField != null)
                {
                    if (!diagnoseCodeSuffixField.Equals(value))
                    {
                        diagnoseCodeSuffixField = value;
                        OnPropertyChanged("DiagnoseCodeSuffix");
                    }
                }
                else
                {
                    diagnoseCodeSuffixField = value;
                    OnPropertyChanged("DiagnoseCodeSuffix");
                }
            }
        }

        public ObservableCollection<string> ReparaturPaket
        {
            get
            {
                return reparaturPaketField;
            }

            set
            {
                if (reparaturPaketField != null)
                {
                    if (!reparaturPaketField.Equals(value))
                    {
                        reparaturPaketField = value;
                        OnPropertyChanged("ReparaturPaket");
                    }
                }
                else
                {
                    reparaturPaketField = value;
                    OnPropertyChanged("ReparaturPaket");
                }
            }
        }

        public string Origin
        {
            get
            {
                return originField;
            }

            set
            {
                if (originField != null)
                {
                    if (!originField.Equals(value))
                    {
                        originField = value;
                        OnPropertyChanged("Origin");
                    }
                }
                else
                {
                    originField = value;
                    OnPropertyChanged("Origin");
                }
            }
        }

        [XmlIgnore]
        IEnumerable<string> IDiagCode.ReparaturPaket => ReparaturPaket;

        public event PropertyChangedEventHandler PropertyChanged;
        public typeDiagCode()
        {
            reparaturPaketField = new ObservableCollection<string>();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override int GetHashCode()
        {
            if (DiagnoseCode != null)
            {
                return DiagnoseCode.GetHashCode();
            }

            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is typeDiagCode typeDiagCode2))
            {
                return false;
            }

            if (DiagnoseCode == null)
            {
                return typeDiagCode2.DiagnoseCode == null;
            }

            return DiagnoseCode.Equals(typeDiagCode2.DiagnoseCode);
        }
    }
}