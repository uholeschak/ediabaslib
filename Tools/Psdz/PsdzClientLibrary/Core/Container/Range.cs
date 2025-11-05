// BMW.Rheingold.Module.ISTA.Range
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [XmlInclude(typeof(StepRange))]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "Range")]
    public class Range : INotifyPropertyChanged
    {
        private ValueLiteral lowerBoundField;
        private ValueLiteral upperBoundField;
        [DataMember]
        public ValueLiteral LowerBound
        {
            get
            {
                return lowerBoundField;
            }

            set
            {
                if (lowerBoundField != null)
                {
                    if (!lowerBoundField.Equals(value))
                    {
                        lowerBoundField = value;
                        OnPropertyChanged("LowerBound");
                    }
                }
                else
                {
                    lowerBoundField = value;
                    OnPropertyChanged("LowerBound");
                }
            }
        }

        [DataMember]
        public ValueLiteral UpperBound
        {
            get
            {
                return upperBoundField;
            }

            set
            {
                if (upperBoundField != null)
                {
                    if (!upperBoundField.Equals(value))
                    {
                        upperBoundField = value;
                        OnPropertyChanged("UpperBound");
                    }
                }
                else
                {
                    upperBoundField = value;
                    OnPropertyChanged("UpperBound");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}