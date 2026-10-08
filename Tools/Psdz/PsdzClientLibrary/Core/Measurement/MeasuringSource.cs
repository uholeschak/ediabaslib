using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Measurement.Common
{
    [DataContract]
    public class MeasuringSource : ModelBase
    {
        [DataMember]
        private IEnumerable<MeasuringCoupling> notSupportedCouplings;

        [DataMember]
        private MeasuringFunctionData function;

        [DataMember]
        private MeasuringSensor sensorName;

        [DataMember]
        private Dictionary<string, MeasuringFunctionData> cache = new Dictionary<string, MeasuringFunctionData>();

        [DataMember]
        public MeasuringSensor SensorName
        {
            get
            {
                return sensorName;
            }
            set
            {
                sensorName = value;
                OnPropertyChanged("SensorName");
            }
        }

        [DataMember]
        public MeasuringFunctionData Function
        {
            get
            {
                return function;
            }
            set
            {
                function = value;
                OnPropertyChanged("Function");
            }
        }

        public static IEnumerable<MeasuringSensor> NotSupportedSensors => new HashSet<MeasuringSensor>
    {
        MeasuringSensor.KVClip,
        MeasuringSensor.RZVCable,
        MeasuringSensor.TDCable,
        MeasuringSensor.TriggerClamp
    };

        public IEnumerable<MeasuringCoupling> NotSupportedCouplings
        {
            get
            {
                return notSupportedCouplings;
            }
            set
            {
                if (value != null)
                {
                    notSupportedCouplings = new HashSet<MeasuringCoupling>(value);
                }
                else
                {
                    notSupportedCouplings = new HashSet<MeasuringCoupling>();
                }
            }
        }

        public MeasuringSource(MeasuringSensor name)
        {
            SensorName = name;
            notSupportedCouplings = new HashSet<MeasuringCoupling>();
        }

        public void Update(MeasuringSource data)
        {
            if (SensorName != data.SensorName)
            {
                SensorName = data.SensorName;
            }
            Function.Update(data.Function);
        }

        public bool LoadFunctionDataFromCache(MeasuringFunction name, MeasuringCoupling coupling)
        {
            MeasuringFunctionData obj = null;
            if (Function != null)
            {
                obj = new MeasuringFunctionData(Function);
            }
            string id = GetId(name, coupling);
            if (cache.Keys.Contains(id))
            {
                Function.Update(cache[id]);
            }
            else
            {
                MeasuringFunctionData value = new MeasuringFunctionData(name, coupling);
                Function = new MeasuringFunctionData(name, coupling, value);
                cache.Add(id, value);
            }
            return !Function.Equals(obj);
        }

        private string GetId(MeasuringFunction name, MeasuringCoupling coupling)
        {
            return name.ToString() + ":" + coupling;
        }

        public bool UpdateFunction(MeasuringFunctionData functionPar)
        {
            List<string> list = new List<string>();
            MeasuringFunctionData measuringFunctionData = functionPar;
            if (measuringFunctionData == null)
            {
                measuringFunctionData = new MeasuringFunctionData(MeasuringFunction.None, MeasuringCoupling.DC);
            }
            bool result = LoadFunctionDataFromCache(measuringFunctionData.FunctionName, measuringFunctionData.Coupling) || !IsEqual(function.Range, measuringFunctionData.Range) || !IsEqual(function.RangeList, measuringFunctionData.RangeList);
            if (!IsEqual(function.RangeList, measuringFunctionData.RangeList))
            {
                Function.RangeList = measuringFunctionData.RangeList;
            }
            if (!IsEqual(function.Range, measuringFunctionData.Range))
            {
                Function.Range = measuringFunctionData.Range;
            }
            NotSupportedCouplings = functionPar.NotSupportedCouplings;
            Function.NotSupportedCouplings = functionPar.NotSupportedCouplings;
            list.Add("NotSupportedCoupling");
            foreach (string item in list)
            {
                OnPropertyChanged(item);
            }
            return result;
        }

        private static bool IsEqual(IList<string> a, IList<string> b)
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
                if (!IsEqual(a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsEqual(string a, string b)
        {
            if (a == null)
            {
                if (b != null)
                {
                    return false;
                }
                return true;
            }
            return a.Equals(b);
        }
    }
}
