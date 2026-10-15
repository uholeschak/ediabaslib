using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class NewCurveData : INotifyPropertyChanged
    {
        [DataMember]
        private int color;
        [DataMember]
        private int index;
        [DataMember]
        private string legendText;
        [DataMember]
        private int style;
        [DataMember]
        private int thickness;
        [DataMember]
        private int yAxis;
        [DataMember]
        private readonly ObservableCollection<double> yPoints;
        [DataMember]
        private int toggleState;
        public int Color
        {
            get
            {
                return color;
            }

            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged("Color");
                }
            }
        }

        public int Index
        {
            get
            {
                return index;
            }

            set
            {
                if (index != value)
                {
                    index = value;
                    OnPropertyChanged("Index");
                }
            }
        }

        public string LegendText
        {
            get
            {
                return legendText;
            }

            set
            {
                if (legendText != value)
                {
                    legendText = value;
                    OnPropertyChanged("LegendText");
                }
            }
        }

        public int Style
        {
            get
            {
                return style;
            }

            set
            {
                if (style != value)
                {
                    style = value;
                    OnPropertyChanged("Style");
                    OnPropertyChanged("Thickness");
                }
            }
        }

        public int Thickness
        {
            get
            {
                if (style == 0 || toggleState == 2)
                {
                    return 0;
                }

                if (toggleState == 1)
                {
                    return thickness + 2;
                }

                return thickness;
            }

            set
            {
                if (thickness != value)
                {
                    thickness = value;
                    OnPropertyChanged("Thickness");
                }
            }
        }

        public int YAxis
        {
            get
            {
                return yAxis;
            }

            set
            {
                if (yAxis != value)
                {
                    yAxis = value;
                    OnPropertyChanged("YAxis");
                }
            }
        }

        public ObservableCollection<double> YPoints => yPoints;
        public int ToggleState => toggleState;

        public event PropertyChangedEventHandler PropertyChanged;
        public NewCurveData()
        {
            color = 1;
            style = 1;
            thickness = 1;
            yAxis = 1;
            toggleState = 0;
            yPoints = new ObservableCollection<double>();
        }

        public void ToggleVisibility()
        {
            toggleState = ++toggleState % 3;
            OnPropertyChanged("ToggleState");
            OnPropertyChanged("Thickness");
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            if (obj is NewCurveData newCurveData)
            {
                return newCurveData.Index == Index;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return index.GetHashCode();
        }
    }
}