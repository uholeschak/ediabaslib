using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class QuickCommandMeasuringServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string additionalText;

        [DataMember]
        private bool areMeasurmentIconsVisible;

        [DataMember]
        private string button1Text;

        [DataMember]
        private string button2Text;

        [DataMember]
        private bool isAnswer1;

        [DataMember]
        private bool isAnswer2;

        [DataMember]
        private bool isManualInput;

        [DataMember]
        private string measuredUnit;

        [DataMember]
        private string measuredValue;

        [DataMember]
        private int? measurementPointPin1;

        [DataMember]
        private int? measurementPointPin2;

        [DataMember]
        private string measurementPointText1;

        [DataMember]
        private string measurementPointText2;

        [DataMember]
        private string questionText;

        public string AdditionalText
        {
            get
            {
                return additionalText;
            }
            set
            {
                if (!object.Equals(additionalText, value))
                {
                    additionalText = value;
                    OnPropertyChanged("AdditionalText");
                }
            }
        }

        public bool AreMeasurmentIconsVisible
        {
            get
            {
                return areMeasurmentIconsVisible;
            }
            set
            {
                if (!object.Equals(areMeasurmentIconsVisible, value))
                {
                    areMeasurmentIconsVisible = value;
                    OnPropertyChanged("AreMeasurmentIconsVisible");
                }
            }
        }

        public string Button1Text
        {
            get
            {
                return button1Text;
            }
            set
            {
                if (!object.Equals(button1Text, value))
                {
                    button1Text = value;
                    OnPropertyChanged("Button1Text");
                }
            }
        }

        public string Button2Text
        {
            get
            {
                return button2Text;
            }
            set
            {
                if (!object.Equals(button2Text, value))
                {
                    button2Text = value;
                    OnPropertyChanged("Button2Text");
                }
            }
        }

        public bool IsAnswer1
        {
            get
            {
                return isAnswer1;
            }
            set
            {
                if (!object.Equals(isAnswer1, value))
                {
                    isAnswer1 = value;
                    OnPropertyChanged("IsAnswer1");
                }
            }
        }

        public bool IsAnswer2
        {
            get
            {
                return isAnswer2;
            }
            set
            {
                isAnswer2 = value;
                OnPropertyChanged("IsAnswer2");
            }
        }

        public bool IsManualInput
        {
            get
            {
                return isManualInput;
            }
            set
            {
                if (!object.Equals(isManualInput, value))
                {
                    isManualInput = value;
                    OnPropertyChanged("IsManualInput");
                }
            }
        }

        public string MeasuredUnit
        {
            get
            {
                return measuredUnit;
            }
            set
            {
                if (!object.Equals(measuredUnit, value))
                {
                    measuredUnit = value;
                    OnPropertyChanged("MeasuredUnit");
                }
            }
        }

        public string MeasuredValue
        {
            get
            {
                return measuredValue;
            }
            set
            {
                if (!object.Equals(measuredValue, value))
                {
                    measuredValue = value;
                    OnPropertyChanged("MeasuredValue");
                }
            }
        }

        public int? MeasurementPointPin1
        {
            get
            {
                return measurementPointPin1;
            }
            set
            {
                if (!object.Equals(measurementPointPin1, value))
                {
                    measurementPointPin1 = value;
                    OnPropertyChanged("MeasurementPointPin1");
                }
            }
        }

        public int? MeasurementPointPin2
        {
            get
            {
                return measurementPointPin2;
            }
            set
            {
                if (!object.Equals(measurementPointPin2, value))
                {
                    measurementPointPin2 = value;
                    OnPropertyChanged("MeasurementPointPin2");
                }
            }
        }

        public string MeasurementPointText1
        {
            get
            {
                return measurementPointText1;
            }
            set
            {
                if (!object.Equals(measurementPointText1, value))
                {
                    measurementPointText1 = value;
                    OnPropertyChanged("MeasurementPointText1");
                }
            }
        }

        public string MeasurementPointText2
        {
            get
            {
                return measurementPointText2;
            }
            set
            {
                if (!object.Equals(measurementPointText2, value))
                {
                    measurementPointText2 = value;
                    OnPropertyChanged("MeasurementPointText2");
                }
            }
        }

        public string QuestionText
        {
            get
            {
                return questionText;
            }
            set
            {
                if (!object.Equals(questionText, value))
                {
                    questionText = value;
                    OnPropertyChanged("QuestionText");
                }
            }
        }
    }
}
