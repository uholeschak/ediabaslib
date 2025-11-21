using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.SecureCoding;
using BMW.Rheingold.Psdz.Model.Sfa;
using BMW.Rheingold.Psdz.Model.Sfa.RequestNcdSignatureResponseCto;
using BMW.Rheingold.Psdz.Model.Tal;
using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal sealed class SecureCodingServiceClient : PsdzDuplexClientBase<ISecureCodingService, IPsdzProgressListener>, ISecureCodingService
    {
        internal SecureCodingServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress)
            : base(progressListener, binding, remoteAddress)
        {
        }

        public IPsdzCheckNcdResultEto CheckNcdAvailabilityForGivenTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin)
        {
            return CallFunction((ISecureCodingService service) => service.CheckNcdAvailabilityForGivenTal(tal, ncdDirectory, vin));
        }

        public IList<IPsdzSecurityBackendRequestFailureCto> RequestCalculationNcdAndSignatureOffline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, string jsonRequestFilePath, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin, IPsdzFa fa, byte[] vpc)
        {
            return CallFunction((ISecureCodingService service) => service.RequestCalculationNcdAndSignatureOffline(sgbmidsForNcdCalculation, jsonRequestFilePath, secureCodingConfigCto, vin, fa, vpc));
        }

        public IPsdzRequestNcdSignatureResponseCto RequestSignatureOnline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin)
        {
            return CallFunction((ISecureCodingService service) => service.RequestSignatureOnline(sgbmidsForNcdCalculation, secureCodingConfigCto, vin));
        }

        public IPsdzNcd ReadNcdFromFile(string ncdDirectory, IPsdzVin vin, IPsdzSgbmId cafdSgbmid, string btldSgbmNumber)
        {
            return CallFunction((ISecureCodingService service) => service.ReadNcdFromFile(ncdDirectory, vin, cafdSgbmid, btldSgbmNumber));
        }
    }
}
