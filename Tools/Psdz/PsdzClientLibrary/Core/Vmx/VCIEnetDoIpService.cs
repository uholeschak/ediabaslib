using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.RheingoldSessionController;
using BMW.Rheingold.xVM;
using BMW.Rheingold.xVM.ENET;
using PsdzClient.Core;
using System;
using System.Net;
using System.Net.Sockets;
using BMW.Rheingold.CoreFramework.DatabaseProvider;

namespace BMW.Rheingold.xVM.ENET
{
    public sealed class VCIEnetDoIpService : AbstractVCIEnetService
    {
        private VCIDevice myDev;

        private object lockObj = new object();

        public bool TriggeredFromEnetPTT;

        public VCIDevice MyDev
        {
            get
            {
                VCIDevice result = null;
                lock (lockObj)
                {
                    if (myDev != null)
                    {
                        result = myDev.Clone() as VCIDevice;
                    }
                }
                return result;
            }
            set
            {
                lock (lockObj)
                {
                    myDev = value;
                }
            }
        }

        internal override int ZgwReservedPort => 13400;

        internal override byte[] ZgwBuffer => new byte[8] { 3, 252, 0, 1, 0, 0, 0, 0 };

        public VCIEnetDoIpService(ILogic logic)
            : base(logic)
        {
        }

        public override void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                UdpState udpState = (UdpState)ar.AsyncState;
                UdpClient udpClient = udpState.UdpClient;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] array = udpClient.EndReceive(ar, ref remoteEP);
                if (!stopZgwReceiving)
                {
                    StartReceiving(udpState);
                }
                if (array == null || array.Length < 33 || SLP.AsUINT32(array, 0) != 66846724)
                {
                    return;
                }
                ushort num = SLP.AsUINT16(array, 25);
                string text = SLP.AsSTRING(array, 8, 17);
                //[-] Log.Debug(xVM.DebugLevel, 5, Log.CurrentMethod(), "IP : '{0}'", remoteEP.Address.ToString());
                //[-] Log.Debug(xVM.DebugLevel, 5, Log.CurrentMethod(), "VIN17 : '{0}' and decimal DiagAddress: '{1}'.", text, num);
                //[-] Log.Debug(xVM.DebugLevel, 6, Log.CurrentMethod(), "Received byte Array with length {0} : {1}", array.Length, string.Join("-", array));
                if (num == 16)
                {
                    Log.Info(Log.CurrentMethod(), "IP : '{0}'", remoteEP.Address.ToString());
                    Log.Info(Log.CurrentMethod(), "VIN17 : '{0}' and decimal DiagAddress: '{1}'.", text, num);
                    string text2 = SLP.AsMacAddress(array, 27, 6);
                    string serialNumber = "DIAGADR" + num.ToString("X2") + "_MAC" + text2 + "_VIN" + text;
                    if (!TriggeredFromEnetPTT)
                    {
                        CreateAndUpdateEnetVciDevice(text, serialNumber, remoteEP.Address.ToString(), text2, isDoip: true);
                    }
                    else
                    {
                        MyDev = GetVciDevice(text, serialNumber, remoteEP.Address.ToString(), text2, isDoip: true);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                Log.Info(Log.CurrentMethod(), "shut down during operation");
            }
            catch (SocketException ex2)
            {
                Log.Warning(Log.CurrentMethod(), "socket error: {0} {1}", ex2.ErrorCode, ex2.Message);
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        internal VCIDevice GetVciDevice(string vin, string serialNumber, string ipAddress, string macAddress, bool isDoip)
        {
            //[-] return new VCIDevice
            //[+] return new VCIDevice(Logic.ClientContext)
            return new VCIDevice(Logic.ClientContext)
            {
                VCIType = VCIDeviceType.PTT,
                Description = "BMW DoIP based car",
                DevId = vin,
                Serial = serialNumber,
                IPAddress = ipAddress,
                MacAddress = macAddress,
                IsDoIP = isDoip,
                Color = "#ffffff",
                DevType = "PTT",
                Imagename = "grafik/imib.jpg",
                State = "4",
                VIN = vin,
                VciChannels = "[0?;1?;2?;3+]"
            };
        }
    }
}
