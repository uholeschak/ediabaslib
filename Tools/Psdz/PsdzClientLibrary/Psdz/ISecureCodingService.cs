using System.Collections.Generic;
using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.RequestNcdSignatureResponseCto;
using BMW.Rheingold.Psdz.Model.Tal;

namespace BMW.Rheingold.Psdz
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

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzProgressListener))]
    [ServiceKnownType(typeof(PsdzCheckNcdResultEto))]
    [ServiceKnownType(typeof(PsdzTal))]
    [ServiceKnownType(typeof(PsdzVin))]
    [ServiceKnownType(typeof(PsdzRequestNcdEto))]
    [ServiceKnownType(typeof(PsdzCalculationNcdResultCto))]
    [ServiceKnownType(typeof(PsdzSecureCodingConfigCto))]
    [ServiceKnownType(typeof(PsdzSecurityBackendRequestFailureCto))]
    [ServiceKnownType(typeof(PsdzRequestNcdSignatureResponseCto))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzNcd))]
    [ServiceKnownType(typeof(PsdzSgbmId))]
    public interface ISecureCodingService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzCheckNcdResultEto CheckNcdAvailabilityForGivenTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzNcd ReadNcdFromFile(string ncdDirectory, IPsdzVin vin, IPsdzSgbmId cafdSgbmid, string btldSgbmNumber);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IList<IPsdzSecurityBackendRequestFailureCto> RequestCalculationNcdAndSignatureOffline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, string jsonRequestFilePath, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin, IPsdzFa fa);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzRequestNcdSignatureResponseCto RequestSignatureOnline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin);
    }
}
