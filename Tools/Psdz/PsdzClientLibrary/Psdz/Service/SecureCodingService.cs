using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.RequestNcdSignatureResponseCto;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class SecureCodingService : ISecureCodingService
    {
        private readonly IWebCallHandler _webCallHandler;

        private string endpointService = "securecoding";

        public SecureCodingService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IPsdzCheckNcdResultEto CheckNcdAvailabilityForGivenTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin)
        {
            try
            {
                CheckNcdAvailabilityForGivenTalRequestModel requestBodyObject = new CheckNcdAvailabilityForGivenTalRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    NcdDirectory = ncdDirectory,
                    Vin = VinMapper.Map(vin)
                };
                return CheckNcdResultEtoMapper.Map(_webCallHandler.ExecuteRequest<CheckNcdResultEtoModel>(endpointService, "checkncdavailabilityforgivental", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzNcd ReadNcdFromFile(string ncdDirectory, IPsdzVin vin, IPsdzSgbmId cafdSgbmid, string btldSgbmNumber)
        {
            try
            {
                ReadNcdFromFileRequestModel requestBodyObject = new ReadNcdFromFileRequestModel
                {
                    NcdDirectoryPath = ncdDirectory,
                    Vin = VinMapper.Map(vin),
                    CafdSgbmId = SgbmIdMapper.Map(cafdSgbmid),
                    BtldSgbmNumber = btldSgbmNumber
                };
                return NcdMapper.Map(_webCallHandler.ExecuteRequest<NcdModel>(endpointService, "readncdfromfile", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IList<IPsdzSecurityBackendRequestFailureCto> RequestCalculationNcdAndSignatureOffline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, string jsonRequestFilePath, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin, IPsdzFa fa, byte[] vpc)
        {
            try
            {
                RequestCalculationNcdAndSignatureOfflineRequestModel requestBodyObject = new RequestCalculationNcdAndSignatureOfflineRequestModel
                {
                    SgbmIdsForNcdCalculation = sgbmidsForNcdCalculation?.Select(RequestNcdEtoMapper.Map).ToList(),
                    JsonFilePath = jsonRequestFilePath,
                    SecureCodingConfigCto = SecureCodingConfigCtoMapper.Map(secureCodingConfigCto),
                    Vin = VinMapper.Map(vin),
                    Fa = FaMapper.Map(fa),
                    Vpc = vpc
                };
                return _webCallHandler.ExecuteRequest<IList<SecurityBackendRequestFailureCtoModel>>(endpointService, "requestcalculationncdandsignatureoffline", Method.Post, requestBodyObject).Data?.Select(SecurityBackendRequestFailureCtoMapper.Map).ToList();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzCheckNcdAvailabilityResultCto CheckNcdAvailabilityForTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API, because this was using a deprecated PSdZ method. Also it was not used in ISTA.");
        }

        public IPsdzRequestNcdSignatureResponseCto RequestSignatureOnline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API, because this was using a deprecated PSdZ method. Also it was not used in ISTA.");
        }
    }
}