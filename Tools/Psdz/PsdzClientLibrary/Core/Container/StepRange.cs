// BMW.Rheingold.Module.ISTA.StepRange
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    [DataContract(Name = "StepRange")]
    public class StepRange : Range, INotifyPropertyChanged
    {
        private ValueLiteral stepField;
        [DataMember]
        public ValueLiteral Step
        {
            get
            {
                return stepField;
            }

            set
            {
                if (stepField != null)
                {
                    if (!stepField.Equals(value))
                    {
                        stepField = value;
                        OnPropertyChanged("Step");
                    }
                }
                else
                {
                    stepField = value;
                    OnPropertyChanged("Step");
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