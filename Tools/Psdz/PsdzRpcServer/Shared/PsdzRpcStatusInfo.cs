namespace PsdzRpcServer.Shared
{
    public class PsdzRpcStatusInfo
    {
        public PsdzRpcStatusInfo(bool psdzInitialized, bool vehicleConnected, bool talPresent, bool hasOptionDict, bool operationActive)
        {
            PsdzInitialized = psdzInitialized;
            VehicleConnected = vehicleConnected;
            TalPresent = talPresent;
            HasOptionDict = hasOptionDict;
            OperationActive = operationActive;
        }

        public bool PsdzInitialized { get; private set; }
        public bool VehicleConnected { get; private set; }
        public bool TalPresent { get; private set; }
        public bool HasOptionDict { get; private set; }
        public bool OperationActive { get; private set; }
    }
}