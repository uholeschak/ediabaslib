using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using System;
using System.Net;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class ConnectionManagerService : IConnectionManagerService
    {
        private readonly IWebCallHandler _webCallHandler;

        private readonly IConnectionFactoryService _connectionFactoryService;

        private readonly IHttpConfigurationService _httpConfigurationService;

        private readonly string _endpointService = "connection";

        private string _logLevel;

        public ConnectionManagerService(IWebCallHandler webCallHandler, IConnectionFactoryService connectionFactoryService, IHttpConfigurationService httpConfigurationService)
        {
            _webCallHandler = webCallHandler;
            _connectionFactoryService = connectionFactoryService;
            _httpConfigurationService = httpConfigurationService;
            _logLevel = (ConfigSettings.getConfigStringAsBoolean("BMW.Rheingold.Logging.Level.Trace.Enabled", defaultValue: false) ? ProdiasLoglevel.INFO : ProdiasLoglevel.ERROR).ToString();
        }

        public bool CheckConnection(IPsdzConnection connection)
        {
            try
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("connection");
                }
                return _webCallHandler.ExecuteRequest<bool>(_endpointService, $"check/{connection.Id}", Method.Get).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzConnectionVerboseResult CheckConnectionVerbose(IPsdzConnection connection)
        {
            try
            {
                if (connection == null)
                {
                    throw new ArgumentNullException("connection");
                }
                return ConnectionMapper.Map(_webCallHandler.ExecuteRequest<CheckConnectionVerboseResultModel>(_endpointService, $"checkVerbose/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void CloseConnection(IPsdzConnection connection)
        {
            try
            {
                if (connection != null)
                {
                    _webCallHandler.ExecuteRequest(_endpointService, $"close/{connection.Id}", Method.Post);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe, bool isTlsAllowed)
        {
            try
            {
                switch (interfaceType)
                {
                    case InterfaceType.Vector:
                        return ConnectOverVector(project, vehicleInfo, bus, baureihe, bauIstufe);
                    case InterfaceType.Omitec:
                        return ConnectOverOmitec(project, vehicleInfo, bus, baureihe, bauIstufe);
                    default:
                        throw new ArgumentException($"InterfaceType '{interfaceType}' is not yet implemented!");
                }
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe, bool isTlsAllowed)
        {
            try
            {
                PsdzHelper.CheckString("project", project);
                PsdzHelper.CheckString("vehicleInfo", vehicleInfo);
                PsdzHelper.CheckString("url", url);
                ConnectionSettingsRequestModel requestBodyObject = new ConnectionSettingsRequestModel
                {
                    Project = project,
                    VehicleInfo = vehicleInfo,
                    Url = url,
                    Baureihe = baureihe,
                    BauIstufe = bauIstufe,
                    LogLevel = _logLevel,
                    IsTlsAllowed = isTlsAllowed
                };
                ApiResult<ConnectionModel> apiResult = _webCallHandler.ExecuteRequest<ConnectionModel>(_endpointService, "connectoverethernet", Method.Post, requestBodyObject);
                return apiResult.IsSuccessful ? ConnectionMapper.Map(apiResult.Data) : null;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan, bool isTlsAllowed)
        {
            try
            {
                PsdzHelper.CheckString("project", project);
                PsdzHelper.CheckString("vehicleInfo", vehicleInfo);
                PsdzHelper.CheckString("url", url);
                if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var result) && IPAddress.TryParse(result.Host, out var address))
                {
                    string arg = address.ToString();
                    int port = result.Port;
                    Log.Debug(Log.CurrentMethod(), $"IP-Address: {arg} - Port: {port}");
                    ConnectionSettingsRequestModel requestBodyObject = new ConnectionSettingsRequestModel
                    {
                        Project = project,
                        VehicleInfo = vehicleInfo,
                        Url = url,
                        AdditionalTransmissionTimeout = additionalTransmissionTimeout,
                        Baureihe = baureihe,
                        BauIstufe = bauIstufe,
                        ShouldSetLinkPropertiesToDCan = shouldSetLinkPropertiesToDCan,
                        LogLevel = _logLevel,
                        IsTlsAllowed = isTlsAllowed
                    };
                    ApiResult<ConnectionModel> apiResult = _webCallHandler.ExecuteRequest<ConnectionModel>(_endpointService, "connectovericom", Method.Post, requestBodyObject);
                    return apiResult.IsSuccessful ? ConnectionMapper.Map(apiResult.Data) : null;
                }
                throw new ArgumentException("Param 'url' is malformed!");
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe, bool isTlsAllowed)
        {
            try
            {
                PsdzHelper.CheckString("project", project);
                PsdzHelper.CheckString("vehicleInfo", vehicleInfo);
                PsdzHelper.CheckString("vin", vin);
                foreach (VehicleId item in _connectionFactoryService.RequestAvailableVehicles())
                {
                    string id = item.Id;
                    if (id != null && id.StartsWith(vin, StringComparison.OrdinalIgnoreCase))
                    {
                        ConnectionSettingsRequestModel requestBodyObject = new ConnectionSettingsRequestModel
                        {
                            Project = project,
                            VehicleInfo = vehicleInfo,
                            Url = item.Url,
                            Baureihe = baureihe,
                            BauIstufe = bauIstufe,
                            LogLevel = _logLevel,
                            IsTlsAllowed = isTlsAllowed
                        };
                        ApiResult<ConnectionModel> apiResult = _webCallHandler.ExecuteRequest<ConnectionModel>(_endpointService, "connectoverethernet", Method.Post, requestBodyObject);
                        return apiResult.IsSuccessful ? ConnectionMapper.Map(apiResult.Data) : null;
                    }
                }
                return null;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe, bool isTlsAllowed)
        {
            try
            {
                PsdzHelper.CheckString("project", project);
                PsdzHelper.CheckString("vehicleInfo", vehicleInfo);
                ConnectionSettingsRequestModel requestBodyObject = new ConnectionSettingsRequestModel
                {
                    Project = project,
                    VehicleInfo = vehicleInfo,
                    BusName = ((bus != null) ? new BusNameModel
                    {
                        Id = bus.Id,
                        Name = bus.Name,
                        DirectAccess = bus.DirectAccess
                    } : null),
                    Baureihe = baureihe,
                    BauIstufe = bauIstufe,
                    LogLevel = _logLevel,
                    IsTlsAllowed = isTlsAllowed
                };
                ApiResult<ConnectionModel> apiResult = _webCallHandler.ExecuteRequest<ConnectionModel>(_endpointService, "connectoverptt", Method.Post, requestBodyObject);
                return apiResult.IsSuccessful ? ConnectionMapper.Map(apiResult.Data) : null;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel)
        {
            _logLevel = prodiasLoglevel.ToString();
        }

        public void RequestShutdown()
        {
            throw new NotImplementedException();
        }

        public int GetHttpServerPort()
        {
            return _httpConfigurationService.GetHttpServerPort();
        }

        private IPsdzConnection ConnectOverVector(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe)
        {
            PsdzHelper.CheckString("project", project);
            PsdzHelper.CheckString("vehicleInfo", vehicleInfo);
            ConnectionSettingsRequestModel requestBodyObject = new ConnectionSettingsRequestModel
            {
                Project = project,
                VehicleInfo = vehicleInfo,
                BusName = ((bus != null) ? new BusNameModel
                {
                    Id = bus.Id,
                    Name = bus.Name,
                    DirectAccess = bus.DirectAccess
                } : null),
                Baureihe = baureihe,
                BauIstufe = bauIstufe,
                LogLevel = _logLevel
            };
            ApiResult<ConnectionModel> apiResult = _webCallHandler.ExecuteRequest<ConnectionModel>(_endpointService, "connectovervector", Method.Post, requestBodyObject);
            if (!apiResult.IsSuccessful)
            {
                return null;
            }
            return ConnectionMapper.Map(apiResult.Data);
        }

        private IPsdzConnection ConnectOverOmitec(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe)
        {
            PsdzHelper.CheckString("project", project);
            PsdzHelper.CheckString("vehicleInfo", vehicleInfo);
            ConnectionSettingsRequestModel requestBodyObject = new ConnectionSettingsRequestModel
            {
                Project = project,
                VehicleInfo = vehicleInfo,
                BusName = ((bus != null) ? new BusNameModel
                {
                    Id = bus.Id,
                    Name = bus.Name,
                    DirectAccess = bus.DirectAccess
                } : null),
                Baureihe = baureihe,
                BauIstufe = bauIstufe,
                LogLevel = _logLevel
            };
            ApiResult<ConnectionModel> apiResult = _webCallHandler.ExecuteRequest<ConnectionModel>(_endpointService, "connectoveromitec", Method.Post, requestBodyObject);
            if (!apiResult.IsSuccessful)
            {
                return null;
            }
            return ConnectionMapper.Map(apiResult.Data);
        }
    }
}