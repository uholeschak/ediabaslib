using System;
using BMW.Rheingold.Psdz;
using BMW.Rheingold.Psdz.Model;
using PsdzClient.Core;
using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace BMW.Rheingold.Psdz
{
    internal class ConnectionFactoryService : IConnectionFactoryService
    {
        private readonly IWebCallHandler _webCallHandler;
        private readonly string _endpointService = "connectionfactory";
        public ConnectionFactoryService(IWebCallHandler webCallHandler)
        {
            _webCallHandler = webCallHandler;
        }

        public IEnumerable<IPsdzTargetSelector> GetTargetSelectors()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<IList<TargetSelectorModel>>(_endpointService, "gettargetselectors", Method.Get).Data?.Select(TargetSelectorMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }

        public IEnumerable<VehicleId> RequestAvailableVehicles()
        {
            try
            {
                return _webCallHandler.ExecuteRequest<IList<VehicleIdModel>>(_endpointService, "requestavailablevehicles", Method.Get).Data?.Select(VehicleIdMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}