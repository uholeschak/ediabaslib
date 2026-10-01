using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.xVM;
using PsdzClient.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using PsdzClient;

#pragma warning disable CS0618
namespace BMW.Rheingold.xVM.ENET
{
    public abstract class AbstractVCIEnetService : IDeviceService, IDisposable
    {
        private const string LOOPBACK_ADDRESS = "127.0.0.1";

        private readonly System.Timers.Timer gcReceivedDevices;

        private readonly HashSet<VCIDevice> receivedDevices;

        private readonly object syncVariableDevs = new object();

        private Dictionary<IPAddress, UdpState> broadCastDictionary;

        private readonly ILogic logic;

        private bool disposed;

        private bool isRunning;

        internal bool stopZgwReceiving;

        internal bool stopZgwSending;

        internal abstract int ZgwReservedPort { get; }

        internal abstract byte[] ZgwBuffer { get; }

        private IEnumerable<VCIDevice> ConnectedDevices
        {
            get
            {
                List<VCIDevice> list = new List<VCIDevice>();
                VCIDevice vCIDevice = logic.VecInfo?.MIB;
                if (vCIDevice != null)
                {
                    list.AddIfNotContains(vCIDevice);
                }
                VCIDevice vCIDevice2 = logic.VecInfo?.VCI;
                if (vCIDevice2 != null)
                {
                    list.AddIfNotContains(vCIDevice2);
                }
                return list;
            }
        }

        public IList<VCIDevice> Devices => receivedDevices.ToList();

        public string FilterAsString
        {
            get
            {
                if (Filter != null)
                {
                    return Filter.ToString();
                }
                return string.Empty;
            }
            set
            {
                Filter = new Regex(value);
            }
        }

        public Regex Filter { get; set; }

        public event EventHandler<NotifyCollectionChangedEventArgs> DevicesChanged;

        public AbstractVCIEnetService(ILogic logic)
        {
            this.logic = logic;
            receivedDevices = new HashSet<VCIDevice>();
            broadCastDictionary = new Dictionary<IPAddress, UdpState>();
            gcReceivedDevices = new System.Timers.Timer(5000.0);
            gcReceivedDevices.AutoReset = true;
            gcReceivedDevices.Elapsed += GcReceivedDevicesOnElapsed;
        }

        ~AbstractVCIEnetService()
        {
            Dispose();
        }

        private void GcReceivedDevicesOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            IEnumerable<VCIDevice> connDevs = ConnectedDevices;
            Predicate<VCIDevice> predicateKillVehicle = (VCIDevice x) => connDevs.Any((VCIDevice connectedDev) => x.Serial != connectedDev.Serial) && x.IsDead;
            Func<VCIDevice, bool> predicate = (VCIDevice x) => predicateKillVehicle(x);
            if (receivedDevices.Any(predicate))
            {
                lock (syncVariableDevs)
                {
                    IList changedItems = receivedDevices.Where(predicate).ToList();
                    NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems);
                    receivedDevices.RemoveWhere(predicateKillVehicle);
                    OnDevicesChanged(args);
                }
            }
        }

        private void OnDevicesChanged(NotifyCollectionChangedEventArgs args)
        {
            if (this.DevicesChanged != null)
            {
                this.DevicesChanged(this, args);
            }
        }

        private void Close()
        {
            foreach (KeyValuePair<IPAddress, UdpState> item in broadCastDictionary)
            {
                try
                {
                    item.Value.UdpClient.Close();
                }
                catch (Exception exception)
                {
                    Log.WarningException(Log.CurrentMethod(), exception);
                }
            }
        }

        private void Flush()
        {
            foreach (KeyValuePair<IPAddress, UdpState> item in broadCastDictionary)
            {
                byte[] dgram = new byte[10];
                try
                {
                    item.Value.UdpClient.Send(dgram, 10, "127.0.0.1", ZgwReservedPort);
                }
                catch (Exception exception)
                {
                    Log.WarningException(Log.CurrentMethod(), exception);
                }
            }
        }

        private void Open()
        {
            //[-] if (!xVM.validLicense)
            //[-] {
            //[-] throw new InvalidLicenseException();
            //[-] }
            broadCastDictionary = new Dictionary<IPAddress, UdpState>();
            try
            {
                foreach (IPAddress item in (from a in GetBroadcastInfos()
                                            select a.BroadcastAddress).ToList())
                {
                    if (!broadCastDictionary.Keys.Contains(item))
                    {
                        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        UdpState value = new UdpState(new UdpClient(iPEndPoint), iPEndPoint);
                        broadCastDictionary.Add(item, value);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private IEnumerable<(IPAddress BroadcastAddress, IPAddress Address)> GetBroadcastInfos()
        {
            IEnumerable<NetworkInterface> enumerable = from a in NetworkInterface.GetAllNetworkInterfaces()
                                                       where a.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                                       where a.OperationalStatus == OperationalStatus.Up
                                                       select a;
            foreach (NetworkInterface item2 in enumerable)
            {
                UnicastIPAddressInformationCollection unicastAddresses = item2.GetIPProperties().UnicastAddresses;
                if (unicastAddresses == null)
                {
                    continue;
                }
                foreach (UnicastIPAddressInformation item3 in unicastAddresses.Where((UnicastIPAddressInformation a) => a.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    IPAddress item = new IPAddress((item3.Address.Address | ~item3.IPv4Mask.Address) & 0xFFFFFFFFu);
                    yield return (BroadcastAddress: item, Address: item3.Address);
                }
            }
        }

        internal void StartReceivingProcesses()
        {
            //[-] Log.Debug(xVM.DebugLevel, 3, Log.CurrentMethod(), "called.");
            //[-] if (!xVM.validLicense)
            //[-] {
            //[-] throw new InvalidLicenseException();
            //[-] }
            foreach (KeyValuePair<IPAddress, UdpState> item in broadCastDictionary)
            {
                StartReceiving(item.Value);
            }
        }

        internal void StartReceiving(UdpState udpState)
        {
            //[-] Log.Debug(xVM.DebugLevel, 3, Log.CurrentMethod(), "called.");
            try
            {
                udpState.UdpClient.BeginReceive(ReceiveCallback, udpState);
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        public abstract void ReceiveCallback(IAsyncResult ar);

        internal void CreateAndUpdateEnetVciDevice(string vin, string serialNumber, string ipAddress, string macAddress, bool isDoip)
        {
            //[-] VCIDevice vCIDevice = new VCIDevice();
            //[+] VCIDevice vCIDevice = new VCIDevice(logic.ClientContext);
            VCIDevice vCIDevice = new VCIDevice(logic.ClientContext);
            vCIDevice.VCIType = VCIDeviceType.ENET;
            vCIDevice.Description = "BMW ethernet based car";
            vCIDevice.DevId = vin;
            vCIDevice.Serial = serialNumber;
            vCIDevice.IPAddress = ipAddress;
            vCIDevice.MacAddress = macAddress;
            vCIDevice.IsDoIP = isDoip;
            vCIDevice.Color = "#ffffff";
            vCIDevice.DevType = "ENET";
            vCIDevice.Imagename = "grafik/imib.jpg";
            vCIDevice.State = "4";
            vCIDevice.VIN = vin;
            vCIDevice.VciChannels = "[0?;1?;2?;3+]";
            if (Filter != null)
            {
                if (Filter.Match(vCIDevice.ToAttrList(addLineFeed: false)).Success)
                {
                    UpdateDevice(vCIDevice);
                }
            }
            else
            {
                UpdateDevice(vCIDevice);
            }
        }

        private void UpdateDevice(VCIDevice device)
        {
            lock (syncVariableDevs)
            {
                VCIDevice vCIDevice = receivedDevices.SingleOrDefault((VCIDevice x) => x.Equals(device));
                if (vCIDevice != null)
                {
                    vCIDevice.SetAlive();
                    return;
                }
                device.SetAlive();
                receivedDevices.AddIfNotContains(device);
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, device);
                OnDevicesChanged(args);
            }
        }

        private void SendZgwBroadcastsCyclically()
        {
            while (!stopZgwSending)
            {
                SendZgwBroadcasts();
                Thread.Sleep(2000);
            }
        }

        public void SendZgwBroadcasts()
        {
            //[-] if (!xVM.validLicense)
            //[-] {
            //[-]     throw new InvalidLicenseException();
            //[-] }
            foreach (KeyValuePair<IPAddress, UdpState> item in broadCastDictionary)
            {
                try
                {
                    UdpClient udpClient = item.Value.UdpClient;
                    IPAddress key = item.Key;
                    udpClient.Send(ZgwBuffer, ZgwBuffer.Length, new IPEndPoint(key, ZgwReservedPort));
                    //[-] Log.Debug(xVM.DebugLevel, 5, Log.CurrentMethod(), "Broadcast has been sent from port '{0}' to IP '{1}:{2}'", ((IPEndPoint)item.Value.UdpClient.Client.LocalEndPoint).Port.ToString(), key.ToString(), ZgwReservedPort);
                }
                catch (Exception exception)
                {
                    Log.WarningException(Log.CurrentMethod(), exception);
                }
            }
        }

        public void Start(IPAddress address)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            if (!isRunning)
            {
                Open();
                stopZgwReceiving = false;
                StartReceivingProcesses();
                stopZgwSending = false;
                Task.Factory.StartNew(SendZgwBroadcastsCyclically);
                gcReceivedDevices.Start();
                isRunning = true;
            }
        }

        public void Stop()
        {
            gcReceivedDevices.Stop();
            stopZgwReceiving = true;
            stopZgwSending = true;
            Flush();
            Close();
            isRunning = false;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                if (isRunning)
                {
                    Stop();
                }
                gcReceivedDevices.Dispose();
            }
            disposed = true;
        }

        [PreserveSource(Added = true)]
        public ILogic Logic => logic;
    }
}
