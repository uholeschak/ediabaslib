using PsdzClientLibrary;

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

        [PreserveSource(Hint = "Modified")]
        static VehicleCommunication()
        {
            debuglevel = 0;
            // [UH] [IGNORE] modified
            _validLicense = true;
            Log.Info("VehicleCommunication.VehicleCommunication()", "ctor called.");
            debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.VehicleCommunication.DebugLevel", debuglevel);
            // [UH] [IGNORE] EnableEDIABASMultiThreading removed
        }
    }
}
