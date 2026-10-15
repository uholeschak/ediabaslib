using BMW.Rheingold.CoreFramework.DatabaseProvider;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class BarControlViewModel : NotifyPropertyChangedBase
    {
        [DataMember]
        private string m_TopText = string.Empty;

        [DataMember]
        private string m_RightText = string.Empty;

        [DataMember]
        private double m_CurrentValue = double.NaN;

        [DataMember]
        private double m_MinValue = double.NaN;

        [DataMember]
        private double m_MaxValue = double.NaN;

        [DataMember]
        private double m_CurrentUIValue = double.NaN;

        [DataMember]
        private bool m_IsValuePointerVisible;

        [DataMember]
        private bool m_IsOverflowPointerVisible;

        [DataMember]
        private bool m_IsUnderflowPointerVisible;

        [DataMember]
        private int m_ScalarValuesPosition;

        [DataMember]
        private int m_BlockNumber;

        [DataMember]
        private int m_MajorDivisions;

        [DataMember]
        private int m_MinorDivisions;

        [DataMember]
        private ScalarValuesControlViewModel m_ScalarValuesViewModel;

        [DataMember]
        private BarColorsControlViewModel m_BarColorsViewModel;

        [DataMember]
        private ScalarLinesControlViewModel m_ScalarLinesControlViewModel;

        [DataMember]
        private string m_IndicatorBorderColor;

        [DataMember]
        private int m_BarIndex;

        [DataMember]
        private int m_BlockBarIndex;

        public BarColorsControlViewModel BarColorsControlViewModel
        {
            get
            {
                return m_BarColorsViewModel;
            }
            set
            {
                if (value != m_BarColorsViewModel)
                {
                    m_BarColorsViewModel = value;
                    OnPropertyChanged("BarColorsControlViewModel");
                }
            }
        }

        public ScalarValuesControlViewModel ScalarValuesControlViewModel
        {
            get
            {
                return m_ScalarValuesViewModel;
            }
            set
            {
                if (m_ScalarValuesViewModel != value)
                {
                    m_ScalarValuesViewModel = value;
                    OnPropertyChanged("ScalarValuesControlViewModel");
                }
            }
        }

        public ScalarLinesControlViewModel ScalarLinesControlViewModel
        {
            get
            {
                return m_ScalarLinesControlViewModel;
            }
            set
            {
                if (m_ScalarLinesControlViewModel != value)
                {
                    m_ScalarLinesControlViewModel = value;
                    OnPropertyChanged("ScalarLinesControlViewModel");
                }
            }
        }

        public double CurrentValue
        {
            get
            {
                return m_CurrentValue;
            }
            set
            {
                if (value != m_CurrentValue)
                {
                    m_CurrentValue = value;
                    UpdateCurrentUIValueAndPointerVisibility();
                    OnPropertyChanged("CurrentValue");
                }
            }
        }

        public double MinValue
        {
            get
            {
                return m_MinValue;
            }
            set
            {
                if (value != m_MinValue)
                {
                    m_MinValue = value;
                    UpdateCurrentUIValueAndPointerVisibility();
                    OnPropertyChanged("MinValue");
                }
            }
        }

        public double MaxValue
        {
            get
            {
                return m_MaxValue;
            }
            set
            {
                if (value != m_MaxValue)
                {
                    m_MaxValue = value;
                    UpdateCurrentUIValueAndPointerVisibility();
                    OnPropertyChanged("MaxValue");
                }
            }
        }

        public string TopText
        {
            get
            {
                return m_TopText;
            }
            set
            {
                if (value != m_TopText)
                {
                    m_TopText = value;
                    OnPropertyChanged("TopText");
                }
            }
        }

        public string RightText
        {
            get
            {
                return m_RightText;
            }
            set
            {
                if (value != m_RightText)
                {
                    m_RightText = value;
                    OnPropertyChanged("RightText");
                }
            }
        }

        public int ScalarValuesPosition
        {
            get
            {
                return m_ScalarValuesPosition;
            }
            set
            {
                if (value != m_ScalarValuesPosition)
                {
                    m_ScalarValuesPosition = value;
                    OnPropertyChanged("ScalarValuesPosition");
                }
            }
        }

        public int BlockNumber
        {
            get
            {
                return m_BlockNumber;
            }
            set
            {
                if (value != m_BlockNumber)
                {
                    m_BlockNumber = value;
                    OnPropertyChanged("BlockNumber");
                }
            }
        }

        public int MajorDivisions
        {
            get
            {
                return m_MajorDivisions;
            }
            set
            {
                if (m_MajorDivisions != value)
                {
                    m_MajorDivisions = value;
                    OnPropertyChanged("MajorDivisions");
                }
            }
        }

        public int MinorDivisions
        {
            get
            {
                return m_MinorDivisions;
            }
            set
            {
                if (m_MinorDivisions != value)
                {
                    m_MinorDivisions = value;
                    OnPropertyChanged("MinorDivisions");
                }
            }
        }

        public double CurrentUIValue
        {
            get
            {
                return m_CurrentUIValue;
            }
            set
            {
                if (m_CurrentUIValue != value)
                {
                    m_CurrentUIValue = value;
                    OnPropertyChanged("CurrentUIValue");
                }
            }
        }

        public bool IsValuePointerVisible
        {
            get
            {
                return m_IsValuePointerVisible;
            }
            set
            {
                if (m_IsValuePointerVisible != value)
                {
                    m_IsValuePointerVisible = value;
                    OnPropertyChanged("IsValuePointerVisible");
                }
            }
        }

        public bool IsOverflowPointerVisible
        {
            get
            {
                return m_IsOverflowPointerVisible;
            }
            set
            {
                if (m_IsOverflowPointerVisible != value)
                {
                    m_IsOverflowPointerVisible = value;
                    OnPropertyChanged("IsOverflowPointerVisible");
                }
            }
        }

        public bool IsUnderflowPointerVisible
        {
            get
            {
                return m_IsUnderflowPointerVisible;
            }
            set
            {
                if (m_IsUnderflowPointerVisible != value)
                {
                    m_IsUnderflowPointerVisible = value;
                    OnPropertyChanged("IsUnderflowPointerVisible");
                }
            }
        }

        public string IndicatorBorderColor
        {
            get
            {
                return m_IndicatorBorderColor;
            }
            set
            {
                if (m_IndicatorBorderColor != value)
                {
                    m_IndicatorBorderColor = value;
                    OnPropertyChanged("IndicatorBorderColor");
                }
            }
        }

        public int BarIndex
        {
            get
            {
                return m_BarIndex;
            }
            set
            {
                m_BarIndex = value;
            }
        }

        public int BlockBarIndex
        {
            get
            {
                return m_BlockBarIndex;
            }
            set
            {
                m_BlockBarIndex = value;
            }
        }

        public bool IsActive => BlockNumber != 0;

        public BarControlViewModel()
        {
            ScalarValuesControlViewModel = new ScalarValuesControlViewModel(this);
            ScalarLinesControlViewModel = new ScalarLinesControlViewModel(this);
            BarColorsControlViewModel = new BarColorsControlViewModel(this);
        }

        private void UpdateCurrentUIValueAndPointerVisibility()
        {
            if (double.IsNaN(CurrentValue) || double.IsNaN(MinValue) || double.IsNaN(MaxValue) || MaxValue <= MinValue)
            {
                bool flag = (IsValuePointerVisible = false);
                bool isOverflowPointerVisible = (IsUnderflowPointerVisible = flag);
                IsOverflowPointerVisible = isOverflowPointerVisible;
            }
            else
            {
                CurrentUIValue = -25.0 + (CurrentValue - MinValue) * 192.0 / (MaxValue - MinValue);
                IsValuePointerVisible = MinValue <= CurrentValue && CurrentValue <= MaxValue;
                IsOverflowPointerVisible = CurrentValue > MaxValue;
                IsUnderflowPointerVisible = CurrentValue < MinValue;
            }
        }
    }
}
