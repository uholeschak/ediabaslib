using System.ComponentModel;

namespace BMW.Rheingold.CoreFramework.DatabaseProvider
{
    public class XEP_QUERYOBJECTSEX : INotifyPropertyChanged
    {
        private decimal idField;
        private string titleField;
        private string attributNameField;
        private string attributWertField;
        private string zielKlasseField;
        private string linkIdField;
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

        public string Title
        {
            get
            {
                return titleField;
            }

            set
            {
                if (titleField != null)
                {
                    if (!titleField.Equals(value))
                    {
                        titleField = value;
                        OnPropertyChanged("Title");
                    }
                }
                else
                {
                    titleField = value;
                    OnPropertyChanged("Title");
                }
            }
        }

        public string AttributName
        {
            get
            {
                return attributNameField;
            }

            set
            {
                if (attributNameField != null)
                {
                    if (!attributNameField.Equals(value))
                    {
                        attributNameField = value;
                        OnPropertyChanged("AttributName");
                    }
                }
                else
                {
                    attributNameField = value;
                    OnPropertyChanged("AttributName");
                }
            }
        }

        public string AttributWert
        {
            get
            {
                return attributWertField;
            }

            set
            {
                if (attributWertField != null)
                {
                    if (!attributWertField.Equals(value))
                    {
                        attributWertField = value;
                        OnPropertyChanged("AttributWert");
                    }
                }
                else
                {
                    attributWertField = value;
                    OnPropertyChanged("AttributWert");
                }
            }
        }

        public string ZielKlasse
        {
            get
            {
                return zielKlasseField;
            }

            set
            {
                if (zielKlasseField != null)
                {
                    if (!zielKlasseField.Equals(value))
                    {
                        zielKlasseField = value;
                        OnPropertyChanged("ZielKlasse");
                    }
                }
                else
                {
                    zielKlasseField = value;
                    OnPropertyChanged("ZielKlasse");
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

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}