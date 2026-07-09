using System;

namespace PsdzClient.Core
{
    public static class LBPFeatureSwitches
    {
        public static class Features
        {
            public const string AllowFsLesenErrorResult = "AllowFsLesenErrorResult";
            public const string AutomaticTestPlanCalc = "AutomaticTestPlanCalc";
            public const string CollectLauncherLogs = "CollectLauncherLogs";
            public const string DownloadAllSdpPatches = "DownloadAllSdpPatches";
            public const string DumpOutOfMemoryException = "DumpOutOfMemoryException";
            public const string DumpStackOverflwExcep = "DumpStackOverflwExcep";
            public const string EarlyStartPsdzWebService = "EarlyStartPsdzWebService";
            public const string EdiabasVersionForNcar = "EdiabasVersionForNcar";
            public const string ExecServCallCertReqProf = "ExecServCallCertReqProf";
            public const string FastaSlowApiJobServCode = "FastaSlowApiJobServCode";
            public const string FdlGateOverTricCloud = "FdlGateOverTricCloud";
            public const string FillFuncJobInFstdat = "FillFuncJobInFstdat";
            public const string FscOverTricCloud = "FscOverTricCloud";
            public const string FsLesenExpertOldCode = "FsLesenExpertOldCode";
            public const string GetEFuseTitlesNewWay = "GetEFuseTitlesNewWay";
            public const string GetEslDocFromAirViaTrz = "GetEslDocFromAirViaTrz";
            public const string IcomNextPCapReadout = "IcomNextPCapReadout";
            public const string IpmOverTricCloud = "IpmOverTricCloud";
            public const string JumpAirInAwp = "JumpAirInAwp";
            public const string KaiServiceHistory = "KaiServiceHistory";
            public const string Login = "Login";
            public const string LogTracingMaximum = "LogTracingMaximum";
            public const string MidaOverTricCloud = "MidaOverTricCloud";
            public const string NewDocTransformation = "NewDocTransformation";
            public const string NopOverTricCloud = "NopOverTricCloud";
            public const string PkiOverTricCloud = "PkiOverTricCloud";
            public const string RenewConAfterEcuReset = "RenewConAfterEcuReset";
            public const string SCBOverTricCloud = "SCBOverTricCloud";
            public const string SccOverTricCloud = "SccOverTricCloud";
            public const string SchedUserHousekeepJob = "SchedUserHousekeepJob";
            public const string SdpOnlnPatchMultisess = "SdpOnlnPatchMultisess";
            public const string SeamLM2OverTricCloud = "SeamLM2OverTricCloud";
            public const string SendAmpToAssistant = "SendAmpToAssistant";
            public const string SendFastaProtToAsst = "SendFastaProtToAsst";
            public const string SessionStateDialog = "SessionStateDialog";
            public const string SFAOverTricCloud = "SFAOverTricCloud";
            public const string ShowCCMTab = "ShowCCMTab";
            public const string ShowImibDisconnectPopUp = "ShowImibDisconnectPopUp";
            public const string ShowNewSessionEnterTab = "ShowNewSessionEnterTab";
            public const string ShowTabFluids = "ShowTabFluids";
            public const string SmartMaintenanceOverTRC = "SmartMaintenanceOverTRC";
            public const string SpecialModeAllowAllBrand = "SpecialModeAllowAllBrand";
            public const string SyncLoginDatabasesJob = "SyncLoginDatabasesJob";
            public const string TCMOverTricCloud = "TCMOverTricCloud";
            public const string UseAlphaRealm = "UseAlphaRealm";
            public const string UsePsdzSeriesFormatter = "UsePsdzSeriesFormatter";
            public const string UseQaBackendForSfaAndFsc = "UseQaBackendForSfaAndFsc";
            public const string UseReducPortRangeMirror = "UseReducPortRangeMirror";
            public const string UseSweParallelDownloader = "UseSweParallelDownloader";
            public const string VcmComparison = "VcmComparison";
            public const string VinRangesOverConWoy = "VinRangesOverConWoy";
            public const string VinRangeUsagesLogging = "VinRangeUsagesLogging";
            public const string VirtualKeyboardTool = "VirtualKeyboardTool";
            public const string Vps25OverTricCloud = "Vps25OverTricCloud";
            public const string VpsOverTricCloud = "VpsOverTricCloud";
            public const string ZgwRepOverstForIpbBoot = "ZgwRepOverstForIpbBoot";
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
                    case "FastaSlowApiJobServCode":
                    case "ShowImibDisconnectPopUp":
                    case "UseReducPortRangeMirror":
                    case "AllowFsLesenErrorResult":
                    case "ExecServCallCertReqProf":
                    case "AutomaticTestPlanCalc":
                    case "DownloadAllSdpPatches":
                    case "RenewConAfterEcuReset":
                    case "SchedUserHousekeepJob":
                    case "SdpOnlnPatchMultisess":
                    case "SyncLoginDatabasesJob":
                    case "CollectLauncherLogs":
                    case "VinRangesOverConWoy":
                    case "ShowTabFluids":
                    case "UseAlphaRealm":
                    case "VcmComparison":
                    case "KaiServiceHistory":
                    case "Login":
                    case "UseQaBackendForSfaAndFsc":
                    case "ZgwRepOverstForIpbBoot":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool DefaultValueAOS(string featureName)
            {
                switch (featureName)
                {
                    case "FastaSlowApiJobServCode":
                    case "ShowImibDisconnectPopUp":
                    case "UseReducPortRangeMirror":
                    case "AllowFsLesenErrorResult":
                    case "ExecServCallCertReqProf":
                    case "CollectLauncherLogs":
                    case "VinRangesOverConWoy":
                    case "ShowTabFluids":
                    case "VcmComparison":
                    case "KaiServiceHistory":
                    case "RenewConAfterEcuReset":
                    case "UseQaBackendForSfaAndFsc":
                    case "ZgwRepOverstForIpbBoot":
                        return true;
                    default:
                        return false;
                }
            }

            public static bool DefaultValueToyota(string featureName)
            {
                switch (featureName)
                {
                    case "ExecServCallCertReqProf":
                    case "FastaSlowApiJobServCode":
                    case "RenewConAfterEcuReset":
                    case "ShowTabFluids":
                    case "UseQaBackendForSfaAndFsc":
                    case "ShowImibDisconnectPopUp":
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