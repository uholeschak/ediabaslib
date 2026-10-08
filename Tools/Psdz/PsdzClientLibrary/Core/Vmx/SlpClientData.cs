using BMW.Rheingold.Psdz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using BMW.Rheingold.CoreFramework;

namespace BMW.Rheingold.xVM
{
    internal class SlpClientData : IDisposable
    {
        private bool disposed;

        private readonly IVDDeviceService subDeviceServiceIVD;

        private readonly HashSet<IPEndPoint> requestAddresses;

        public const string SLP_MCAST_ADDRESS = "239.255.255.253";

        public const int SLP_RESERVED_PORT = 427;

        public UdpClient UdpClient { get; }

        public SlpClientData(IPAddress adapterAddress, IPAddress addressToSend, IVDDeviceService subDeviceServiceIVD)
        {
            this.subDeviceServiceIVD = subDeviceServiceIVD;
            UdpClient = new UdpClient(new IPEndPoint(adapterAddress, 0));
            UdpClient.Client.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.MulticastSender.SendTimeout", 500);
            UdpClient.Client.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.MulticastSender.ReceiveTimeout", 5000);
            requestAddresses = new HashSet<IPEndPoint>();
            requestAddresses.Add(new IPEndPoint(IPAddress.Parse("239.255.255.253"), 427));
            requestAddresses.Add(new IPEndPoint(addressToSend, 427));
            bool configStringAsBoolean = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.PresentationFramework.ConnectionManagerDialog.SendQueryBroadcasts", defaultValue: true);
            string configString = ConfigSettings.getConfigString("xVM_ZGW_BROADCAST", "255.255.255.255,169.254.255.255");
            if (configStringAsBoolean)
            {
                AddZgwBroadcastAddresses(configString);
            }
        }

        private void AddZgwBroadcastAddresses(string addresses)
        {
            if (!string.IsNullOrEmpty(addresses))
            {
                List<IPAddress> collection = NetUtils.Convert(addresses.Split(',')).ToList();
                IPAddress item = IPAddress.Parse("169.254.255.255");
                collection.AddIfNotContains(item);
                AddRangeOfRequestAddresses(collection);
            }
        }

        public void AddRangeOfRequestAddresses(IEnumerable<IPAddress> requestAddresses)
        {
            this.requestAddresses.AddRange(requestAddresses.Select((IPAddress address) => new IPEndPoint(address, 427)));
        }

        public void Send()
        {
            SLPAttrRqst sLPAttrRqst = new SLPAttrRqst
            {
                langtag = "en",
                flags = 0,
                extoffset = 0u,
                xid = SLP.GlobalPacketID++,
                scopelist = "default"
            };
            sLPAttrRqst.SetupSendBuffer();
            foreach (IPEndPoint requestAddress in requestAddresses)
            {
                UdpClient.SendAsync(sLPAttrRqst.sendbuffer, sLPAttrRqst.sendbuffer.Length, requestAddress);
            }
            if (subDeviceServiceIVD == null || !subDeviceServiceIVD.DeviceIPs.Any())
            {
                return;
            }
            foreach (IPAddress item in NetUtils.Convert(subDeviceServiceIVD.DeviceIPs))
            {
                UdpClient.SendAsync(sLPAttrRqst.sendbuffer, sLPAttrRqst.sendbuffer.Length, new IPEndPoint(item, 427));
            }
        }

        public void Close()
        {
            try
            {
                UdpClient.Close();
            }
            catch (Exception exception)
            {
                Log.WarningException("UdpClientData..Close()", exception);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    UdpClient.Dispose();
                }
                disposed = true;
            }
        }
    }
}
