using System.Diagnostics;

namespace BMW.Rheingold.CoreFramework.Metrics
{
    public class Metric
    {
        public long Counter { get; set; }

        public Stopwatch Watch { get; set; } = new Stopwatch();

        public int ExecutionCount { get; set; }
    }
}