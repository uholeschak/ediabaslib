using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using PsdzClient.Core;
using PsdzClient.Core.Container;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BMW.Rheingold.CoreFramework.Programming.Data.Ecu;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient;
using PsdzClient.Programming;

namespace PsdzClient.Psdz
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

        private readonly IProtocolBasic fastaService;

        public readonly IPsdzCentralConnectionService psdzCentralConnectionService;

        private readonly ISecureDiagnosticsService secureDiagnosticsService;

        private readonly IHttpConfigurationService httpConfigurationService;

        [PreserveSource(Hint = "EdiabasConnectionManager", Placeholder = true)]
        private PlaceholderType EdiabasConnection { get; }

        [PreserveSource(Hint = "PsdzConnectionManager", Placeholder = true)]
        private PlaceholderType PsdzConnectionManager { get; }

        [PreserveSource(Hint = "IICOMHandler", Placeholder = true)]
        private PlaceholderType ICOMHandler { get; }

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

        [PreserveSource(Hint = "IProtocolBasic protocoller, IICOMHandler icomHandler, removed")]
        internal ConnectionManager(IPsdz psdz, IVehicle vehicle, IProgMsgListener progMsgListener, bool shouldSetConnectionToDcan = false, int connectionPort = -1)
            : base(progMsgListener)
        {
            // [IGNORE] PsdzConnectionManager = new PsdzConnectionManager(psdz, protocoller);
            // [IGNORE] psdzCentralConnectionService = PsdzCentralConnectionService.CreateInstance(PsdzConnectionManager);
            // [IGNORE] ServiceLocator.Current.TryAddService(psdzCentralConnectionService);
            // [IGNORE] EdiabasConnection = new EdiabasConnectionManager(ecuKom, progMsgListener);
            Vehicle = vehicle;
            this.shouldSetConnectionToDcan = shouldSetConnectionToDcan;
            ConnectionPort = connectionPort;
            // [IGNORE] ICOMHandler = icomHandler;
            // [IGNORE] fastaService = protocoller;
            secureDiagnosticsService = psdz.SecureDiagnosticsService;
            AvoidTlsConnection = false;
            httpConfigurationService = psdz.HttpConfigurationService;
        }

        private bool IsConnectedViaPtt()
        {
            return VCI.VCIType == VCIDeviceType.PTT;
        }

        private bool IsConnectedViaENET()
        {
            return VCI.VCIType == VCIDeviceType.ENET;
        }

        [PreserveSource(Hint = "Cleaned")]
        internal IPsdzConnection ConnectToProject(string projectName, string vehicleInfo, int diagPort)
        {
            throw new NotImplementedException();
        }

        private void RegisterCallbackAndPassCertificatesToPsdz(IPsdzConnection connection)
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


        [PreserveSource(Hint = "Added")]
        public void RegisterCallbackAndPassCertificatesToPsdzPublic(IPsdzConnection connection)
        {
            RegisterCallbackAndPassCertificatesToPsdz(connection);
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


        [PreserveSource(Hint = "Cleaned")]
        internal bool IsConnected(IPsdzConnection connection)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        internal bool IsConnected(IPsdzConnection connection, out string psdzMessage)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        internal IPsdzConnection ConnectToProject(string projectName, string vehicleInfo, bool restartHsfzOnError)
        {
            throw new NotImplementedException();
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

        [PreserveSource(Hint = "Cleaned")]
        internal void SwitchFromEDIABASToPSdZIfConnectedViaPTTOrENET(PsdzContext context)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        internal void SwitchFromEDIABASToPSdZ(PsdzContext context)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        internal void SwitchFromPSdZToEDIABASIfConnectedViaPTTOrENET(PsdzContext context)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        internal void SwitchFromPSdZToEDIABAS(PsdzContext context, bool isDoIP)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        internal void CloseEdiabasConnectionIfConnectedViaPTTOrENET()
        {
            throw new NotImplementedException();
        }

        internal IPsdzConnection SwitchFromEDIABASToPSdZIfConnectedViaPTTOrENET(bool restartHsfzOnError = false)
        {
            CloseEdiabasConnectionIfConnectedViaPTTOrENET();
            return ConnectToPsdz(restartHsfzOnError);
        }

        [PreserveSource(Hint = "Cleaned")]
        internal void SwitchFromPSdZToEDIABASIfConnectedViaPTTOrENET(IPsdzConnection connection)
        {
            throw new NotImplementedException();
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

        [PreserveSource(Hint = "Cleaned")]
        private IPsdzConnection ConnectToCar(string projectName, string vehicleInfo, int defaultVehicleConnectionPort)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        private IPsdzConnection ConnectToMotorcycle(string projectName, string vehicleInfo, int defaultMotorcycleConnectionPort, int fallbackMotorcycleConnectionPort)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        private IPsdzConnection DoHsfzRestartAndConnectAgain(string projectName, string vehicleInfo, int port)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        private bool DoIcomRestartAndConnectAgain(string projectName, string vehicleInfo, int port, out IPsdzConnection connection)
        {
            throw new NotImplementedException();
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

        [PreserveSource(Hint = "Cleaned")]
        private void ReconnectToPsdz(PsdzContext context)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "Cleaned")]
        private IPsdzConnection ConnectToPsdz(bool restartHsfzOnError = false)
        {
            throw new NotImplementedException();
        }

    }
}