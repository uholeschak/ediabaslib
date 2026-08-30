using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace BMW.Rheingold.Module.ISTA
{
    [DataContract]
    public class SampledPointsIndexContainer : INotifyPropertyChanged
    {
        [DataMember]
        private readonly ObservableCollection<int> sampledIndexes;
        private readonly double sampleDistance;
        private double lastSampledValue;
        public ObservableCollection<int> SampledIndexes => sampledIndexes;

        public event PropertyChangedEventHandler PropertyChanged;
        public SampledPointsIndexContainer(double minVal, double maxVal, int curveCount, int targetSamplePointsOnXAxis, int maxSamplePointsOnXAxis)
        {
            sampledIndexes = new ObservableCollection<int>();
            int samplePointPerAxisPerCurveNumber = GetSamplePointPerAxisPerCurveNumber(curveCount, targetSamplePointsOnXAxis, maxSamplePointsOnXAxis);
            sampleDistance = GetSampleDistance(minVal, maxVal, samplePointPerAxisPerCurveNumber);
        }

        public bool TryAddingSampleIndex(int index, double value)
        {
            if (index >= 0 && (!sampledIndexes.Any() || Math.Abs(value - lastSampledValue) > sampleDistance))
            {
                SampledIndexes.Add(index);
                lastSampledValue = value;
                return true;
            }

            return false;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double GetSampleDistance(double minVal, double maxVal, int samplePointsPerAxis)
        {
            return (maxVal - minVal) / (double)samplePointsPerAxis;
        }

        private int GetSamplePointPerAxisPerCurveNumber(int curveCount, int targetSamplePointsOnXAxis, int maxSamplePointsOnXAxis)
        {
            if (curveCount > 0)
            {
                int num = targetSamplePointsOnXAxis * 30 / curveCount;
                if (num > maxSamplePointsOnXAxis)
                {
                    return maxSamplePointsOnXAxis;
                }

                return num;
            }

            return targetSamplePointsOnXAxis;
        }
    }
}