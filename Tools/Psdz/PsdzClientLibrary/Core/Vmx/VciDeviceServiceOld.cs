using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.xVM;
using System;
using System.Collections;
using System.Collections.Concurrent;
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
using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.xVM
{
    [Obsolete("There is a new implementation 'VciDeviceService'.")]
    public sealed class VciDeviceServiceOld : IDeviceService, IDisposable
    {
        private readonly IPEndPoint multicastEndpoint = new IPEndPoint(IPAddress.Any, 0);
        private readonly HashSet<VCIDevice> receivedDevices;
        private readonly ConcurrentQueue<SLPHeader> receivedResponses;
        private readonly AutoResetEvent autoEvent;
        private readonly System.Timers.Timer gcReceivedDevices;
        private readonly object syncVariableDevs = new object ();
        private readonly string[] acceptedMeasurementDeviceTypes;
        private readonly bool imibDeviceReservation;
        private bool disposed;
        private Socket multicastSenderAndListener;
        private bool stopCreatingVciDevices;
        private bool stopCyclicBroadcast;
        private bool unicastClientReceivingIsRunning;
        private UdpClient unicastClient;
        private Socket unicastClientSocket;
        private Socket unicastListener;
        private bool isRunning;
        private bool sendVCIBroadCasts;
        private string xVM_ZGW_BROADCASTMask;
        public const string SLP_MCAST_ADDRESS = "239.255.255.253";
        public const int SLP_RESERVED_PORT = 427;
        private IVDDeviceService subDeviceServiceIVD;
        public IList<VCIDevice> Devices
        {
            get
            {
                lock (syncVariableDevs)
                {
                    return receivedDevices.ToList();
                }
            }
        }

        public string FilterAsString
        {
            get
            {
                return Filter.ToString();
            }

            set
            {
                Filter = new Regex(value);
            }
        }

        public Regex Filter { get; set; }

        public event EventHandler<NotifyCollectionChangedEventArgs> DevicesChanged;
        public VciDeviceServiceOld()
        {
            receivedDevices = new HashSet<VCIDevice>();
            autoEvent = new AutoResetEvent(initialState: false);
            receivedResponses = new ConcurrentQueue<SLPHeader>();
            subDeviceServiceIVD = new IVDDeviceService();
            imibDeviceReservation = ConfigSettings.getConfigStringAsBoolean("TesterGUI.IMIBDeviceReservation", defaultValue: true);
            acceptedMeasurementDeviceTypes = ConfigSettings.getConfigString("BMW.Rheingold.ConnectionManager.AcceptedMeasurementDeviceTypes", "IMIB_R2, IMIB_NX")?.Split(',');
            sendVCIBroadCasts = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.PresentationFramework.ConnectionManagerDialog.SendQueryBroadcasts", defaultValue: true);
            xVM_ZGW_BROADCASTMask = ConfigSettings.getConfigString("xVM_ZGW_BROADCAST", "255.255.255.255,169.254.255.255");
            gcReceivedDevices = new System.Timers.Timer(5000.0);
            gcReceivedDevices.AutoReset = true;
            gcReceivedDevices.Elapsed += GcReceivedDevicesOnElapsed;
        }

        ~VciDeviceServiceOld()
        {
            Dispose();
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
                if (multicastSenderAndListener != null)
                {
                    multicastSenderAndListener.Dispose();
                }

                if (unicastListener != null)
                {
                    unicastListener.Dispose();
                }

                if (unicastClient != null)
                {
                    unicastClient.Close();
                }

                if (unicastClientSocket != null)
                {
                    unicastClientSocket.Close();
                }

                if (gcReceivedDevices != null)
                {
                    gcReceivedDevices.Stop();
                }

                if (gcReceivedDevices != null)
                {
                    gcReceivedDevices.Dispose();
                }
            }

            disposed = true;
        }

        public void Start()
        {
            if (!isRunning)
            {
                InitializeUnicastClient();
                InitializeUnicastClientSocket();
                InitializeMulticastSender();
                InitializeUnicastSender((IPEndPoint)multicastSenderAndListener.LocalEndPoint);
                stopCyclicBroadcast = false;
                Task.Factory.StartNew(CyclicBroadcastDeviceRequest);
                stopCreatingVciDevices = false;
                Task.Factory.StartNew(CreateVciDevices);
                subDeviceServiceIVD.Start();
                gcReceivedDevices.Start();
                isRunning = true;
            }
        }

        public void Stop()
        {
            stopCreatingVciDevices = true;
            stopCyclicBroadcast = true;
            subDeviceServiceIVD.Stop();
            autoEvent.Set();
            gcReceivedDevices.Stop();
            multicastSenderAndListener.Close();
            unicastListener.Close();
            unicastClient.Close();
            unicastClientSocket.Close();
            isRunning = false;
        }

        private static bool EqualsDevice(VCIDevice deviceA, VCIDevice deviceB)
        {
            if (((string.IsNullOrEmpty(deviceA.State) && string.IsNullOrEmpty(deviceB.State)) || (!string.IsNullOrEmpty(deviceA.State) && deviceA.State.Equals(deviceB.State))) && ((string.IsNullOrEmpty(deviceA.Kl15Voltage) && string.IsNullOrEmpty(deviceB.Kl15Voltage)) || (!string.IsNullOrEmpty(deviceA.Kl15Voltage) && deviceA.Kl15Voltage.Equals(deviceB.Kl15Voltage))) && ((string.IsNullOrEmpty(deviceA.Kl30Voltage) && string.IsNullOrEmpty(deviceB.Kl30Voltage)) || (!string.IsNullOrEmpty(deviceA.Kl30Voltage) && deviceA.Kl30Voltage.Equals(deviceB.Kl30Voltage))) && ((string.IsNullOrEmpty(deviceA.AccuCapacity) && string.IsNullOrEmpty(deviceB.AccuCapacity)) || (!string.IsNullOrEmpty(deviceA.AccuCapacity) && deviceA.AccuCapacity.Equals(deviceB.AccuCapacity))) && ((string.IsNullOrEmpty(deviceA.VIN) && string.IsNullOrEmpty(deviceB.VIN)) || (!string.IsNullOrEmpty(deviceA.VIN) && deviceA.VIN.Equals(deviceB.VIN))) && ((string.IsNullOrEmpty(deviceA.SignalStrength) && string.IsNullOrEmpty(deviceB.SignalStrength)) || (!string.IsNullOrEmpty(deviceA.SignalStrength) && deviceA.SignalStrength.Equals(deviceB.SignalStrength))))
            {
                if (!string.IsNullOrEmpty(deviceA.VciChannels) || !string.IsNullOrEmpty(deviceB.VciChannels))
                {
                    if (!string.IsNullOrEmpty(deviceA.VciChannels))
                    {
                        return deviceA.VciChannels.Equals(deviceB.VciChannels);
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        private bool IsValidDevice(VCIDevice device)
        {
            if (imibDeviceReservation)
            {
                return device.IsSupportedImibOrICOM(acceptedMeasurementDeviceTypes);
            }

            return true;
        }

        private void CreateVciDevices()
        {
            string configString = ConfigSettings.getConfigString("Bmw.Rheingold.ConnectionManager.DefaultICOMData", "");
            while (!stopCreatingVciDevices)
            {
                if (receivedResponses.IsEmpty)
                {
                    autoEvent.WaitOne();
                }

                if (stopCreatingVciDevices || !receivedResponses.TryDequeue(out var result) || result.functionid != 7 || result.length <= 25)
                {
                    continue;
                }

                SLPAttrRply sLPAttrRply = (SLPAttrRply)result;
                if (string.IsNullOrEmpty(sLPAttrRply.attrlist))
                {
                    continue;
                }

                Match match = null;
                if (Filter != null)
                {
                    match = Filter.Match(sLPAttrRply.attrlist);
                }

                if (match != null && !match.Success)
                {
                    continue;
                }

                VCIDevice newDevice = SLP.ScanDeviceFromAttrList(sLPAttrRply);
                if (newDevice == null || !IsValidDevice(newDevice))
                {
                    continue;
                }

                NotifyCollectionChangedEventArgs e = null;
                lock (syncVariableDevs)
                {
                    VCIDevice vCIDevice = receivedDevices.SingleOrDefault((VCIDevice x) => x.Equals(newDevice));
                    if (vCIDevice != null)
                    {
                        vCIDevice.SetAlive();
                        if (!EqualsDevice(vCIDevice, newDevice) || (match != null && match.Success))
                        {
                            IList list = new ArrayList();
                            list.Add(vCIDevice.Clone());
                            receivedDevices.Remove(vCIDevice);
                            receivedDevices.Add(newDevice);
                            IList list2 = new ArrayList();
                            list2.Add(newDevice);
                            e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, list2, list);
                        }
                        else
                        {
                            Log.Debug("CreateVciDevice", "Same device without filter found!");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(configString) && string.Equals(newDevice.Serial, configString))
                        {
                            newDevice.IsMarkedToDefault = true;
                        }

                        receivedDevices.Add(newDevice);
                        e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newDevice);
                    }

                    if (e != null)
                    {
                        OnDevicesChanged(e);
                    }
                }
            }
        }

        private void SendVciBroadcasts(SLPAttrRqst slpRequest)
        {
            if (!string.IsNullOrEmpty(xVM_ZGW_BROADCASTMask))
            {
                string[] array = xVM_ZGW_BROADCASTMask.Split(',');
                array.ForEach((string x) =>
                {
                    x.Trim();
                });
                List<string> list = new List<string>(array);
                if (!xVM_ZGW_BROADCASTMask.Contains("169.254.255.255"))
                {
                    list.AddIfNotContains("169.254.255.255");
                }

                SendUnicastToDevice(slpRequest, list);
            }
        }

        private void CyclicBroadcastDeviceRequest()
        {
            while (!stopCyclicBroadcast)
            {
                SLPAttrRqst sLPAttrRqst = new SLPAttrRqst();
                sLPAttrRqst.langtag = "en";
                sLPAttrRqst.flags = 0;
                sLPAttrRqst.extoffset = 0u;
                sLPAttrRqst.xid = SLP.GlobalPacketID++;
                sLPAttrRqst.scopelist = "default";
                sLPAttrRqst.SetupSendBuffer();
                try
                {
                    if (multicastSenderAndListener != null)
                    {
                        multicastSenderAndListener.SendTo(sLPAttrRqst.sendbuffer, new IPEndPoint(IPAddress.Parse("239.255.255.253"), 427));
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("VciDeviceService.CyclicBroadcastDeviceRequest()", exception);
                }

                SendUnicastToDevice(sLPAttrRqst, subDeviceServiceIVD.DeviceIPs);
                if (sendVCIBroadCasts)
                {
                    SendVciBroadcasts(sLPAttrRqst);
                }

                Thread.Sleep(500);
            }
        }

        private void GcReceivedDevicesOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Predicate<VCIDevice> predicateKillVehicle = (VCIDevice x) => x.IsDead;
            Func<VCIDevice, bool> predicate = (VCIDevice x) => predicateKillVehicle(x);
            if (receivedDevices.Any(predicate))
            {
                lock (syncVariableDevs)
                {
                    NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, receivedDevices.Where(predicate).ToList());
                    receivedDevices.RemoveWhere(predicateKillVehicle);
                    OnDevicesChanged(args);
                }
            }
        }

        private void InitializeMulticastSender()
        {
            multicastSenderAndListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            multicastSenderAndListener.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.MulticastSender.SendTimeout", 500);
            multicastSenderAndListener.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.MulticastSender.ReceiveTimeout", 5000);
            multicastSenderAndListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
            if (SLP.MulticastTimeToLive.HasValue)
            {
                object socketOption = multicastSenderAndListener.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive);
                if (socketOption != null)
                {
                    Log.Info("VciDeviceService.InitializeMulticastSender()", "MULTICAST TTL found: {0}; new value: {1}", socketOption, SLP.MulticastTimeToLive);
                }

                multicastSenderAndListener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, SLP.MulticastTimeToLive.Value);
            }

            multicastSenderAndListener.Bind(new IPEndPoint(IPAddress.Any, 427));
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in allNetworkInterfaces)
            {
                if (!networkInterface.SupportsMulticast || networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                IEnumerable<UnicastIPAddressInformation> enumerable = networkInterface.GetIPProperties().UnicastAddresses.Where((UnicastIPAddressInformation x) => x.Address != null && x.Address.AddressFamily == AddressFamily.InterNetwork);
                if (enumerable == null)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation item in enumerable)
                {
                    IPv4InterfaceProperties iPv4Properties = networkInterface.GetIPProperties().GetIPv4Properties();
                    if (iPv4Properties != null)
                    {
                        try
                        {
                            Log.Info("SLPReflector.Open()", "Adapter: {0} with index: {1}", networkInterface.Name, IPAddress.HostToNetworkOrder(iPv4Properties.Index));
                            multicastSenderAndListener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse("239.255.255.253"), item.Address));
                        }
                        catch (Exception exception)
                        {
                            Log.ErrorException("VciDeviceService.InitializeMulticastSender()", exception);
                        }
                    }
                }
            }

            byte[] buffer = new byte[4096];
            SocketState state = new SocketState(multicastSenderAndListener, buffer, "multicastSender");
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            multicastSenderAndListener.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveFromMulticastSenderCallback, state);
            Log.Info("SLP.Open()", "multicastSender is bound to:{0}", multicastSenderAndListener.LocalEndPoint);
        }

        private void InitializeUnicastClient()
        {
            unicastClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            unicastClient.Client.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastClient.SendTimeout", 500);
            unicastClient.Client.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastClient.ReceiveTimeout", 5000);
            unicastClient.DontFragment = SLP.DontFragment;
            unicastClient.EnableBroadcast = true;
            if (SLP.Ttl.HasValue)
            {
                unicastClient.Ttl = SLP.Ttl.Value;
            }
        }

        private void InitializeUnicastClientSocket()
        {
            unicastClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            unicastClientSocket.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.MulticastSender.SendTimeout", 500);
            unicastClientSocket.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.MulticastSender.ReceiveTimeout", 5000);
            unicastClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
            unicastClientSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
            unicastClientSocket.DontFragment = SLP.DontFragment;
            unicastClientSocket.EnableBroadcast = true;
            if (SLP.Ttl.HasValue)
            {
                unicastClientSocket.Ttl = SLP.Ttl.Value;
            }
        }

        private void InitializeUnicastSender(IPEndPoint multicastSenderLocalEndPoint)
        {
            unicastListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            unicastListener.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastListener.ReceiveTimeout", 5000);
            unicastListener.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastListener.SendTimeout", 5000);
            unicastListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
            unicastListener.Bind(multicastSenderLocalEndPoint);
            unicastListener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, optionValue: true);
            byte[] buffer = new byte[4096];
            SocketState state = new SocketState(unicastListener, buffer, "unicastListener");
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            unicastListener.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveFromUnicastListenerCallback, state);
            Log.Info("SLP.Open()", "unicastListener is bound to:{0}", unicastListener.LocalEndPoint);
        }

        private void OnDevicesChanged(NotifyCollectionChangedEventArgs args)
        {
            DevicesChanged?.Invoke(this, args);
        }

        private void SendUnicastToDevice(SLPAttrRqst myAttrRqst, IList<string> deviceIPs)
        {
            if (deviceIPs == null || deviceIPs.Count <= 0)
            {
                return;
            }

            if (!unicastClientReceivingIsRunning)
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    SocketState state = new SocketState(unicastClientSocket, buffer, "ReceiveCallbackUnicastClientSocket");
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    unicastClientSocket.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveCallbackUnicastClientSocket, state);
                }
                catch (Exception exception)
                {
                    Log.ErrorException("VciDeviceService.SendUnicastToDevice()", exception);
                }
            }

            foreach (string deviceIP in deviceIPs)
            {
                try
                {
                    unicastClientSocket.SendTo(myAttrRqst.sendbuffer, new IPEndPoint(IPAddress.Parse(deviceIP.Trim()), 427));
                    SLP.GlobalPacketID++;
                }
                catch (FormatException)
                {
                    Log.Debug("VciDeviceService.SendUnicastToDevice()", "The IP-address {0} has the wrong format for unicast send.", deviceIP);
                }
                catch (Exception exception2)
                {
                    Log.ErrorException("VciDeviceService.SendUnicastToDevice()", exception2);
                }
            }
        }

        private void ReceiveFromMulticastSenderCallback(IAsyncResult ar)
        {
            try
            {
                if (ar == null || !ar.IsCompleted || !(ar.AsyncState is SocketState))
                {
                    return;
                }

                try
                {
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    SocketFlags socketFlags = SocketFlags.None;
                    SocketState obj = ar.AsyncState as SocketState;
                    obj.WorkSocket.EndReceiveMessageFrom(ar, ref socketFlags, ref endPoint, out var _);
                    SLPHeader sLPHeader = SLP.SLPPacketAnalyzer(obj.Buffer);
                    if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                    {
                        sLPHeader.sender = endPoint as IPEndPoint;
                        receivedResponses.Enqueue(sLPHeader);
                        autoEvent.Set();
                    }
                }
                finally
                {
                    byte[] buffer = new byte[4096];
                    SocketState socketState = new SocketState(multicastSenderAndListener, buffer, "multicastSender");
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    multicastSenderAndListener.BeginReceiveMessageFrom(socketState.Buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveFromMulticastSenderCallback, socketState);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.ReceiveCallback()", exception);
            }
        }

        private void ReceiveFromUnicastListenerCallback(IAsyncResult ar)
        {
            try
            {
                if (ar == null || !ar.IsCompleted || !(ar.AsyncState is SocketState))
                {
                    return;
                }

                try
                {
                    EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                    SocketFlags socketFlags = SocketFlags.None;
                    SocketState obj = ar.AsyncState as SocketState;
                    obj.WorkSocket.EndReceiveMessageFrom(ar, ref socketFlags, ref endPoint, out var _);
                    SLPHeader sLPHeader = SLP.SLPPacketAnalyzer(obj.Buffer);
                    if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                    {
                        sLPHeader.sender = endPoint as IPEndPoint;
                        receivedResponses.Enqueue(sLPHeader);
                        autoEvent.Set();
                    }
                }
                finally
                {
                    byte[] buffer = new byte[4096];
                    SocketState socketState = new SocketState(unicastListener, buffer, "unicastListener");
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    unicastListener.BeginReceiveMessageFrom(socketState.Buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveFromMulticastSenderCallback, socketState);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.ReceiveCallback()", exception);
            }
        }

        private void ReceiveFromUnicastCallback(IAsyncResult ar)
        {
            try
            {
                new IPEndPoint(IPAddress.Any, 0);
                if (ar == null || !ar.IsCompleted || !(ar.AsyncState is UdpState))
                {
                    return;
                }

                UdpState udpState = ar.AsyncState as UdpState;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    if (udpState.UdpClient.Client != null)
                    {
                        byte[] inBuffer = udpState.UdpClient.EndReceive(ar, ref remoteEP);
                        unicastClientReceivingIsRunning = false;
                        SLPHeader sLPHeader = SLP.SLPPacketAnalyzer(inBuffer);
                        if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                        {
                            sLPHeader.sender = remoteEP;
                            receivedResponses.Enqueue(sLPHeader);
                            autoEvent.Set();
                        }
                    }
                }
                finally
                {
                    if (unicastClient != null)
                    {
                        unicastClient.BeginReceive(ReceiveFromUnicastCallback, new UdpState(unicastClient));
                        unicastClientReceivingIsRunning = true;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.ReceiveCallback()", exception);
            }
        }

        private void ReceiveCallbackUnicastClientSocket(IAsyncResult ar)
        {
            try
            {
                if (ar == null || !ar.IsCompleted || !(ar.AsyncState is SocketState))
                {
                    return;
                }

                SocketState socketState = ar.AsyncState as SocketState;
                EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                SocketFlags socketFlags = SocketFlags.None;
                try
                {
                    socketState.WorkSocket.EndReceiveMessageFrom(ar, ref socketFlags, ref endPoint, out var _);
                    unicastClientReceivingIsRunning = false;
                    SLPHeader sLPHeader = SLP.SLPPacketAnalyzer(socketState.Buffer);
                    if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                    {
                        sLPHeader.sender = endPoint as IPEndPoint;
                        receivedResponses.Enqueue(sLPHeader);
                        autoEvent.Set();
                    }
                }
                finally
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        SocketState state = new SocketState(unicastClientSocket, buffer, "ReceiveCallbackUnicastClientSocket");
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        unicastClientSocket.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveCallbackUnicastClientSocket, state);
                        unicastClientReceivingIsRunning = true;
                    }
                    catch (Exception)
                    {
                        unicastClientReceivingIsRunning = false;
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.ReceiveCallback()", exception);
            }
        }

        public void Start(IPAddress address)
        {
            throw new NotImplementedException();
        }
    }
}