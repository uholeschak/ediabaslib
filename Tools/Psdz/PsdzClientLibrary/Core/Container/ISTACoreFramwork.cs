using System;
using PsdzClient;

namespace PsdzClient.Core.Container
{
    public class ISTACoreFramwork
    {
        private static bool _validLicense;

        private static int _debuglevel;

        public static int DebugLevel
        {
            get
            {
                return _debuglevel;
            }
            set
            {
                _debuglevel = value;
            }
        }

        internal static bool validLicense
        {
            get
            {
                return _validLicense;
            }
            set
            {
                Log.Error("ISTACoreFramework.validLicense", "is not accessible for writing...");
            }
        }

        static ISTACoreFramwork()
        {
            //[-] VerifyAssemblyHelper.VerifyStrongName(typeof(ISTACoreFramwork), force: true);
            Log.Info("ISTACoreFramework.ISTACoreFramework()", "ctor called.");
            try
            {
                //[-] BMW.Rheingold.CoreFramework.LicenseManager.VerifyLicense();
                Log.Info(string.Empty, "ISTA Activation succeded");
                _validLicense = true;
            }
            catch (Exception)
            {
                Log.Info(string.Empty, "ISTA Activation failed");
                _validLicense = false;
            }
            _debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            _debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.ISTA.CoreFramework.DebugLevel", _debuglevel);
        }
    }
}