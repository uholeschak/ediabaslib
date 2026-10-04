using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    public interface IProgrammingTokenService
    {
        IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, string requestFilePath);

        IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, int tokenVersion, string requestFilePath);
    }
}