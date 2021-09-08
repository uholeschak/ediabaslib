using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzKdsActionIdEto
    {
        CHECK_PARING_CONSISTENCY,
        CUT_COMMUNICATION,
        LOCK_ECU,
        REPAIR_OR_CLEAR_DATA,
        SET_OPERATION_MODE,
        SHOW_REACTION,
        TEST_SIGNATURE,
        TRIGGER_FREE_PAIRING,
        TRIGGER_INDIVIDUALIZATION,
        TRIGGER_VERIFICATION
    }

	class KdsServiceClient : PsdzDuplexClientBase<IKdsService, IPsdzProgressListener>, IKdsService
	{
		internal KdsServiceClient(IPsdzProgressListener progressListener, Binding binding, EndpointAddress remoteAddress) : base(progressListener, binding, remoteAddress)
		{
		}

		public IPsdzKdsClientsForRefurbishResultCto GetKdsClientsForRefurbish(IPsdzConnection connection, int retries, int timeBetweenRetries)
		{
			return base.CallFunction<IPsdzKdsClientsForRefurbishResultCto>((IKdsService service) => service.GetKdsClientsForRefurbish(connection, retries, timeBetweenRetries));
		}

		public IPsdzPerformQuickKdsCheckResultCto PerformQuickKdsCheck(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries)
		{
			return base.CallFunction<IPsdzPerformQuickKdsCheckResultCto>((IKdsService service) => service.PerformQuickKdsCheck(connection, kdsId, retries, timeBetweenRetries));
		}

		public IPsdzKdsActionStatusResultCto PerformRefurbishProcess(IPsdzConnection connection, IPsdzKdsIdCto kdsId, IPsdzSecureTokenEto secureToken, PsdzKdsActionIdEto psdzKdsActionId, int retries, int timeBetweenRetries)
		{
			return base.CallFunction<IPsdzKdsActionStatusResultCto>((IKdsService service) => service.PerformRefurbishProcess(connection, kdsId, secureToken, psdzKdsActionId, retries, timeBetweenRetries));
		}

		public IPsdzReadPublicKeyResultCto ReadPublicKey(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries)
		{
			return base.CallFunction<IPsdzReadPublicKeyResultCto>((IKdsService service) => service.ReadPublicKey(connection, kdsId, retries, timeBetweenRetries));
		}

		public IPsdzKdsActionStatusResultCto SwitchOnComponentTheftProtection(IPsdzConnection connection, IPsdzKdsIdCto kdsId, int retries, int timeBetweenRetries)
		{
			return base.CallFunction<IPsdzKdsActionStatusResultCto>((IKdsService service) => service.SwitchOnComponentTheftProtection(connection, kdsId, retries, timeBetweenRetries));
		}
	}
}
