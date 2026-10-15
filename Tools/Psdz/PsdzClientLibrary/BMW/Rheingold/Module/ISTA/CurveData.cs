using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class CurveData : INotifyPropertyChanged
    {
        [DataMember]
        private ObservableCollection<Tuple<double, double>> curve = new ObservableCollection<Tuple<double, double>>();
        [DataMember]
        private bool isVisible;
        [DataMember]
        private bool isY2;
        [DataMember]
        private string name;
        [DataMember]
        private int strokeColor;
        [DataMember]
        private string text;
        [DataMember]
        private double x;
        [DataMember]
        private double y;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public bool IsY2
        {
            get
            {
                return isY2;
            }

            set
            {
                isY2 = value;
                OnPropertyChanged("IsY2");
            }
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }

            set
            {
                if (isVisible != value)
                {
                    isVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        public int StrokeColor
        {
            get
            {
                return strokeColor;
            }

            set
            {
                if (strokeColor != value)
                {
                    strokeColor = value;
                    OnPropertyChanged("StrokeColor");
                }
            }
        }

        [IgnoreDataMember]
        public ICollection<Tuple<double, double>> Points { get; private set; }

        public double X
        {
            get
            {
                return x;
            }

            set
            {
                if (x != value)
                {
                    x = value;
                    OnPropertyChanged("X");
                }
            }
        }

        public double Y
        {
            get
            {
                return y;
            }

            set
            {
                if (y != value)
                {
                    y = value;
                    OnPropertyChanged("Y");
                }
            }
        }

        public IEnumerable<Tuple<double, double>> Curve => curve;

        public event PropertyChangedEventHandler PropertyChanged;
        public CurveData()
        {
            Points = new Collection<Tuple<double, double>>();
        }

        public void Update(CurveData curve)
        {
            Text = curve.Text;
            IsVisible = !string.IsNullOrEmpty(text);
            X = curve.X;
            Y = curve.Y;
            IsY2 = curve.IsY2;
            StrokeColor = curve.StrokeColor;
        }

        public void AddToCurve(double x, double y, double minValueX)
        {
            if (!double.IsNaN(x) && !double.IsNaN(y))
            {
                if (x == minValueX)
                {
                    curve.Clear();
                }

                if (!curve.Any() || x != 0.0 || y != 0.0)
                {
                    curve.Add(new Tuple<double, double>(x, y));
                }

                X = x;
                Y = y;
            }
        }

        public void CutCurve(CurveNewDataCotainer curveNewContainer)
        {
            double[, ] curveNew = curveNewContainer.CurveNew;
            curve.Clear();
            if (curveNew != null && curveNew.GetLength(0) > 0)
            {
                curve.Add(new Tuple<double, double>(curveNew[0, 0], curveNew[0, 1]));
                for (int i = 1; i < curveNew.GetLength(0) && (curveNew[i, 0] != 0.0 || curveNew[i, 1] != 0.0); i++)
                {
                    curve.Add(new Tuple<double, double>(curveNew[i, 0], curveNew[i, 1]));
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}