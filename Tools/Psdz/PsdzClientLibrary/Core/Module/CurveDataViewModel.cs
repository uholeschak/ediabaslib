using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace BMW.Rheingold.Module.ISTA
{
    public class CurveDataViewModel : INotifyPropertyChanged
    {
        private CurveData data;

        private PointCollection curvesToView;

        private string toolTipContent;

        public CurveData Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                OnPropertyChanged("Data");
            }
        }

        public PointCollection CurveToView
        {
            get
            {
                return curvesToView;
            }
            set
            {
                curvesToView = value;
                OnPropertyChanged("CurveToView");
            }
        }

        public int StrokeThickness { get; set; }

        public bool CurveInPoints { get; set; }

        public string ToolTipContent
        {
            get
            {
                return toolTipContent;
            }
            set
            {
                toolTipContent = value;
                OnPropertyChanged("ToolTipContent");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CurveDataViewModel(CurveData data, PointCollection curveToView, bool curveInPoints, string toolTipContent)
        {
            Data = data;
            CurveToView = curveToView;
            CurveInPoints = curveInPoints;
            ToolTipContent = toolTipContent;
            StrokeThickness = 1;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
