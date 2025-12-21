using System;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Changed to IDisposable", InheritanceModified = true)]
    public class IstaIcsServiceClient : IDisposable
    {
        [PreserveSource(Hint = "Cleaned")]
        private void ValidateHost()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool IsContinuousWorkingPermitted()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool IsThisDeviceRegistered()
        {
            return false;
        }

        [PreserveSource(Hint = "IcsUsageRightsContainer", Placeholder = true)]
        public PlaceholderType LoadUsageRightsTyped()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "IcsClientConfiguration", Placeholder = true)]
        public PlaceholderType GetClientConfiguration()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "IcsDataPackageList", Placeholder = true)]
        public PlaceholderType FindPackageListTyped()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        public DateTime GetLastTimeOnline()
        {
            return DateTime.MinValue;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetDefaultSharedStoragePath()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetSharedStoragePath()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public void UploadErrorReport(string zipArchiv)
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetEnvironmentUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetEnvironment()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetMarketLanguage()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetISPIProcessServicesURL()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "IcsNetworkCredential", Placeholder = true)]
        public PlaceholderType GetNetworkCredentials()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetSecureWebRequestHeader(string url)
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public void VerifyLicense()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public IDictionary<string, string> GetLocationBasedParameters()
        {
            return new Dictionary<string, string>();
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetEcuValidationUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetLogin2faRequired()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetSecureFeatureActivationUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string PrepareSecureFeatureActivationQaProdUrlIfRequired(string url)
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetSecureCodingUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetSecureCodingFallbackUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetTricCentralUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetTricBitsUploadUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetIVDServerUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetBMWConnectionCheckUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetBMWConnectionCheckFallbackUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetISTAEdgeAPIUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetCKFUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetCKFNewUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetAirServiceUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetAirForkServiceUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool GetSessionTakeover()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public int GetAutomaticLogoutTime()
        {
            return 0;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetSec4DiagUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetActivateSdpOnlinePatch()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool GetAirTeileClearingEnabled()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetWebEAMNextStage()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetActivateICOMReboot()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "get from registry or default value", OriginalHash = "E886871F0E5E807DF68460715F4D8DE5")]
        public (bool IsActive, string Message) GetFeatureEnabledStatus(string feature, bool checkLbps = true)
        {
            string configString = ConfigSettings.getConfigString(LBPFeatureSwitches.FeatureRegistryKey(feature));
            if (!string.IsNullOrEmpty(configString) && bool.TryParse(configString, out var result))
            {
                return (IsActive: result, Message: "REGISTRY KEY");
            }

            bool flag = LBPFeatureSwitches.Features.DefaultValue(feature, IstaMode.HO);
            return (IsActive: flag, Message: "DEFAULT VALUE");
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool GetVehicleMetricsEnabled()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetLoginDbOperationRetryCount()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetLoginDbTimeoutTermToTermRule()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool GetSec4DiagEnabledInBackground()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetNVIWhitelisteEReihe()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetTricCloudUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public int? GetJvmHeapspace()
        {
            return null;
        }

        [PreserveSource(Hint = "Cleaned")]
        public int? GetTimeoutLengthWebserviceStart()
        {
            return null;
        }

        [PreserveSource(Hint = "Added")]
        public bool IsAvailable()
        {
            return false;
        }

        [PreserveSource(Hint = "Added")]
        public void Dispose()
        {
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetCurrentLocationWebEamNext()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public bool GetKaiServiceHistoryEnabled()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetConWoyAPIClientUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "Cleaned")]
        public string GetUseConwoyStorage()
        {
            return string.Empty;
        }
    }
}