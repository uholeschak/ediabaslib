using System;
using System.Collections.Generic;

namespace PsdzClient.Core
{
    [PreserveSource(Hint = "Changed to IDisposable", InheritanceModified = true)]
    public class IstaIcsServiceClient : IDisposable
    {
        [PreserveSource(Cleaned = true)]
        private void ValidateHost()
        {
        }

        [PreserveSource(Cleaned = true)]
        public bool IsContinuousWorkingPermitted()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
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

        [PreserveSource(Cleaned = true)]
        public DateTime GetLastTimeOnline()
        {
            return DateTime.MinValue;
        }

        [PreserveSource(Cleaned = true)]
        public string GetDefaultSharedStoragePath()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetSharedStoragePath()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public void UploadErrorReport(string zipArchiv)
        {
        }

        [PreserveSource(Cleaned = true)]
        public string GetEnvironmentUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetEnvironment()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetMarketLanguage()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetISPIProcessServicesURL()
        {
            return string.Empty;
        }

        [PreserveSource(Hint = "IcsNetworkCredential", Placeholder = true)]
        public PlaceholderType GetNetworkCredentials()
        {
            return PlaceholderType.Value;
        }

        [PreserveSource(Cleaned = true)]
        public string GetSecureWebRequestHeader(string url)
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public void VerifyLicense()
        {
        }

        [PreserveSource(Cleaned = true)]
        public IDictionary<string, string> GetLocationBasedParameters()
        {
            return new Dictionary<string, string>();
        }

        [PreserveSource(Cleaned = true)]
        public string GetEcuValidationUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetLogin2faRequired()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetSecureFeatureActivationUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string PrepareSecureFeatureActivationQaProdUrlIfRequired(string url)
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetSecureCodingUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetSecureCodingFallbackUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetTricCentralUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetTricBitsUploadUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetIVDServerUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetBMWConnectionCheckUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetBMWConnectionCheckFallbackUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetISTAEdgeAPIUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetCKFUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetCKFNewUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetAirServiceUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetAirForkServiceUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public bool GetSessionTakeover()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public int GetAutomaticLogoutTime()
        {
            return 0;
        }

        [PreserveSource(Cleaned = true)]
        public string GetSec4DiagUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetActivateSdpOnlinePatch()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public bool GetAirTeileClearingEnabled()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public string GetWebEAMNextStage()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetActivateICOMReboot()
        {
            return string.Empty;
        }

        public (bool IsActive, string Message) GetFeatureEnabledStatus(string feature, bool checkLbps = true)
        {
            //[-] ValidateHost();
            //[-] return CallFunction((IIstaIcsService channel) => channel.GetFeatureEnabledStatus(feature, checkLbps));
            //[+] string configString = ConfigSettings.getConfigString(LBPFeatureSwitches.FeatureRegistryKey(feature));
            string configString = ConfigSettings.getConfigString(LBPFeatureSwitches.FeatureRegistryKey(feature));
            //[+] if (!string.IsNullOrEmpty(configString) && bool.TryParse(configString, out var result))
            if (!string.IsNullOrEmpty(configString) && bool.TryParse(configString, out var result))
            //[+] {
            {
                //[+] return (IsActive: result, Message: "REGISTRY KEY");
                return (IsActive: result, Message: "REGISTRY KEY");
            //[+] }
            }
            //[+] bool flag = LBPFeatureSwitches.Features.DefaultValue(feature, IstaMode.HO);
            bool flag = LBPFeatureSwitches.Features.DefaultValue(feature, IstaMode.HO);
            //[+] return (IsActive: flag, Message: "DEFAULT VALUE");
            return (IsActive: flag, Message: "DEFAULT VALUE");
        }

        [PreserveSource(Cleaned = true)]
        public bool GetVehicleMetricsEnabled()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public string GetLoginDbOperationRetryCount()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetLoginDbTimeoutTermToTermRule()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public bool GetSec4DiagEnabledInBackground()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public string GetNVIWhitelisteEReihe()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetTricCloudUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public int? GetJvmHeapspace()
        {
            return null;
        }

        [PreserveSource(Cleaned = true)]
        public int? GetTimeoutLengthWebserviceStart()
        {
            return null;
        }

        [PreserveSource(Added = true)]
        public bool IsAvailable()
        {
            return false;
        }

        [PreserveSource(Added = true)]
        public void Dispose()
        {
        }

        [PreserveSource(Cleaned = true)]
        public string GetCurrentLocationWebEamNext()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public bool GetKaiServiceHistoryEnabled()
        {
            return false;
        }

        [PreserveSource(Cleaned = true)]
        public string GetConWoyAPIClientUrl()
        {
            return string.Empty;
        }

        [PreserveSource(Cleaned = true)]
        public string GetUseConwoyStorage()
        {
            return string.Empty;
        }
    }
}