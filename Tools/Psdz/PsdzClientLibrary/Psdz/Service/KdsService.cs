using System;
using System.Net.Http;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Kds;
using BMW.Rheingold.Psdz.Model.Sfa;
using PsdzClient.Core;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class KdsService : IKdsService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string endpointService = "kds";
        public KdsService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IPsdzKdsClientsForRefurbishResultCto GetKdsClientsForRefurbish(IPsdzConnection connection, int retries, int timeBetweenRetries)
        {
            try
            {
                return KdsClientsForRefurbishResultCtoMapper.Map(_webCallHandler.ExecuteRequest<KdsClientsForRefurbishResultCtoModel>(endpointService, $"getkdsclientsforrefurbish/{connection.Id}?" + $"retries={retries}&" + $"timeBetweenRetries={timeBetweenRetries}", HttpMethod.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzPerformQuickKdsCheckResultCto PerformQuickKdsCheck(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries)
        {
            try
            {
                PerformQuickKdsCheckRequestModel requestBodyObject = new PerformQuickKdsCheckRequestModel
                {
                    KdsId = KdsIdCtoMapper.Map(kdsId),
                    Retries = retries,
                    TimeBetweenRetries = timeBetweenRetries
                };
                return PerformQuickKdsCheckResultCtoMapper.Map(_webCallHandler.ExecuteRequest<PerformQuickKdsCheckResultCtoModel>(endpointService, $"performquickkdscheck/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzPerformQuickKdsCheckResultCto PerformQuickKdsCheckSP25(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries = 3, int timeBetweenRetries = 10000)
        {
            try
            {
                PerformQuickKdsCheckSP25RequestModel requestBodyObject = new PerformQuickKdsCheckSP25RequestModel
                {
                    KdsId = KdsIdCtoMapper.Map(kdsId),
                    Retries = retries,
                    TimeBetweenRetries = timeBetweenRetries
                };
                return PerformQuickKdsCheckSP25ResultCtoMapper.Map(_webCallHandler.ExecuteRequest<PerformQuickKdsCheckSP25ResultCtoModel>(endpointService, $"performquickkdschecksp25/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzKdsActionStatusResultCto PerformRefurbishProcess(IPsdzConnection connection, IPsdzKdsIdCto kdsId, IPsdzSecureTokenEto secureToken, PsdzKdsActionIdEto psdzKdsActionId, int retries, int timeBetweenRetries)
        {
            try
            {
                KdsActionIdEtoMapper kdsActionIdEtoMapper = new KdsActionIdEtoMapper();
                PerformRefurbishProcessRequestModel requestBodyObject = new PerformRefurbishProcessRequestModel
                {
                    KdsId = KdsIdCtoMapper.Map(kdsId),
                    Retries = retries,
                    TimeBetweenRetries = timeBetweenRetries,
                    SecureToken = SecureTokenEtoMapper.Map(secureToken),
                    KdsActionId = kdsActionIdEtoMapper.GetValue(psdzKdsActionId)
                };
                return KdsActionStatusResultCtoMapper.Map(_webCallHandler.ExecuteRequest<KdsActionStatusResultCtoModel>(endpointService, $"performrefurbishprocess/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzReadPublicKeyResultCto ReadPublicKey(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries)
        {
            try
            {
                return ReadPublicKeyResultCtoMapper.Map(_webCallHandler.ExecuteRequest<ReadPublicKeyResultCtoModel>(endpointService, $"readpublickey/{connection.Id}?" + $"kdsId={kdsId.Id}&" + $"retries={retries}&" + $"timeBetweenRetries={timeBetweenRetries}", HttpMethod.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzKdsActionStatusResultCto SwitchOnComponentTheftProtection(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries)
        {
            try
            {
                SwitchOnComponentTheftProtectionRequestModel requestBodyObject = new SwitchOnComponentTheftProtectionRequestModel
                {
                    KdsId = KdsIdCtoMapper.Map(kdsId),
                    Retries = retries,
                    TimeBetweenRetries = timeBetweenRetries
                };
                return KdsActionStatusResultCtoMapper.Map(_webCallHandler.ExecuteRequest<KdsActionStatusResultCtoModel>(endpointService, $"switchoncomponenttheftprotection/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}