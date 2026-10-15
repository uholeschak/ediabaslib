using BMW.Authoring.Helper;
using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class ColoredRectangle : NotifyPropertyChangedBase
    {
        [IgnoreDataMember]
        private ServiceDialogColor m_FilledColor;

        [DataMember]
        private double m_CanvasLeft;

        [DataMember]
        private double m_Width;

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

        public double Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                if (m_Width != value)
                {
                    m_Width = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        public ServiceDialogColor FilledColor
        {
            get
            {
                return m_FilledColor;
            }
            set
            {
                if (m_FilledColor != value)
                {
                    m_FilledColor = value;
                    OnPropertyChanged("FilledColor");
                }
            }
        }

        private ColoredRectangle()
        {
        }

        public ColoredRectangle(ServiceDialogColor color, double canvasLeft, double width)
        {
            FilledColor = color;
            CanvasLeft = canvasLeft;
            Width = width;
        }
    }
}
