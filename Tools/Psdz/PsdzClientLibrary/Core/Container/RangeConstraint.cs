// BMW.Rheingold.Module.ISTA.RangeConstraint
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "RangeConstraint")]
    public class RangeConstraint : AConstraint, INotifyPropertyChanged
    {
        private ObservableCollection<Range> rangesField;
        [XmlArrayItem(IsNullable = false)]
        [DataMember]
        public ObservableCollection<Range> Ranges
        {
            get
            {
                return rangesField;
            }

            set
            {
                if (rangesField != null)
                {
                    if (!rangesField.Equals(value))
                    {
                        rangesField = value;
                        OnPropertyChanged("Ranges");
                    }
                }
                else
                {
                    rangesField = value;
                    OnPropertyChanged("Ranges");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;
        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}