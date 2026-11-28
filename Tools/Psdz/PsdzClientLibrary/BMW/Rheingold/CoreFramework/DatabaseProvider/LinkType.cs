using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class LinkType : INotifyPropertyChanged
    {
        private decimal infoObjectControlIdField;
        private string linkIdField;
        private bool ctordoneField;
        public decimal InfoObjectControlId
        {
            get
            {
                return infoObjectControlIdField;
            }

            set
            {
                _ = infoObjectControlIdField;
                if (!infoObjectControlIdField.Equals(value))
                {
                    infoObjectControlIdField = value;
                    OnPropertyChanged("InfoObjectControlId");
                }
            }
        }

        public string LinkId
        {
            get
            {
                return linkIdField;
            }

            set
            {
                if (linkIdField != null)
                {
                    if (!linkIdField.Equals(value))
                    {
                        linkIdField = value;
                        OnPropertyChanged("LinkId");
                    }
                }
                else
                {
                    linkIdField = value;
                    OnPropertyChanged("LinkId");
                }
            }
        }

        [DefaultValue(true)]
        public bool ctordone
        {
            get
            {
                return ctordoneField;
            }

            set
            {
                if (!ctordoneField.Equals(value))
                {
                    ctordoneField = value;
                    OnPropertyChanged("ctordone");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public LinkType()
        {
            ctordoneField = true;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}