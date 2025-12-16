using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CS0169,CS0414
namespace PsdzClient.Utility
{
    [PreserveSource(Hint = "Simplified")]
    public class TimeMetricsUtility
    {
        private static Lazy<TimeMetricsUtility> instance = new Lazy<TimeMetricsUtility>(() => new TimeMetricsUtility(), isThreadSafe: true);

        [PreserveSource(Hint = "TimeMetrics", Placeholder = true)]
        private static PlaceholderType currentMetrics;

        private static bool metricsEnabled;

        private bool apiEnabled;

        private bool fastaApiEnabled;

        private int fastaApiDetailsCount;

        [PreserveSource(Hint = "List<TimeMetrics>", Placeholder = true)]
        private static PlaceholderType metrics = new PlaceholderType();

        public static TimeMetricsUtility Instance => instance.Value;

        private TimeMetricsUtility()
        {
            metricsEnabled = ShouldEnableMetrics();
            if (metricsEnabled)
            {
                apiEnabled = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Diagnostics.VehicleTestApiMetricsEnabled", defaultValue: true);
                fastaApiEnabled = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Diagnostics.VehicleTestFastaApiMetricsEnabled", defaultValue: false);
                fastaApiDetailsCount = ConfigSettings.getConfigint("BMW.Rheingold.Diagnostics.VehicleTestMetricsFastaApiDetailsCount", 0);
            }
        }

        public static bool ShouldEnableMetrics()
        {
            if (Process.GetCurrentProcess().ProcessName != "IstaOperation")
            {
                return false;
            }
            bool defaultValue = false;
            using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
            {
                if (istaIcsServiceClient.IsAvailable())
                {
                    defaultValue = istaIcsServiceClient.GetVehicleMetricsEnabled();
                }
            }
            return ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Diagnostics.VehicleTestMetricsEnabled", defaultValue);
        }

        [PreserveSource(Hint = "Cleaned")]
        public void ApiJobEnd(string ecu, string job, string args, int argsLength)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void ApiJobStart(string ecu, string job, string args, int argsLength)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void InitializePsdzStart()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void InitializePsdzStop()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void PopupClosed(string description)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void PopupShown(string description)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void PsdzUsageStart()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void PsdzUsageStop()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void RuleEnd()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void RuleStart()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void Sleep(int milliseconds, string reason)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void TestModuleEnd(string name)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void TestModuleStart(string name)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void VehicleCommunicationEnd(object[] args)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void VehicleCommunicationStart(object[] args, string methodName)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void Start(TimeMetricsStage stage)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public void Stop()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public List<MetricApiJob> GetSlowFastaJobs()
        {
            return new List<MetricApiJob>();
        }

        [PreserveSource(Hint = "Cleaned")]
        public TimeSpan GetPopupDuration()
        {
            return TimeSpan.Zero;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetCurrentlyRunningModuleName()
        {
            return "ISTA";
        }
    }
}
