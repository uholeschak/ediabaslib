using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.xVM;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using PsdzClient;

namespace BMW.Rheingold.xVM
{
    public class SLP
    {
        public const string LOOPBACK_ADDRESS = "127.0.0.1";

        public const string SLP_BCAST_ADDRESS = "255.255.255.255";

        public const byte SLP_FUNCT_ATTRRPLY = 7;

        public const byte SLP_FUNCT_ATTRRQST = 6;

        public const byte SLP_FUNCT_DAADVERT = 8;

        public const byte SLP_FUNCT_SAADVERT = 11;

        public const byte SLP_FUNCT_SRVACK = 5;

        public const byte SLP_FUNCT_SRVDEREG = 4;

        public const byte SLP_FUNCT_SRVREG = 3;

        public const byte SLP_FUNCT_SRVRPLY = 2;

        public const byte SLP_FUNCT_SRVRQST = 1;

        public const byte SLP_FUNCT_SRVTYPERPLY = 10;

        public const byte SLP_FUNCT_SRVTYPERQST = 9;

        public const long SLP_MAX_DATAGRAM_SIZE = 1400L;

        public const string SLP_MCAST_ADDRESS = "239.255.255.253";

        public const int SLP_RESERVED_PORT = 427;

        public const string SLPv1_DA_MCAST_ADDRESS = "224.0.1.35";

        public static ushort GlobalPacketID;

        private Socket multicastSender;

        private UdpClient unicastClient;

        private Socket unicastClientSocket;

        private uint unicastClientSocketQueueAge;

        private bool unicastClientSocketQueueArmed;

        private Socket unicastListener;

        private List<SLPHeader> unicastPacketBuffer;

        public static bool DontFragment => ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.xVM.SLP.DontFragment", defaultValue: false);

        public static short? Ttl
        {
            get
            {
                string configString = ConfigSettings.getConfigString("BMW.Rheingold.xVM.SLP.TTL", null);
                if (!string.IsNullOrEmpty(configString) && short.TryParse(configString, out var result) && result > 0 && result < 256)
                {
                    return result;
                }
                return null;
            }
        }

        public static int? MulticastTimeToLive
        {
            get
            {
                string configString = ConfigSettings.getConfigString("BMW.Rheingold.xVM.SLP.MulticastTimeToLive", "64");
                if (!string.IsNullOrEmpty(configString) && short.TryParse(configString, out var result) && result > 0 && result < 256)
                {
                    return result;
                }
                return null;
            }
        }

        public IList<SLPHeader> UnicastPacketBuffer => unicastPacketBuffer;

        static SLP()
        {
            GlobalPacketID = (ushort)new Random().Next(65535);
        }

        public SLP()
        {
            Log.Info("SLP.SLP()", "Setting up packet buffers.");
            unicastPacketBuffer = new List<SLPHeader>();
        }

        public static string AsSTRING(byte[] ucp, ushort offset, ushort length)
        {
            StringBuilder stringBuilder = new StringBuilder();
            try
            {
                for (int i = 0; i < length; i++)
                {
                    stringBuilder.Append(Convert.ToChar(ucp[offset + i]));
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.AsSTRING()", exception);
            }
            return stringBuilder.ToString();
        }

        public static string AsMacAddress(byte[] ucp, ushort offset, ushort length, string delimeter = "")
        {
            IList<string> list = new List<string>();
            try
            {
                for (int i = 0; i < length; i++)
                {
                    list.Add(ucp[offset + i].ToString("X2"));
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.AsSTRING()", exception);
            }
            return string.Join(delimeter, list);
        }

        public static string AsHexadecimalString(byte[] byteBuffer)
        {
            if (byteBuffer == null)
            {
                return null;
            }
            StringBuilder stringBuilder = new StringBuilder(byteBuffer.Length * 2);
            foreach (byte b in byteBuffer)
            {
                stringBuilder.AppendFormat("{0:X2}", b);
            }
            return stringBuilder.ToString();
        }

        public static ushort AsUINT16(byte[] ucp, ushort offset)
        {
            return checked((ushort)((ucp[offset] << 8) | ucp[offset + 1]));
        }

        public static uint AsUINT24(byte[] ucp, ushort offset)
        {
            return checked((uint)((ucp[offset] << 16) | (ucp[offset + 1] << 8) | ucp[offset + 2]));
        }

        public static uint AsUINT32(byte[] ucp, ushort offset)
        {
            return checked((uint)((ucp[offset] << 24) | (ucp[offset + 1] << 16) | (ucp[offset + 2] << 8) | ucp[offset + 3]));
        }

        public static SLPString AttrListToSLPString(Dictionary<string, string> attrList)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (attrList == null)
            {
                return null;
            }
            foreach (KeyValuePair<string, string> attr in attrList)
            {
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "({0}={1}),", attr.Key, attr.Value));
            }
            return stringBuilder.ToString().TrimEnd(',');
        }

        public static Dictionary<string, string> ParseAttrList(SLPString attrlist)
        {
            //[-] Log.Debug(xVM.DebugLevel, 6, "SLP.ParseAttrList()", "called.");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(attrlist.Str))
                {
                    string[] array = attrlist.Str.Split(',');
                    foreach (string text in array)
                    {
                        if (!string.IsNullOrEmpty(text))
                        {
                            string[] array2 = text.TrimStart('(').TrimEnd(')').Split('=');
                            if (array2.Length == 2)
                            {
                                dictionary.Add(array2[0], array2[1]);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.ParseAttrList()", exception);
            }
            //[-] Log.Debug(xVM.DebugLevel, 6, "SLP.ParseAttrList()", "ended.");
            return dictionary;
        }

        public static SLPHeader SLPPacketAnalyzer(byte[] inBuffer)
        {
            try
            {
                return SlpPacketAnalyzerExceptionUnsave(inBuffer);
            }
            catch (ArgumentException ex)
            {
                Log.Warning("SLP.SLPPacketAnalyzer()", ex.Message);
            }
            catch (Exception exception)
            {
                Log.WarningException("SLPHeader.SLPPacketAnalyzer()", exception);
            }
            return null;
        }

        public static SLPHeader SlpPacketAnalyzerExceptionUnsave(byte[] inBuffer)
        {
            if (inBuffer == null)
            {
                return null;
            }
            byte[] array = inBuffer.Clone() as byte[];
            switch (array[1])
            {
                case 0:
                    return new SLPHeader();
                case 1:
                    {
                        SLPSrvRqst sLPSrvRqst = new SLPSrvRqst();
                        sLPSrvRqst.ParseHeader(array);
                        sLPSrvRqst.ParsePayload();
                        return sLPSrvRqst;
                    }
                case 2:
                    {
                        SLPSrvRply sLPSrvRply = new SLPSrvRply();
                        sLPSrvRply.ParseHeader(array);
                        sLPSrvRply.ParsePayload();
                        return sLPSrvRply;
                    }
                case 9:
                    {
                        SLPSrvTypeRqst sLPSrvTypeRqst = new SLPSrvTypeRqst();
                        sLPSrvTypeRqst.ParseHeader(array);
                        sLPSrvTypeRqst.ParsePayload();
                        return sLPSrvTypeRqst;
                    }
                case 10:
                    {
                        SLPSrvTypeRply sLPSrvTypeRply = new SLPSrvTypeRply();
                        sLPSrvTypeRply.ParseHeader(array);
                        sLPSrvTypeRply.ParsePayload();
                        return sLPSrvTypeRply;
                    }
                case 6:
                    {
                        SLPAttrRqst sLPAttrRqst = new SLPAttrRqst();
                        sLPAttrRqst.ParseHeader(array);
                        sLPAttrRqst.ParsePayload();
                        return sLPAttrRqst;
                    }
                case 7:
                    {
                        SLPAttrRply sLPAttrRply = new SLPAttrRply();
                        sLPAttrRply.ParseHeader(array);
                        sLPAttrRply.ParsePayload();
                        return sLPAttrRply;
                    }
                default:
                    throw new ArgumentException($"*** Function id:{array[1]} ***", "SLPAttrRply.SLPTypeId", null);
            }
        }

        public static VCIDevice ScanDeviceFromAttrList(SLPAttrRply myAttrRply, params string[] argsDevices)
        {
            try
            {
                Dictionary<string, string> dictionary = ParseAttrList(myAttrRply.attrlist);
                string text = "";
                if (argsDevices.Length != 0)
                {
                    text = argsDevices[0];
                }
                VCIDevice vCIDevice;
                if (text.Equals("IMIB"))
                {
                    if (!(dictionary["DevType"] == "IMIB"))
                    {
                        return null;
                    }
                    vCIDevice = new VCIDevice(VCIDeviceType.IMIB, dictionary["DevId"], "Vehicle interface");
                    string description = (dictionary.ContainsKey("ImibType") ? dictionary["ImibType"] : null);
                    vCIDevice.Imagename = "grafik/imib.jpg";
                    vCIDevice.Description = vCIDevice.getVCIDescription(VCIDeviceType.IMIB);
                    vCIDevice.Description1 = description;
                    vCIDevice.DevType = "IMIB";
                }
                else if (dictionary["DevType"] == "ICOM-Next")
                {
                    vCIDevice = new VCIDevice(VCIDeviceType.ICOM, dictionary["DevId"], "Vehicle interface");
                    vCIDevice.Imagename = "grafik/icom.jpg";
                    vCIDevice.Description = vCIDevice.getVCIDescription(VCIDeviceType.ICOM);
                    vCIDevice.DevType = "ICOM";
                    vCIDevice.DeviceTypeDetail = DeviceTypeDetails.ICOMNext;
                    if (dictionary.ContainsKey("DevTypeExt"))
                    {
                        vCIDevice.DevTypeExt = dictionary["DevTypeExt"];
                    }
                }
                else if (dictionary["DevType"] == "IMIB")
                {
                    vCIDevice = new VCIDevice(VCIDeviceType.IMIB, dictionary["DevId"], "Vehicle interface");
                    string description2 = (dictionary.ContainsKey("ImibType") ? dictionary["ImibType"] : null);
                    vCIDevice.Imagename = "grafik/imib.jpg";
                    vCIDevice.Description = vCIDevice.getVCIDescription(VCIDeviceType.IMIB);
                    vCIDevice.Description1 = description2;
                    vCIDevice.DevType = "IMIB";
                }
                else
                {
                    if (!(dictionary["DevType"] == "ENET"))
                    {
                        return null;
                    }
                    vCIDevice = new VCIDevice(VCIDeviceType.ENET, dictionary["DevId"], "Vehicle interface");
                    vCIDevice.Description = vCIDevice.getVCIDescription(VCIDeviceType.ENET);
                    vCIDevice.DevType = "ENET";
                }
                if (dictionary.ContainsKey("Service"))
                {
                    vCIDevice.Service = dictionary["Service"];
                }
                if (dictionary.ContainsKey("Serial"))
                {
                    vCIDevice.Serial = dictionary["Serial"];
                }
                if (dictionary.ContainsKey("MacAddress"))
                {
                    vCIDevice.MacAddress = dictionary["MacAddress"];
                }
                if (dictionary.ContainsKey("WLANMacAddress"))
                {
                    vCIDevice.WLANMacAddress = dictionary["WLANMacAddress"];
                }
                if (dictionary.ContainsKey("ImageVersionBoot"))
                {
                    vCIDevice.ImageVersionBoot = dictionary["ImageVersionBoot"];
                }
                if (dictionary.ContainsKey("ImageVersionApplication"))
                {
                    vCIDevice.ImageVersionApplication = dictionary["ImageVersionApplication"];
                }
                if (dictionary.ContainsKey("ImageVersionPackage"))
                {
                    vCIDevice.ImageVersionPackage = dictionary["ImageVersionPackage"];
                }
                if (dictionary.ContainsKey("Color"))
                {
                    vCIDevice.Color = dictionary["Color"];
                }
                if (dictionary.ContainsKey("Counter"))
                {
                    vCIDevice.Counter = dictionary["Counter"];
                }
                if (dictionary.ContainsKey("State"))
                {
                    try
                    {
                        if ("ICOM".Equals(vCIDevice.DevType, StringComparison.OrdinalIgnoreCase))
                        {
                            if (dictionary["State"] != "5" && IsFirmwareUpdateRequired(vCIDevice))
                            {
                                vCIDevice.State = (IsIcomUnsupported(dictionary) ? "15" : "14");
                            }
                            else
                            {
                                vCIDevice.State = (IsIcomUnsupported(dictionary) ? "15" : dictionary["State"]);
                            }
                        }
                        else
                        {
                            vCIDevice.State = (IsIcomUnsupported(dictionary) ? "15" : dictionary["State"]);
                        }
                    }
                    catch (Exception)
                    {
                        vCIDevice.State = dictionary["State"];
                    }
                }
                if (dictionary.ContainsKey("SignalStrength"))
                {
                    vCIDevice.SignalStrength = dictionary["SignalStrength"];
                }
                if (dictionary.ContainsKey("UUID"))
                {
                    vCIDevice.UUID = dictionary["UUID"];
                }
                if (dictionary.ContainsKey("Kl15Voltage"))
                {
                    vCIDevice.Kl15Voltage = (IsIcomUnsupported(dictionary) ? string.Empty : (dictionary["Kl15Voltage"] + " mV"));
                }
                if (dictionary.ContainsKey("Kl30Voltage"))
                {
                    vCIDevice.Kl30Voltage = dictionary["Kl30Voltage"] + " mV";
                }
                if (dictionary.ContainsKey("Kl15Trigger"))
                {
                    vCIDevice.Kl15Trigger = dictionary["Kl15Trigger"];
                }
                if (dictionary.ContainsKey("PwfState"))
                {
                    vCIDevice.PwfState = dictionary["PwfState"];
                }
                if (dictionary.ContainsKey("IPAddress"))
                {
                    if (IsIcomUnsupported(dictionary))
                    {
                        vCIDevice.IPAddress = string.Empty;
                    }
                    else
                    {
                        vCIDevice.IPAddress = dictionary["IPAddress"];
                    }
                }
                else if (IsIcomUnsupported(dictionary))
                {
                    vCIDevice.IPAddress = string.Empty;
                }
                else
                {
                    vCIDevice.IPAddress = myAttrRply.sender.Address.ToString();
                }
                if (!string.IsNullOrEmpty(vCIDevice.UUID))
                {
                    VCIDevice.UUIDString2UUID(vCIDevice.UUID, out var leastSigBits, out var mostSigBits);
                    vCIDevice.leastSigBits = leastSigBits;
                    vCIDevice.mostSigBits = mostSigBits;
                }
                if (dictionary.ContainsKey("Owner"))
                {
                    vCIDevice.Owner = dictionary["Owner"];
                }
                if (dictionary.ContainsKey("VIN"))
                {
                    vCIDevice.VIN = (IsIcomUnsupported(dictionary) ? string.Empty : dictionary["VIN"]);
                }
                if (dictionary.ContainsKey("Gateway"))
                {
                    vCIDevice.Gateway = dictionary["Gateway"];
                }
                if (dictionary.ContainsKey("DoIP"))
                {
                    vCIDevice.IsDoIP = ((dictionary["DoIP"].Equals("Yes") || dictionary["DoIP"].Equals("true")) ? true : false);
                }
                if (dictionary.ContainsKey("Netmask"))
                {
                    vCIDevice.Netmask = dictionary["Netmask"];
                }
                if (dictionary.ContainsKey("Port") && int.TryParse(dictionary["Port"], out var result))
                {
                    vCIDevice.Port = result;
                }
                if (dictionary.ContainsKey("VciChannels"))
                {
                    vCIDevice.VciChannels = (IsIcomUnsupported(dictionary) ? string.Empty : dictionary["VciChannels"]);
                }
                if (dictionary.ContainsKey("NetworkType"))
                {
                    vCIDevice.NetworkType = dictionary["NetworkType"];
                    vCIDevice.OnPropertyChanged("NetworkTypeLabel");
                }
                if (dictionary.ContainsKey("AccuCapacity"))
                {
                    vCIDevice.AccuCapacity = dictionary["AccuCapacity"];
                }
                if (dictionary.ContainsKey("PowerSupply") && ushort.TryParse(dictionary["PowerSupply"], out var result2))
                {
                    vCIDevice.PowerSupply = result2;
                }
                if (dictionary.ContainsKey("isSimulation"))
                {
                    vCIDevice.IsSimulation = dictionary["isSimulation"].Equals("true");
                }
                return vCIDevice;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.ScanDeviceFromAttrList()", exception);
            }
            return null;
        }

        private static bool IsIcomUnsupported(Dictionary<string, string> myDevice)
        {
            if (myDevice.TryGetValue("DevTypeExt", out var value))
            {
                if (!(value == "ICOM_A1"))
                {
                    return value == "ICOM_A2";
                }
                return true;
            }
            return false;
        }

        [PreserveSource(Cleaned = true)]
        private static bool IsFirmwareUpdateRequired(VCIDevice device)
        {
            return false;
        }

        public static void ToUINT16(byte[] ucp, ushort val, ushort offset)
        {
            checked
            {
                try
                {
                    if (ucp != null && ucp.Length > offset + 1)
                    {
                        ucp[offset] = (byte)((val >> 8) & 0xFF);
                        ucp[1 + offset] = (byte)(val & 0xFF);
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("SLP.ToUINT16()", exception);
                }
            }
        }

        public static void ToUINT24(byte[] ucp, uint val, ushort offset)
        {
            checked
            {
                try
                {
                    if (ucp != null && ucp.Length > offset + 2)
                    {
                        ucp[offset] = (byte)((val >> 16) & 0xFF);
                        ucp[1 + offset] = (byte)((val >> 8) & 0xFF);
                        ucp[2 + offset] = (byte)(val & 0xFF);
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("SLP.ToUINT24()", exception);
                }
            }
        }

        public static void ToUINT32(byte[] ucp, uint val, ushort offset)
        {
            checked
            {
                try
                {
                    if (ucp != null && ucp.Length > offset + 3)
                    {
                        ucp[offset] = (byte)((val >> 24) & 0xFF);
                        ucp[1 + offset] = (byte)((val >> 16) & 0xFF);
                        ucp[2 + offset] = (byte)((val >> 8) & 0xFF);
                        ucp[3 + offset] = (byte)(val & 0xFF);
                    }
                }
                catch (Exception exception)
                {
                    Log.WarningException("SLP.ToUINT32()", exception);
                }
            }
        }

        public void Close()
        {
            try
            {
                if (unicastListener != null)
                {
                    unicastListener.Close();
                    unicastListener = null;
                }
                if (multicastSender != null)
                {
                    multicastSender.Close();
                    multicastSender = null;
                }
                if (unicastClient != null)
                {
                    unicastClient.Close();
                    unicastClient = null;
                }
                if (unicastClientSocket != null)
                {
                    unicastClientSocket.Close();
                    unicastClientSocket = null;
                }
                unicastClientSocketQueueArmed = false;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.Close()", exception);
            }
        }

        public void Flush()
        {
            byte[] array = new byte[10];
            try
            {
                if (unicastListener != null)
                {
                    SLPUnicastSend((IPEndPoint)unicastListener.LocalEndPoint, array);
                }
                SLPUnicastSend(new IPEndPoint(IPAddress.Loopback, 427), array);
                if (unicastClient != null)
                {
                    unicastClient.Send(array, 10, new IPEndPoint(IPAddress.Loopback, ((IPEndPoint)unicastClient.Client.LocalEndPoint).Port));
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.Flush()", exception);
            }
        }

        public ushort GetGlobalPacketID()
        {
            return GlobalPacketID;
        }

        public bool Open()
        {
            try
            {
                unicastClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                unicastClient.Client.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastClient.SendTimeout", 500);
                unicastClient.Client.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastClient.ReceiveTimeout", 5000);
                unicastClient.DontFragment = DontFragment;
                unicastClient.EnableBroadcast = true;
                if (Ttl.HasValue)
                {
                    unicastClient.Ttl = Ttl.Value;
                }
                if (unicastClientSocket == null)
                {
                    unicastClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    unicastClientSocket.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.IVMUtils.SendTimeout", 500);
                    unicastClientSocket.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.IVMUtils.ReceiveTimeout", 5000);
                    unicastClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
                    unicastClientSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                    unicastClientSocket.DontFragment = DontFragment;
                    unicastClientSocket.EnableBroadcast = true;
                    if (Ttl.HasValue)
                    {
                        unicastClientSocket.Ttl = Ttl.Value;
                    }
                }
                if (multicastSender == null)
                {
                    multicastSender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    multicastSender.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.IVMUtils.SendTimeout", 500);
                    multicastSender.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.IVMUtils.ReceiveTimeout", 5000);
                    multicastSender.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
                    multicastSender.Bind(new IPEndPoint(IPAddress.Any, 427));
                    if (MulticastTimeToLive.HasValue)
                    {
                        object socketOption = multicastSender.GetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive);
                        if (socketOption != null)
                        {
                            Log.Info("SLP.Open()", "MULTICAST TTL found: {0}; new value: {1}", socketOption, MulticastTimeToLive);
                        }
                        multicastSender.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, MulticastTimeToLive.Value);
                    }
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
                                Log.Info("SLPReflector.Open()", "Adapter: {0} with index: {1}", networkInterface.Name, IPAddress.HostToNetworkOrder(iPv4Properties.Index));
                                multicastSender.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse("239.255.255.253"), item.Address));
                            }
                        }
                    }
                    multicastSender.Connect(new IPEndPoint(IPAddress.Parse("239.255.255.253"), 427));
                    byte[] buffer = new byte[4096];
                    SocketState state = new SocketState(multicastSender, buffer, "multicastSender");
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    multicastSender.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveCallback, state);
                    Log.Info("SLP.Open()", "multicastSender is bound to:{0}", multicastSender.LocalEndPoint);
                }
                IPEndPoint localEP = multicastSender.LocalEndPoint as IPEndPoint;
                unicastListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                unicastListener.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastListener.ReceiveTimeout", 5000);
                unicastListener.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastListener.SendTimeout", 5000);
                unicastListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
                unicastListener.Bind(localEP);
                unicastListener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, optionValue: true);
                byte[] buffer2 = new byte[4096];
                SocketState state2 = new SocketState(unicastListener, buffer2, "unicastListener");
                EndPoint remoteEP2 = new IPEndPoint(IPAddress.Any, 0);
                unicastListener.BeginReceiveMessageFrom(buffer2, 0, 4096, SocketFlags.None, ref remoteEP2, ReceiveCallback, state2);
                Log.Info("SLP.Open()", "unicastListener is bound to:{0}", unicastListener.LocalEndPoint);
            }
            catch (Exception exception)
            {
                Log.ErrorException("SLP.SLPOpen()", exception);
                return false;
            }
            return true;
        }

        public void SLPMulticastSend(byte[] buffer)
        {
            try
            {
                if (multicastSender != null)
                {
                    multicastSender.SendTo(buffer, new IPEndPoint(IPAddress.Parse("239.255.255.253"), 427));
                }
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.SLPMulticastSend()", exception);
            }
            GlobalPacketID++;
        }

        public string SLPToString(SLPHeader myHeader)
        {
            if (myHeader == null)
            {
                return "header was null";
            }
            StringBuilder stringBuilder = new StringBuilder();
            switch (myHeader.functionid)
            {
                case 1:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SrvRqst\n");
                        SLPSrvRqst arg = (SLPSrvRqst)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}\n", arg);
                        break;
                    }
                case 2:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SrvRply\n");
                        SLPSrvRply sLPSrvRply = (SLPSrvRply)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, sLPSrvRply.ToString());
                        break;
                    }
                case 9:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SrvTypeRqst\n");
                        SLPSrvTypeRqst sLPSrvTypeRqst = (SLPSrvTypeRqst)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, sLPSrvTypeRqst.ToString());
                        break;
                    }
                case 10:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SrvTypeRply\n");
                        SLPSrvTypeRply sLPSrvTypeRply = (SLPSrvTypeRply)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, sLPSrvTypeRply.ToString());
                        break;
                    }
                case 6:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "AttrRqst\n");
                        SLPAttrRqst sLPAttrRqst = (SLPAttrRqst)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, sLPAttrRqst.ToString());
                        break;
                    }
                case 7:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "AttrRply\n");
                        SLPAttrRply sLPAttrRply = (SLPAttrRply)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, sLPAttrRply.ToString());
                        break;
                    }
                case 11:
                    {
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "SAAdvert\n");
                        SLPSAAdvert sLPSAAdvert = (SLPSAAdvert)myHeader;
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                        stringBuilder.AppendFormat(CultureInfo.InvariantCulture, sLPSAAdvert.ToString());
                        break;
                    }
                default:
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "*** Function id:{0} ***\n", myHeader.functionid);
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Received from: {0}\n", myHeader.sender);
                    break;
            }
            return stringBuilder.ToString();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                SocketFlags socketFlags = SocketFlags.None;
                if (ar != null && ar.IsCompleted && ar.AsyncState is SocketState)
                {
                    SocketState socketState = ar.AsyncState as SocketState;
                    try
                    {
                        socketState.WorkSocket.EndReceiveMessageFrom(ar, ref socketFlags, ref endPoint, out var _);
                        SLPHeader sLPHeader = SLPPacketAnalyzer(socketState.Buffer);
                        if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                        {
                            sLPHeader.sender = endPoint as IPEndPoint;
                            unicastPacketBuffer.Add(sLPHeader);
                        }
                        return;
                    }
                    finally
                    {
                        socketState.WorkSocket.BeginReceiveMessageFrom(socketState.Buffer, 0, 4096, SocketFlags.None, ref endPoint, ReceiveCallback, socketState);
                    }
                }
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
                        SLPHeader sLPHeader2 = SLPPacketAnalyzer(udpState.UdpClient.EndReceive(ar, ref remoteEP));
                        if (sLPHeader2 != null && sLPHeader2.validParsed && ((byte)7).Equals(sLPHeader2.functionid))
                        {
                            sLPHeader2.sender = new IPEndPoint(remoteEP.Address, remoteEP.Port);
                            unicastPacketBuffer.Add(sLPHeader2);
                        }
                    }
                }
                finally
                {
                    if (unicastClient != null)
                    {
                        unicastClient.BeginReceive(ReceiveCallback, new UdpState(unicastClient));
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
                    unicastClientSocketQueueArmed = false;
                    SLPHeader sLPHeader = SLPPacketAnalyzer(socketState.Buffer);
                    if (sLPHeader != null && sLPHeader.validParsed && sLPHeader.functionid == 7)
                    {
                        sLPHeader.sender = endPoint as IPEndPoint;
                        unicastPacketBuffer.Add(sLPHeader);
                    }
                }
                finally
                {
                    try
                    {
                        byte[] buffer = new byte[4096];
                        SocketState state = new SocketState(unicastClientSocket, buffer, "ReceiveCallbackUnicastClientSocket");
                        if (unicastClientSocket != null)
                        {
                            unicastClientSocket.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref endPoint, ReceiveCallbackUnicastClientSocket, state);
                            unicastClientSocketQueueArmed = true;
                        }
                    }
                    catch (Exception)
                    {
                        unicastClientSocketQueueArmed = false;
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

        public void SLPUnicastClientSend(byte[] sendBuffer, uint sendLength, string remoteHost, int remotePort)
        {
            try
            {
                unicastClientSocket.SendTo(sendBuffer, new IPEndPoint(IPAddress.Parse(remoteHost), remotePort));
                if (!unicastClientSocketQueueArmed)
                {
                    byte[] buffer = new byte[4096];
                    SocketState state = new SocketState(unicastClientSocket, buffer, "unicastClientSocket");
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    unicastClientSocket.BeginReceiveMessageFrom(buffer, 0, 4096, SocketFlags.None, ref remoteEP, ReceiveCallbackUnicastClientSocket, state);
                    unicastClientSocketQueueArmed = true;
                }
                else
                {
                    unicastClientSocketQueueAge++;
                    if (unicastClientSocketQueueAge > 50)
                    {
                        unicastClientSocketQueueAge = 0u;
                        unicastClientSocketQueueArmed = false;
                    }
                }
            }
            catch (SocketException)
            {
                unicastClientSocketQueueArmed = false;
            }
            catch (Exception exception)
            {
                Log.WarningException("SLP.SLPUnicastClientSend()", exception);
                unicastClientSocketQueueArmed = false;
            }
            GlobalPacketID++;
        }

        public void SLPUnicastPacketBufferReset()
        {
            if (unicastPacketBuffer != null)
            {
                unicastPacketBuffer.Clear();
            }
        }

        public void SLPUnicastSend(IPEndPoint endPoint, byte[] buffer)
        {
            try
            {
                SlpUnicastSendExceptionUnsave(endPoint, buffer);
            }
            catch (Exception ex)
            {
                Log.Warning("SLP.SLPUnicastSend()", "Failed with exception for Endpoint: {0} exception: {1}", endPoint, ex);
            }
        }

        public void SlpUnicastSendExceptionUnsave(IPEndPoint endPoint, byte[] buffer)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastSender.SendTimeout", 5000);
            socket.ReceiveTimeout = ConfigSettings.getConfigint("BMW.Rheingold.xVM.UnicastSender.ReceiveTimeout", 5000);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            socket.DontFragment = DontFragment;
            socket.MulticastLoopback = ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.xVM.UnicastSender.MulticastLoopback", defaultValue: true);
            socket.EnableBroadcast = true;
            socket.Bind(new IPEndPoint(IPAddress.Any, 427));
            if (Ttl.HasValue)
            {
                socket.Ttl = Ttl.Value;
            }
            socket.SendTo(buffer, endPoint);
            socket.Close();
        }
    }
}
