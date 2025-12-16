using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class MacrosService : IMacrosService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string endpointService = "macros";
        public MacrosService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzSgbmId> CheckSoftwareEntries(IEnumerable<IPsdzSgbmId> sgbmIds)
        {
            try
            {
                CheckSwesRequestModel requestBodyObject = new CheckSwesRequestModel
                {
                    SgbmIdList = sgbmIds?.Select(SgbmIdMapper.Map).ToList()
                };
                return _webCallHandler.ExecuteRequest<List<SgbmIdModel>>(endpointService, "checkswes", HttpMethod.Post, requestBodyObject).Data?.Select(SgbmIdMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuList(IPsdzFa fa, IPsdzIstufe iStufe)
        {
            try
            {
                GetInstalledECUListRequestModel getInstalledECUListRequestModel = new GetInstalledECUListRequestModel();
                getInstalledECUListRequestModel.Fa = FaMapper.Map(fa);
                getInstalledECUListRequestModel.Ilevel = iStufe?.Value;
                getInstalledECUListRequestModel.DiagAddressModels = new DiagAddressModel[0];
                getInstalledECUListRequestModel.Blacklisted = true;
                GetInstalledECUListRequestModel requestBodyObject = getInstalledECUListRequestModel;
                return _webCallHandler.ExecuteRequest<List<EcuIdentifierModel>>(endpointService, "getinstalledeculist", HttpMethod.Post, requestBodyObject).Data?.Select(EcuIdentifierMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuListWithConnection(IPsdzConnection connection, IPsdzFa fa, IPsdzIstufe iStufe)
        {
            try
            {
                GetInstalledECUListRequestModel getInstalledECUListRequestModel = new GetInstalledECUListRequestModel();
                getInstalledECUListRequestModel.Fa = FaMapper.Map(fa);
                getInstalledECUListRequestModel.Ilevel = iStufe?.Value;
                getInstalledECUListRequestModel.DiagAddressModels = new DiagAddressModel[0];
                getInstalledECUListRequestModel.Blacklisted = true;
                GetInstalledECUListRequestModel requestBodyObject = getInstalledECUListRequestModel;
                return _webCallHandler.ExecuteRequest<List<EcuIdentifierModel>>(endpointService, $"getinstalledeculist/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuIdentifierMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        internal string ExecuteTal(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, List<IPsdzSwtApplication> swtApplicationList, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
        {
            try
            {
                ExecuteTalRequestModel requestBodyObject = new ExecuteTalRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    Fa = FaMapper.Map(faTarget),
                    Svt = SvtMapper.Map(svtTarget),
                    SwtApplicationList = null,
                    Vin = VinMapper.Map(vin),
                    TalExecutionConfig = TalExecutionConfigMapper.Map(talExecutionSettings)
                };
                return _webCallHandler.ExecuteRequest<string>(endpointService, $"executetal/{connection.Id}", HttpMethod.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}