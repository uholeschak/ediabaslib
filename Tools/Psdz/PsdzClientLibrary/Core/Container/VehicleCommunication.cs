using System;
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

        static VehicleCommunication()
        {
            Log.Info("VehicleCommunication.VehicleCommunication()", "ctor called.");
            try
            {
                //[-] BMW.Rheingold.CoreFramework.LicenseManager.VerifyLicense();
                Log.Info(string.Empty, "ISTA Activation succeeded");
                _validLicense = true;
            }
            catch
            {
                Log.Info(string.Empty, "ISTA Activation failed");
                _validLicense = false;
            }

            debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.VehicleCommunication.DebugLevel", debuglevel);
            try
            {
                bool configStringAsBoolean = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.VehicleCommunication.EnableEDIABASMultiThreading", defaultValue: false);
                Log.Info("VehicleCommunication.VehicleCommunication()", "Setting up EDIABAS threading mode: {0}", configStringAsBoolean);
            //[-] bool flag = API.enableMultiThreading(configStringAsBoolean);
            //[-] Log.Info("VehicleCommunication.VehicleCommunication()", "Switching to EDIABAS threading mode result: {0}", flag);
            }
            catch (Exception exception)
            {
                Log.WarningException("VehicleCommunication.VehicleCommunication()", exception);
            }
        }
    }
}