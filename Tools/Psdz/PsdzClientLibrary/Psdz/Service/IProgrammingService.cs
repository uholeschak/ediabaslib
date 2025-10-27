using System.Collections.Generic;
using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.Swt;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzConnection))]
    [ServiceKnownType(typeof(PsdzSwtAction))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzSvt))]
    [ServiceKnownType(typeof(PsdzSwtApplicationId))]
    [ServiceKnownType(typeof(PsdzAsamJobInputDictionary))]
    public interface IProgrammingService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> CheckProgrammingCounter(IPsdzConnection connection, IPsdzTal tal);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        bool DisableFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IDictionary<string, object> ExecuteAsamJob(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, string jobId, IPsdzAsamJobInputDictionary inputDictionary);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        long GetExecutionTimeEstimate(IPsdzConnection connection, IPsdzTal tal, bool isParallel);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        byte[] GetFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void RequestBackupdata(string talExecutionId, string targetDir);

        [OperationContract(Name = "RequestSwtStatusForSingleEcu")]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId, bool periodicalCheck);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, bool periodicalCheck);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        bool StoreFsc(IPsdzConnection connection, byte[] fsc, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void TslUpdate(IPsdzConnection connection, bool complete, IPsdzSvt svtActual, IPsdzSvt svtTarget);
    }
}
