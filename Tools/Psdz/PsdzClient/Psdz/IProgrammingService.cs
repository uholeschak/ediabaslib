using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
	[ServiceKnownType(typeof(PsdzSwtAction))]
	[ServiceKnownType(typeof(PsdzEcuIdentifier))]
	[ServiceContract(SessionMode = SessionMode.Required)]
	[ServiceKnownType(typeof(PsdzTal))]
	[ServiceKnownType(typeof(PsdzConnection))]
	[ServiceKnownType(typeof(PsdzSwtApplicationId))]
	[ServiceKnownType(typeof(PsdzSvt))]
	[ServiceKnownType(typeof(PsdzAsamJobInputDictionary))]
	public interface IProgrammingService
	{
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IEnumerable<IPsdzEcuIdentifier> CheckProgrammingCounter(IPsdzConnection connection, IPsdzTal tal);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		bool DisableFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IDictionary<string, object> ExecuteAsamJob(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, string jobId, IPsdzAsamJobInputDictionary inputDictionary);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		long GetExecutionTimeEstimate(IPsdzConnection connection, IPsdzTal tal, bool isParallel);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		byte[] GetFsc(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		void RequestBackupdata(string talExecutionId, string targetDir);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract(Name = "RequestSwtStatusForSingleEcu")]
		IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId, bool periodicalCheck);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzSwtAction RequestSwtAction(IPsdzConnection connection, bool periodicalCheck);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		bool StoreFsc(IPsdzConnection connection, byte[] fsc, IPsdzEcuIdentifier ecuIdentifier, IPsdzSwtApplicationId swtApplicationId);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void TslUpdate(IPsdzConnection connection, bool complete, IPsdzSvt svtActual, IPsdzSvt svtTarget);
	}
}
