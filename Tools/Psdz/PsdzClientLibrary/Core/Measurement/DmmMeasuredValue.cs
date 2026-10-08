using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class DmmMeasuredValue : ModelBase
    {
        [DataMember]
        private float value;

        [DataMember]
        private MeasuringRangeStatus rangeStatus;

        [DataMember]
        private string unit;

        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public MeasuringRangeStatus RangeStatus
        {
            get
            {
                return rangeStatus;
            }
            set
            {
                rangeStatus = value;
                OnPropertyChanged("RangeStatus");
            }
        }

        public string Unit
        {
            get
            {
                return unit;
            }
            set
            {
                unit = value;
                OnPropertyChanged("Unit");
            }
        }

        public void Update(DmmMeasuredValue data)
        {
            if (Value != data.Value)
            {
                Value = data.Value;
            }
            if (RangeStatus != data.RangeStatus)
            {
                RangeStatus = data.RangeStatus;
            }
            if (Unit != data.Unit)
            {
                Unit = data.Unit;
            }
        }
    }
}
