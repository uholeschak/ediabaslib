using PolyType;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PsdzClient.Programming.ProgrammingJobs;

namespace PsdzRpcServer.Shared;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPsdzRpcService : IDisposable
{
    Task<bool> Connect(string parameter);
    Task<bool> Disconnect(string parameter);
    Task CancelOperation();
    Task<bool> ConnectVehicle(string istaFolder, string remoteHost, bool useIcom, int addTimeout = 1000);
    Task<bool> DisconnectVehicle();
    Task<bool> VehicleFunctions(OperationType operationType);
    Task<string> GetLanguage();
    Task<bool> SetLanguage(string language);
    Task<bool> GetLicenseValid();
    Task<bool> SetLicenseValid(bool licenseValid);
    Task<bool> IsPsdzInitialized();
    Task<bool> IsVehicleConnected();
    Task<bool> IsTalPresent();
    Task<string> GetVehicleVin();
    Task<List<OptionsItem>> GetSelectedOptions();
}