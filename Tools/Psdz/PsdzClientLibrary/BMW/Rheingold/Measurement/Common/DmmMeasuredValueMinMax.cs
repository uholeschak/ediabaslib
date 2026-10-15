using System;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class DmmMeasuredValueMinMax : DmmMeasuredValue
    {
        [DataMember]
        private float max;
        [DataMember]
        private bool resetMinMax;
        [DataMember]
        private float min;
        [DataMember]
        private string range;
        public float Max
        {
            get
            {
                return max;
            }

            private set
            {
                max = value;
                OnPropertyChanged("Max");
            }
        }

        public float Min
        {
            get
            {
                return min;
            }

            private set
            {
                min = value;
                OnPropertyChanged("Min");
            }
        }

        public string Range
        {
            get
            {
                return range;
            }

            private set
            {
                range = value;
                OnPropertyChanged("Range");
            }
        }

        public event EventHandler MeasuredValueChanged;
        public void Update(DmmMeasuredValueMinMax data)
        {
            Update((DmmMeasuredValue)data);
            if (Range != data.Range)
            {
                Range = data.Range;
            }

            if (Min != data.Min)
            {
                Min = data.Min;
            }

            if (Max != data.Max)
            {
                Max = data.Max;
            }
        }

        public void SetMeasuredValue(float readValue, string range, MeasuringRangeStatus readRangeStatus, bool setMinMax, string unit)
        {
            bool flag = false;
            Unit = unit;
            if (setMinMax)
            {
                if (resetMinMax)
                {
                    Min = readValue;
                    Max = readValue;
                    resetMinMax = false;
                    flag = true;
                }
                else
                {
                    if (readValue < Min)
                    {
                        Min = readValue;
                        flag = true;
                    }

                    if (readValue > Max)
                    {
                        Max = readValue;
                        flag = true;
                    }
                }
            }
            else if (!resetMinMax)
            {
                resetMinMax = true;
                Min = 0f;
                Max = 0f;
                flag = true;
            }

            flag = flag || RangeStatus != readRangeStatus;
            RangeStatus = readRangeStatus;
            flag = flag || Value != readValue;
            Value = readValue;
            flag = flag || (!string.IsNullOrEmpty(range) && Range != range);
            Range = (string.IsNullOrEmpty(range) ? string.Empty : range);
            if (flag && MeasuredValueChanged != null)
            {
                MeasuredValueChanged(this, EventArgs.Empty);
            }
        }
    }
}