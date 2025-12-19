using PsdzClient;

namespace PsdzClient.Core.Container
{
    public sealed class VehicleCommunication
    {
        private static int debuglevel;

        private static bool _validLicense;

        public static int DebugLevel
        {
            get
            {
                return debuglevel;
            }
            set
            {
                debuglevel = value;
            }
        }

        public static bool validLicense => _validLicense;

        [PreserveSource(Hint = "License modified", OriginalHash = "8A55A937F33E877507003C6920BB1500")]
        static VehicleCommunication()
        {
            // [UH] [IGNORE] modified
            _validLicense = true;
            Log.Info("VehicleCommunication.VehicleCommunication()", "ctor called.");
            debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.VehicleCommunication.DebugLevel", debuglevel);
            // [UH] [IGNORE] EnableEDIABASMultiThreading removed
        }
    }
}
