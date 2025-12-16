using System;
using System.Net.Http;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public class ProgrammingTokenService : IProgrammingTokenService
    {
        private readonly IWebCallHandler webCallHandler;
        private readonly string serviceName = "programmingtoken";
        public ProgrammingTokenService(IWebCallHandler webCallHandler)
        {
            this.webCallHandler = webCallHandler;
        }

        public IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, string requestFilePath)
        {
            try
            {
                RequestProgrammingTokensOfflineWithGenericResultRequestModel requestBodyObject = new RequestProgrammingTokensOfflineWithGenericResultRequestModel
                {
                    Vin = VinMapper.Map(vin),
                    Tal = TalMapper.Map(tal),
                    SvtCurrent = SvtMapper.Map(svtCurrent),
                    SvtTarget = SvtMapper.Map(svtTarget),
                    RequestFilePath = requestFilePath
                };
                return ProgrammingTokensResultCtoMapper.Map(webCallHandler.ExecuteRequest<ProgrammingTokensResultCtoModel>(serviceName, $"requestprogrammingtokensofflinewithgenericresult/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzProgrammingTokensResultCto RequestProgrammingTokensOfflineWithGenericResult(IPsdzConnection connection, IPsdzVin vin, IPsdzTal tal, IPsdzSvt svtCurrent, IPsdzSvt svtTarget, int tokenVersion, string requestFilePath)
        {
            try
            {
                RequestProgrammingTokensOfflineWithGenericResultRequestModel requestBodyObject = new RequestProgrammingTokensOfflineWithGenericResultRequestModel
                {
                    Vin = VinMapper.Map(vin),
                    Tal = TalMapper.Map(tal),
                    SvtCurrent = SvtMapper.Map(svtCurrent),
                    SvtTarget = SvtMapper.Map(svtTarget),
                    TokenVersion = tokenVersion,
                    RequestFilePath = requestFilePath
                };
                return ProgrammingTokensResultCtoMapper.Map(webCallHandler.ExecuteRequest<ProgrammingTokensResultCtoModel>(serviceName, $"requestprogrammingtokensofflinewithtokenversionwithgenericresult/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}