using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.SecureCoding;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Sfa.RequestNcdSignatureResponseCto;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Tal;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
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
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzCheckNcdResultEto CheckNcdAvailabilityForGivenTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzNcd ReadNcdFromFile(string ncdDirectory, IPsdzVin vin, IPsdzSgbmId cafdSgbmid, string btldSgbmNumber);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IList<IPsdzSecurityBackendRequestFailureCto> RequestCalculationNcdAndSignatureOffline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, string jsonRequestFilePath, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin, IPsdzFa fa, byte[] vpc);

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IPsdzRequestNcdSignatureResponseCto RequestSignatureOnline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin);
    }
}
