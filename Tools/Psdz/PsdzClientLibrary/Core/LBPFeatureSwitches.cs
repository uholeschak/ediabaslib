using System;

namespace PsdzClient.Core
{
    public static class LBPFeatureSwitches
    {
        public static class Features
        {
            public const string AutomaticTestPlanCalculation = "AutomaticTestPlanCalculation";
            public const string CheckRemoteUserDbStatusJob = "CheckRemoteUserDbStatusJob";
            public const string CollectLauncherLogs = "CollectLauncherLogs";
            public const string ConwoyStorage = "ConwoyStorage";
            public const string DownloadAllSdpPatches = "DownloadAllSdpPatches";
            public const string DumpOutOfMemoryException = "DumpOutOfMemoryException";
            public const string DumpStackOverflowException = "DumpStackOverflowException";
            public const string EarlyStartPsdzWebService = "EarlyStartPsdzWebService";
            public const string ExecuteServiceCallCertreqprofiles = "ExecuteServiceCallCertreqprofiles";
            public const string ExpSgbmIdVal = "ExpSgbmIdVal";
            public const string ExpSgbmIdValSmacTrnStrt = "ExpSgbmIdValSmacTrnStrt";
            public const string FastaSlowApiJobsSerivceCode = "FastaSlowApiJobsSerivceCode";
            public const string FdlGateOverTricCloud = "FdlGateOverTricCloud";
            public const string FillFunctionalJobsInFstdatIsActive = "FillFunctionalJobsInFstdatIsActive";
            public const string FixedWebView2Runtime = "FixedWebView2Runtime";
            public const string FixedWebView2RuntimeForLogin = "FixedWebView2RuntimeForLogin";
            public const string ForceUsingJava64Bit = "ForceUsingJava64Bit";
            public const string FscOverTricCloud = "FscOverTricCloud";
            public const string KaiServiceHistory = "KaiServiceHistory";
            public const string Login = "Login";
            public const string LogTracingMaximum = "LogTracingMaximum";
            public const string MidaOverTricCloud = "MidaOverTricCloud";
            public const string NopOverTricCloud = "NopOverTricCloud";
            public const string PkiOverTricCloud = "PkiOverTricCloud";
            public const string RenewConnectionAfterEcuReset = "RenewConnectionAfterEcuReset";
            public const string SCBOverTricCloud = "SCBOverTricCloud";
            public const string SccOverTricCloud = "SccOverTricCloud";
            public const string ScheduleLoginUserHousekeepingJob = "ScheduleLoginUserHousekeepingJob";
            public const string SdpOnlinePatchAndMultisession = "SdpOnlinePatchAndMultisession";
            public const string SFAOverTricCloud = "SFAOverTricCloud";
            public const string SynchronizeLoginDatabasesJob = "SynchronizeLoginDatabasesJob";
            public const string UseMirrorProtocol = "UseMirrorProtocol";
            public const string UsePsdzSeriesFormatter = "UsePsdzSeriesFormatter";
            public const string UseQaBackendForSfaAndFsc = "UseQaBackendForSfaAndFsc";
            public const string UseReducedPortRangeForMirror = "UseReducedPortRangeForMirror";
            public const string UseSharedUserDatabase = "UseSharedUserDatabase";
            public const string UseSweParallelDownloader = "UseSweParallelDownloader";
            public const string VcmComparison = "VcmComparison";
            public const string VinRangesOverConWoy = "VinRangesOverConWoy";
            public const string Vps25OverTricCloud = "Vps25OverTricCloud";
            public const string VpsOverTricCloud = "VpsOverTricCloud";
            public static bool DefaultValue(string featureName, IstaMode mode)
            {
                switch (mode)
                {
                    case IstaMode.HO:
                        return DefaultValueHO(featureName);
                    case IstaMode.AOS:
                        return DefaultValueAOS(featureName);
                    case IstaMode.Toyota:
                        return DefaultValueToyota(featureName);
                    default:
                        throw new ArgumentOutOfRangeException($"IstaMode.{mode} not supported by default value switch");
                }
            }

            public static bool DefaultValueHO(string featureName)
            {
                switch (featureName)
                {
                    case "CheckRemoteUserDbStatusJob":
                    case "ConwoyStorage":
                    case "ExecuteServiceCallCertreqprofiles":
                    case "FastaSlowApiJobsSerivceCode":
                    case "EarlyStartPsdzWebService":
                    case "FixedWebView2Runtime":
                    case "KaiServiceHistory":
                    case "Login":
                    case "RenewConnectionAfterEcuReset":
                    case "ScheduleLoginUserHousekeepingJob":
                    case "SdpOnlinePatchAndMultisession":
                    case "SynchronizeLoginDatabasesJob":
                    case "UseMirrorProtocol":
                    case "UseQaBackendForSfaAndFsc":
                    case "UseReducedPortRangeForMirror":
                    case "VcmComparison":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool DefaultValueAOS(string featureName)
            {
                switch (featureName)
                {
                    case "ConwoyStorage":
                    case "ExecuteServiceCallCertreqprofiles":
                    case "FastaSlowApiJobsSerivceCode":
                    case "FixedWebView2Runtime":
                    case "KaiServiceHistory":
                    case "RenewConnectionAfterEcuReset":
                    case "SdpOnlinePatchAndMultisession":
                    case "UseMirrorProtocol":
                    case "UseQaBackendForSfaAndFsc":
                    case "UseReducedPortRangeForMirror":
                    case "VcmComparison":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool DefaultValueToyota(string featureName)
            {
                switch (featureName)
                {
                    case "ConwoyStorage":
                    case "ExecuteServiceCallCertreqprofiles":
                    case "FastaSlowApiJobsSerivceCode":
                    case "FixedWebView2Runtime":
                    case "RenewConnectionAfterEcuReset":
                    case "SdpOnlinePatchAndMultisession":
                    case "UseMirrorProtocol":
                    case "UseQaBackendForSfaAndFsc":
                    case "VcmComparison":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool FeatureWithRegistryKey(string featureName)
            {
                if (featureName == "Login")
                {
                    return false;
                }

                return true;
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

        public static string FeatureCheckLBPsUntilIstaVersion(string feature)
        {
            return feature + "_CheckLBPsUntilIstaVersion";
        }

        public static string OutputMessage(string feature, bool value, string type)
        {
            return $"[FEATURE SWITCH] Feature: `{feature}` flag set using: `{type}` with value: `{value}`";
        }
    }
}