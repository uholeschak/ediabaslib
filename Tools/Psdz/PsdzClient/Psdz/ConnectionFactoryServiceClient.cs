using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace PsdzClient.Psdz
{
    class ConnectionFactoryServiceClient : PsdzClientBase<IConnectionFactoryService>, IConnectionFactoryService
    {
        internal ConnectionFactoryServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public IEnumerable<IPsdzTargetSelector> GetTargetSelectors()
        {
            return base.CallFunction<IEnumerable<IPsdzTargetSelector>>((IConnectionFactoryService m) => m.GetTargetSelectors());
        }
    }
}
