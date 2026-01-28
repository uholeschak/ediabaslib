using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CS0169, CS0414
namespace PsdzClient.Utility
{
    public class TimeMetricsUtility
    {
        private static Lazy<TimeMetricsUtility> instance = new Lazy<TimeMetricsUtility>(() => new TimeMetricsUtility(), isThreadSafe: true);
        [PreserveSource(Hint = "TimeMetrics", Placeholder = true)]
        private static PlaceholderType currentMetrics;
        private bool metricsEnabled;
        private bool apiEnabled;
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

        [PreserveSource(Cleaned = true)]
        public void DumpResults()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void ApiJobEnd(string ecu, string job, string args, int argsLength)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void ApiJobStart(string ecu, string job, string args, int argsLength)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void BackendCallEnd(BackendServiceType type)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void BackendCallStart(BackendServiceType type)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void DbQueryEnd()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void DbQueryStart()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void FastaEnd()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void FastaStart()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void InitializePsdzStart()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void InitializePsdzStop()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void PopupClosed(string description)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void PopupShown(string description)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void PsdzUsageStart()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void PsdzUsageStop()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void RuleEnd()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void RuleStart()
        {
        }

        [PreserveSource(Cleaned = true)]
        public void Sleep(int milliseconds, string reason)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void TestModuleEnd(string name)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void TestModuleStart(string name)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void VehicleCommunicationEnd(object[] args)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void VehicleCommunicationStart(object[] args, string methodName)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void Start(TimeMetricsStage stage)
        {
        }

        [PreserveSource(Cleaned = true)]
        public void Stop()
        {
        }

        [PreserveSource(Cleaned = true)]
        public List<MetricApiJob> GetSlowFastaJobs()
        {
            return new List<MetricApiJob>();
        }

        [PreserveSource(Cleaned = true)]
        public TimeSpan GetPopupDuration()
        {
            return TimeSpan.Zero;
        }

        [PreserveSource(Cleaned = true)]
        public string GetCurrentlyRunningModuleName()
        {
            return "ISTA";
        }
    }
}