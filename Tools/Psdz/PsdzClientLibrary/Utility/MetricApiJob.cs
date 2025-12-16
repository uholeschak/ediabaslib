using System;

namespace PsdzClient.Utility
{
    public class MetricApiJob : Metric
    {
        private string ecu;

        private string job;

        private string args;

        private string source;

        public string Ecu => ecu ?? string.Empty;

        public string Job => job;

        public string Args => args;

        public string Source => source;

        public DateTime StartTime { get; } = DateTime.UtcNow;

        public DateTime EndTime { get; set; }

        public MetricApiJob(string ecu, string job, string args, int argsLength, string source)
        {
            this.ecu = ecu;
            this.job = job;
            this.source = source;
            if (argsLength != -1)
            {
                this.args = $"byte[{argsLength}]";
            }
            else if (string.IsNullOrWhiteSpace(args))
            {
                this.args = "-";
            }
            else
            {
                this.args = args;
            }
        }

        public override string ToString()
        {
            return InternalToString(Ecu, Job, Args, Source);
        }

        public string ToString(int ecuLen, int jobLen, int argsLen)
        {
            return Ecu.PadRight(ecuLen) + " / " + Job.PadRight(jobLen) + " / " + Args.PadRight(argsLen) + " -> (" + Source + ")";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MetricApiJob)
            {
                return obj.GetHashCode() == GetHashCode();
            }
            return false;
        }

        private static string InternalToString(string ecu, string job, string args, string source)
        {
            return ecu + " / " + job + " / " + args + " / " + source;
        }

        public static int GetCalculatedHash(string ecu, string job, string args, string source)
        {
            return InternalToString(ecu, job, args, source).GetHashCode();
        }
    }
}