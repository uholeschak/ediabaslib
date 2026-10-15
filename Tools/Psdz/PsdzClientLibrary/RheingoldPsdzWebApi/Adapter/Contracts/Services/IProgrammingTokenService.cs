using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecurityManagement.ProgrammingTokensResultCto;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    public interface IProgrammingTokenService
    {
        IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, string requestFilePath);

        IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, int tokenVersion, string requestFilePath);
    }
}