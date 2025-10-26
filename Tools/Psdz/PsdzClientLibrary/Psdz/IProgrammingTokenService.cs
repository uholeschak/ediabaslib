using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    public interface IProgrammingTokenService
    {
        IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, string requestFilePath);

        IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, int tokenVersion, string requestFilePath);
    }
}