using PsdzClient.Utility;
using System;

namespace PsdzClient.Core
{
    public static class EcuTreeInitializer
    {
        [PreserveSource(Hint = "getBordnetFromDatabase removed", SignatureModified = true)]
        public static void Initialize(ILogger logger, Action<string> logMissingBordnet = null)
        {
            EcuTreeLogger.Initialize(logger);
            //[-]VehicleLogistics.Initialize(getBordnetFromDatabase, logMissingBordnet);
            //[+]VehicleLogistics.Initialize(logMissingBordnet);
            VehicleLogistics.Initialize(logMissingBordnet);
        }

        [PreserveSource(Added = true)]
        static EcuTreeInitializer()
        {
            Initialize(new NugetLogger());
        }
    }
}