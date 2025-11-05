// BMW.Rheingold.Module.ISTA.StepConstraint
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
    [DataContract(Name = "StepConstraint")]
    public class StepConstraint : AConstraint, INotifyPropertyChanged
    {
        private ObservableCollection<StepRange> stepRangesField;
        [XmlArrayItem(IsNullable = false)]
        [DataMember]
        public ObservableCollection<StepRange> StepRanges
        {
            get
            {
                return stepRangesField;
            }

            set
            {
                if (stepRangesField != null)
                {
                    if (!stepRangesField.Equals(value))
                    {
                        stepRangesField = value;
                        OnPropertyChanged("StepRanges");
                    }
                }
                else
                {
                    stepRangesField = value;
                    OnPropertyChanged("StepRanges");
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