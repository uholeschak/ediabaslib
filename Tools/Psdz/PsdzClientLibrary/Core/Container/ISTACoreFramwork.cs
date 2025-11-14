using PsdzClientLibrary;

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

        [PreserveSource(Hint = "Modified")]
        static ISTACoreFramwork()
        {
            // [UH] [IGNORE] modified
            _validLicense = true;
            _debuglevel = ConfigSettings.getConfigint("DebugLevel", 0);
            _debuglevel = ConfigSettings.getConfigint("BMW.Rheingold.ISTA.CoreFramework.DebugLevel", _debuglevel);
        }
    }
}