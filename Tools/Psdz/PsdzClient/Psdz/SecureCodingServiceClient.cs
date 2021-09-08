using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
	class SecureCodingServiceClient : PsdzDuplexClientBase<ISecureCodingService, IPsdzProgressListener>, ISecureCodingService
	{
		internal SecureCodingServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress) : base(progressListener, binding, remoteAddress)
		{
		}

		public IPsdzCheckNcdResultEto CheckNcdAvailabilityForGivenTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin)
		{
			return base.CallFunction<IPsdzCheckNcdResultEto>((ISecureCodingService service) => service.CheckNcdAvailabilityForGivenTal(tal, ncdDirectory, vin));
		}

		public IPsdzCheckNcdAvailabilityResultCto CheckNcdAvailabilityForTal(IPsdzTal tal, string ncdDirectory, IPsdzVin vin)
		{
			return base.CallFunction<IPsdzCheckNcdAvailabilityResultCto>((ISecureCodingService service) => service.CheckNcdAvailabilityForTal(tal, ncdDirectory, vin));
		}

		public IPsdzCalculationNcdResultCto RequestCalculationNcd(IList<IPsdzRequestNcdEto> cafsForNcdCalculationEto, IPsdzFa fa, IPsdzSecureCodingConfigCto secureCodingConfigCto, PsdzCodingTypeEnum codingType)
		{
			return base.CallFunction<IPsdzCalculationNcdResultCto>((ISecureCodingService service) => service.RequestCalculationNcd(cafsForNcdCalculationEto, fa, secureCodingConfigCto, codingType));
		}

		public IList<IPsdzSecurityBackendRequestFailureCto> RequestCalculationNcdAndSignatureOffline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, string jsonRequestFilePath, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin, IPsdzFa fa)
		{
			return base.CallFunction<IList<IPsdzSecurityBackendRequestFailureCto>>((ISecureCodingService service) => service.RequestCalculationNcdAndSignatureOffline(sgbmidsForNcdCalculation, jsonRequestFilePath, secureCodingConfigCto, vin, fa));
		}

		public IPsdzRequestNcdSignatureResponseCto RequestSignatureOnline(IList<IPsdzRequestNcdEto> sgbmidsForNcdCalculation, IPsdzSecureCodingConfigCto secureCodingConfigCto, IPsdzVin vin)
		{
			return base.CallFunction<IPsdzRequestNcdSignatureResponseCto>((ISecureCodingService service) => service.RequestSignatureOnline(sgbmidsForNcdCalculation, secureCodingConfigCto, vin));
		}

		public bool SaveNCD(IPsdzNcd ncd, string btldSgbmNumber, IPsdzSgbmId cafdSgbmid, string ncdDirectory, IPsdzVin vin)
		{
			return base.CallFunction<bool>((ISecureCodingService service) => service.SaveNCD(ncd, btldSgbmNumber, cafdSgbmid, ncdDirectory, vin));
		}

		public IPsdzNcd ReadNcdFromFile(string ncdDirectory, IPsdzVin vin, IPsdzSgbmId cafdSgbmid, string btldSgbmNumber)
		{
			return base.CallFunction<IPsdzNcd>((ISecureCodingService service) => service.ReadNcdFromFile(ncdDirectory, vin, cafdSgbmid, btldSgbmNumber));
		}
	}
}
