using System.Diagnostics;

namespace PsdzClient.Utility
{
    public class Metric
    {
        public long Counter { get; set; }

        public Stopwatch Watch { get; set; } = new Stopwatch();

        public int ExecutionCount { get; set; }
    }
}