// BMW.Rheingold.Module.ISTA.MultipleValue
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
    [DataContract(Name = "MultipleValue")]
    public class MultipleValue : ValueBase, INotifyPropertyChanged
    {
        private ObservableCollection<ValueLiteral> literalsField;
        [XmlArrayItem("Value", IsNullable = false)]
        [DataMember]
        public ObservableCollection<ValueLiteral> Literals
        {
            get
            {
                return literalsField;
            }

            set
            {
                if (literalsField != null)
                {
                    if (!literalsField.Equals(value))
                    {
                        literalsField = value;
                        OnPropertyChanged("Literals");
                    }
                }
                else
                {
                    literalsField = value;
                    OnPropertyChanged("Literals");
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