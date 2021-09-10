using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Client;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
	[ServiceContract(SessionMode = SessionMode.Required)]
	[ServiceKnownType(typeof(PsdzConnection))]
	public interface IConnectionManagerService
	{
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		bool CheckConnection(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void CloseConnection(IPsdzConnection connection);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		[FaultContract(typeof(ArgumentException))]
		IPsdzConnection ConnectOverBus(string project, string vehicleInfo, PsdzBus bus, InterfaceType interfaceType, string baureihe, string bauIstufe);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		[FaultContract(typeof(ArgumentException))]
		IPsdzConnection ConnectOverEthernet(string project, string vehicleInfo, string url, string baureihe, string bauIstufe);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		[FaultContract(typeof(ArgumentException))]
		IPsdzConnection ConnectOverIcom(string project, string vehicleInfo, string url, int additionalTransmissionTimeout, string baureihe, string bauIstufe, IcomConnectionType connectionType, bool shouldSetLinkPropertiesToDCan);

		[FaultContract(typeof(ArgumentException))]
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzConnection ConnectOverVin(string project, string vehicleInfo, string vin, string baureihe, string bauIstufe);

		[FaultContract(typeof(ArgumentException))]
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzConnection ConnectOverPtt(string project, string vehicleInfo, PsdzBus bus, string baureihe, string bauIstufe);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		void SetProdiasLogLevel(ProdiasLoglevel prodiasLoglevel);

		[OperationContract]
		void RequestShutdown();

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		int GetHttpServerPort();
	}
}
