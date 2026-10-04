using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.ISTA.CoreFramework;
using PsdzClient;
using PsdzClient.Core;

namespace BMW.ISPI.TRIC.ISTA.EcuTree.Utilities
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