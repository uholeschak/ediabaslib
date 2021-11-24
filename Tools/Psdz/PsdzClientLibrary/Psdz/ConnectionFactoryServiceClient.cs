using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz.Client
{
    class ConnectionFactoryServiceClient : PsdzClientBase<IConnectionFactoryService>, IConnectionFactoryService
    {
        public ConnectionFactoryServiceClient(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public IEnumerable<IPsdzTargetSelector> GetTargetSelectors()
        {
            return base.CallFunction<IEnumerable<IPsdzTargetSelector>>((IConnectionFactoryService m) => m.GetTargetSelectors());
        }
    }
}
