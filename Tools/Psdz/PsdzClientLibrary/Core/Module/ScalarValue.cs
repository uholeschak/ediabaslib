using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class ScalarValue : NotifyPropertyChangedBase
    {
        [DataMember]
        private double m_CanvasLeft;

        [DataMember]
        private string m_Value;

        public double CanvasLeft
        {
            get
            {
                return m_CanvasLeft;
            }
            set
            {
                if (m_CanvasLeft != value)
                {
                    m_CanvasLeft = value;
                    OnPropertyChanged("CanvasLeft");
                }
            }
        }

        public string Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Value != value)
                {
                    m_Value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        private ScalarValue()
        {
        }

        public ScalarValue(string value, double canvasLeft)
        {
            Value = value;
            CanvasLeft = canvasLeft;
        }
    }
}
