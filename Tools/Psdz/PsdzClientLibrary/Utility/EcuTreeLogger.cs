using PsdzClient.Core;

namespace PsdzClient.Utility
{
    internal static class EcuTreeLogger
    {
        private static ILogger instance;

        internal static ILogger Instance => instance;

        internal static void Initialize(ILogger logger)
        {
            instance = logger;
        }

        [PreserveSource(Added = true)]
        static EcuTreeLogger()
        {
            Initialize(new NugetLogger());
        }
    }
}