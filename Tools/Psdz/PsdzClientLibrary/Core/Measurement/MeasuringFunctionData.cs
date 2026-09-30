using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class MeasuringFunctionData : ModelBase, ICloneable
    {
        private MeasuringCoupling coupling;

        private MeasuringFunction functionName;

        private IEnumerable<MeasuringCoupling> notSupportedCouplings;

        private string range;

        private string rangeOnDevice;

        private IList<string> rangeList;

        private MeasuringFunctionData cache;

        [DataMember]
        public MeasuringCoupling Coupling
        {
            get
            {
                return coupling;
            }
            set
            {
                coupling = value;
                OnPropertyChanged("Coupling");
            }
        }

        [DataMember]
        public MeasuringFunction FunctionName
        {
            get
            {
                return functionName;
            }
            set
            {
                functionName = value;
                OnPropertyChanged("FunctionName");
            }
        }

        [DataMember]
        public IEnumerable<MeasuringCoupling> NotSupportedCouplings
        {
            get
            {
                return notSupportedCouplings;
            }
            set
            {
                notSupportedCouplings = value;
                OnPropertyChanged("NotSupportedCouplings");
            }
        }

        [DataMember]
        public string Range
        {
            get
            {
                return range;
            }
            set
            {
                range = value;
                if (cache != null)
                {
                    cache.range = value;
                }
                OnPropertyChanged("Range");
            }
        }

        [DataMember]
        public string RangeOnDevice
        {
            get
            {
                return rangeOnDevice;
            }
            set
            {
                rangeOnDevice = value;
                OnPropertyChanged("RangeOnDevice");
            }
        }

        [DataMember]
        public IList<string> RangeList
        {
            get
            {
                return rangeList;
            }
            set
            {
                rangeList = value;
                if (cache != null)
                {
                    cache.rangeList = value;
                }
                OnPropertyChanged("RangeList");
            }
        }

        public MeasuringFunctionData()
        {
        }

        public MeasuringFunctionData(MeasuringFunction name, MeasuringCoupling coupling, MeasuringFunctionData cache = null)
        {
            FunctionName = name;
            Coupling = coupling;
            this.cache = cache;
        }

        public MeasuringFunctionData(MeasuringFunctionData source)
        {
            Coupling = source.Coupling;
            FunctionName = source.FunctionName;
            Range = source.Range;
            if (source.RangeList != null)
            {
                RangeList = new List<string>(source.RangeList);
            }
        }

        public void Update(MeasuringFunctionData data)
        {
            cache = data;
            if (Coupling != data.Coupling)
            {
                Coupling = data.Coupling;
            }
            if (FunctionName != data.FunctionName)
            {
                FunctionName = data.FunctionName;
            }
            NotSupportedCouplings = data.NotSupportedCouplings;
            if (Range != data.Range)
            {
                Range = data.Range;
            }
            if (RangeOnDevice != data.RangeOnDevice)
            {
                RangeOnDevice = data.RangeOnDevice;
            }
            RangeList = data.RangeList;
        }

        public object Clone()
        {
            return new MeasuringFunctionData(this);
        }

        public override bool Equals(object obj)
        {
            MeasuringFunctionData measuringFunctionData = obj as MeasuringFunctionData;
            if (measuringFunctionData == null)
            {
                return false;
            }
            if (Coupling == measuringFunctionData.Coupling && FunctionName == measuringFunctionData.FunctionName && Range == measuringFunctionData.Range)
            {
                if (measuringFunctionData.RangeList != null || RangeList != null)
                {
                    if (measuringFunctionData.RangeList != null && RangeList != null)
                    {
                        return IsEqual(RangeList, measuringFunctionData.RangeList);
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        public static bool IsEqual(IList<string> a, IList<string> b)
        {
            if (a == null)
            {
                if (b != null)
                {
                    return false;
                }
                return true;
            }
            if (b == null || a.Count != b.Count)
            {
                return false;
            }
            for (int i = 0; i < a.Count; i++)
            {
                if (!a[i].Equals(b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return Coupling.GetHashCode() ^ FunctionName.GetHashCode() ^ Range.GetHashCode() ^ RangeList.GetHashCode();
        }

        public static bool operator ==(MeasuringFunctionData a, MeasuringFunctionData b)
        {
            if ((object)a == b)
            {
                return true;
            }
            if ((object)a == null || (object)b == null)
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(MeasuringFunctionData a, MeasuringFunctionData b)
        {
            return !(a == b);
        }
    }
}
