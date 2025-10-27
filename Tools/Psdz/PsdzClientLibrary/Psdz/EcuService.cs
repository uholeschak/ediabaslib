using System;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public class EcuService : IEcuService
    {
        private readonly IWebCallHandler _webCallHandler;

        private readonly string _endpointService = "ecu";

        public EcuService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzEcuContextInfo> RequestEcuContextInfos(IPsdzConnection psdzConnection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
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
                return _webCallHandler.ExecuteRequest<IList<EcuContextInfoModel>>(_endpointService, $"requestcontextinfofromvehicle/{psdzConnection.Id}", Method.Post, requestBodyObject).Data?.Select(EcuContextInfoMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection psdzConnection)
        {
            try
            {
                return StandardSvtMapper.Map(_webCallHandler.ExecuteRequest<StandardSvtModel>(_endpointService, $"requestsvt/{psdzConnection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzStandardSvt RequestSvt(IPsdzConnection psdzConnection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSvtRequestModel requestBodyObject = new RequestSvtRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return StandardSvtMapper.Map(_webCallHandler.ExecuteRequest<StandardSvtModel>(_endpointService, $"requestsvtwithinstalledecus/{psdzConnection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSvt RequestSvtWithSmacs(IPsdzConnection psdzConnection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSvtRequestModel requestBodyObject = new RequestSvtRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return SvtMapper.Map(_webCallHandler.ExecuteRequest<SvtModel>(_endpointService, $"requestsvtwithsmacs/{psdzConnection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSvt RequestSVTwithSmAcAndMirror(IPsdzConnection psdzConnection, IEnumerable<IPsdzEcuIdentifier> installedEcus)
        {
            try
            {
                RequestSVTwithSmAcAndMirrorRequestModel requestBodyObject = new RequestSVTwithSmAcAndMirrorRequestModel
                {
                    InstalledEcus = installedEcus.Select(EcuIdentifierMapper.Map).ToList()
                };
                return SvtMapper.Map(_webCallHandler.ExecuteRequest<SvtModel>(_endpointService, $"requestsvtwithsmacandmirror/{psdzConnection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzResponse UpdatePiaPortierungsmaster(IPsdzConnection psdzConnection, IPsdzSvt svt)
        {
            try
            {
                UpdatePiaPortierungsmasterRequestModel requestBodyObject = new UpdatePiaPortierungsmasterRequestModel
                {
                    Svt = SvtMapper.Map(svt)
                };
                return ResponseMapper.Map(_webCallHandler.ExecuteRequest<ResponseCtoModel>(_endpointService, $"updatepiaportierungsmaster/{psdzConnection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}