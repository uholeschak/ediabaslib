using PsdzClient.Utility;
using System;
using System.Collections.Generic;
using static PsdzClient.PsdzDatabase;

namespace PsdzClient.Core
{
    public static class EcuTreeInitializer
    {
        public static void Initialize(Func<IEcuTreeVehicle, ICollection<BordnetsData>> getBordnetFromDatabase, ILogger logger, Action<string> logMissingBordnet = null)
        {
            EcuTreeLogger.Initialize(logger);
            VehicleLogistics.Initialize(getBordnetFromDatabase, logMissingBordnet);
        }
    }
}