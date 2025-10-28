using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Certificate;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using System;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class CertificateManagementService : ICertificateManagementService
    {
        private readonly IWebCallHandler _webCallHandler;

        private readonly string endpointService = "securitymanagement";

        public CertificateManagementService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public PsdzCertMemoryObject[] CalculateBindingDistribution(PsdzCertMemoryObject[] bindingsFromCbb, PsdzCertMemoryObject[] bindingsFromVehicle)
        {
            try
            {
                CalculateBindingDistributionRequestModel requestBodyObject = new CalculateBindingDistributionRequestModel
                {
                    BindingsFromCbb = bindingsFromCbb?.Select(SecurityMemoryObjectEtoMapper.Map).ToList(),
                    BindingsFromVehicle = bindingsFromVehicle?.Select(SecurityMemoryObjectEtoMapper.Map).ToList()
                };
                return _webCallHandler.ExecuteRequest<SecurityMemoryObjectEtoModel[]>(endpointService, "calculatebindingdistribution", Method.Post, requestBodyObject).Data?.Select(SecurityMemoryObjectEtoMapper.Map).ToArray();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacksOffline(string bindingsFile)
        {
            try
            {
                string text = (string.IsNullOrEmpty(bindingsFile) ? null : bindingsFile);
                return FetchCertificatesBindingsAndKeypacksCalculationResultMapper.Map(_webCallHandler.ExecuteRequest<FetchCertificatesBindingsAndKeypacksCalculationResultModel>(endpointService, "fetchcertificatesbindingsandkeypacksoffline?bindingsfile=" + text, Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzFetchEcuCertCheckingResult FetchEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries)
        {
            try
            {
                FetchEcuSecCheckingRequestModel requestBodyObject = new FetchEcuSecCheckingRequestModel
                {
                    SvtIst = SvtMapper.Map(svtIst),
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList(),
                    Retries = retries
                };
                return FetchEcuSecCheckingResultMapper.Map(_webCallHandler.ExecuteRequest<FetchEcuSecCheckingResultModel>(endpointService, $"fetchecusecchecking/{connection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzReadCertMemoryObjectResult ReadSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObjectType certMemoryObjectType)
        {
            try
            {
                ReadSecurityMemoryObjectsRequestModel requestBodyObject = new ReadSecurityMemoryObjectsRequestModel
                {
                    SvtIst = SvtMapper.Map(svtIst),
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList(),
                    SecurityMemoryObjectType = new SecurityMemoryObjectTypeEtoMapper().GetValue(certMemoryObjectType)
                };
                return ReadSecurityMemoryObjectResultMapper.Map(_webCallHandler.ExecuteRequest<ReadSecurityMemoryObjectResultModel>(endpointService, $"readsecuritymemoryobjects/{connection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzBindingCalculationFailure[] RequestCertificatesBindingsAndKeypacksOffline(PsdzCertMemoryObject[] certificates, string requestFile, string client, string system, IPsdzVin vin, IPsdzSvt svt)
        {
            try
            {
                RequestCertificatesBindingsAndKeypacksOfflineRequestModel requestBodyObject = new RequestCertificatesBindingsAndKeypacksOfflineRequestModel
                {
                    Certificates = certificates?.Select(SecurityMemoryObjectEtoMapper.Map).ToList(),
                    RequestFile = requestFile,
                    Client = client,
                    System = system,
                    Vin = VinMapper.Map(vin),
                    Svt = SvtMapper.Map(svt)
                };
                return _webCallHandler.ExecuteRequest<SecurityBackendRequestFailureCtoModel[]>(endpointService, "requestcertificatesbindingsandkeypacksoffline", Method.Post, requestBodyObject).Data?.Select(BindingCalculationFailureMapper.Map).ToArray();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzRequestEcuSecCheckingResult RequestEcuSecChecking(IPsdzConnection connection, IPsdzSvt svtIst, IPsdzEcuIdentifier[] ecus, int retries)
        {
            try
            {
                RequestEcuSecCheckingRequestModel requestBodyObject = new RequestEcuSecCheckingRequestModel
                {
                    SvtIst = SvtMapper.Map(svtIst),
                    Ecus = ecus?.Select(EcuIdentifierMapper.Map).ToList(),
                    Retries = retries
                };
                return RequestEcuSecCheckingResultMapper.Map(_webCallHandler.ExecuteRequest<RequestEcuSecCheckingResultModel>(endpointService, $"requestecusecchecking/{connection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzEcuFailureResponse[] WriteSecurityMemoryObjects(IPsdzConnection connection, IPsdzSvt svtIst, PsdzCertMemoryObject[] certificates)
        {
            try
            {
                WriteSecurityMemoryObjectsRequestModel requestBodyObject = new WriteSecurityMemoryObjectsRequestModel
                {
                    SvtIst = SvtMapper.Map(svtIst),
                    Certificates = certificates?.Select(SecurityMemoryObjectEtoMapper.Map).ToList()
                };
                return _webCallHandler.ExecuteRequest<EcuFailureResponseCtoModel[]>(endpointService, $"writesecuritymemoryobjects/{connection.Id}", Method.Post, requestBodyObject).Data?.Select(EcuFailureResponseCtoMapper.Map).ToArray();
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzResponse[] CheckBackendConnection(string[] cbbUrls, int timeout)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }

        public PsdzCertMemoryObject[] DeleteCertificatesWithRole(string role, IPsdzEcuIdentifier[] ecus, PsdzCertMemoryObject[] memoryObjects)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }

        public PsdzFetchBindingCalculationResult FetchCertificatesBindingsAndKeypacks(PsdzBindingCalculationRequestId requestId)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }

        public PsdzBindingCalculationRequestId RequestBindingCalculation(PsdzCertMemoryObject[] certificates, string[] cbbUrls, string client, string system, IPsdzVin vin, int retries, int timeout, string[] certificatesRevocationList)
        {
            throw new NotImplementedException("Not implemented in the PSdZ Web API.");
        }
    }
}