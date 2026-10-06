using BMW.Rheingold.CoreFramework;
using System;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BMW.Rheingold.Psdz;
using RestSharp;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Ecu;
using RheingoldPsdzWebApi.Adapter.Contracts.Services;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Services
{
    public class EcuService : IEcuService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string _endpointService = "ecu";
        public EcuService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestContextInfoFromVehicleRequestModel requestBodyObject = new RequestContextInfoFromVehicleRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList(),
                    WantedContextItems = new List<EcuContextItemModel>
                    {
                        EcuContextItemModel.FLASH_TIMING_PARAMETER,
                        EcuContextItemModel.LAST_PROGRAMMING_DATE,
                        EcuContextItemModel.MANUFACTURING_DATE,
                        EcuContextItemModel.PERFORMED_FLASH_CYCLES,
                        EcuContextItemModel.PROGRAMMING_COUNTER,
                        EcuContextItemModel.REMAINING_FLASH_CYCLES
                    }
                };
                return _webCallHandler.ExecuteRequest<IList<EcuContextInfoModel>>(_endpointService, $"requestcontextinfofromvehicle/{connection.Id}", HttpMethod.Post, requestBodyObject).Data?.Select(EcuContextInfoMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection)
        {
            try
            {
                return StandardSvtMapper.Map(_webCallHandler.ExecuteRequest<StandardSvtModel>(_endpointService, $"requestsvt/{connection.Id}", HttpMethod.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSvtRequestModel requestBodyObject = new RequestSvtRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return StandardSvtMapper.Map(_webCallHandler.ExecuteRequest<StandardSvtModel>(_endpointService, $"requestsvtwithinstalledecus/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSvt RequestSvtWithSmacs(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSvtRequestModel requestBodyObject = new RequestSvtRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return SvtMapper.Map(_webCallHandler.ExecuteRequest<SvtModel>(_endpointService, $"requestsvtwithsmacs/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSvt RequestSVTReference(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSvtRequestModel requestBodyObject = new RequestSvtRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return SvtMapper.Map(_webCallHandler.ExecuteRequest<SvtModel>(_endpointService, $"requestsvtrsvtreference/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSvt RequestSVTwithSmAcAndMirror(IPsdzConnection connection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSVTwithSmAcAndMirrorRequestModel requestBodyObject = new RequestSVTwithSmAcAndMirrorRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return SvtMapper.Map(_webCallHandler.ExecuteRequest<SvtModel>(_endpointService, $"requestsvtwithsmacandmirror/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection connection, IPsdzSvt svt)
        {
            try
            {
                UpdatePiaPortierungsmasterRequestModel requestBodyObject = new UpdatePiaPortierungsmasterRequestModel
                {
                    Svt = SvtMapper.Map(svt)
                };
                return ResponseMapper.Map(_webCallHandler.ExecuteRequest<ResponseCtoModel>(_endpointService, $"updatepiaportierungsmaster/{connection.Id}", HttpMethod.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}