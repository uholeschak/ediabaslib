using BMW.Rheingold.Psdz.Model;
using PsdzClientLibrary;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace BMW.Rheingold.Psdz.Client
{
    [PreserveSource(Removed = true)]
    internal sealed class ConnectionFactoryServiceClient : PsdzClientBase<IConnectionFactoryService>, IConnectionFactoryService
    {
        internal ConnectionFactoryServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public IEnumerable<VehicleId> RequestAvailableVehicles()
        {
            return CallFunction((IConnectionFactoryService m) => m.RequestAvailableVehicles());
        }

        public IEnumerable<IPsdzTargetSelector> GetTargetSelectors()
        {
            return CallFunction((IConnectionFactoryService m) => m.GetTargetSelectors());
        }
    }
}
