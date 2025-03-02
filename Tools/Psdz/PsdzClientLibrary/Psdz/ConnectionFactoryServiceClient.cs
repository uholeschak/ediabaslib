using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using BMW.Rheingold.Psdz.Model;

namespace BMW.Rheingold.Psdz.Client
{
    internal sealed class ConnectionFactoryServiceClient : PsdzClientBase<IConnectionFactoryService>, IConnectionFactoryService
    {
        internal ConnectionFactoryServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IEnumerable<IPsdzTargetSelector> GetTargetSelectors()
        {
            return CallFunction((IConnectionFactoryService m) => m.GetTargetSelectors());
        }
    }
}
