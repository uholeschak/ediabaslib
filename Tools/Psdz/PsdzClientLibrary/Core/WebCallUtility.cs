using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System;

namespace PsdzClient.Core
{
    public static class WebCallUtility
    {
        private const string GoogleDNSIp = "8.8.8.8";

        private const string BaiduDNSIp = "180.76.76.76";

        private const int PingRetryCount = 4;

        public static bool CheckForInternetConnection()
        {
            try
            {
                // [UH] removed
#if false
                if (!PingBMWConnectionCheck() && !PingBMWConnectionCheck(isFallback: true))
                {
                    string dealerCountry = LicenseHelper.DealerInstance.DealerData?.OutletCountry;
                    bool flag = IndustrialCustomerManager.Instance.IsIndustrialCustomerBrand("TOYOTA");
                    bool isOssModeActive = ConfigSettings.IsOssModeActive;
                    bool dnsConnectionStatus = CheckDNSConnection(dealerCountry);
                    bool tricConnectionStatus = !(flag || isOssModeActive) && CheckTricResult();
                    return LogToFastaAndReturnConnectionStatus(connectionCheckSucceeded: true, dnsConnectionStatus, tricConnectionStatus, flag);
                }
#endif
                return LogToFastaAndReturnConnectionStatus();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return LogToFastaAndReturnConnectionStatus();
            }
        }

        public static bool CheckForIntranetConnection()
        {
            try
            {
                //return CallTricEndpoint(TRICZentralUtility.ServerAddressServices + "/health");
                return false;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return false;
            }
        }

        private static bool CheckDNSConnection(string dealerCountry)
        {
            if (!string.IsNullOrEmpty(dealerCountry) && dealerCountry == "CN")
            {
                return PingBaiduDNSToCheckForInternetConnection();
            }
            return PingGoogleDNSToCheckForInternetConnection();
        }

        // [UH] removed
#if false
        private static bool CheckTricResult()
        {
            string serverAddressServices = TRICZentralUtility.ServerAddressServices;
            return CallTricEndpoint(serverAddressServices.Contains(".com") ? (serverAddressServices + "/health") : (serverAddressServices.Replace(".net", ".com") + "/health"));
        }

        private static bool CallTricEndpoint(string url)
        {
            if (!ServiceLocator.Current.TryGetService<INetworkTypeVerificationService>(out var service))
            {
                service = UnityContainerWrapper.Current.Resolve<INetworkTypeVerificationService>(Array.Empty<ResolverOverride>());
            }
            try
            {
                return service.IsNetworkTypeAvailable(url).Result;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return false;
            }
        }
#endif

        private static bool LogToFastaAndReturnConnectionStatus(bool connectionCheckSucceeded = false, bool dnsConnectionStatus = false, bool tricConnectionStatus = false, bool isToyota = false)
        {
            bool flag = connectionCheckSucceeded && (dnsConnectionStatus || tricConnectionStatus);
            if (!ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.OnlineMode", defaultValue: true))
            {
                return flag;
            }
            ServiceLocator.Current.TryGetService<IFasta2Service>(out var service);
            if (flag)
            {
                if (isToyota ? dnsConnectionStatus : (dnsConnectionStatus && tricConnectionStatus))
                {
                    if (isToyota)
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to a common internet server was successful.", LayoutGroup.D, allowMultipleEntries: false, bufferIfSessionNotStarted: false, null, null);
                    }
                    else
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to a both (common internet server and TricCentral) was successful.", LayoutGroup.D, allowMultipleEntries: false, bufferIfSessionNotStarted: false, null, null);
                    }
                }
                else
                {
                    string text = (dnsConnectionStatus ? "a common internet server was successful" : "TricCentral was successful");
                    string text2 = (dnsConnectionStatus ? "TricCentral" : "a common internet server");
                    if (isToyota)
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to " + text + " was not successful.", LayoutGroup.D, allowMultipleEntries: false, bufferIfSessionNotStarted: false, null, null);
                    }
                    else
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to " + text + " but the connection to " + text2 + " was not successful.", LayoutGroup.D, allowMultipleEntries: false, bufferIfSessionNotStarted: false, null, null);
                    }
                }
            }
            else
            {
                service?.AddServiceCode("SUC01_NoInternetConnection_nu_LF", "The client probably has no internet connection.", LayoutGroup.D, allowMultipleEntries: false, bufferIfSessionNotStarted: false, null, null);
            }
            return flag;
        }

        // [UH] removed
#if false
        private static bool PingBMWConnectionCheck(bool isFallback = false)
        {
            if (ConfigSettings.IsOssModeActive && !OSSPortal.PortalParameters.IsIcsConfig)
            {
                Log.Info(Log.CurrentMethod(), "AOS without ics4bdr can\u00b4t retrieve location based parameters");
                return false;
            }
            string text = "";
            using (IstaIcsServiceClient istaIcsServiceClient = new IstaIcsServiceClient())
            {
                if (!istaIcsServiceClient.IsAvailable())
                {
                    Log.Warning(Log.CurrentMethod(), "ICS was not available");
                    return false;
                }
                text = ((!isFallback) ? istaIcsServiceClient.GetBMWConnectionCheckUrl() : istaIcsServiceClient.GetBMWConnectionCheckFallbackUrl());
                if (string.IsNullOrEmpty(text))
                {
                    Log.Warning(Log.CurrentMethod(), "URL returned was null or empty");
                    return false;
                }
            }
            Log.Info(Log.CurrentMethod(), "Ping LBP BMWConnectionCheck URL: " + text + " to check for connectivity.");
            text = Regex.Replace(text, "http[s]{0,1}\\:\\/\\/", "");
            return PingIpWithRetries(text, "PingBMWConnectionCheck");
        }
#endif

        private static bool PingGoogleDNSToCheckForInternetConnection()
        {
            Log.Info(Log.CurrentMethod(), "Ping google DNS at 8.8.8.8 to check for internet connectivity.");
            return PingIpWithRetries("8.8.8.8", "PingGoogleDNSToCheckForInternetConnection");
        }

        private static bool PingBaiduDNSToCheckForInternetConnection()
        {
            Log.Info(Log.CurrentMethod(), "Ping Baidu DNS at 180.76.76.76 to check for internet connectivity.");
            return PingIpWithRetries("180.76.76.76", "PingBaiduDNSToCheckForInternetConnection");
        }

        private static bool PingIpWithRetries(string ip, string methodName)
        {
            for (int i = 0; i < 4; i++)
            {
                PingReply pingReply = PingIP(ip);
                if (pingReply != null && pingReply.Status == IPStatus.Success)
                {
                    return true;
                }
            }
            Log.Warning(Log.CurrentMethod(), "[" + methodName + "] The client probably has no connection to the internet.");
            return false;
        }

        private static PingReply PingIP(string ip)
        {
            Log.Info(Log.CurrentMethod(), "Pinging " + ip);
            try
            {
                PingReply pingReply = new Ping().Send(ip, 1000);
                Log.Info(Log.CurrentMethod(), "Status: " + pingReply.Status.ToString() + "; Time: " + pingReply.RoundtripTime + "; Address: " + pingReply.Address);
                return pingReply;
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
            return null;
        }
    }
}