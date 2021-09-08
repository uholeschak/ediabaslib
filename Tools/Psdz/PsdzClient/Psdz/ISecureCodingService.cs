using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzCodingTypeEnum
    {
        FA,
        FDL,
        NCD,
        NCD_PROVIDED,
        SHIPMENT,
        FWL
    }

	[ServiceKnownType(typeof(PsdzTal))]
	[ServiceKnownType(typeof(PsdzCalculationNcdResultCto))]
	[ServiceKnownType(typeof(PsdzSecureCodingConfigCto))]
	[ServiceKnownType(typeof(PsdzRequestNcdEto))]
	[ServiceKnownType(typeof(PsdzCheckNcdResultEto))]
	[ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
	[ServiceKnownType(typeof(PsdzVin))]
	[ServiceKnownType(typeof(PsdzRequestNcdSignatureResponseCto))]
	[ServiceKnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
	[ServiceKnownType(typeof(PsdzFa))]
	[ServiceKnownType(typeof(PsdzNcd))]
	[ServiceKnownType(typeof(PsdzSgbmId))]
	public interface ISecureCodingService
	{
		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzCheckNcdResultEto CheckNcdAvailabilityForGivenTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzCheckNcdAvailabilityResultCto CheckNcdAvailabilityForTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzCalculationNcdResultCto RequestCalculationNcd(IList<IPsdzRequestNcdEto> cafsForNcdCalculationEto, IPsdzFa fa, IPsdzSecureCodingConfigCto secureCodingConfigCto, PsdzCodingTypeEnum codingType);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IPsdzNcd ReadNcdFromFile(string ncdDirectory, IPsdzVin vin, IPsdzSgbmId cafdSgbmid, string btldSgbmNumber);

		[OperationContract]
		[FaultContract(typeof(PsdzRuntimeException))]
		IList<IPsdzSecurityBackendRequestFailureCto> RequestCalculationNcdAndSignatureOffline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, string jsonRequestFilePath, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin, IPsdzFa fa);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		IPsdzRequestNcdSignatureResponseCto RequestSignatureOnline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin);

		[FaultContract(typeof(PsdzRuntimeException))]
		[OperationContract]
		bool SaveNCD(IPsdzNcd ncd, string btldSgbmNumber, IPsdzSgbmId cafdSgbmid, string ncdDirectory, IPsdzVin vin);
	}
}
