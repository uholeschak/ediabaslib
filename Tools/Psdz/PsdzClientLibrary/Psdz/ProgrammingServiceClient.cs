using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal sealed class ProgrammingServiceClient : PsdzClientBase<IProgrammingService>, IProgrammingService
    {
        internal ProgrammingServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public string ExecuteAsync(IPsdzConnection connection, IPsdzTal tal, IPsdzSvt svtTarget, IPsdzFa faTarget, object value, IPsdzVin vin, TalExecutionSettings talExecutionSettings)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public string ExecuteHDDUpdate(IPsdzConnection connection, IPsdzTal tal, IPsdzFa fa, IPsdzVin vin, TalExecutionSettings configs)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public IPsdzTal RequestExecutionStatus(string executionId)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public IPsdzTal RequestHddUpdateStatus(string executionId)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public void Release(string executionId)
        {
            throw new NotImplementedException();
        }

        [PreserveSource(Hint = "For backward compatibility")]
        public IPsdzTal Cancel(string executionId)
        {
            throw new NotImplementedException();
        }

        [OperationContract]
        public IEnumerable<IPsdzEcuIdentifier> CheckProgrammingCounter(IPsdzConnection connection, IPsdzTal tal)
        {
            return CallFunction((IProgrammingService m) => m.CheckProgrammingCounter(connection, tal));
        }

        public bool DisableFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
        {
            return CallFunction((IProgrammingService m) => m.DisableFsc(connection, ecuIdentifier, swtApplicationId));
        }

        public IDictionary<string, object> ExecuteAsamJob(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, string jobId, IPsdzAsamJobInputDictionary inputDictionary)
        {
            return CallFunction((IProgrammingService m) => m.ExecuteAsamJob(connection, ecuIdentifier, jobId, inputDictionary));
        }

        public long GetExecutionTimeEstimate(IPsdzConnection connection, IPsdzTal tal, bool isParallel)
        {
            return CallFunction((IProgrammingService m) => m.GetExecutionTimeEstimate(connection, tal, isParallel));
        }

        public byte[] GetFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
        {
            return CallFunction((IProgrammingService m) => m.GetFsc(connection, ecuIdentifier, swtApplicationId));
        }

        public void RequestBackupdata(string talExecutionId, string targetDir)
        {
            CallMethod(delegate (IProgrammingService m)
            {
                m.RequestBackupdata(talExecutionId, targetDir);
            });
        }

        public IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId, bool periodicalCheck)
        {
            return CallFunction((IProgrammingService m) => m.RequestSwtAction(connection, ecuIdentifier, swtApplicationId, periodicalCheck));
        }

        public IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, bool periodicalCheck)
        {
            return CallFunction((IProgrammingService m) => m.RequestSwtAction(connection, periodicalCheck));
        }

        public bool StoreFsc(IPsdzConnection connection, byte[] fsc, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId)
        {
            return CallFunction((IProgrammingService m) => m.StoreFsc(connection, fsc, ecuIdentifier, swtApplicationId));
        }

        public void TslUpdate(IPsdzConnection connection, bool complete, IPsdzSvt svtActual, IPsdzSvt svtTarget)
        {
            CallMethod(delegate (IProgrammingService m)
            {
                m.TslUpdate(connection, complete, svtActual, svtTarget);
            });
        }
    }
}
