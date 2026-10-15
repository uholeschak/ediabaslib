using BMW.Rheingold.ISTA.CoreFramework.ServiceDialoge;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class NewKurvendisplayDlgModel : ServiceDialogModelBase
    {
        [DataMember]
        private readonly ObservableCollection<KurvenDisplayActionButton> actionButtons;
        [DataMember]
        private readonly ObservableCollection<NewCurveData> curves;
        [DataMember]
        private readonly ObservableCollection<double> lowerLimitY;
        [DataMember]
        private readonly ObservableCollection<double> minYValue;
        [DataMember]
        private readonly ObservableCollection<double> upperLimitY;
        [DataMember]
        private readonly ObservableCollection<double> xPoints;
        [DataMember]
        private readonly ObservableCollection<double> yAxisDivision;
        [DataMember]
        private readonly ObservableCollection<string> yAxisLegendName;
        [DataMember]
        private readonly ObservableCollection<string> yAxisText;
        [DataMember]
        private ObservableCollection<string> backgroundColor;
        [DataMember]
        private string conclusionText;
        [DataMember]
        private string headerText;
        [DataMember]
        private bool horizontalOverflowScrollingEnabled;
        [DataMember]
        private double horizontalZoom;
        [DataMember]
        private string introductionText;
        [DataMember]
        private ReferenceAxis referenceVerticalZoomAxis;
        [DataMember]
        private bool isStatic;
        [DataMember]
        private bool linearInterpolationEnabled;
        [DataMember]
        private double maxXValue;
        [DataMember]
        private ObservableCollection<double> maxYValue;
        [DataMember]
        private double minXValue;
        [DataMember]
        private double originalMaxXValue;
        [DataMember]
        private double[] originalMaxYValue;
        private bool originalValuesInitialized;
        [DataMember]
        private SampledPointsIndexContainer sampledPointsContainer;
        [DataMember]
        private bool showCurveNumbers;
        [DataMember]
        private double verticalScrollBarMaxYReference;
        [DataMember]
        private double verticalScrollBarMinYReference;
        [DataMember]
        private int verticalValuesReferenceCurveIndex;
        [DataMember]
        private ObservableCollection<double> verticalZoom;
        [DataMember]
        private double xAxisDivision;
        [DataMember]
        private string xAxisText;
        [DataMember]
        private ObservableCollection<int> selectedVerticalAxis;
        [DataMember]
        private bool gridEnabled;
        [DataMember]
        private string toggleGridBtnBackground;
        private List<double> ticksX;
        private List<double> ticksY;
        public ObservableCollection<KurvenDisplayActionButton> ActionButtons => actionButtons;

        public string ConclusionText
        {
            get
            {
                return conclusionText;
            }

            set
            {
                if (conclusionText != value)
                {
                    conclusionText = value;
                    OnPropertyChanged("ConclusionText");
                }
            }
        }

        public ObservableCollection<string> BackgroundColor
        {
            get
            {
                return backgroundColor;
            }

            set
            {
                if (backgroundColor != value)
                {
                    backgroundColor = value;
                    OnPropertyChanged("BackgroundColor");
                }
            }
        }

        public ObservableCollection<NewCurveData> Curves => curves;

        public string HeaderText
        {
            get
            {
                return headerText;
            }

            set
            {
                if (headerText != value)
                {
                    headerText = value;
                    OnPropertyChanged("HeaderText");
                }
            }
        }

        public bool HorizontalOverflowScrollingEnabled
        {
            get
            {
                return horizontalOverflowScrollingEnabled;
            }

            set
            {
                if (horizontalOverflowScrollingEnabled != value)
                {
                    horizontalOverflowScrollingEnabled = value;
                    OnPropertyChanged("HorizontalOverflowScrollingEnabled");
                    OnPropertyChanged("IsHorizontalScrollBarEnabled");
                }
            }
        }

        public double HorizontalZoom
        {
            get
            {
                return horizontalZoom;
            }

            set
            {
                if (horizontalZoom != value)
                {
                    horizontalZoom = value;
                    OnPropertyChanged("HorizontalZoom");
                }
            }
        }

        public string IntroductionText
        {
            get
            {
                return introductionText;
            }

            set
            {
                if (introductionText != value)
                {
                    introductionText = value;
                    OnPropertyChanged("IntroductionText");
                }
            }
        }

        public bool IsHorizontalScrollBarEnabled
        {
            get
            {
                if (!IsStatic)
                {
                    return HorizontalOverflowScrollingEnabled;
                }

                return true;
            }
        }

        public bool IsStatic
        {
            get
            {
                return isStatic;
            }

            set
            {
                if (isStatic != value)
                {
                    isStatic = value;
                    OnPropertyChanged("IsStatic");
                    OnPropertyChanged("IsVerticalScrollBarEnabled");
                }
            }
        }

        public bool IsVerticalScrollBarEnabled => isStatic;

        public bool LinearInterpolationEnabled
        {
            get
            {
                return linearInterpolationEnabled;
            }

            set
            {
                if (linearInterpolationEnabled != value)
                {
                    linearInterpolationEnabled = value;
                    OnPropertyChanged("LinearInterpolationEnabled");
                }
            }
        }

        public ObservableCollection<double> LowerLimitY => lowerLimitY;

        public double MaxXValue
        {
            get
            {
                return maxXValue;
            }

            set
            {
                if (maxXValue != value)
                {
                    maxXValue = value;
                    OnPropertyChanged("MaxXValue");
                }
            }
        }

        public ObservableCollection<double> MaxYValue => maxYValue;

        public double MinXValue
        {
            get
            {
                return minXValue;
            }

            set
            {
                if (minXValue != value)
                {
                    minXValue = value;
                    OnPropertyChanged("MinXValue");
                }
            }
        }

        public ObservableCollection<double> MinYValue => minYValue;

        public SampledPointsIndexContainer SampledPointsContainer
        {
            get
            {
                return sampledPointsContainer;
            }

            set
            {
                if (sampledPointsContainer != value)
                {
                    sampledPointsContainer?.SampledIndexes.ClearEventInvocations("CollectionChanged");
                    sampledPointsContainer = value;
                    OnPropertyChanged("SampledPointsContainer");
                }
            }
        }

        public bool ShowCurveNumbers
        {
            get
            {
                return showCurveNumbers;
            }

            set
            {
                if (showCurveNumbers != value)
                {
                    showCurveNumbers = value;
                    OnPropertyChanged("ShowCurveNumbers");
                }
            }
        }

        public ObservableCollection<double> UpperLimitY => upperLimitY;

        public double VerticalScrollBarMaxYReference
        {
            get
            {
                return verticalScrollBarMaxYReference;
            }

            set
            {
                if (verticalScrollBarMaxYReference != value)
                {
                    verticalScrollBarMaxYReference = value;
                    OnPropertyChanged("VerticalScrollBarMaxYReference");
                }
            }
        }

        public double VerticalScrollBarMinYReference
        {
            get
            {
                return verticalScrollBarMinYReference;
            }

            set
            {
                if (verticalScrollBarMinYReference != value)
                {
                    verticalScrollBarMinYReference = value;
                    OnPropertyChanged("VerticalScrollBarMinYReference");
                }
            }
        }

        public int VerticalValuesReferenceCurveIndex
        {
            get
            {
                return verticalValuesReferenceCurveIndex;
            }

            set
            {
                if (verticalValuesReferenceCurveIndex != value)
                {
                    verticalValuesReferenceCurveIndex = value;
                    OnPropertyChanged("VerticalValuesReferenceCurveIndex");
                }
            }
        }

        public ReferenceAxis ReferenceVerticalZoomAxis
        {
            get
            {
                return referenceVerticalZoomAxis;
            }

            set
            {
                if (referenceVerticalZoomAxis != value)
                {
                    referenceVerticalZoomAxis = value;
                    OnPropertyChanged("ReferenceVerticalZoomAxis");
                }
            }
        }

        public ObservableCollection<double> VerticalZoom
        {
            get
            {
                return verticalZoom;
            }

            set
            {
                if (verticalZoom != value)
                {
                    verticalZoom = value;
                    OnPropertyChanged("VerticalZoom");
                }
            }
        }

        public double XAxisDivision
        {
            get
            {
                return xAxisDivision;
            }

            set
            {
                if (xAxisDivision != value)
                {
                    xAxisDivision = value;
                    OnPropertyChanged("XAxisDivision");
                }
            }
        }

        public string XAxisText
        {
            get
            {
                return xAxisText;
            }

            set
            {
                if (xAxisText != value)
                {
                    xAxisText = value;
                    OnPropertyChanged("XAxisText");
                }
            }
        }

        public ObservableCollection<double> XPoints => xPoints;
        public ObservableCollection<double> YAxisDivision => yAxisDivision;
        public ObservableCollection<string> YAxisLegendName => yAxisLegendName;
        public ObservableCollection<string> YAxisText => yAxisText;

        public ObservableCollection<int> SelectedVerticalAxis
        {
            get
            {
                return selectedVerticalAxis;
            }

            private set
            {
                if (selectedVerticalAxis != value)
                {
                    selectedVerticalAxis = value;
                    OnPropertyChanged("SelectedVerticalAxis");
                }
            }
        }

        public bool GridEnabled
        {
            get
            {
                return gridEnabled;
            }

            private set
            {
                if (gridEnabled != value)
                {
                    gridEnabled = value;
                    OnPropertyChanged("GridEnabled");
                }
            }
        }

        public string ToggleGridBtnBackground
        {
            get
            {
                return toggleGridBtnBackground;
            }

            private set
            {
                if (toggleGridBtnBackground != value)
                {
                    toggleGridBtnBackground = value;
                    OnPropertyChanged("ToggleGridBtnBackground");
                }
            }
        }

        public List<double> TicksX
        {
            get
            {
                return ticksX;
            }

            set
            {
                if (ticksX != value)
                {
                    ticksX = value;
                    OnPropertyChanged("TicksX");
                }
            }
        }

        public List<double> TicksY
        {
            get
            {
                return ticksY;
            }

            set
            {
                if (ticksY != value)
                {
                    ticksY = value;
                    OnPropertyChanged("TicksY");
                }
            }
        }

        public Rect GridRenderingRect { get; set; }

        public NewKurvendisplayDlgModel()
        {
            linearInterpolationEnabled = true;
            horizontalZoom = 1.0;
            verticalZoom = new ObservableCollection<double>(new double[4] { 1.0, 1.0, 1.0, 1.0 });
            backgroundColor = new ObservableCollection<string>(new string[4] { "#CCCCCC", "#CCCCCC", "#CCCCCC", "#CCCCCC" });
            curves = new ObservableCollection<NewCurveData>();
            lowerLimitY = new ObservableCollection<double>(new double[4]);
            maxYValue = new ObservableCollection<double>(new double[4]);
            minYValue = new ObservableCollection<double>(new double[4]);
            upperLimitY = new ObservableCollection<double>(new double[4]);
            xPoints = new ObservableCollection<double>();
            yAxisDivision = new ObservableCollection<double>(new double[4]);
            yAxisLegendName = new ObservableCollection<string>(new string[4]);
            yAxisText = new ObservableCollection<string>(new string[4]);
            actionButtons = new ObservableCollection<KurvenDisplayActionButton>();
            selectedVerticalAxis = new ObservableCollection<int>();
            gridEnabled = false;
            toggleGridBtnBackground = "#CCCCCC";
        }

        public void ApplyZoom(ZoomParam param)
        {
            switch (param)
            {
                case ZoomParam.IncreaseHorizontal:
                    HorizontalZoom++;
                    break;
                case ZoomParam.DecreaseHorizontal:
                    HorizontalZoom--;
                    break;
                case ZoomParam.IncreaseVertical:
                    AddVerticalZoom(1);
                    break;
                case ZoomParam.DecreaseVertical:
                    AddVerticalZoom(-1);
                    break;
            }
        }

        public void ResetOrizontalZoom()
        {
            for (int i = 0; i < 4; i++)
            {
                VerticalZoom[i] = 1.0;
            }
        }

        public void ToggleGrid()
        {
            GridEnabled = !GridEnabled;
            if (GridEnabled)
            {
                ToggleGridBtnBackground = "#669999";
            }
            else
            {
                ToggleGridBtnBackground = "#CCCCCC";
            }
        }

        public void SelectVerticalAxis(ReferenceAxis referenceAxis)
        {
            if (!SelectedVerticalAxis.Contains((int)referenceAxis))
            {
                SelectedVerticalAxis.Add((int)referenceAxis);
            }

            UpdateReferenceVerticalAxis();
        }

        public void DeselectVerticalAxis(ReferenceAxis referenceAxis)
        {
            SelectedVerticalAxis.Remove((int)referenceAxis);
            UpdateReferenceVerticalAxis();
        }

        public bool CanApplyZoom(ZoomParam param)
        {
            bool result = false;
            switch (param)
            {
                case ZoomParam.IncreaseHorizontal:
                    result = HorizontalZoom <= 8.0;
                    break;
                case ZoomParam.DecreaseHorizontal:
                    result = HorizontalZoom > -8.0;
                    break;
                case ZoomParam.IncreaseVertical:
                    result = CanIncreaseVerticalZoom();
                    break;
                case ZoomParam.DecreaseVertical:
                    result = CanDecreaseVerticalZooml();
                    break;
            }

            return result;
        }

        public void InitializeOriginalValues()
        {
            if (!originalValuesInitialized)
            {
                originalMaxXValue = MaxXValue;
                int count = MaxYValue.Count;
                originalMaxYValue = new double[count];
                for (int i = 0; i < count; i++)
                {
                    originalMaxYValue[i] = MaxYValue[i];
                }

                originalValuesInitialized = true;
            }
        }

        public void UpdateReferenceVerticalAxis()
        {
            ObservableCollection<NewCurveData> observableCollection = Curves;
            if (observableCollection == null || observableCollection.Count <= 0)
            {
                return;
            }

            double item = MaxYValue.Max();
            int index = MaxYValue.IndexOf(item);
            double num = double.MinValue;
            double num2 = MinYValue[index];
            double num3 = MaxYValue[index];
            int num4 = 1;
            double maxZoom = VerticalZoom.Max();
            IEnumerable<NewCurveData> enumerable;
            if (maxZoom > 1.0)
            {
                IEnumerable<int> maxZoomedAxis =
                    from x in Enumerable.Range(0, 4)
                    where VerticalZoom[x] == maxZoom
                    select x;
                enumerable = curves.Where((NewCurveData x) => maxZoomedAxis.Contains(x.YAxis - 1));
            }
            else
            {
                IEnumerable<NewCurveData> enumerable2;
                if (SelectedVerticalAxis.Count != 0)
                {
                    enumerable2 = Curves.Where((NewCurveData x) => SelectedVerticalAxis.Contains(x.YAxis));
                }
                else
                {
                    IEnumerable<NewCurveData> enumerable3 = Curves;
                    enumerable2 = enumerable3;
                }

                enumerable = enumerable2;
            }

            ReferenceAxis referenceAxis = ReferenceAxis.Y1;
            foreach (NewCurveData item2 in enumerable)
            {
                double num5 = item2.YPoints.Max();
                double num6 = MaxYValue[item2.YAxis - 1];
                double num7 = MinYValue[item2.YAxis - 1];
                double num8 = (num5 - num6) / num6;
                if (num8 > num)
                {
                    num = num8;
                    num2 = num7;
                    num3 = num6;
                    num4 = item2.Index;
                    referenceAxis = (ReferenceAxis)item2.YAxis;
                }
            }

            VerticalScrollBarMaxYReference = num3;
            VerticalScrollBarMinYReference = num2;
            VerticalValuesReferenceCurveIndex = num4;
            ReferenceVerticalZoomAxis = referenceAxis;
        }

        private void AddVerticalZoom(int zoomValueToBeAdded)
        {
            if (SelectedVerticalAxis.Count == 0)
            {
                for (int i = 0; i < GraphConstants.AXIS_NUMBER; i++)
                {
                    VerticalZoom[i] += zoomValueToBeAdded;
                }

                return;
            }

            foreach (int item in SelectedVerticalAxis)
            {
                VerticalZoom[item - 1] += zoomValueToBeAdded;
            }
        }

        private bool CanIncreaseVerticalZoom()
        {
            return CompareVerticalZoom((double x, double y) => x <= y, 8.0);
        }

        private bool CanDecreaseVerticalZooml()
        {
            return CompareVerticalZoom((double x, double y) => x > y, -8.0);
        }

        private bool CompareVerticalZoom(Func<double, double, bool> compareFunc, double zoomLevel)
        {
            bool flag = false;
            if (SelectedVerticalAxis.Count == 0)
            {
                for (int i = 1; i < GraphConstants.AXIS_NUMBER; i++)
                {
                    flag |= compareFunc(VerticalZoom[i], zoomLevel);
                }
            }
            else
            {
                foreach (int item in SelectedVerticalAxis)
                {
                    flag |= compareFunc(VerticalZoom[item - 1], zoomLevel);
                }
            }

            return flag;
        }
    }
}