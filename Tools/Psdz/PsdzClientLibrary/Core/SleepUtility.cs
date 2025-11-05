using System.Threading.Tasks;
using System.Threading;
using PsdzClient.Utility;

namespace PsdzClient.Core
{
    public class SleepUtility
    {
        public static void ThreadSleep(int milliseconds, string reason)
        {
            if (milliseconds != 0)
            {
                Thread.Sleep(milliseconds);
                TimeMetricsUtility.Instance.Sleep(milliseconds, reason);
            }
        }

        public static async Task TaskDelay(int milliseconds, string reason)
        {
            if (milliseconds != 0)
            {
                await Task.Delay(milliseconds);
                TimeMetricsUtility.Instance.Sleep(milliseconds, reason);
            }
        }
    }
}