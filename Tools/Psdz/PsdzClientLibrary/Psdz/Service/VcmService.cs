using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public class VcmService : IVcmService
    {
        private readonly IWebCallHandler webCallHandler;

        private readonly string serviceName = "vcm";

        public VcmService(IWebCallHandler webCallHandler)
        {
            this.webCallHandler = webCallHandler;
        }

        public IPsdzIstufenTriple GetIStufenTripleActual(IPsdzConnection connection)
        {
            try
            {
                return ILevelTripleMapper.Map(webCallHandler.ExecuteRequest<ILevelTripleModel>(serviceName, $"requestistufen/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzIstufenTriple GetIStufenTripleBackup(IPsdzConnection connection)
        {
            try
            {
                return ILevelTripleMapper.Map(webCallHandler.ExecuteRequest<ILevelTripleModel>(serviceName, $"requestistufenfrombackup/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzStandardFa GetStandardFaActual(IPsdzConnection connection)
        {
            try
            {
                return StandardFaMapper.Map(webCallHandler.ExecuteRequest<StandardFaModel>(serviceName, $"requestfa/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzStandardFa GetStandardFaBackup(IPsdzConnection connection)
        {
            try
            {
                return StandardFaMapper.Map(webCallHandler.ExecuteRequest<StandardFaModel>(serviceName, $"requestfabackup/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzStandardFp GetStandardFp(IPsdzConnection connection)
        {
            try
            {
                return StandardFpMapper.Map(webCallHandler.ExecuteRequest<StandardFpModel>(serviceName, $"requestfp/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzStandardSvt GetStandardSvtActual(IPsdzConnection connection)
        {
            try
            {
                webCallHandler.ExecuteRequest(serviceName, $"generatesvtist/{connection.Id}", Method.Post);
                return StandardSvtMapper.Map(webCallHandler.ExecuteRequest<StandardSvtModel>(serviceName, $"requestsvtactual/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

        public IPsdzVin GetVinFromBackup(IPsdzConnection connection)
        {
            try
            {
                ApiResult<string> apiResult = webCallHandler.ExecuteRequest<string>(serviceName, $"requestvinfrombackup/{connection.Id}", Method.Get);
                return new PsdzVin
                {
                    Value = apiResult.Data
                };
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzVin GetVinFromMaster(IPsdzConnection connection)
        {
            try
            {
                ApiResult<string> apiResult = webCallHandler.ExecuteRequest<string>(serviceName, $"requestvinfrommaster/{connection.Id}", Method.Get);
                return new PsdzVin
                {
                    Value = apiResult.Data
                };
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzReadVpcFromVcmCto RequestVpcFromVcm(IPsdzConnection connection)
        {
            try
            {
                return ReadVpcFromVcmCtoMapper.Map(webCallHandler.ExecuteRequest<ReadVpcFromVcmCtoModel>(serviceName, $"requestVpcFromVcm/{connection.Id}", Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public PsdzResultStateEto WriteFa(IPsdzConnection connection, IPsdzStandardFa standardFa)
        {
            try
            {
                WriteFaRequestModel requestBodyObject = new WriteFaRequestModel
                {
                    Fa = StandardFaMapper.Map(standardFa)
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writefa/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        public PsdzResultStateEto WriteFaToBackup(IPsdzConnection connection, IPsdzStandardFa standardFa)
        {
            try
            {
                WriteFaRequestModel requestBodyObject = new WriteFaRequestModel
                {
                    Fa = StandardFaMapper.Map(standardFa)
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writefatobackup/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        public PsdzResultStateEto WriteFp(IPsdzConnection connection, IPsdzStandardFp standardFp)
        {
            try
            {
                WriteFpRequestModel requestBodyObject = new WriteFpRequestModel
                {
                    FpAsString = standardFp.AsString
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writefp/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        public PsdzResultStateEto WriteFp(IPsdzConnection connection, string standardFp)
        {
            try
            {
                WriteFpRequestModel requestBodyObject = new WriteFpRequestModel
                {
                    FpAsString = standardFp
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writefp/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        public PsdzResultStateEto WriteIStufen(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent)
        {
            try
            {
                WriteIStufenRequestModel requestBodyObject = new WriteIStufenRequestModel
                {
                    IStufeCurrent = iStufeCurrent,
                    IStufeLast = iStufeLast,
                    IStufeShipment = iStufeShipment
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writeistufen/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        public PsdzResultStateEto WriteIStufenToBackup(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent)
        {
            try
            {
                WriteIStufenRequestModel requestBodyObject = new WriteIStufenRequestModel
                {
                    IStufeCurrent = iStufeCurrent,
                    IStufeLast = iStufeLast,
                    IStufeShipment = iStufeShipment
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writeistufentobackup/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        public PsdzResultStateEto WriteSvt(IPsdzConnection connection, IPsdzStandardSvt standardSvt)
        {
            try
            {
                WriteSvtRequestModel requestBodyObject = new WriteSvtRequestModel
                {
                    StandardSvt = StandardSvtMapper.Map(standardSvt)
                };
                return GetResult(webCallHandler.ExecuteRequest<PsdzResultStateEto>(serviceName, $"writesvt/{connection.Id}", Method.Post, requestBodyObject));
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                return PsdzResultStateEto.FINISHED_WITH_ERROR;
            }
        }

        private PsdzResultStateEto GetResult(ApiResult<PsdzResultStateEto> res)
        {
            if (res.IsSuccessful)
            {
                return res.Data;
            }
            return PsdzResultStateEto.FINISHED_WITH_ERROR;
        }
    }
}