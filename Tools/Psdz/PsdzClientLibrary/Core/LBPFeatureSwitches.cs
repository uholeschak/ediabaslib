namespace PsdzClient.Core
{
    public static class LBPFeatureSwitches
    {
        public static class Features
        {
            public const string ForceUsageInternetUrl = "ForceUsageInternetUrl";

            public const string PsdzWebservice = "PsdzWebservice";

            public const string VcmComparison = "VcmComparison";

            public const string UseJreWithTLS13Support = "UseJreWithTLS13Support";

            public const string SdpOnlinePatchAndMultisession = "SdpOnlinePatchAndMultisession";

            public const string ForceUsingJava64Bit = "ForceUsingJava64Bit";

            public static bool DefaultValue(string featureName)
            {
                switch (featureName)
                {
                    case "UseJreWithTLS13Support":
                    case "VcmComparison":
                    case "SdpOnlinePatchAndMultisession":
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static class SwitchTypes
        {
            public const string RegistryKey = "REGISTRY KEY";

            public const string EnabledByDeviceLBP = "ENABLED BY DEVICE LBP";

            public const string DeactivatedByOutletLBP = "DEACTIVATED BY OUTLET LBP";

            public const string ActivatedByLBP = "ACTIVATED BY LBP";

            public const string MissingClientConfiguration = "MISSING CLIENT CONFIGURATION - RETURN DEFAULT VALUE";

            public const string DefaultValue = "DEFAULT VALUE";

            public const string DisabledByIstaVersion = "DISABLED BY ISTA VERSION";
        }

        public static string FeatureActivateKey(string feature)
        {
            return feature + "_Activate";
        }

        public static string FeatureActivateDevicesKey(string feature)
        {
            return feature + "_ActivateDevices";
        }

        public static string FeatureDeactivateOutletsKey(string feature)
        {
            return feature + "_DeactivateOutlets";
        }

        public static string FeatureRegistryKey(string feature)
        {
            return "BMW.Rheingold." + feature + "_Activate";
        }

        public static string FeatureCheckLBPsSinceIstaVersion(string feature)
        {
            return feature + "_CheckLBPsSinceIstaVersion";
        }

        public static string OutputMessage(string feature, bool value, string type)
        {
            return $"[FEATURE SWITCH] Feature: `{feature}` flag set using: `{type}` with value: `{value}`";
        }
    }
}
