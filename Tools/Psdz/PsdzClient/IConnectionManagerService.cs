using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	[ServiceContract(SessionMode = SessionMode.Required)]
	[ServiceKnownType(typeof(PsdzConnection))]
	public interface IConnectionManagerService
	{
		// Token: 0x06000020 RID: 32
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		bool CheckConnection(IPsdzConnection connection);

		// Token: 0x06000021 RID: 33
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void CloseConnection(IPsdzConnection connection);

		// Token: 0x06000022 RID: 34
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		[FaultContract(typeof(ArgumentException))]
		IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe);

		// Token: 0x06000023 RID: 35
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		[FaultContract(typeof(ArgumentException))]
		IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe);

		// Token: 0x06000024 RID: 36
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		[FaultContract(typeof(ArgumentException))]
		IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan);

		// Token: 0x06000025 RID: 37
		[FaultContract(typeof(ArgumentException))]
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe);

		// Token: 0x06000026 RID: 38
		[FaultContract(typeof(ArgumentException))]
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe);

		// Token: 0x06000027 RID: 39
		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel);

		// Token: 0x06000028 RID: 40
		[OperationContract]
		void RequestShutdown();

		// Token: 0x06000029 RID: 41
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		int GetHttpServerPort();
	}
}
