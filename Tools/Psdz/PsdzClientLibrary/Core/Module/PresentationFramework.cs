using System;
using PsdzClient.Core;
using System.Windows;

namespace BMW.Rheingold.PresentationFramework
{
    public sealed class PresentationFramework
    {
        private static int _debuglevel;

        private static bool _validLicense;

        public static int DebugLevel
        {
            get
            {
                return _debuglevel;
            }
            set
            {
                if (_debuglevel != value)
                {
                    _debuglevel = value;
                }
            }
        }

        internal static bool validLicense => _validLicense;

        static PresentationFramework()
        {
            Log.Info("PresentationFramework.PresentationFramework()", "ctor called.");
            try
            {
                //[-] LicenseManager.VerifyLicense();
                Log.Info(string.Empty, "ISTA Activation succeeded");
                _validLicense = true;
            }
            catch
            {
                Log.Info(string.Empty, "ISTA Activation failed");
                _validLicense = false;
            }
            _debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            _debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.PresentationFramework.DebugLevel", _debuglevel);
        }

        public static object FindResource(string name)
        {
            try
            {
                return new ComponentResourceKey(typeof(PresentationFramework), name);
            }
            catch (Exception exception)
            {
                Log.ErrorException("PresentationFramework.FindResource()", exception);
            }
            return null;
        }
    }
}
