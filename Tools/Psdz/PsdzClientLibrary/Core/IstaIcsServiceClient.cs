using System.Collections.Generic;
using System;
using System.ServiceModel;

namespace PsdzClient.Core
{
    public class IstaIcsServiceClient : IDisposable
    {
        public void Dispose()
        {
        }

        public bool IsAvailable()
        {
            return false;
        }

        public bool IsContinuousWorkingPermitted()
        {
            return false;
        }

        public bool IsThisDeviceRegistered()
        {
            return false;
        }

#if false
        public IcsUsageRightsContainer LoadUsageRightsTyped()
        {
            return null;
        }

        public IcsClientConfiguration GetClientConfiguration()
        {
        }

        public IcsDataPackageList FindPackageListTyped()
        {
        }
#endif

        public DateTime GetLastTimeOnline()
        {
            return DateTime.MinValue;
        }

        public string GetDefaultSharedStoragePath()
        {
            return string.Empty;
        }

        public string GetSharedStoragePath()
        {
            return string.Empty;
        }

        public void UploadErrorReport(string zipArchiv)
        {
        }

        public string GetEnvironmentUrl()
        {
            return string.Empty;
        }

        public string GetEnvironment()
        {
            return string.Empty;
        }

        public string GetMarketLanguage()
        {
            return string.Empty;
        }

        public string GetISPIProcessServicesURL()
        {
            return string.Empty;
        }

#if false
        public IcsNetworkCredential GetNetworkCredentials()
        {
            return null;
        }
#endif

        public string GetSecureWebRequestHeader(string url)
        {
            return string.Empty;
        }

        public void VerifyLicense()
        {
        }

        public IDictionary<string, string> GetLocationBasedParameters()
        {
            return new Dictionary<string, string>();
        }

        public string GetEcuValidationUrl()
        {
            return string.Empty;
        }

        public string GetLogin2faRequired()
        {
            return string.Empty;
        }

        public string GetCurrentLocationWebEamNext()
        {
            return string.Empty;
        }

        public string GetSecureFeatureActivationUrl()
        {
            return string.Empty;
        }

        public string GetSecureCodingUrl()
        {
            return string.Empty;
        }

        public string GetSecureCodingFallbackUrl()
        {
            return string.Empty;
        }

        public string GetTricCentralUrl()
        {
            return string.Empty;
        }

        public string GetTricBitsUploadUrl()
        {
            return string.Empty;
        }

        public string GetIVDServerUrl()
        {
            return string.Empty;
        }

        public string GetBMWConnectionCheckUrl()
        {
            return string.Empty;
        }

        public string GetBMWConnectionCheckFallbackUrl()
        {
            return string.Empty;
        }

        public string GetISTAEdgeAPIUrl()
        {
            return string.Empty;
        }

        public string GetCKFUrl()
        {
            return string.Empty;
        }

        public string GetCKFNewUrl()
        {
            return string.Empty;
        }

        public bool GetKaiServiceHistoryEnabled()
        {
            return false;
        }

        public string GetAirServiceUrl()
        {
            return string.Empty;
        }

        public string GetAirForkServiceUrl()
        {
            return string.Empty;
        }

        public bool GetSessionTakeover()
        {
            return false;
        }

        public int GetAutomaticLogoutTime()
        {
            return 0;
        }

        public string GetSec4DiagUrl()
        {
            return string.Empty;
        }

        public string GetConWoyAPIClientUrl()
        {
            return string.Empty;
        }

        public string GetUseConwoyStorage()
        {
            return string.Empty;
        }

        public string GetActivateSdpOnlinePatch()
        {
            return string.Empty;
        }

        public bool GetAirTeileClearingEnabled()
        {
            return false;
        }

        public string GetWebEAMNextStage()
        {
            return string.Empty;
        }

        public string GetActivateICOMReboot()
        {
            return string.Empty;
        }

        // [UH] get from registry or default value
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

        public bool GetVehicleMetricsEnabled()
        {
            return false;
        }

        public string GetLoginDbOperationRetryCount()
        {
            return string.Empty;
        }

        public string GetLoginDbTimeoutTermToTermRule()
        {
            return string.Empty;
        }

        public bool GetSec4DiagEnabledInBackground()
        {
            return false;
        }

        public string GetNVIWhitelisteEReihe()
        {
            return string.Empty;
        }

        public string GetTricCloudUrl()
        {
            return string.Empty;
        }

        public int? GetJvmHeapspace()
        {
            return null;
        }

        public int? GetTimeoutLengthWebserviceStart()
        {
            return null;
        }
    }
}