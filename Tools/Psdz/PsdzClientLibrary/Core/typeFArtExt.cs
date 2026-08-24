using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class typeFArtExt : IFArtExt, INotifyPropertyChanged
    {
        private long f_ART_NRField;

        private string f_ART_TEXTField;

        public long F_ART_NR
        {
            get
            {
                return f_ART_NRField;
            }
            set
            {
                if (!f_ART_NRField.Equals(value))
                {
                    f_ART_NRField = value;
                    OnPropertyChanged("F_ART_NR");
                }
            }
        }

        public string F_ART_TEXT
        {
            get
            {
                return f_ART_TEXTField;
            }
            set
            {
                if (f_ART_TEXTField != null)
                {
                    if (!f_ART_TEXTField.Equals(value))
                    {
                        f_ART_TEXTField = value;
                        OnPropertyChanged("F_ART_TEXT");
                    }
                }
                else
                {
                    f_ART_TEXTField = value;
                    OnPropertyChanged("F_ART_TEXT");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override bool Equals(object obj)
        {
            if (obj is typeFArtExt typeFArtExt2 && F_ART_NR.Equals(typeFArtExt2.F_ART_NR))
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return F_ART_NR.GetHashCode();
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
