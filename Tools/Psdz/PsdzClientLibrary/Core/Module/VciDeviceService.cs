using BMW.Rheingold.CoreFramework.Contracts;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.VehicleCommunication;
using PsdzClient.Core;
using PsdzClient.Core.Container;
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

#pragma warning disable CS4014
namespace BMW.Rheingold.xVM
{
    public sealed class VciDeviceService : IDeviceService, IDisposable
    {
        private SynchronizedCollection<SlpClientData> udpClients = new SynchronizedCollection<SlpClientData>();

        private bool disposed;

        private bool cyclicSlpRequestIsRunning;

        private bool vciDevicesCreatingLoopIsRunning;

        private readonly BlockingCollection<SLPHeader> receivedResponses;

        private readonly SynchronizedCollection<VCIDevice> receivedDevices;

        private readonly string[] acceptedMeasurementDeviceTypes;

        private readonly bool imibDeviceReservation;

        private readonly System.Timers.Timer gcReceivedDevices;

        private readonly string serialOfDefaultIcom;

        private bool isAlreadyRunning;

        private const int SlpRequestWaitingTime = 1000;

        private IEnumerable<string> knownIpAdresses;

        private static bool isDHCPActivationCalled;

        private bool icomDHCPActivationFailed;

        private CancellationTokenSource receivedResponsesTokenSource;

        private string icomDHCPNetwork = "192.168.4.0";

        private string icomDHCPNetworkMask = "255.255.255.0";

        private readonly IVDDeviceService subDeviceServiceIVD;

        private readonly IEcuKom ecuKom;

        public IList<VCIDevice> Devices => receivedDevices.ToList();

        public Regex Filter { get; set; }

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

        public event EventHandler<NotifyCollectionChangedEventArgs> DevicesChanged;

        public VciDeviceService(IEcuKom ecuKom)
        {
            serialOfDefaultIcom = ConfigSettings.getConfigString("Bmw.Rheingold.ConnectionManager.DefaultICOMData", "");
            imibDeviceReservation = ConfigSettings.getConfigStringAsBoolean("TesterGUI.IMIBDeviceReservation", defaultValue: true);
            acceptedMeasurementDeviceTypes = ConfigSettings.getConfigString("BMW.Rheingold.ConnectionManager.AcceptedMeasurementDeviceTypes", "IMIB_R2, IMIB_NX")?.TrimSplit(',');
            receivedResponses = new BlockingCollection<SLPHeader>();
            receivedDevices = new SynchronizedCollection<VCIDevice>();
            subDeviceServiceIVD = new IVDDeviceService();
            receivedResponsesTokenSource = new CancellationTokenSource();
            icomDHCPNetwork = NetUtils.Convert(ConfigSettings.getConfigString("BMW.Rheingold.ConnectionManager.ICOM.DHCP.Network", icomDHCPNetwork))?.ToString() ?? icomDHCPNetwork;
            icomDHCPNetworkMask = NetUtils.Convert(ConfigSettings.getConfigString("BMW.Rheingold.ConnectionManager.ICOM.DHCP.NetworkMask", icomDHCPNetworkMask))?.ToString() ?? icomDHCPNetworkMask;
            gcReceivedDevices = new System.Timers.Timer(5000.0);
            gcReceivedDevices.AutoReset = true;
            gcReceivedDevices.Elapsed += GcReceivedDevicesOnElapsed;
            this.ecuKom = ecuKom ?? new ECUKom("Rheingold", new List<string>());
        }

        private IPAddress BuildNetLocalBroadCastAddress(IPAddress networkMask, IPAddress netAddress)
        {
            byte[] addressBytes = networkMask.GetAddressBytes();
            byte[] array = new byte[addressBytes.Length];
            byte[] addressBytes2 = netAddress.GetAddressBytes();
            for (int i = 0; i < addressBytes.Length; i++)
            {
                array[i] = (byte)(addressBytes2[i] | ~addressBytes[i]);
            }
            return new IPAddress(array);
        }

        private void InitializeUdpClients()
        {
            Log.Info("VciDeviceService.InitializeUdpClients()", "Initialize UDP clients");
            udpClients = GetRegisteredNetworkadapter();
        }

        private SynchronizedCollection<SlpClientData> UpdateUdpClients()
        {
            SynchronizedCollection<SlpClientData> registeredNetworkadapter = GetRegisteredNetworkadapter();
            udpClients.AddRange(registeredNetworkadapter);
            return registeredNetworkadapter;
        }

        private SynchronizedCollection<SlpClientData> GetRegisteredNetworkadapter()
        {
            SynchronizedCollection<SlpClientData> synchronizedCollection = new SynchronizedCollection<SlpClientData>();
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in allNetworkInterfaces)
            {
                if (!networkInterface.SupportsMulticast || networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }
                foreach (UnicastIPAddressInformation item in networkInterface.GetIPProperties().UnicastAddresses.Where((UnicastIPAddressInformation x) => x.Address.AddressFamily == AddressFamily.InterNetwork))
                {
                    if (!udpClients.Select((SlpClientData b) => (b.UdpClient.Client.LocalEndPoint as IPEndPoint).Address.ToString()).Contains(item.Address.ToString()))
                    {
                        Log.Info("VciDeviceService.GetRegisteredNetworkadapter()", "Networkadapter '{0}' registered.", networkInterface.Name);
                        IPAddress addressToSend = BuildNetLocalBroadCastAddress(item.IPv4Mask, item.Address);
                        SlpClientData slpClientData = null;
                        try
                        {
                            slpClientData = new SlpClientData(item.Address, addressToSend, subDeviceServiceIVD);
                        }
                        catch (Exception exception)
                        {
                            Log.ErrorException("VciDeviceService.GetRegisteredNetworkadapter()", exception);
                            continue;
                        }
                        synchronizedCollection.Add(slpClientData);
                    }
                }
            }
            return synchronizedCollection;
        }

        private async Task ReceiveData(UdpClient udpClient)
        {
            while (cyclicSlpRequestIsRunning)
            {
                try
                {
                    UdpReceiveResult udpReceiveResult = await udpClient.ReceiveAsync().ConfigureAwait(continueOnCapturedContext: false);
                    SLPHeader sLPHeader = SLP.SLPPacketAnalyzer(udpReceiveResult.Buffer);
                    if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                    {
                        sLPHeader.sender = udpReceiveResult.RemoteEndPoint;
                        receivedResponses.TryAdd(sLPHeader, -1, receivedResponsesTokenSource.Token);
                    }
                    else
                    {
                        LogIncorrectHeader(sLPHeader, udpReceiveResult.RemoteEndPoint);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Info("TaskFinished", "OperationCanceled");
                    goto IL_0181;
                }
                catch (ObjectDisposedException)
                {
                    Log.Info("TaskFinished", "ObjectDisposed");
                    goto IL_0181;
                }
                catch (Exception exception)
                {
                    Log.WarningException("VciDeviceService.TaskFinished()", exception);
                    goto IL_0181;
                }
                continue;
                IL_0181:
                await Task.Delay(5000);
            }
        }

        private void LogIncorrectHeader(SLPHeader header, IPEndPoint remoteIp)
        {
            string arg = ((header == null) ? "is null" : (header.validParsed ? $"has byte function: {header.functionid}" : "was not parsed correctly"));
            Log.Info("TaskFinished", $"Packet from {remoteIp} ignored because header {arg}");
        }

        private void CyclicBroadcastDeviceRequest()
        {
            while (cyclicSlpRequestIsRunning)
            {
                SearchForNewAvailableNetworks();
                udpClients.ForEach(delegate (SlpClientData x)
                {
                    x.Send();
                });
                Thread.Sleep(1000);
            }
        }

        private bool IsValidDevice(VCIDevice device)
        {
            if (imibDeviceReservation)
            {
                return device.IsSupportedImibOrICOM(acceptedMeasurementDeviceTypes);
            }
            return true;
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

        private void OnDevicesChanged(NotifyCollectionChangedEventArgs args)
        {
            this.DevicesChanged?.Invoke(this, args);
        }

        private void CreateVciDevices()
        {
            int num = 0;
            List<string> list = new List<string>();
            Dictionary<string, DateTime> sendersTable = new Dictionary<string, DateTime>();
            int configint = ConfigSettings.getConfigint("Bmw.Rheingold.SlpPackage.LogCooldownMilliseconds", 5000);
            while (vciDevicesCreatingLoopIsRunning)
            {
                num++;
                if (!vciDevicesCreatingLoopIsRunning)
                {
                    Log.Info("CreateVciDevices()", "Exit loop with: '{0}'", num);
                    break;
                }
                SLPHeader item;
                try
                {
                    if (!receivedResponses.TryTake(out item, -1, receivedResponsesTokenSource.Token) || item.functionid != 7 || item.length <= 25)
                    {
                        if (item != null)
                        {
                            Log.Info("CreateVciDevices()", "slpPacket.functionid {0}, SLPTypeId of the package {1}, slpPacket.length {2}", item.functionid, (byte)7, item.length);
                        }
                        else
                        {
                            Log.Info("CreateVciDevices()", "slpPacket is null after dequeuing trial");
                        }
                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    continue;
                }
                SLPAttrRply sLPAttrRply = item as SLPAttrRply;
                if (sLPAttrRply != null)
                {
                    LogSlpReplyToFile(sLPAttrRply, sendersTable, configint);
                }
                else
                {
                    Log.Info("VciDeviceService.CreateVciDevices()", "slpReply is null");
                }
                if (string.IsNullOrEmpty(sLPAttrRply?.attrlist))
                {
                    Log.Info("CreateVciDevices()", "Loop continued, slpReply.attrlist is null or empty");
                    continue;
                }
                Match match = null;
                if (Filter != null)
                {
                    match = Filter.Match(sLPAttrRply.attrlist);
                }
                if (match != null && !match.Success)
                {
                    Log.Info("CreateVciDevices()", "Match is null or its flag Success is set to false, m.Success: {0}", match.Success);
                    continue;
                }
                VCIDevice newDevice = SLP.ScanDeviceFromAttrList(sLPAttrRply);
                if (newDevice == null || !IsValidDevice(newDevice))
                {
                    Log.Info("CreateVciDevices()", "newDevice is null or invalid");
                    continue;
                }
                NotifyCollectionChangedEventArgs e = null;
                VCIDevice vCIDevice = receivedDevices.SingleOrDefault((VCIDevice x) => x.Equals(newDevice));
                Log.Debug("VciDeviceService.CreateVciDevices()", "Match of registered {0} and reveived vci device: {1}", vCIDevice, receivedDevices);
                if (!list.Contains(newDevice.DevId))
                {
                    Log.Info(Log.CurrentMethod(), "Following properties are detected for {0}: IP-Address {1}, Gateway ECU Diag-Address {2}", newDevice.DevId, newDevice.IPAddress, newDevice.Gateway);
                    list.Add(newDevice.DevId);
                }
                if (ActivateIcomDhcpServerForApipaNetworkIfNeccessary(newDevice.IPAddress))
                {
                    continue;
                }
                if (vCIDevice != null)
                {
                    vCIDevice.SetAlive();
                    if (!"127.0.0.1".Equals(newDevice.IPAddress) && (!EqualsDevice(vCIDevice, newDevice) || (match != null && match.Success)))
                    {
                        IList list2 = new ArrayList();
                        list2.Add(vCIDevice.Clone());
                        receivedDevices.Remove(vCIDevice);
                        receivedDevices.Add(newDevice);
                        IList list3 = new ArrayList();
                        list3.Add(newDevice);
                        e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, list3, list2);
                    }
                    else
                    {
                        Log.Debug("CreateVciDevice", "Same device without filter found!");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(serialOfDefaultIcom) && string.Equals(newDevice.Serial, serialOfDefaultIcom))
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

        private void LogSlpReplyToFile(SLPAttrRply slpReply, Dictionary<string, DateTime> sendersTable, int logCooldownInMillisec)
        {
            string key = slpReply.sender.ToString().Trim();
            DateTime now = DateTime.Now;
            if (sendersTable.TryGetValue(key, out var value))
            {
                if ((now - value).TotalMilliseconds >= (double)logCooldownInMillisec)
                {
                    Log.Info("VciDeviceService.CreateVciDevices()", "slpReply.attrList: {0}, IPAdress: {1}", slpReply?.attrlist, slpReply?.sender);
                    sendersTable[key] = now;
                }
            }
            else
            {
                Log.Info("VciDeviceService.CreateVciDevices()", "slpReply.attrList: {0}, IPAdress: {1}", slpReply?.attrlist, slpReply?.sender);
                sendersTable.Add(key, now);
            }
        }

        private bool ActivateIcomDhcpServerForApipaNetworkIfNeccessary(string deviceIpAddress)
        {
            bool num = !icomDHCPActivationFailed && deviceIpAddress.StartsWith("169.254");
            bool flag = NetworkInterface.GetAllNetworkInterfaces().Any((NetworkInterface adapter) => adapter.Supports(NetworkInterfaceComponent.IPv4) && adapter.GetIPProperties().GetIPv4Properties().IsAutomaticPrivateAddressingActive);
            if (num && flag)
            {
                if (!isDHCPActivationCalled)
                {
                    Log.Info(Log.CurrentMethod(), "The DHCP server will be activated due to an APIPA connection");
                    string text = "STEUERN_DHCP_CONTROL_STR";
                    IEcuJob ecuJob = ecuKom.ExecuteJobOverEnetActivateDHCP(deviceIpAddress, "W2V", text, "YES;" + icomDHCPNetwork + ";" + icomDHCPNetworkMask, isDoIP: false);
                    if (ecuJob != null && ecuJob.IsOkay())
                    {
                        Log.Info(Log.CurrentMethod(), "Executing " + text + " was successful.");
                        NetUtils.RenewIpConfig();
                    }
                    else
                    {
                        Log.Warning(Log.CurrentMethod(), $"Executing {text} failed with Error: {0}", ecuJob?.JobErrorText ?? "Job result was null");
                        icomDHCPActivationFailed = true;
                    }
                    SetIsDHCPActivationCalled(value: true);
                    return true;
                }
                if (!icomDHCPNetwork.StartsWith("169.254"))
                {
                    return true;
                }
            }
            return false;
        }

        private static void SetIsDHCPActivationCalled(bool value)
        {
            isDHCPActivationCalled = value;
        }

        private void GcReceivedDevicesOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            List<VCIDevice> list = receivedDevices.Where((VCIDevice x) => x.IsDead).ToList();
            if (list.Any())
            {
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list);
                list.ForEach(delegate (VCIDevice d)
                {
                    receivedDevices.Remove(d);
                });
                OnDevicesChanged(args);
            }
        }

        private void BeginReceiveLoop()
        {
            BeginReceiveLoop(udpClients);
        }

        private void BeginReceiveLoop(SynchronizedCollection<SlpClientData> clients)
        {
            foreach (SlpClientData client in clients)
            {
                ReceiveData(client.UdpClient);
            }
        }

        private void SearchForNewAvailableNetworks()
        {
            IEnumerable<string> first = (from b in NetworkInterface.GetAllNetworkInterfaces()
                                         where b.OperationalStatus == OperationalStatus.Up
                                         select b).SelectMany((NetworkInterface a) => a.GetIPProperties().UnicastAddresses.Select((UnicastIPAddressInformation b) => b.Address.ToString()));
            IEnumerable<string> obj = ((knownIpAdresses != null) ? first.Except(knownIpAdresses) : null);
            if (obj != null && obj.Any())
            {
                try
                {
                    Log.Info("VciDeviceService.SearchForNewAvailableNetworks()", "New Network detected: " + string.Join(", ", first.Except(knownIpAdresses)));
                    Update();
                }
                catch (Exception exception)
                {
                    Log.ErrorException("VciDeviceService.SearchForNewAvailableNetworks()", exception);
                }
            }
            knownIpAdresses = first;
        }

        private void Update()
        {
            if (isAlreadyRunning)
            {
                SynchronizedCollection<SlpClientData> clients = UpdateUdpClients();
                BeginReceiveLoop(clients);
            }
        }

        public void Start()
        {
            if (!isAlreadyRunning)
            {
                isAlreadyRunning = true;
                InitializeUdpClients();
                cyclicSlpRequestIsRunning = true;
                Task.Factory.StartNew(CyclicBroadcastDeviceRequest);
                vciDevicesCreatingLoopIsRunning = true;
                SetIsDHCPActivationCalled(value: false);
                icomDHCPActivationFailed = false;
                Task.Factory.StartNew(CreateVciDevices);
                gcReceivedDevices.Start();
                subDeviceServiceIVD.Start();
                BeginReceiveLoop();
            }
        }

        public void Start(IPAddress address)
        {
            throw new NotImplementedException();
        }

        public void RenewIpConfigInCaseOfIcomDhcp(string ipAddress)
        {
            if (NetUtils.IsInSameSubnet(ipAddress, icomDHCPNetwork, icomDHCPNetworkMask))
            {
                NetUtils.RenewIpConfig();
            }
        }

        public void Stop()
        {
            cyclicSlpRequestIsRunning = false;
            vciDevicesCreatingLoopIsRunning = false;
            gcReceivedDevices.Stop();
            subDeviceServiceIVD.Stop();
            isAlreadyRunning = false;
            receivedResponsesTokenSource.Cancel();
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
                udpClients.ForEach(delegate (SlpClientData x)
                {
                    x.Close();
                });
                gcReceivedDevices?.Stop();
                gcReceivedDevices?.Dispose();
                udpClients.Clear();
                receivedDevices.Clear();
            }
            disposed = true;
        }
    }
}
