using BMW.Rheingold.CoreFramework;
using BMW.Rheingold.xVM;
using System;
using System.Net;
using System.Net.Sockets;

namespace BMW.Rheingold.xVM.ENET
{
    public sealed class VCIEnetHsfzService : AbstractVCIEnetService
    {
        internal override int ZgwReservedPort => 6811;

        internal override byte[] ZgwBuffer => new byte[6] { 0, 0, 0, 0, 0, 17 };

        public VCIEnetHsfzService(ILogic logic)
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
                if (array.Length < 6)
                {
                    return;
                }
                ushort num = SLP.AsUINT16(array, 4);
                string text = SLP.AsSTRING(array, 6, (ushort)(array.Length - 6));
                //[-] Log.Debug(xVM.DebugLevel, 5, Log.CurrentMethod(), "IP : '{0}'", remoteEP.Address.ToString());
                //[-] Log.Debug(xVM.DebugLevel, 5, Log.CurrentMethod(), "Array as string : '{0}'", text);
                //[-] Log.Debug(xVM.DebugLevel, 6, Log.CurrentMethod(), "Received byte Array with length {0} : {1}", array.Length, string.Join("-", array));
                if (num == 17 && !string.IsNullOrEmpty(text))
                {
                    string[] array2 = text.Split(new string[1] { "BMW" }, StringSplitOptions.None);
                    if (array2.Length >= 3)
                    {
                        string vin = array2[2].Substring(3);
                        string macAddress = array2[1].Substring(3);
                        CreateAndUpdateEnetVciDevice(vin, text, remoteEP.Address.ToString(), macAddress, isDoip: false);
                    }
                }
                if (num == 16)
                {
                    Log.Info(Log.CurrentMethod(), "received KL15 response");
                }
                if (num == 19)
                {
                    Log.Info(Log.CurrentMethod(), "received control status (diag access usage) response");
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
    }
}
