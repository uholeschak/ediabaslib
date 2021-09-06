using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	[ServiceKnownType(typeof(PsdzSvt))]
	[ServiceKnownType(typeof(PsdzFa))]
	[ServiceKnownType(typeof(PsdzStandardFa))]
	[ServiceKnownType(typeof(PsdzFp))]
	[ServiceKnownType(typeof(PsdzStandardFp))]
	[ServiceContract(SessionMode = SessionMode.Required)]
	[ServiceKnownType(typeof(PsdzStandardSvt))]
	[ServiceKnownType(typeof(PsdzIstufenTriple))]
	[ServiceKnownType(typeof(PsdzConnection))]
	[ServiceKnownType(typeof(PsdzVin))]
	public interface IVcmService
	{
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzIstufenTriple GetIStufenTripleActual(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzIstufenTriple GetIStufenTripleBackup(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzStandardFa GetStandardFaActual(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzStandardFa GetStandardFaBackup(IPsdzConnection connection);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzStandardFp GetStandardFp(IPsdzConnection connection);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzStandardSvt GetStandardSvtActual(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzVin GetVinFromBackup(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzVin GetVinFromMaster(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void WriteFa(IPsdzConnection connection, IPsdzStandardFa standardFa);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		void WriteFaToBackup(IPsdzConnection connection, IPsdzStandardFa standardFa);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		void WriteFp(IPsdzConnection connection, IPsdzStandardFp standardFp);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		void WriteIStufen(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void WriteIStufenToBackup(IPsdzConnection connection, string iStufeShipment, string iStufeLast, string iStufeCurrent);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void WriteSvt(IPsdzConnection connection, IPsdzStandardSvt standardSvt);
	}
}
