using PsdzClient;
using System;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace PsdzClient.Core
{
    public static class WebCallUtility
    {
        private const string GoogleDNSIp = "8.8.8.8";
        private const string BaiduDNSIp = "180.76.76.76";
        private const int PingRetryCount = 4;
        [PreserveSource(Hint = "Simplified")]
        public static bool CheckForInternetConnection()
        {
            try
            {
                return LogToFastaAndReturnConnectionStatus();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return LogToFastaAndReturnConnectionStatus();
            }
        }

        [PreserveSource(Hint = "Simplified")]
        public static bool CheckForIntranetConnection()
        {
            try
            {
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

        [PreserveSource(Hint = "Cleaned")]
        private static bool CheckTricResult()
        {
            return false;
        }

        [PreserveSource(Hint = "Cleaned")]
        private static bool CallTricEndpoint(string url)
        {
            return false;
        }

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
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to a common internet server was successful.", LayoutGroup.D);
                    }
                    else
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to a both (common internet server and TricCentral) was successful.", LayoutGroup.D);
                    }
                }
                else
                {
                    string text = (dnsConnectionStatus ? "a common internet server was successful" : "TricCentral was successful");
                    string text2 = (dnsConnectionStatus ? "TricCentral" : "a common internet server");
                    if (isToyota)
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to " + text + " was not successful.", LayoutGroup.D);
                    }
                    else
                    {
                        service?.AddServiceCode("SUC02_ProblemWithBackendCommunication_nu_LF", "The connection to " + text + " but the connection to " + text2 + " was not successful.", LayoutGroup.D);
                    }
                }
            }
            else
            {
                service?.AddServiceCode("SUC01_NoInternetConnection_nu_LF", "The client probably has no internet connection.", LayoutGroup.D);
            }

            return flag;
        }

        [PreserveSource(Hint = "Cleaned")]
        private static bool PingBMWConnectionCheck(bool isFallback = false)
        {
            return false;
        }

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