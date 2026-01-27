using PsdzClient.Contracts;
using PsdzClient.Core;

namespace PsdzClient.Programming
{
    [PreserveSource(Hint = "Dummy class", SuppressWarning = true)]
    public class BackendConnectorProcessor
    {
        [PreserveSource(Hint = "errorManager removed")]
        public BoolResultObject<string> GetBackendServiceUrl(BackendServiceType serviceType, ContextError contextError, bool fallbackUrl = false)
        {
            string text = GetBackendUrlFromRegKey(serviceType, fallbackUrl);
            return new BoolResultObject<string>(BoolResultObject.SuccessResult, text);
        }

        private static string GetBackendUrlFromRegKey(BackendServiceType serviceType, bool fallbackUrl = false)
        {
            string text = "";
            switch (serviceType)
            {
                case BackendServiceType.SfaTokenDirect:
                case BackendServiceType.SfaTokenDirectForVehicle:
                case BackendServiceType.SfaNewestPackageForVehicle:
                case BackendServiceType.SfaTokenRequest:
                case BackendServiceType.SfaNewFeatureForVehicle:
                    text = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SFA.SecureFeatureActivationUrl");
                    break;
                case BackendServiceType.EcuValidation:
                    text = ConfigSettings.getConfigString("BMW.Rheingold.Programming.CertificateManagement.CBBEcuValidationUrl");
                    break;
                case BackendServiceType.SecureCoding:
                    if (!fallbackUrl)
                    {
                        text = ConfigSettings.getConfigString("BMW.Rheingold.Programming.Security.SC.SecureCodingUrls");
                    }
                    break;
            }
            Log.Info(Log.CurrentMethod(), $"Config value for {serviceType}: {text}");
            return text;
        }
    }
}