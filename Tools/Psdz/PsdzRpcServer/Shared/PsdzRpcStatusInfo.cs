using System;

namespace PsdzRpcServer.Shared
{
    public class PsdzRpcStatusInfo
    {
        public PsdzRpcStatusInfo(bool psdzInitialized, bool vehicleConnected, bool talPresent, bool hasOptionsDict, bool cancelPossible, DateTime lastUpdated)
        {
            PsdzInitialized = psdzInitialized;
            VehicleConnected = vehicleConnected;
            TalPresent = talPresent;
            HasOptionsDict = hasOptionsDict;
            CancelPossible = cancelPossible;
            LastUpdated = lastUpdated;
        }

        public bool PsdzInitialized { get; private set; }
        public bool VehicleConnected { get; private set; }
        public bool TalPresent { get; private set; }
        public bool HasOptionsDict { get; private set; }
        public bool CancelPossible { get; private set; }

        public DateTime LastUpdated { get; private set; }
    }
}