// BMW.Rheingold.Module.ISTA.StepConstraint
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DataContract(Name = "StepConstraint")]
    [DesignerCategory("code")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class StepConstraint : AConstraint, INotifyPropertyChanged
    {
        private ObservableCollection<StepRange> stepRangesField;

        [DataMember]
        [XmlArrayItem(IsNullable = false)]
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