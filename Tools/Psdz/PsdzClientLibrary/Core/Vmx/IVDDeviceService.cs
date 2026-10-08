using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework;
using PsdzClient;

#pragma warning disable CS0649
namespace BMW.Rheingold.xVM
{
    public class IVDDeviceService
    {
        private const string UrlTemp = "{0}/IVD/IcomVerwaltungsdienst/{1}?dealerNr={2}&Outlet={3}";

        private static IDictionary<string, string> ivdEndpointMap = new Dictionary<string, string>
    {
        { "ICOM", "GetIcomData" },
        { "IMIB", "GetImibData" }
    };

        private readonly IEnumerable<string> deviceIPs;

        private string dealerNr;

        private string outletNr;

        private string trzUrl;

        private bool isRunning;

        private bool canRun;

        private bool stopSendRequestToIVD;

        public IList<string> DeviceIPs => deviceIPs.ToList();

        [PreserveSource(Cleaned = true)]
        public IVDDeviceService()
        {
            canRun = false;
            deviceIPs = new ConcurrentBag<string>();
            isRunning = false;
        }

        public void Start()
        {
            if (canRun)
            {
                stopSendRequestToIVD = false;
                Task.Factory.StartNew(SendRequestToIVD);
                isRunning = true;
                Log.Info("IVDDeviceService.Start()", "This service has been started. Dealernr: {0} - Outlet: {1}", dealerNr, outletNr);
            }
            else
            {
                Log.Warning("IVDDeviceService.Start()", "This service can not be started. Dealernr: {0} - Outlet: {1}", dealerNr, outletNr);
            }
        }

        public void Stop()
        {
            if (isRunning)
            {
                stopSendRequestToIVD = true;
                isRunning = false;
                Log.Info("IVDDeviceService.Stop()", "This service has been stopped.");
            }
            else
            {
                Log.Warning("IVDDeviceService.Stop()", "This service was not running");
            }
        }

        [PreserveSource(Hint="No change", SignatureModified = true)]
        private void SendRequestToIVD()
        {
            while (!stopSendRequestToIVD)
            {
                SendRequestFor("ICOM");
                SendRequestFor("IMIB");
                Thread.Sleep(20000);
            }
        }

        private void SendRequestFor(string type)
        {
            if (!ivdEndpointMap.TryGetValue(type, out var value))
            {
                Log.Warning("IVDDeviceService.ReadIVDDeviceList()", "The given type '" + type + "' is not supported.");
                return;
            }
            string url = $"{trzUrl}/IVD/IcomVerwaltungsdienst/{value}?dealerNr={dealerNr}&Outlet={outletNr}";
            object[] array = SendRequestToIVD(url);
            if (array == null || !array.Any())
            {
                Log.Info("IVDDeviceService.ReadIVDDeviceList()", "No registered " + type + "s found");
                return;
            }
            IEnumerable<string> enumerable = ReadIVDDeviceList(array);
            Log.Info("IVDDeviceService.ReadIVDDeviceList()", $"Following registered {type}s found: {0}", enumerable.ToStringItems());
        }

        [PreserveSource(Cleaned = true)]
        private object[] SendRequestToIVD(string url)
        {
            object[] result = null;
            return result;
        }

        private IEnumerable<string> ReadIVDDeviceList(object[] result)
        {
            IList<string> list = new List<string>();
            for (int i = 0; i < result.Length; i++)
            {
                if (!(result[i] is IDictionary dictionary) || !dictionary.Contains("IcomIP"))
                {
                    continue;
                }
                string text = dictionary["IcomIP"] as string;
                if (!string.IsNullOrEmpty(text))
                {
                    if (!deviceIPs.Contains(text))
                    {
                        ((ConcurrentBag<string>)deviceIPs).Add(text);
                    }
                    list.AddIfNotContains(text);
                }
            }
            return list;
        }
    }
}
