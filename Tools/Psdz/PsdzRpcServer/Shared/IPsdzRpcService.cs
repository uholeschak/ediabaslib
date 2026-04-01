using PolyType;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PsdzRpcServer.Shared
{
    [JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
    public partial interface IPsdzRpcService : IDisposable
    {
        Task<bool> OperationActive();
        Task<bool> IsCancelPossible();
        Task CancelOperation();
        Task<bool> SetupLog4Net(string logFile);
        Task<bool> ResetStarterGuard();
        Task<string> GetIstaInstallLocation();
        Task<bool> StartProgrammingService(string istaFolder);
        Task<bool> StopProgrammingService(string istaFolder, bool force = false);
        Task<bool> ConnectVehicle(string istaFolder, string remoteHost, bool useIcom, int addTimeout = 1000);
        Task<bool> DisconnectVehicle();
        Task<bool> VehicleFunctions(PsdzOperationType operationType);
        Task<List<string>> GetLanguages();
        Task<string> GetLanguage();
        Task<bool> SetLanguage(string language, bool matchLanguage = false);
        Task<bool> GetLicenseValid();
        Task<bool> SetLicenseValid(bool licenseValid);
        Task<bool> GetCacheClearRequired();
        Task<bool> SetCacheClearRequired(bool cacheClearRequired);
        Task<bool> GetGenServiceModules();
        Task<bool> SetGenServiceModules(bool genServiceModules);
        Task<PsdzRpcCacheType> GetCacheResponseType();
        Task<bool> IsPsdzInitialized();
        Task<bool> IsVehicleConnected();
        Task<bool> IsTalPresent();
        Task<string> GetVehicleVin();
        Task<string> GetPsdzServiceHostLogDir();
        Task<List<PsdzRpcOptionType>> GetOptionTypes();
        Task<List<PsdzRpcOptionItem>> GetSelectedOptions(PsdzRpcSwiRegisterEnum? swiRegisterEnum);
        Task<PsdzSwiRegisterGroupEnum> GetSwiRegisterGroup(PsdzRpcSwiRegisterEnum swiRegisterEnum);
        Task<bool> SelectOption(PsdzRpcOptionItem optionItem, bool select);
        Task<bool> ClearOptionsDict();
        Task<bool> HasOptionsDict();
        Task<bool> UpdateTargetFa(bool reset);
    }
}
