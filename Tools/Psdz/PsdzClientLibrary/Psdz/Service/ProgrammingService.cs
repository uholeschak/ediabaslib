using System;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    public class ProgrammingService : IProgrammingService
    {
        private IWebCallHandler _webCallHandler;

        private string endpointService = "programming";

        public ProgrammingService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzEcuIdentifier> CheckProgrammingCounter(IPsdzConnection connection, IPsdzTal tal)
        {
            try
            {
                CheckProgrammingCounterRequestModel requestBodyObject = new CheckProgrammingCounterRequestModel
                {
                    Tal = TalMapper.Map(tal)
                };
                return _webCallHandler.ExecuteRequest<IEnumerable<EcuIdentifierModel>>(endpointService, $"checkprogrammingcounter/{connection.Id}", Method.Post, requestBodyObject).Data?.Select(EcuIdentifierMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public bool DisableFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
        {
            try
            {
                DisableFscRequestModel requestBodyObject = new DisableFscRequestModel
                {
                    EcuIdentifier = EcuIdentifierMapper.Map(ecuIdentifier),
                    SwtApplicationId = SwtApplicationIdMapper.Map(swtApplicationId)
                };
                return _webCallHandler.ExecuteRequest<bool>(endpointService, $"disablefsc/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IDictionary<string, object> ExecuteAsamJob(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, string jobId, IPsdzAsamJobInputDictionary inputDictionary)
        {
            try
            {
                ExecuteAsamJobRequestModel requestBodyObject = new ExecuteAsamJobRequestModel
                {
                    EcuIdentifier = EcuIdentifierMapper.Map(ecuIdentifier),
                    JobId = jobId,
                    InputMap = inputDictionary.GetCopy()
                };
                return _webCallHandler.ExecuteRequest<IDictionary<string, object>>(endpointService, $"executeasamjob/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public long GetExecutionTimeEstimate(IPsdzConnection connection, IPsdzTal tal, bool isParallel)
        {
            try
            {
                GetExecutionTimeEstimateRequestmodel requestBodyObject = new GetExecutionTimeEstimateRequestmodel
                {
                    Tal = TalMapper.Map(tal),
                    Parallel = isParallel
                };
                return _webCallHandler.ExecuteRequest<long>(endpointService, $"getexecutiontimeestimate/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public byte[] GetFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
        {
            try
            {
                GetFscRequestModel requestBodyObject = new GetFscRequestModel
                {
                    EcuIdentifier = EcuIdentifierMapper.Map(ecuIdentifier),
                    SwtApplicationId = SwtApplicationIdMapper.Map(swtApplicationId)
                };
                return _webCallHandler.ExecuteRequest<byte[]>(endpointService, $"getfsc/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void RequestBackupdata(string executionId, string targetDir)
        {
            try
            {
                RequestBackupDataRequestModel requestBodyObject = new RequestBackupDataRequestModel
                {
                    BackupPath = targetDir
                };
                _webCallHandler.ExecuteRequest(endpointService, "requestbackupdata?executionId=" + executionId, Method.Post, requestBodyObject);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId, bool periodicalCheck)
        {
            try
            {
                RequestSwtActionForEcuRequestModel requestBodyObject = new RequestSwtActionForEcuRequestModel
                {
                    EcuIdentifier = EcuIdentifierMapper.Map(ecuIdentifier),
                    PeriodicalCheck = periodicalCheck,
                    SwtApplicationId = SwtApplicationIdMapper.Map(swtApplicationId)
                };
                return SwtActionMapper.Map(_webCallHandler.ExecuteRequest<SwtActionModel>(endpointService, $"requestswtstatusforecu/{connection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, bool periodicalCheck)
        {
            try
            {
                RequestSwtActionRequestModel requestBodyObject = new RequestSwtActionRequestModel
                {
                    PeriodicalCheck = periodicalCheck
                };
                return SwtActionMapper.Map(_webCallHandler.ExecuteRequest<SwtActionModel>(endpointService, $"requestswtstatus/{connection.Id}", Method.Post, requestBodyObject).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public bool StoreFsc(IPsdzConnection connection, byte[] fsc, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
        {
            try
            {
                StoreFscRequestModel requestBodyObject = new StoreFscRequestModel
                {
                    EcuIdentifier = EcuIdentifierMapper.Map(ecuIdentifier),
                    SwtApplicationId = SwtApplicationIdMapper.Map(swtApplicationId),
                    Fsc = fsc
                };
                return _webCallHandler.ExecuteRequest<bool>(endpointService, $"storefsc/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public string ExecuteHDDUpdate(IPsdzConnection connection, IPsdzTal tal, IPsdzFa fa, IPsdzVin vin, TalExecutionSettings configs)
        {
            try
            {
                ExecuteHddUpdateRequestmodel requestBodyObject = new ExecuteHddUpdateRequestmodel
                {
                    Tal = TalMapper.Map(tal),
                    TalExecutionConfig = TalExecutionConfigMapper.Map(configs),
                    Fa = FaMapper.Map(fa),
                    Vin = VinMapper.Map(vin)
                };
                return _webCallHandler.ExecuteRequest<string>(endpointService, $"executehddupdate/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void TslUpdate(IPsdzConnection connection, bool complete, IPsdzSvt svtActual, IPsdzSvt svtTarget)
        {
            try
            {
                TslUpdateRequestModel requestBodyObject = new TslUpdateRequestModel
                {
                    Complete = complete,
                    SvtTarget = SvtMapper.Map(svtTarget),
                    SvtActual = SvtMapper.Map(svtActual)
                };
                _webCallHandler.ExecuteRequest(endpointService, $"tslupdate/{connection.Id}", Method.Post, requestBodyObject);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal RequestExecutionStatus(string executionId)
        {
            try
            {
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "requestexecutionstatus?executionId=" + executionId, Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IPsdzTal RequestHddUpdateStatus(string executionId)
        {
            try
            {
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "requesthddupdatestatus?executionId=" + executionId, Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public void Release(string executionId)
        {
            _webCallHandler.ExecuteRequest(endpointService, "release?executionId=" + executionId, Method.Post);
        }

        public IPsdzTal Cancel(string executionId)
        {
            try
            {
                return TalMapper.Map(_webCallHandler.ExecuteRequest<TalModel>(endpointService, "cancel?executionId=" + executionId, Method.Get).Data);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public string ExecuteAsync(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, object value, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
        {
            try
            {
                ExecuteTalAsyncRequestModel requestBodyObject = new ExecuteTalAsyncRequestModel
                {
                    Tal = TalMapper.Map(tal),
                    FaTarget = FaMapper.Map(faTarget),
                    SvtTarget = SvtMapper.Map(svtTarget),
                    TalExecutionConfig = TalExecutionConfigMapper.Map(talExecutionSettings),
                    Vin = VinMapper.Map(vin)
                };
                return _webCallHandler.ExecuteRequest<string>(endpointService, $"executeasync/{connection.Id}", Method.Post, requestBodyObject).Data;
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}