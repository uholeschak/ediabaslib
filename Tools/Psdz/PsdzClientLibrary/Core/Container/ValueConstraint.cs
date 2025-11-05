// BMW.Rheingold.Module.ISTA.ValueConstraint
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
    [DataContract(Name = "ValueConstraint")]
    public class ValueConstraint : AConstraint, INotifyPropertyChanged
    {
        private ObservableCollection<ValueLiteral> valuesField;
        [XmlArrayItem("Value", IsNullable = false)]
        [DataMember]
        public ObservableCollection<ValueLiteral> Values
        {
            get
            {
                return valuesField;
            }

            set
            {
                if (valuesField != null)
                {
                    if (!valuesField.Equals(value))
                    {
                        valuesField = value;
                        OnPropertyChanged("Values");
                    }
                }
                else
                {
                    valuesField = value;
                    OnPropertyChanged("Values");
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