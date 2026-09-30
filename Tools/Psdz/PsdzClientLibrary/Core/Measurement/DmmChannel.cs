using BMW.Rheingold.Measurement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using BMW.Rheingold.Measurement.Common.Data;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class DmmChannel : ModelBase
    {
        [DataMember]
        private MeasuringSource source;
        [DataMember]
        private bool isHold;
        [DataMember]
        private bool isMinMax;
        [DataMember]
        private DmmMeasuredValueMinMax measuredValue;
        [DataMember]
        private int channelNo;
        [DataMember]
        private Dictionary<string, MeasuringSource> cache = new Dictionary<string, MeasuringSource>();
        public MeasuringSource Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
                OnPropertyChanged("Source");
            }
        }

        public bool IsHold
        {
            get
            {
                return isHold;
            }

            set
            {
                isHold = value;
                OnPropertyChanged("IsHold");
            }
        }

        public bool IsMinMax
        {
            get
            {
                return isMinMax;
            }

            set
            {
                isMinMax = value;
                OnPropertyChanged("IsMinMax");
            }
        }

        public DmmMeasuredValueMinMax MeasuredValue
        {
            get
            {
                return measuredValue;
            }

            set
            {
                measuredValue = value;
                OnPropertyChanged("MeasuredValue");
            }
        }

        public int ChannelNo
        {
            get
            {
                return channelNo;
            }

            private set
            {
                channelNo = value;
                OnPropertyChanged("ChannelNo");
            }
        }

        public DmmChannelType ChannelData
        {
            get
            {
                DmmChannelType dmmChannelType = null;
                if (Source != null)
                {
                    dmmChannelType = new DmmChannelType();
                    dmmChannelType.SourceName = Source.SensorName.ToString();
                    if (Source.SensorName != MeasuringSensor.None)
                    {
                        dmmChannelType.Coupling = Source.Function.Coupling.ToString();
                        dmmChannelType.Function = Source.Function.FunctionName.ToString();
                        dmmChannelType.Range = Source.Function.Range;
                    }
                }

                return dmmChannelType;
            }
        }

        public event EventHandler<PropertyChangedEventArgs> ModelChanged;
        public void Update(DmmChannel data)
        {
            if (ChannelNo != data.ChannelNo)
            {
                ChannelNo = data.ChannelNo;
            }

            if (IsMinMax != data.IsMinMax)
            {
                IsMinMax = data.IsMinMax;
            }

            if (IsHold != data.IsHold)
            {
                IsHold = data.IsHold;
            }

            if (Source == null)
            {
                Source = data.Source;
            }
            else
            {
                Source.Update(data.Source);
            }

            if (MeasuredValue == null)
            {
                MeasuredValue = data.MeasuredValue;
            }
            else
            {
                MeasuredValue.Update(data.MeasuredValue);
            }
        }

        public DmmChannel()
        {
            Source = new MeasuringSource(MeasuringSensor.None);
            MeasuredValue = new DmmMeasuredValueMinMax();
        }

        public DmmChannel(int no)
        {
            ChannelNo = no;
            if (no != 1 && no != 2)
            {
                throw new ArgumentException("Number \"no\" must not be " + no + ", but 1 or 2.");
            }

            MeasuredValue = new DmmMeasuredValueMinMax();
        }

        public bool SetSource(MeasuringSensor name)
        {
            bool result = true;
            if (Source != null && Source.SensorName == name)
            {
                result = false;
            }

            if (cache.Keys.Contains(name.ToString()))
            {
                Source = cache[name.ToString()];
            }
            else
            {
                Source = new MeasuringSource(name);
                cache.Add(name.ToString(), Source);
            }

            return result;
        }

        public void NotifyModelChanged(string propName)
        {
            ModelChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void Reset()
        {
            cache.Clear();
            IsHold = false;
            IsMinMax = false;
            Source = null;
        }
    }
}