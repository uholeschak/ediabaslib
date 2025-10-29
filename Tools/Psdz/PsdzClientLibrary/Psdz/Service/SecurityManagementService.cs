using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.SecurityManagement;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class SecurityManagementService : ISecurityManagementService
    {
        private IWebCallHandler _webCallHandler;

        private string endpointService = "securitymanagement";

        public SecurityManagementService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzEcuIdentifier> GenerateECUlistWithIPsecBitmasksDiffering(IPsdzConnection connection, byte[] targetBm, IDictionary<IPsdzEcuIdentifier, byte[]> ecuBms)
        {
            try
            {
                GenerateEcuListWithIPsecBitmasksDifferingRequestModel requestBodyObject = new GenerateEcuListWithIPsecBitmasksDifferingRequestModel
                {
                    EcuBitmasks = ecuBms.Select((KeyValuePair<IPsdzEcuIdentifier, byte[]> kvPair) => new KeyValuePairModel<EcuIdentifierModel, byte[]>
                    {
                        Key = EcuIdentifierMapper.Map(kvPair.Key),
                        Value = kvPair.Value
                    }).ToList(),
                    TargetBitmask = targetBm
                };
                return _webCallHandler.ExecuteRequest<IList<EcuIdentifierModel>>(endpointService, "generateeculistwithipsecbitmasksdiffering", Method.Post, requestBodyObject).Data?.Select(EcuIdentifierMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTargetBitmask GenerateIPSecTargetBitmask(IPsdzConnection connection, IPsdzSvt svt)
        {
            try
            {
                GenerateIPSecTargetBitmaskRequestModel requestBodyObject = new GenerateIPSecTargetBitmaskRequestModel
                {
                    Svt = SvtMapper.Map(svt)
                };
                ApiResult<TargetBitmaskModel> apiResult = _webCallHandler.ExecuteRequest<TargetBitmaskModel>(endpointService, $"generateipsectargetbitmask/{connection.Id}", Method.Post, requestBodyObject);
                return new PsdzTargetBitmask
                {
                    FailedEcus = apiResult.Data?.FailedEcus.Select(EcuFailureResponseCtoMapper.MapCto).ToList(),
                    TargetBitmask = apiResult.Data?.TargetBitmask
                };
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzEcuIdentifier> GetIPsecEnabledECUs(IPsdzSvt svt)
        {
            try
            {
                GetIPsecEnabledEcusRequestModel requestBodyObject = new GetIPsecEnabledEcusRequestModel
                {
                    Svt = SvtMapper.Map(svt)
                };
                return _webCallHandler.ExecuteRequest<IList<EcuIdentifierModel>>(endpointService, "getipsecenabledecus", Method.Post, requestBodyObject).Data?.Select(EcuIdentifierMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzReadEcuUidResultCto readEcuUid(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            try
            {
                ReadEcuUidRequestModel requestBodyObject = new ReadEcuUidRequestModel
                {
                    Ecus = ecus.Select(EcuIdentifierMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svt)
                };
                return ReadEcuUidResultMapper.Map(_webCallHandler.ExecuteRequest<ReadEcuUidResultModel>(endpointService, $"readecuuid/{connection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzIPsecEcuBitmaskResultCto ReadIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, IPsdzSvt svt)
        {
            try
            {
                ReadIPsecBitmasksRequestModel requestBodyObject = new ReadIPsecBitmasksRequestModel
                {
                    Ecus = ecus.Select(EcuIdentifierMapper.Map).ToList(),
                    Svt = SvtMapper.Map(svt)
                };
                ApiResult<IPsecEcuBitmaskResultCtoModel> apiResult = _webCallHandler.ExecuteRequest<IPsecEcuBitmaskResultCtoModel>(endpointService, $"readipsecbitmasks/{connection.Id}", Method.Post, requestBodyObject);
                return new PsdzIPsecEcuBitmaskResultCto
                {
                    SuccessEcus = apiResult.Data?.SuccessEcus.ToDictionary((KeyValuePairModel<EcuIdentifierModel, byte[]> x) => EcuIdentifierMapper.Map(x.Key), (KeyValuePairModel<EcuIdentifierModel, byte[]> y) => y.Value),
                    FailedEcus = apiResult.Data?.FailedEcus.Select((EcuFailureResponseCtoModel x) => EcuFailureResponseCtoMapper.MapCto(x))
                };
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<IPsdzEcuFailureResponseCto> WriteIPsecBitmasks(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> ecus, byte[] targetBm, IPsdzSvt svt)
        {
            try
            {
                WriteIPsecBitmasksRequestModel requestBodyObject = new WriteIPsecBitmasksRequestModel
                {
                    Ecus = ecus.Select(EcuIdentifierMapper.Map).ToList(),
                    TargetBm = targetBm,
                    Svt = SvtMapper.Map(svt)
                };
                return _webCallHandler.ExecuteRequest<IList<EcuFailureResponseCtoModel>>(endpointService, $"writeipsecbitmasks/{connection.Id}", Method.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.MapCto);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}