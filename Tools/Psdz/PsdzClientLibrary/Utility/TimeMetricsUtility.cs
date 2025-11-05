using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Instrumentation;

namespace PsdzClient.Utility
{
    public class TimeMetricsUtility
    {
        private static Lazy<TimeMetricsUtility> instance = new Lazy<TimeMetricsUtility>(() => new TimeMetricsUtility(), isThreadSafe: true);

        //private static TimeMetrics currentMetrics;

        private static bool metricsEnabled;

        private bool apiEnabled;

        private bool fastaApiEnabled;

        private int fastaApiDetailsCount;

        //private static List<TimeMetrics> metrics = new List<TimeMetrics>();

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

        // [UH] modified
        public void ApiJobEnd(string ecu, string job, string args, int argsLength)
        {
            //currentMetrics?.ApiJobEnd(ecu, job, args, argsLength);
        }

        // [UH] modified
        public void ApiJobStart(string ecu, string job, string args, int argsLength)
        {
            //currentMetrics?.ApiJobStart(ecu, job, args, argsLength);
        }

        // [UH] modified
        public void Sleep(int milliseconds, string reason)
        {
            //currentMetrics?.Sleep(milliseconds, reason);
        }
    }
}
