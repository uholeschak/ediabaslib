// BMW.Rheingold.Module.ISTA.Value
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Value")]
    public class Value : ValueBase, INotifyPropertyChanged
    {
        private ValueLiteral literalField;
        [DataMember]
        public ValueLiteral Literal
        {
            get
            {
                return literalField;
            }

            set
            {
                if (literalField != null)
                {
                    if (!literalField.Equals(value))
                    {
                        literalField = value;
                        OnPropertyChanged("Literal");
                    }
                }
                else
                {
                    literalField = value;
                    OnPropertyChanged("Literal");
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