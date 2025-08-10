using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using PsdzClient.Programming;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace PsdzClientLibrary.Psdz
{
    public class ConnectionManager : ProgrammingMessageListener, IPsdzProg
    {
        protected const int DEFAULT_VEHICLE_CONNECTION_PORT = 50160;

        protected const int DEFAULT_SP25_VEHICLE_CONNECTION_PORT = 50162;

        protected const int DEFAULT_ETHERNET_CONNECTION_PORT = 6801;

        protected const int DEFAULT_SP25_ETHERNET_CONNECTION_PORT = 13400;

        protected const int DEFAULT_MOTORCYCLE_CONNECTION_PORT = 50960;

        protected const int FALLBACK_MOTORCYCLE_CONNECTION_PORT = 52410;

        protected const int ICOM_REBOOT_RETRY = 2;

        private bool shouldSetConnectionToDcan;

        //private readonly IProtocolBasic fastaService;

        //public readonly IPsdzCentralConnectionService psdzCentralConnectionService;

        private readonly ISecureDiagnosticsService secureDiagnosticsService;

        //private EdiabasConnectionManager EdiabasConnection { get; }

        //private PsdzConnectionManager PsdzConnectionManager { get; }

        //private IICOMHandler ICOMHandler { get; }

        public bool AvoidTlsConnection { get; set; }

        protected virtual IVehicle Vehicle { get; }

        protected virtual string Vin17 => Vehicle?.VIN17;

        protected virtual string EReihe => Vehicle?.Ereihe;

        protected virtual bool? IsDoIP => Vehicle?.IsDoIP;

        protected virtual bool IsEES25Vehicle
        {
            get
            {
                if (Vehicle != null)
                {
                    return Vehicle.Classification.IsNCar;
                }
                return false;
            }
        }

        protected virtual string BRV => Vehicle?.Baureihenverbund;

        protected virtual IVciDevice VCI => Vehicle?.VCI;

        protected virtual string BauIstufe => Vehicle?.ILevelWerk;

        protected virtual IcomConnectionType IcomConnectionType => IcomConnectionType.DCan;

        internal bool IsNotConnectedViaPttAndEnet
        {
            get
            {
                if (!IsConnectedViaPtt())
                {
                    return !IsConnectedViaENET();
                }
                return false;
            }
        }

        internal bool IsConnectedViaPttOrEnet => !IsNotConnectedViaPttAndEnet;

        internal int ConnectionPort { get; set; }

        // [UH] arguments removed
        internal ConnectionManager(IPsdz psdz, IVehicle vehicle, IEcuKom ecuKom, IProgMsgListener progMsgListener, bool shouldSetConnectionToDcan = false, int connectionPort = -1)
            : base(progMsgListener)
        {
            //PsdzConnectionManager = new PsdzConnectionManager(psdz, protocoller);
            //psdzCentralConnectionService = PsdzCentralConnectionService.CreateInstance(PsdzConnectionManager);
            //ServiceLocator.Current.TryAddService(psdzCentralConnectionService);
            //EdiabasConnection = new EdiabasConnectionManager(ecuKom, progMsgListener);
            Vehicle = vehicle;
            this.shouldSetConnectionToDcan = shouldSetConnectionToDcan;
            ConnectionPort = connectionPort;
            //ICOMHandler = icomHandler;
            //fastaService = protocoller;
            secureDiagnosticsService = psdz.SecureDiagnosticsService;
            AvoidTlsConnection = false;
        }

        private bool IsConnectedViaPtt()
        {
            return VCI.VCIType == VCIDeviceType.PTT;
        }

        private bool IsConnectedViaENET()
        {
            return VCI.VCIType == VCIDeviceType.ENET;
        }

#if false
        internal IPsdzConnection ConnectToProject(string projectName, string vehicleInfo, int diagPort)
        {
            bool isTlsAllowed = VCI.IsDoIP && !AvoidTlsConnection;
            if (isTlsAllowed && !ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Psdz.ConnectionIsTlsAllowed", defaultValue: true))
            {
                isTlsAllowed = false;
            }
            string url;
            IPsdzConnection psdzConnection;
            string psdzFctName;
            switch (VCI.VCIType)
            {
                case VCIDeviceType.SIM:
                    url = "tcp://127.0.0.1:6801";
                    psdzConnection = psdzCentralConnectionService.OpenConnection(() => PsdzConnectionManager.ConnectionManagerService.ConnectOverEthernet(projectName, vehicleInfo, url, EReihe, BauIstufe, isTlsAllowed));
                    psdzFctName = string.Format(CultureInfo.InvariantCulture, "Psdz.ConnectOverEthernet('{0}', '{1}', '{2}')", projectName, vehicleInfo, url);
                    break;
                case VCIDeviceType.ICOM:
                    url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", VCI?.IPAddress, diagPort);
                    psdzConnection = psdzCentralConnectionService.OpenConnection(() => PsdzConnectionManager.ConnectionManagerService.ConnectOverIcom(projectName, vehicleInfo, url, 1000, EReihe, BauIstufe, IcomConnectionType, shouldSetConnectionToDcan, isTlsAllowed));
                    psdzFctName = string.Format(CultureInfo.InvariantCulture, "Psdz.ConnectOverIcom('{0}', '{1}', '{2}', {3}, '{4}', '{5}', '{6}', '{7}')", projectName, vehicleInfo, url, 1000, EReihe, BauIstufe, IcomConnectionType, shouldSetConnectionToDcan);
                    break;
                case VCIDeviceType.EDIABAS:
                    {
                        string ipFromEdiabasIni = EdiabasConnection.GetIpFromEdiabasIni();
                        url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", ipFromEdiabasIni, diagPort);
                        psdzConnection = psdzCentralConnectionService.OpenConnection(() => PsdzConnectionManager.ConnectionManagerService.ConnectOverEthernet(projectName, vehicleInfo, url, EReihe, BauIstufe, isTlsAllowed));
                        psdzFctName = string.Format(CultureInfo.InvariantCulture, "Psdz.ConnectOverEthernet('{0}', '{1}', '{2}', '{3}', '{4}')", projectName, vehicleInfo, url, EReihe, BauIstufe);
                        break;
                    }
                case VCIDeviceType.ENET:
                    url = string.Format(CultureInfo.InvariantCulture, "tcp://{0}:{1}", VCI.IPAddress, diagPort);
                    psdzConnection = psdzCentralConnectionService.OpenConnection(() => PsdzConnectionManager.ConnectionManagerService.ConnectOverEthernet(projectName, vehicleInfo, url, EReihe, BauIstufe, isTlsAllowed));
                    psdzFctName = string.Format(CultureInfo.InvariantCulture, "Psdz.ConnectOverEthernet('{0}', '{1}', '{2}', '{3}', '{4}')", projectName, vehicleInfo, url, EReihe, BauIstufe);
                    break;
                case VCIDeviceType.PTT:
                    {
                        BusObject busObject = new BusObject(1, "DCan");
                        psdzConnection = psdzCentralConnectionService.OpenConnection(() => PsdzConnectionManager.ConnectionManagerService.ConnectOverPtt(projectName, vehicleInfo, PsdzBus.BUSNAME_D_CAN, EReihe, BauIstufe, isTlsAllowed));
                        psdzFctName = string.Format(CultureInfo.InvariantCulture, "Psdz.ConnectOverPtt('{0}', '{1}', {2}, '{3}', '{4}')", projectName, vehicleInfo, busObject.Name, EReihe, BauIstufe);
                        break;
                    }
                default:
                    psdzConnection = psdzCentralConnectionService.OpenConnection(() => PsdzConnectionManager.ConnectionManagerService.ConnectOverVin(projectName, vehicleInfo, Vin17, EReihe, BauIstufe, isTlsAllowed));
                    psdzFctName = string.Format(CultureInfo.InvariantCulture, "Psdz.ConnectOverVin('{0}', '{1}', '{2}', '{3}', '{4}')", projectName, vehicleInfo, Vin17, EReihe, BauIstufe);
                    break;
            }
            bool success = psdzConnection != null;
            LogPsdzCall("ConnectToProject()", psdzFctName, success);
            if (isTlsAllowed)
            {
                RegisterCallbackAndPassCertificatesToPsdz(psdzConnection);
                secureDiagnosticsService.UnlockGateway(psdzConnection);
            }
            return psdzConnection;
        }
#endif
        public void RegisterCallbackAndPassCertificatesToPsdz(IPsdzConnection connection)
        {
            Log.Info(Log.CurrentMethod(), "Registering callbacks and passing certificates to psdz");
            if (!ServiceLocator.Current.TryGetService<ISec4DiagHandler>(out var service))
            {
                return;
            }
            try
            {
                Log.Info(Log.CurrentMethod(), "Generating certificates");
                string vin = ((!string.IsNullOrEmpty(Vin17)) ? Vin17 : VCI.VIN);
                service.GenerateS29ForPSdZ(vin);
                X509Certificate2 s29CertPSdZ = service.Sec4DiagCertificates.S29CertPSdZ;
                AsymmetricKeyParameter asymmetricKeyParameter = service.Service29KeyPair?.Private;
                X509Certificate2 caCert = service.Sec4DiagCertificates.CaCert;
                X509Certificate2 subCaCert = service.Sec4DiagCertificates.SubCaCert;
                if (s29CertPSdZ != null && asymmetricKeyParameter != null)
                {
                    Log.Info(Log.CurrentMethod(), "Registering Callback");
                    byte[] s29CertificateChainByteArray = calculateAuthService29Certificate(s29CertPSdZ, subCaCert, caCert);
                    PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(asymmetricKeyParameter);
                    secureDiagnosticsService.RegisterAuthService29Callback(s29CertificateChainByteArray, privateKeyInfo.ToAsn1Object().GetDerEncoded(), connection);
                }
            }
            catch (Exception exception)
            {
                Log.WarningException(Log.CurrentMethod(), exception);
            }
        }

        private byte[] calculateAuthService29Certificate(X509Certificate2 s29Certificate, X509Certificate2 subCaCertificate, X509Certificate2 caCertificate)
        {
            Log.Info(Log.CurrentMethod(), "Calculating S29 certificate chain");
            byte[] array = BitConverter.GetBytes((short)s29Certificate.RawData.Length).Reverse().ToArray();
            byte[] rawCertData = s29Certificate.GetRawCertData();
            byte[] array2 = BitConverter.GetBytes((short)subCaCertificate.RawData.Length).Reverse().ToArray();
            byte[] rawCertData2 = subCaCertificate.GetRawCertData();
            byte[] array3 = BitConverter.GetBytes((short)caCertificate.RawData.Length).Reverse().ToArray();
            byte[] rawCertData3 = caCertificate.GetRawCertData();
            Log.Debug(Log.CurrentMethod(), "Lenght of S29 Certificte in bytes array: " + BitConverter.ToString(array));
            Log.Debug(Log.CurrentMethod(), "S29 Certificte in bytes array: " + BitConverter.ToString(rawCertData));
            Log.Debug(Log.CurrentMethod(), "Lenght of subCA Certificte in bytes array: " + BitConverter.ToString(array2));
            Log.Debug(Log.CurrentMethod(), "subCA Certificte in bytes array: " + BitConverter.ToString(rawCertData2));
            Log.Debug(Log.CurrentMethod(), "Lenght of CA Certificte in bytes array: " + BitConverter.ToString(array3));
            Log.Debug(Log.CurrentMethod(), "CA Certificte in bytes array: " + BitConverter.ToString(rawCertData3));
            return array.Concat(rawCertData).Concat(array2).Concat(rawCertData2)
                .Concat(array3)
                .Concat(rawCertData3)
                .ToArray();
        }

#if false
        internal bool IsConnected(IPsdzConnection connection)
        {
            return PsdzConnectionManager.IsConnected(connection);
        }

        internal bool IsConnected(IPsdzConnection connection, out string psdzMessage)
        {
            return PsdzConnectionManager.IsConnected(connection, out psdzMessage);
        }

        internal IPsdzConnection ConnectToProject(string projectName, string vehicleInfo, bool restartHsfzOnError)
        {
            IPsdzConnection psdzConnection = ConnectToProject(projectName, vehicleInfo);
            if (psdzConnection == null || PsdzConnectionManager.IsConnected(psdzConnection))
            {
                return psdzConnection;
            }
            if (restartHsfzOnError && VCI.VCIType == VCIDeviceType.ICOM && !(Vehicle?.Classification.IsSp2025 ?? false))
            {
                Log.Info(Log.CurrentMethod(), "Try to restart HSFZ before connecting ...");
                Hsfz hsfz = EdiabasConnection.CreateHsfz();
                VCIDevice device = (VCIDevice)VCI;
                IICOMHandler iCOMHandler = ICOMHandler;
                if (iCOMHandler != null && iCOMHandler.DoHsfzRestart(ref device, hsfz, Vin17, BRV, EReihe, IsDoIP.Value))
                {
                    return ConnectToProject(projectName, vehicleInfo);
                }
            }
            return null;
        }

        internal IPsdzConnection ConnectToProjectOverDcan(string projectName, string vehicleInfo)
        {
            shouldSetConnectionToDcan = true;
            return ConnectToProject(projectName, vehicleInfo);
        }

        public virtual IPsdzConnection ConnectToProject(string projectName, string vehicleInfo)
        {
            IVehicle vehicle = Vehicle;
            if (vehicle != null && vehicle.Classification.IsMotorcycle())
            {
                return ConnectToMotorcycle(projectName, vehicleInfo, 50960, 52410);
            }
            int defaultVehicleConnectionPort = (UseTheDoipPort() ? 50162 : 50160);
            return ConnectToCar(projectName, vehicleInfo, defaultVehicleConnectionPort);
        }

        internal void RenewPsdzConnection(PsdzContext psdzContext)
        {
            try
            {
                Log.Info(Log.CurrentMethod(), "Called");
                psdzCentralConnectionService.ReleaseConnection();
                ReconnectToPsdz(psdzContext);
                Log.Info(Log.CurrentMethod(), "Finished");
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
            }
        }

        internal void SwitchFromEDIABASToPSdZIfConnectedViaPTTOrENET(PsdzContext context)
        {
            CloseEdiabasConnectionIfConnectedViaPTTOrENET();
            if (PsdzConnectionManager.IsConnected(context?.Connection))
            {
                Log.Info(Log.CurrentMethod(), "The PSDZ connection is already open.");
                return;
            }
            ReconnectToPsdz(context);
            Log.Info(Log.CurrentMethod(), "The PSDZ connection has been reopened.");
        }

        internal void SwitchFromEDIABASToPSdZ(PsdzContext context)
        {
            Log.Info(Log.CurrentMethod(), "Closing EDIABAS connection and reopening connection to car.");
            if (context == null)
            {
                throw new ArgumentNullException("PsdzProgramming.context");
            }
            if (context.Connection != null && PsdzConnectionManager.IsConnected(context.Connection))
            {
                Log.Info(Log.CurrentMethod(), "The PsdzConnection is already established. So no need to switch");
                return;
            }
            EdiabasConnection.CloseConnection();
            Pause();
            ReconnectToPsdz(context);
            Log.Info(Log.CurrentMethod(), "EDIABAS connection closed and PSDZ connection reopened.");
        }

        internal void SwitchFromPSdZToEDIABASIfConnectedViaPTTOrENET(PsdzContext context)
        {
            if (IsNotConnectedViaPttAndEnet)
            {
                Log.Info(Log.CurrentMethod(), " ISTA is NOT connected via PTT or ENET, no action required.");
            }
            else
            {
                SwitchFromPSdZToEDIABAS(context, Vehicle.IsDoIP);
            }
        }

        internal void SwitchFromPSdZToEDIABAS(PsdzContext context, bool isDoIP)
        {
            Log.Info(Log.CurrentMethod(), "Called");
            if (context?.Connection != null)
            {
                psdzCentralConnectionService.ReleaseConnection();
                Pause();
                Log.Info(Log.CurrentMethod(), "The PSdZ connection closed.");
            }
            else
            {
                string text = ((context == null) ? "context" : "Connection");
                Log.Info(Log.CurrentMethod(), "The PSdZ connection cannot be closed because object '" + text + "' is null");
            }
            EdiabasConnection.OpenConnection(VCI, isDoIP);
            Log.Info(Log.CurrentMethod(), "The EDIABAS connection reopened.");
        }

        internal IPsdzConnection SwitchFromEDIABASToPSdZIfConnectedViaPTTOrENET(bool restartHsfzOnError = false)
        {
            CloseEdiabasConnectionIfConnectedViaPTTOrENET();
            return ConnectToPsdz(restartHsfzOnError);
        }

        internal void CloseEdiabasConnectionIfConnectedViaPTTOrENET()
        {
            if (IsNotConnectedViaPttAndEnet)
            {
                Log.Info(Log.CurrentMethod(), " ISTA is NOT connected via PTT or ENET, no action required.");
                return;
            }
            Log.Info(Log.CurrentMethod(), " ISTA is connected via PTT or ENET -> Closing EDIABAS connection");
            EdiabasConnection.CloseConnection();
            Pause();
        }

        internal void SwitchFromPSdZToEDIABASIfConnectedViaPTTOrENET(IPsdzConnection connection)
        {
            if (IsConnectedViaPttOrEnet)
            {
                Log.Info(Log.CurrentMethod(), "Closing PSdZ connection");
                psdzCentralConnectionService.ReleaseConnection();
                Log.Info(Log.CurrentMethod(), " ISTA is connected via PTT or ENET -> Reopening EDIABAS connection");
                Pause();
                EdiabasConnection.OpenConnection(VCI, Vehicle.IsDoIP);
            }
        }

        private void LogPsdzCall(string method, string psdzFctName, bool success)
        {
            if (success)
            {
                LogDebug(method, "{0}: OK", psdzFctName);
            }
            else
            {
                LogError(method, "{0}: failed!", psdzFctName);
            }
        }

        private void Pause()
        {
            int configint = ConfigSettings.getConfigint("BMW.Rheingold.VehicleCommunication.PTT.Disconnection.WaitTime", 0);
            if (configint > 0)
            {
                Log.Info("PsdzProg.Pause()", "Give some time for the disconnection to finish.", configint / 1000);
                SleepUtility.ThreadSleep(configint, "ConnectionManager.Pause");
            }
        }

        private IPsdzConnection ConnectToCar(string projectName, string vehicleInfo, int defaultVehicleConnectionPort)
        {
            Log.Debug("ConnectionManager.ConnectToCar", "vehicle is car, connecting...");
            if (VCI != null && VCI.VCIType == VCIDeviceType.ENET)
            {
                int diagPort = (UseTheDoipPort() ? 13400 : 6801);
                return ConnectToProject(projectName, vehicleInfo, diagPort);
            }
            int num = ((ConnectionPort == -1) ? defaultVehicleConnectionPort : ConnectionPort);
            IPsdzConnection connection = ConnectToProject(projectName, vehicleInfo, num);
            if (!PsdzConnectionManager.IsConnected(connection) && !AvoidTlsConnection)
            {
                if (VCI.VCIType == VCIDeviceType.PTT)
                {
                    throw new PsdzConnectionException("PTT connection to the car failed!", "n/a", "n/a", appendIPsBeforeAndAfterHSFZRestart: false);
                }
                if (!IsEES25Vehicle)
                {
                    connection = DoHsfzRestartAndConnectAgain(projectName, vehicleInfo, num);
                }
                if (connection == null || IsEES25Vehicle)
                {
                    if (ICOMHandler == null || VCI.VCIType != VCIDeviceType.ICOM || !ConfigSettings.GetActivateICOMReboot())
                    {
                        throw new PsdzConnectionException($"Connection failed for '{VCI.VCIType}'.", "n/a", "n/a", appendIPsBeforeAndAfterHSFZRestart: false);
                    }
                    if (!DoIcomRestartAndConnectAgain(projectName, vehicleInfo, num, out connection))
                    {
                        throw new PsdzConnectionException("Connection failed after ICOM Restart.", "n/a", "n/a", appendIPsBeforeAndAfterHSFZRestart: false);
                    }
                }
            }
            else if (VCI.VCIType == VCIDeviceType.ICOM)
            {
                fastaService.AddServiceCode(ServiceCodes.CON04_PsdzConnectionSuccessfulWithoutHsfzRestart_nu_LF, "PSdZ connection is successful.", LayoutGroup.X);
            }
            return connection;
        }

        private IPsdzConnection ConnectToMotorcycle(string projectName, string vehicleInfo, int defaultMotorcycleConnectionPort, int fallbackMotorcycleConnectionPort)
        {
            Log.Debug("ConnectionManager.ConnectToMotorcycle", " vehicle is motorcycle, connecting...");
            int num = ((ConnectionPort == -1) ? defaultMotorcycleConnectionPort : ConnectionPort);
            IPsdzConnection psdzConnection;
            if (num == 52410)
            {
                Log.Info("ConnectionManager.ConnectToMotorcycle()", "Trying to establish a connection with DCAN");
                shouldSetConnectionToDcan = true;
                psdzConnection = ConnectToProject(projectName, vehicleInfo, num);
                Log.Info("ConnectionManager.ConnectToMotorcycle()", $"Connection with DCAN established: {PsdzConnectionManager.IsConnected(psdzConnection)}");
            }
            else
            {
                psdzConnection = ConnectToProject(projectName, vehicleInfo, num);
            }
            if (PsdzConnectionManager.IsConnected(psdzConnection))
            {
                ConnectionPort = num;
            }
            else
            {
                LogWarn("ConnectionManager.ConnectToMotorcycle()", $"Connection over port [{num}] failed.");
                psdzCentralConnectionService.ReleaseConnection();
                Log.Info("ConnectionManager.ConnectToMotorcycle()", $"Trying fallback: establish a connection with DCAN (PORT: {fallbackMotorcycleConnectionPort})");
                shouldSetConnectionToDcan = true;
                psdzConnection = ConnectToProject(projectName, vehicleInfo, fallbackMotorcycleConnectionPort);
                Log.Info("ConnectionManager.ConnectToMotorcycle()", $"Connection with DCAN established: {PsdzConnectionManager.IsConnected(psdzConnection)}");
                if (PsdzConnectionManager.IsConnected(psdzConnection))
                {
                    ConnectionPort = fallbackMotorcycleConnectionPort;
                }
                else
                {
                    LogWarn("ConnectionManager.ConnectToMotorcycle()", "Connection over DCAN failed.");
                    psdzConnection = null;
                }
            }
            return psdzConnection;
        }

        private IPsdzConnection DoHsfzRestartAndConnectAgain(string projectName, string vehicleInfo, int port)
        {
            if (ICOMHandler == null)
            {
                Log.Error(Log.CurrentMethod(), "HSFZ cannot be restarted because ICOMHandler is null.");
                return null;
            }
            Hsfz hsfz = EdiabasConnection.CreateHsfz();
            VCIDevice device = (VCIDevice)VCI;
            string zgmIpAddress = hsfz.GetZgmIpAddress(device.IPAddress, BRV, EReihe, IsDoIP.Value);
            if (!ICOMHandler.DoHsfzRestart(ref device, hsfz, Vin17, BRV, EReihe, IsDoIP.Value))
            {
                Log.Error(Log.CurrentMethod(), "HSFZ restart is failed.");
                return null;
            }
            psdzCentralConnectionService.ReleaseConnection();
            IPsdzConnection psdzConnection = ConnectToProject(projectName, vehicleInfo, port);
            if (!PsdzConnectionManager.IsConnected(psdzConnection))
            {
                string zgmIpAddress2 = hsfz.GetZgmIpAddress(VCI.IPAddress, BRV, EReihe, IsDoIP.Value);
                Log.Error(Log.CurrentMethod(), "Connection failed after HSFZ restart - ZgmIPAddressBefore: " + zgmIpAddress + " - ZgmIPAddressAfter: " + zgmIpAddress2);
                fastaService.AddServiceCode(string.Format(ServiceCodes.CON09_NoPsdzConnectionAfterHsfzRestart_nu_LF), "No PSdZ connection after HSFZ restart.", LayoutGroup.X, allowMultipleEntries: true);
                return null;
            }
            fastaService.AddServiceCode(string.Format(ServiceCodes.CON08_PsdzConnectionSuccessfulAfterHsfzRestart_nu_LF), "PSdZ connection is successful after HSFZ restart.", LayoutGroup.X, allowMultipleEntries: true);
            return psdzConnection;
        }

        private bool DoIcomRestartAndConnectAgain(string projectName, string vehicleInfo, int port, out IPsdzConnection connection)
        {
            connection = null;
            if (ICOMHandler.RestartCounter >= 2)
            {
                Log.Info(Log.CurrentMethod(), $"ICOM '{VCI.DevId}' was already restarted {2} times without success in this session.");
            }
            else
            {
                Log.Info(Log.CurrentMethod(), "HsfzRestart failed - Restart ICOM.");
                VCIDevice device = (VCIDevice)VCI;
                psdzCentralConnectionService.ReleaseConnection();
                if (ICOMHandler.RestartIcom(ref device, Vin17))
                {
                    connection = ConnectToProject(projectName, vehicleInfo, port);
                    if (PsdzConnectionManager.IsConnected(connection))
                    {
                        Log.Info(Log.CurrentMethod(), "ICOM Reboot was successful - Reset Restart Counter");
                        fastaService.AddServiceCode(ServiceCodes.CON10_PsdzConnectionSuccessfulAfterIcomReboot_nu_LF, "PSdZ connection is successful after ICOM reboot.", LayoutGroup.X, allowMultipleEntries: true);
                        ICOMHandler.RestartCounter = 0;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool UseTheDoipPort()
        {
            if (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Programming.PsdzProg.UseDoipPortForSp25", defaultValue: true))
            {
                if (!IsEES25Vehicle)
                {
                    if (IsDoIP.Value)
                    {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

        private void ReconnectToPsdz(PsdzContext context)
        {
            IPsdzTargetSelector targetSelector = PsdzConnectionManager.GetTargetSelector(EReihe);
            ConnectToProject(targetSelector.Project, targetSelector.VehicleInfo);
            if (context.Connection == null || !PsdzConnectionManager.IsConnected(context.Connection))
            {
                throw new PsdzConnectionException("Connection to the vehicle failed!", "n/a", "n/a", appendIPsBeforeAndAfterHSFZRestart: false);
            }
        }

        private IPsdzConnection ConnectToPsdz(bool restartHsfzOnError = false)
        {
            IPsdzTargetSelector targetSelector = PsdzConnectionManager.GetTargetSelector(EReihe);
            Log.Info(Log.CurrentMethod(), "Opening a PSdZ connection to the vehicle");
            IPsdzConnection psdzConnection = ConnectToProject(targetSelector.Project, targetSelector.VehicleInfo, restartHsfzOnError);
            if (psdzConnection == null || !PsdzConnectionManager.IsConnected(psdzConnection))
            {
                throw new PsdzConnectionException("Connection to the vehicle failed!", "n/a", "n/a", appendIPsBeforeAndAfterHSFZRestart: false);
            }
            Log.Info(Log.CurrentMethod(), "PSdZ connection opened successfully");
            return psdzConnection;
        }
#endif
    }
}