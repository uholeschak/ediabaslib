using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PsdzClient
{
    class ConnectionFactoryServiceClient : PsdzClientBase<IConnectionFactoryService>, IConnectionFactoryService
    {
        // Token: 0x06000160 RID: 352 RVA: 0x00002DA0 File Offset: 0x00000FA0
        internal ConnectionFactoryServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        // Token: 0x06000161 RID: 353 RVA: 0x00002DAA File Offset: 0x00000FAA
        public IEnumerable<IPsdzTargetSelector> GetTargetSelectors()
        {
            return base.CallFunction<IEnumerable<IPsdzTargetSelector>>((IConnectionFactoryService m) => m.GetTargetSelectors());
        }
    }
}
