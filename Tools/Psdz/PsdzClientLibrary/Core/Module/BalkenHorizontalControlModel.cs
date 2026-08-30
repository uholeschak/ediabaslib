using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class BalkenHorizontalControlModel : INotifyPropertyChanged
    {
        private const int colorRed = 11;

        private Regex formatRegex = new Regex("(?<=FORMAT=\")[^\"]+");

        [DataMember]
        private string txtOverBalkenTextbox;

        [DataMember]
        private string txtBalkenTextbox;

        [DataMember]
        private double barValue;

        [DataMember]
        private string barValueFormat;

        [DataMember]
        private double barMin;

        [DataMember]
        private double barMax;

        [DataMember]
        private int barColor;

        [DataMember]
        private double barUpperLimit;

        [DataMember]
        private double barLowerLimit;

        public string TxtOverBalkenTextbox
        {
            get
            {
                return txtOverBalkenTextbox;
            }
            set
            {
                if (txtOverBalkenTextbox != value)
                {
                    txtOverBalkenTextbox = value;
                    OnPropertyChanged("TxtOverBalkenTextbox");
                }
            }
        }

        public string TxtBalkenTextbox
        {
            get
            {
                return txtBalkenTextbox;
            }
            set
            {
                if (txtBalkenTextbox != value)
                {
                    txtBalkenTextbox = value;
                    OnPropertyChanged("TxtBalkenTextbox");
                }
            }
        }

        public double BarValue
        {
            get
            {
                return barValue;
            }
            set
            {
                if (barValue != value)
                {
                    barValue = value;
                    OnPropertyChanged("BarValue");
                }
            }
        }

        public string BarValueFormat
        {
            get
            {
                return barValueFormat;
            }
            set
            {
                if (barValueFormat != value)
                {
                    barValueFormat = value;
                    OnPropertyChanged("BarValueFormat");
                }
            }
        }

        public double BarMin
        {
            get
            {
                return barMin;
            }
            set
            {
                if (barMin != value)
                {
                    barMin = value;
                    OnPropertyChanged("BarMin");
                }
            }
        }

        public double BarMax
        {
            get
            {
                return barMax;
            }
            set
            {
                if (barMax != value)
                {
                    barMax = value;
                    OnPropertyChanged("BarMax");
                }
            }
        }

        public int BarColor
        {
            get
            {
                return barColor;
            }
            set
            {
                if (barColor != value)
                {
                    barColor = value;
                    OnPropertyChanged("BarColor");
                }
            }
        }

        public double BarUpperLimit
        {
            get
            {
                return barUpperLimit;
            }
            set
            {
                if (barUpperLimit != value)
                {
                    barUpperLimit = value;
                    OnPropertyChanged("BarUpperLimit");
                }
            }
        }

        public double BarLowerLimit
        {
            get
            {
                return barLowerLimit;
            }
            set
            {
                if (barLowerLimit != value)
                {
                    barLowerLimit = value;
                    OnPropertyChanged("BarLowerLimit");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BalkenHorizontalControlModel()
        {
            BarValue = 0.0;
            BarMin = 0.0;
            BarMax = 100.0;
            BarColor = 11;
            BarUpperLimit = 0.0;
            BarLowerLimit = 0.0;
        }

        internal void SetValues(IList<string> lang, ParameterContainer inParam, ParameterContainer outParam, ParameterContainer inoutParam, string identifier)
        {
            try
            {
                ITextLocator textLocator;
                double value;
                if (string.IsNullOrEmpty(identifier))
                {
                    textLocator = inParam.getParameter("i_OverBalkenTextbox", null) as ITextLocator;
                    value = Convert.ToDouble(inParam.getParameter("i_OBalkenfarbgrenze", 0));
                    double value2 = Convert.ToDouble(inParam.getParameter("i_UBalkenfarbgrenze", 0));
                    LowerLimit(value2);
                }
                else
                {
                    textLocator = inParam.getParameter($"i_OverBalken{identifier}Text", null) as ITextLocator;
                    value = Convert.ToDouble(inParam.getParameter($"i_Balken{identifier}farbgrenze", 0));
                }
                ITextContent textContent = inParam.getParameter($"i_Balken{identifier}Textbox", null) as ITextContent;
                TxtOverBalkenTextbox = textLocator?.TextContent.GetTextForUI(lang)[0].TextItem;
                TxtBalkenTextbox = textContent?.GetTextForUI(lang)[0].TextItem;
                BarValue = Convert.ToDouble(inoutParam.getParameter($"i_Balkenwert{identifier}", 0.0));
                Minimum(Convert.ToDouble(inParam.getParameter($"i_Balken{identifier}Min", 0.0)));
                Maximum(Convert.ToDouble(inParam.getParameter($"i_Balken{identifier}Max", 0.0)));
                BarColor = Convert.ToInt32(inParam.getParameter($"i_Balken{identifier}farbe", 0));
                if (textLocator != null)
                {
                    Match match = formatRegex.Match(textLocator.Text);
                    BarValueFormat = (match.Success ? match.Value : string.Empty);
                }
                UpperLimit(value);
            }
            catch (Exception exception)
            {
                Log.ErrorException("BalkenHorizontalControlModel.SetValues()", exception);
            }
        }

        private void LowerLimit(double value)
        {
            if (value > BarMin && value < barMax)
            {
                BarLowerLimit = value;
            }
            else
            {
                BarLowerLimit = 0.0;
            }
        }

        private void UpperLimit(double value)
        {
            if (value > BarMin && value < barMax)
            {
                BarUpperLimit = value;
            }
            else
            {
                BarUpperLimit = 0.0;
            }
        }

        private void Maximum(double value)
        {
            BarMax = value;
            LowerLimit(BarLowerLimit);
            UpperLimit(BarUpperLimit);
        }

        private void Minimum(double value)
        {
            BarMin = value;
            LowerLimit(BarLowerLimit);
            UpperLimit(BarUpperLimit);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
