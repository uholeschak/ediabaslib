using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class MeasuringServiceDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private string adaptionsTextFlow;

        [DataMember]
        private string button1Text;

        [DataMember]
        private string button2Text;

        [DataMember]
        private bool isAnswer1;

        [DataMember]
        private bool isAnswer2;

        [DataMember]
        private bool isCh1Enter;

        [DataMember]
        private bool isCh1Measure;

        [DataMember]
        private bool isCh2;

        [DataMember]
        private bool isManualInput;

        [DataMember]
        private bool isToleranzFrage;

        [DataMember]
        private string measuredValue1;

        [DataMember]
        private double measuredValue2;

        [DataMember]
        private string measuringUnit1;

        [DataMember]
        private string measuringUnit2;

        [DataMember]
        private string toleranzFeldFrageText;

        public string AdaptionsTextFlow
        {
            get
            {
                return adaptionsTextFlow;
            }
            set
            {
                if (!object.Equals(adaptionsTextFlow, value))
                {
                    adaptionsTextFlow = value;
                    OnPropertyChanged("AdaptionsTextFlow");
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
                if (IsAnswer2 != value)
                {
                    isAnswer2 = value;
                    OnPropertyChanged("IsAnswer2");
                }
            }
        }

        public bool IsCh1Enter
        {
            get
            {
                return isCh1Enter;
            }
            set
            {
                if (!object.Equals(isCh1Enter, value))
                {
                    isCh1Enter = value;
                    OnPropertyChanged("IsCh1Enter");
                }
            }
        }

        public bool IsCh1Measure
        {
            get
            {
                return isCh1Measure;
            }
            set
            {
                if (!object.Equals(isCh1Measure, value))
                {
                    isCh1Measure = value;
                    OnPropertyChanged("IsCh1Measure");
                }
            }
        }

        public bool IsCh2
        {
            get
            {
                return isCh2;
            }
            set
            {
                if (!object.Equals(isCh2, value))
                {
                    isCh2 = value;
                    OnPropertyChanged("IsCh2");
                }
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

        public bool IsToleranzFrage
        {
            get
            {
                return isToleranzFrage;
            }
            set
            {
                if (!object.Equals(isToleranzFrage, value))
                {
                    isToleranzFrage = value;
                    OnPropertyChanged("IsToleranzFrage");
                }
            }
        }

        public string MeasuredValue1
        {
            get
            {
                return measuredValue1;
            }
            set
            {
                if (!object.Equals(measuredValue1, value))
                {
                    measuredValue1 = value;
                    OnPropertyChanged("MeasuredValue1");
                }
            }
        }

        public double MeasuredValue2
        {
            get
            {
                return measuredValue2;
            }
            set
            {
                if (!object.Equals(measuredValue2, value))
                {
                    measuredValue2 = value;
                    OnPropertyChanged("MeasuredValue2");
                }
            }
        }

        public string MeasuringUnit1
        {
            get
            {
                return measuringUnit1;
            }
            set
            {
                if (!object.Equals(measuringUnit1, value))
                {
                    measuringUnit1 = value;
                    OnPropertyChanged("MeasuringUnit1");
                }
            }
        }

        public string MeasuringUnit2
        {
            get
            {
                return measuringUnit2;
            }
            set
            {
                if (!object.Equals(measuringUnit2, value))
                {
                    measuringUnit2 = value;
                    OnPropertyChanged("MeasuringUnit2");
                }
            }
        }

        public string ToleranzFeldFrageText
        {
            get
            {
                return toleranzFeldFrageText;
            }
            set
            {
                if (!object.Equals(toleranzFeldFrageText, value))
                {
                    toleranzFeldFrageText = value;
                    OnPropertyChanged("ToleranzFeldFrageText");
                }
            }
        }
    }
}
