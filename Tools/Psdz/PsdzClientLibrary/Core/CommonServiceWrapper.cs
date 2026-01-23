using System.Collections.Generic;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Class simplified")]
    public class CommonServiceWrapper
    {
        private static readonly object featureSwitchLockObj = new object();
        private static readonly List<string> loggedSwitches = new List<string>();

        public CommonServiceWrapper()
        {
        }

        public bool IsAvailable()
        {
            return true;
        }

        [PreserveSource(Hint = "Simplified, from DetermineFeatureEnabledStatus")]
        public (bool IsActive, string Message) GetFeatureEnabledStatus(string feature, bool checkLbps = true)
        {
            string configString = ConfigSettings.getConfigString(LBPFeatureSwitches.FeatureRegistryKey(feature));
            if (!string.IsNullOrEmpty(configString) && bool.TryParse(configString, out var result))
            {
                return (IsActive: result, Message: GetAndLogOutputMessage(feature, result, "REGISTRY KEY"));
            }
            bool flag = LBPFeatureSwitches.Features.DefaultValue(feature, IstaMode.HO);
            return (IsActive: flag, Message: GetAndLogOutputMessage(feature, flag, "DEFAULT VALUE"));
        }

        private string GetAndLogOutputMessage(string feature, bool value, string type)
        {
            string text = LBPFeatureSwitches.OutputMessage(feature, value, type);
            Log.Info(Log.CurrentMethod(), text);
            return text;
        }
    }
}
