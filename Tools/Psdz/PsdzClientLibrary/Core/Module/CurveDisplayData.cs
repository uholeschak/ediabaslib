using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class CurveDisplayData : INotifyPropertyChanged
    {
        public const double VerticalStepLength = 35.0;
        public const double HorizontalStepLength = 30.0;
        [DataMember]
        private double border1;
        [DataMember]
        private double border2;
        [DataMember]
        private bool curveInPoints;
        [DataMember]
        private int curveThickness;
        [DataMember]
        private double initialMinValue;
        [DataMember]
        private double maxXValue;
        [DataMember]
        private double maxY2Value;
        [DataMember]
        private double maxYValue;
        [DataMember]
        private double minXValue;
        [DataMember]
        private double minY2Value;
        [DataMember]
        private double minYValue;
        [DataMember]
        private string unitX;
        [DataMember]
        private string unitY;
        [DataMember]
        private string unitY2;
        [DataMember]
        private double xTeiler = 1.0;
        [DataMember]
        private int y2AxisColor;
        [DataMember]
        private double y2Teiler = 1.0;
        [DataMember]
        private int yAxisColor;
        [DataMember]
        private double yTeiler = 1.0;
        private Tuple<bool, double>[] borderLinesYValues;
        private double xAxisLength;
        private double yAxisLength;
        private int maxYSteps;
        private int maxY2Steps;
        private int maxXSteps;
        private double yAxisMargin;
        private double[] horizontalChartLineYValues;
        private LineDescriptionData[] horizontalChartLineY2Values;
        private IEnumerable<LineDescriptionData> yLineDescription;
        private IEnumerable<LineDescriptionData> y2LineDescription;
        private IEnumerable<LineDescriptionData> xLineDescription;
        private double[] verticalChartLineXValues;
        private double yPositionOfXAxisDescription;
        private bool isY2Visible;
        private ObservableCollectionEx<CurveDataViewModel> curvesToView = new ObservableCollectionEx<CurveDataViewModel>();
        public double XTeiler
        {
            get
            {
                return xTeiler;
            }

            set
            {
                if (xTeiler != value)
                {
                    if (value > 0.0)
                    {
                        xTeiler = value;
                    }
                    else
                    {
                        Log.Warning("CurveDisplayData.XTeiler", "Value of property 'XTeiler' is lower 0. This is not allowed. The value is instead set to 1.");
                        xTeiler = 1.0;
                    }

                    OnPropertyChanged("XTeiler");
                }
            }
        }

        public double YTeiler
        {
            get
            {
                return yTeiler;
            }

            set
            {
                if (yTeiler != value)
                {
                    if (value > 0.0)
                    {
                        yTeiler = value;
                    }
                    else
                    {
                        Log.Warning("CurveDisplayData.YTeiler", "Value of property 'YTeiler' is lower 0. This is not allowed. The value is instead set to 1.");
                        yTeiler = 1.0;
                    }

                    OnPropertyChanged("YTeiler");
                }
            }
        }

        public double Y2Teiler
        {
            get
            {
                return y2Teiler;
            }

            set
            {
                if (y2Teiler != value)
                {
                    if (value > 0.0)
                    {
                        y2Teiler = value;
                    }
                    else
                    {
                        Log.Warning("CurveDisplayData.Y2Teiler", "Value of property 'Y2Teiler' is lower 0. This is not allowed. The value is instead set to 1.");
                        y2Teiler = 1.0;
                    }

                    OnPropertyChanged("Y2Teiler");
                }
            }
        }

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

        public double MaxYValue
        {
            get
            {
                return maxYValue;
            }

            set
            {
                if (maxYValue != value)
                {
                    maxYValue = value;
                    OnPropertyChanged("MaxYValue");
                }
            }
        }

        public double MaxY2Value
        {
            get
            {
                return maxY2Value;
            }

            set
            {
                if (maxY2Value != value)
                {
                    maxY2Value = value;
                    OnPropertyChanged("MaxY2Value");
                }
            }
        }

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

        public double MinYValue
        {
            get
            {
                return minYValue;
            }

            set
            {
                if (minYValue != value)
                {
                    minYValue = value;
                    OnPropertyChanged("MinYValue");
                }
            }
        }

        public double MinY2Value
        {
            get
            {
                return minY2Value;
            }

            set
            {
                if (minY2Value != value)
                {
                    minY2Value = value;
                    OnPropertyChanged("MinY2Value");
                }
            }
        }

        public double Border1
        {
            get
            {
                return border1;
            }

            set
            {
                if (border1 != value)
                {
                    border1 = value;
                    OnPropertyChanged("Border1");
                }
            }
        }

        public double Border2
        {
            get
            {
                return border2;
            }

            set
            {
                if (border2 != value)
                {
                    border2 = value;
                    OnPropertyChanged("Border2");
                }
            }
        }

        public string UnitX
        {
            get
            {
                return unitX;
            }

            set
            {
                if (unitX != value)
                {
                    unitX = value;
                    OnPropertyChanged("UnitX");
                }
            }
        }

        public string UnitY
        {
            get
            {
                return unitY;
            }

            set
            {
                if (unitY != value)
                {
                    unitY = value;
                    OnPropertyChanged("UnitY");
                }
            }
        }

        public string UnitY2
        {
            get
            {
                return unitY2;
            }

            set
            {
                if (unitY2 != value)
                {
                    unitY2 = value;
                    OnPropertyChanged("UnitY2");
                }
            }
        }

        public int YAxisColor
        {
            get
            {
                return yAxisColor;
            }

            set
            {
                if (yAxisColor != value)
                {
                    yAxisColor = value;
                    OnPropertyChanged("YAxisColor");
                }
            }
        }

        public int Y2AxisColor
        {
            get
            {
                return y2AxisColor;
            }

            set
            {
                if (y2AxisColor != value)
                {
                    y2AxisColor = value;
                    OnPropertyChanged("Y2AxisColor");
                }
            }
        }

        public int CurveThickness
        {
            get
            {
                return curveThickness;
            }

            set
            {
                if (curveThickness != value)
                {
                    curveThickness = value;
                    OnPropertyChanged("CurveThickness");
                }
            }
        }

        public bool CurveInPoints
        {
            get
            {
                return curveInPoints;
            }

            set
            {
                if (curveInPoints != value)
                {
                    curveInPoints = value;
                    OnPropertyChanged("CurveInPoints");
                }
            }
        }

        public double InitialMinValue
        {
            get
            {
                return initialMinValue;
            }

            set
            {
                if (initialMinValue != value)
                {
                    initialMinValue = value;
                    OnPropertyChanged("InitialMinValue");
                }
            }
        }

        public double XAxisLength
        {
            get
            {
                return xAxisLength;
            }

            set
            {
                xAxisLength = value;
                OnPropertyChanged("XAxisLength");
            }
        }

        public double YAxisLength
        {
            get
            {
                return yAxisLength;
            }

            set
            {
                yAxisLength = value;
                OnPropertyChanged("YAxisLength");
            }
        }

        public int MaxYSteps
        {
            get
            {
                return maxYSteps;
            }

            set
            {
                maxYSteps = value;
                OnPropertyChanged("MaxYSteps");
            }
        }

        public int MaxY2Steps
        {
            get
            {
                return maxY2Steps;
            }

            set
            {
                maxY2Steps = value;
                OnPropertyChanged("MaxY2Steps");
            }
        }

        public int MaxXSteps
        {
            get
            {
                return maxXSteps;
            }

            set
            {
                maxXSteps = value;
                OnPropertyChanged("MaxXSteps");
            }
        }

        public double YAxisMargin
        {
            get
            {
                return yAxisMargin;
            }

            set
            {
                yAxisMargin = value;
                OnPropertyChanged("YAxisMargin");
            }
        }

        public double[] HorizontalChartLineYValues
        {
            get
            {
                return horizontalChartLineYValues;
            }

            set
            {
                horizontalChartLineYValues = value;
                OnPropertyChanged("HorizontalChartLineYValues");
            }
        }

        public LineDescriptionData[] HorizontalChartLineY2Values
        {
            get
            {
                return horizontalChartLineY2Values;
            }

            set
            {
                horizontalChartLineY2Values = value;
                OnPropertyChanged("HorizontalChartLineY2Values");
            }
        }

        public double[] VerticalChartLineXValues
        {
            get
            {
                return verticalChartLineXValues;
            }

            set
            {
                verticalChartLineXValues = value;
                OnPropertyChanged("VerticalChartLineXValues");
            }
        }

        public Tuple<bool, double>[] BorderLinesYValues
        {
            get
            {
                return borderLinesYValues;
            }

            set
            {
                borderLinesYValues = value;
                OnPropertyChanged("BorderLinesYValues");
            }
        }

        public IEnumerable<LineDescriptionData> YLineDescription
        {
            get
            {
                return yLineDescription;
            }

            set
            {
                yLineDescription = value;
                OnPropertyChanged("YLineDescription");
            }
        }

        public IEnumerable<LineDescriptionData> Y2LineDescription
        {
            get
            {
                return y2LineDescription;
            }

            set
            {
                y2LineDescription = value;
                OnPropertyChanged("Y2LineDescription");
            }
        }

        public IEnumerable<LineDescriptionData> XLineDescription
        {
            get
            {
                return xLineDescription;
            }

            set
            {
                xLineDescription = value;
                OnPropertyChanged("XLineDescription");
            }
        }

        public double YPositionOfXAxisDescription
        {
            get
            {
                return yPositionOfXAxisDescription;
            }

            set
            {
                yPositionOfXAxisDescription = value;
                OnPropertyChanged("YPositionOfXAxisDescription");
            }
        }

        public ObservableCollectionEx<CurveDataViewModel> CurvesToView
        {
            get
            {
                return curvesToView;
            }

            set
            {
                curvesToView = value;
                OnPropertyChanged("CurvesToView");
            }
        }

        public bool IsY2Visible
        {
            get
            {
                return isY2Visible;
            }

            set
            {
                isY2Visible = value;
                OnPropertyChanged("IsY2Visible");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if ("Border1".Equals(propertyName) || "Border2".Equals(propertyName) || "MinYValue".Equals(propertyName) || "YTeiler".Equals(propertyName))
            {
                UpdateBorderLinesYValues();
            }
            else if ("XTeiler".Equals(propertyName) || "MaxXValue".Equals(propertyName) || "MinXValue".Equals(propertyName))
            {
                UpdateXAxisLength();
                UpdateVerticalChartLineXValues();
            }
            else if ("YTeiler".Equals(propertyName) || "MaxYValue".Equals(propertyName) || "MinYValue".Equals(propertyName))
            {
                UpdateYAxisLength();
                UpdateHorizontalChartLineYValues();
            }
            else if ("Y2Teiler".Equals(propertyName) || "MaxY2Value".Equals(propertyName) || "MinY2Value".Equals(propertyName))
            {
                UpdateMaxY2Steps();
                UpdateY2Visible();
                if (IsY2Visible)
                {
                    UpdateHorizontalChartLineY2Values();
                    return;
                }

                y2LineDescription = new List<LineDescriptionData>();
                HorizontalChartLineY2Values = new LineDescriptionData[0];
            }
            else if ("YAxisLength".Equals(propertyName))
            {
                UpdateMaxYSteps();
                UpdateYAxisMArgin();
                UpdateVerticalChartLineXValues();
                YPositionOfXAxisDescription = YAxisLength + 10.0;
            }
            else if ("XAxisLength".Equals(propertyName))
            {
                UpdateMaxXSteps();
                if (IsY2Visible)
                {
                    UpdateHorizontalChartLineY2Values();
                    return;
                }

                y2LineDescription = new List<LineDescriptionData>();
                HorizontalChartLineY2Values = new LineDescriptionData[0];
            }
            else if ("MaxYSteps".Equals(propertyName))
            {
                UpdateYAxisMArgin();
            }
            else if ("YAxisMargin".Equals(propertyName))
            {
                UpdateHorizontalChartLineYValues();
            }
            else if ("MaxXSteps".Equals(propertyName))
            {
                UpdateVerticalChartLineXValues();
            }
        }

        private void UpdateBorderLinesYValues()
        {
            double[] array = new double[2]
            {
                Border1,
                Border2
            };
            Tuple<bool, double>[] array2 = new Tuple<bool, double>[2];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] <= MaxYValue && array[i] >= MinYValue)
                {
                    double num = array[i];
                    num = (num - MinYValue) / YTeiler;
                    num *= 35.0;
                    num = YAxisLength - num;
                    array2[i] = new Tuple<bool, double>(item1: true, num);
                }
                else
                {
                    array2[i] = new Tuple<bool, double>(item1: false, -1.0);
                }
            }

            BorderLinesYValues = array2;
        }

        private void UpdateXAxisLength()
        {
            double num = ((XTeiler <= 0.0) ? 1.0 : XTeiler);
            XAxisLength = (MaxXValue - MinXValue) / num * 30.0;
        }

        private void UpdateYAxisLength()
        {
            double num = ((YTeiler <= 0.0) ? 1.0 : YTeiler);
            double num2 = (MaxYValue - MinYValue) / num * 35.0;
            if (IsY2Visible)
            {
                double num3 = (MaxY2Value - MinY2Value) / Y2Teiler * 35.0;
                if (num3 > num2)
                {
                    num2 = num3;
                }
            }

            YAxisLength = ((num2 <= 0.0) ? 0.0 : (num2 + 17.5));
        }

        private void UpdateMaxYSteps()
        {
            MaxYSteps = (int)(YAxisLength / 35.0);
        }

        private void UpdateMaxY2Steps()
        {
            MaxY2Steps = (int)((MaxY2Value - MinY2Value) / Y2Teiler);
        }

        private void UpdateMaxXSteps()
        {
            MaxXSteps = (int)(XAxisLength / 30.0);
        }

        private void UpdateYAxisMArgin()
        {
            YAxisMargin = YAxisLength - (double)MaxYSteps * 35.0;
        }

        private void UpdateHorizontalChartLineYValues()
        {
            int num = ((MaxYSteps >= 0) ? MaxYSteps : 0);
            List<LineDescriptionData> list = new List<LineDescriptionData>();
            double[] array = new double[num + 1];
            for (int num2 = num; num2 > 0; num2--)
            {
                array[num2 - 1] = (double)(MaxYSteps - num2) * 35.0 + YAxisMargin;
                double content = (double)num2 * YTeiler + MinYValue;
                list.Add(new LineDescriptionData { Content = content, TextPosition = new Point(10.0, array[num2 - 1] - 8.0) });
            }

            array[num] = YAxisLength;
            list.Add(new LineDescriptionData { Content = MinYValue, TextPosition = new Point(10.0, array[num] - 8.0) });
            YLineDescription = list;
            HorizontalChartLineYValues = array;
        }

        private void UpdateHorizontalChartLineY2Values()
        {
            int num = ((MaxY2Steps >= 0) ? MaxY2Steps : 0);
            int num2 = 0;
            if (num < MaxYSteps)
            {
                num2 = MaxYSteps - num;
            }

            List<LineDescriptionData> list = new List<LineDescriptionData>();
            double[] array = new double[num + 1];
            for (int num3 = num; num3 > 0; num3--)
            {
                array[num3 - 1] = (double)(MaxY2Steps - num3) * 35.0 + YAxisMargin + (double)num2 * 35.0;
                double content = (double)num3 * Y2Teiler + MinY2Value;
                list.Add(new LineDescriptionData { Content = content, TextPosition = new Point(XAxisLength + 10.0, array[num3 - 1] - 8.0), DividerEndPoint = XAxisLength + 6.0 });
            }

            array[num] = YAxisLength;
            list.Add(new LineDescriptionData { Content = MinY2Value, TextPosition = new Point(XAxisLength + 10.0, array[num] - 8.0), DividerEndPoint = XAxisLength + 6.0 });
            Y2LineDescription = list;
            HorizontalChartLineY2Values = array.Select((double y) => new LineDescriptionData { DividerEndPoint = XAxisLength + 6.0, TextPosition = new Point(10.0, y) }).ToArray();
        }

        private void UpdateVerticalChartLineXValues()
        {
            int num = ((MaxXSteps >= 0) ? MaxXSteps : 0);
            double[] array = new double[num + 1];
            List<LineDescriptionData> list = new List<LineDescriptionData>();
            array[num] = 0.0;
            list.Add(new LineDescriptionData { Content = MinXValue, TextPosition = new Point(array[num], 0.0), DividerEndPoint = YAxisLength + 6.0 });
            for (int num2 = num; num2 > 0; num2--)
            {
                array[num2 - 1] = (double)(MaxXSteps - num2 + 1) * 30.0;
                list.Add(new LineDescriptionData { Content = (double)(MaxXSteps - num2 + 1) * XTeiler + MinXValue, TextPosition = new Point(array[num2 - 1], 0.0), DividerEndPoint = YAxisLength + 6.0 });
            }

            XLineDescription = list;
            VerticalChartLineXValues = array;
        }

        public void UpDateCurvesToView(IEnumerable<CurveData> curves)
        {
            if (CurvesToView == null)
            {
                CurvesToView = new ObservableCollectionEx<CurveDataViewModel>();
            }
            else
            {
                CurvesToView.Clear();
            }

            foreach (CurveData curf in curves)
            {
                if (curf.Curve != null)
                {
                    DrawCurve(curf, curf.IsY2);
                }
            }
        }

        private void AddPointsToView(PointCollection points, CurveData data)
        {
            CurveDataViewModel curveDataViewModel = new CurveDataViewModel(data, points, CurveInPoints, null);
            curveDataViewModel.StrokeThickness = CurveThickness;
            CurvesToView.Add(curveDataViewModel);
        }

        private void DrawCurve(CurveData curveData, bool isY2values)
        {
            PointCollection pointCollection = new PointCollection();
            double num;
            double num2;
            double num3;
            string text;
            if (isY2values)
            {
                num = Y2Teiler;
                num2 = MinY2Value;
                num3 = MaxY2Value;
                text = UnitY2;
            }
            else
            {
                num = YTeiler;
                num2 = MinYValue;
                num3 = MaxYValue;
                text = UnitY;
            }

            Matrix matrix = new Matrix(30.0 / XTeiler, 0.0, 0.0, -35.0 / num, 0.0, 0.0);
            Point point = new Point(0.0, YAxisLength);
            Point point2 = new Point(MinXValue, num2);
            foreach (Tuple<double, double> item3 in curveData.Curve)
            {
                double item = item3.Item1;
                double item2 = item3.Item2;
                Point point3 = new Point(item, item2);
                if (item > MaxXValue || item < MinXValue)
                {
                    continue;
                }

                if (item2 >= num2 && item2 <= num3)
                {
                    Point value = point + matrix.Transform(point3 - point2);
                    if (CurveInPoints)
                    {
                        PointCollection pointCollection2 = new PointCollection();
                        Point value2 = new Point(value.X + (double)CurveThickness, value.Y);
                        Point value3 = new Point(value.X - (double)CurveThickness, value.Y);
                        Point value4 = new Point(value.X, value.Y + (double)CurveThickness);
                        Point value5 = new Point(value.X, value.Y - (double)CurveThickness);
                        pointCollection2.Add(value2);
                        pointCollection2.Add(value4);
                        pointCollection2.Add(value3);
                        pointCollection2.Add(value5);
                        pointCollection2.Add(value2);
                        pointCollection2.Add(value4);
                        string toolTipContent = UnitX + "= " + item3.Item1.ToString("######0.000") + "/ " + text + "= " + item3.Item2.ToString("######0.000");
                        CurveDataViewModel curveDataViewModel = new CurveDataViewModel(curveData, pointCollection2, CurveInPoints, toolTipContent);
                        curveDataViewModel.StrokeThickness = CurveThickness + 2;
                        CurvesToView.Add(curveDataViewModel);
                    }
                    else
                    {
                        pointCollection.Add(value);
                    }
                }
                else if (!CurveInPoints)
                {
                    AddPointsToView(pointCollection, curveData);
                }
            }

            if (!CurveInPoints)
            {
                AddPointsToView(pointCollection, curveData);
            }
        }

        private void UpdateY2Visible()
        {
            IsY2Visible = !Y2Teiler.Equals(1.0) || !MinY2Value.Equals(0.0) || !MaxY2Value.Equals(0.0);
        }

        public void Update()
        {
            UpdateXAxisLength();
            UpdateYAxisLength();
            UpdateMaxYSteps();
            UpdateMaxXSteps();
            UpdateYAxisMArgin();
            UpdateHorizontalChartLineYValues();
            UpdateVerticalChartLineXValues();
            UpdateBorderLinesYValues();
            UpdateY2Visible();
            if (IsY2Visible)
            {
                UpdateMaxY2Steps();
                UpdateHorizontalChartLineY2Values();
            }
        }
    }
}