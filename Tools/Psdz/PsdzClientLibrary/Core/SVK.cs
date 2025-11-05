using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
    public class SVK : ISvk, INotifyPropertyChanged
    {
        private ObservableCollection<string> xWE_SGBMIDField;
        private string pROG_DATUMField;
        private long? pROG_KMField;
        private int? pROG_TESTField;
        private bool ctordoneField;
        private ICollection<int> prozessklasseWert;
        [XmlIgnore]
        IEnumerable<string> ISvk.XWE_SGBMID => XWE_SGBMID;

        public ObservableCollection<string> XWE_SGBMID
        {
            get
            {
                return xWE_SGBMIDField;
            }

            set
            {
                if (xWE_SGBMIDField != null)
                {
                    if (!xWE_SGBMIDField.Equals(value))
                    {
                        xWE_SGBMIDField = value;
                        OnPropertyChanged("XWE_SGBMID");
                    }
                }
                else
                {
                    xWE_SGBMIDField = value;
                    OnPropertyChanged("XWE_SGBMID");
                }
            }
        }

        public string PROG_DATUM
        {
            get
            {
                return pROG_DATUMField;
            }

            set
            {
                if (pROG_DATUMField != null)
                {
                    if (!pROG_DATUMField.Equals(value))
                    {
                        pROG_DATUMField = value;
                        OnPropertyChanged("PROG_DATUM");
                    }
                }
                else
                {
                    pROG_DATUMField = value;
                    OnPropertyChanged("PROG_DATUM");
                }
            }
        }

        public long? PROG_KM
        {
            get
            {
                return pROG_KMField;
            }

            set
            {
                if (pROG_KMField.HasValue)
                {
                    if (!pROG_KMField.Equals(value))
                    {
                        pROG_KMField = value;
                        OnPropertyChanged("PROG_KM");
                    }
                }
                else
                {
                    pROG_KMField = value;
                    OnPropertyChanged("PROG_KM");
                }
            }
        }

        public int? PROG_TEST
        {
            get
            {
                return pROG_TESTField;
            }

            set
            {
                if (pROG_TESTField.HasValue)
                {
                    if (!pROG_TESTField.Equals(value))
                    {
                        pROG_TESTField = value;
                        OnPropertyChanged("PROG_TEST");
                    }
                }
                else
                {
                    pROG_TESTField = value;
                    OnPropertyChanged("PROG_TEST");
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

        [XmlIgnore]
        public ICollection<int> XWE_PROZESSKLASSE_WERT
        {
            get
            {
                return prozessklasseWert;
            }

            set
            {
                prozessklasseWert = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SVK()
        {
            xWE_SGBMIDField = new ObservableCollection<string>();
            ctordoneField = true;
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}