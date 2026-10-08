using BMW.Rheingold.CoreFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using RheingoldPsdzWebApi.Adapter.Contracts;
using RheingoldPsdzWebApi.Adapter.Contracts.DomainObjects;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Services;
using RheingoldPsdzWebApi.Adapter.Contracts.TransferObjects;
using RheingoldPsdzWebApi.Adapter.Mapper;

namespace RheingoldPsdzWebApi.Adapter.Services
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
                return _webCallHandler.ExecuteRequest<IList<TargetSelectorModel>>(_endpointService, "gettargetselectors", HttpMethod.Get).Data?.Select(TargetSelectorMapper.Map);
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
                return _webCallHandler.ExecuteRequest<IList<VehicleIdModel>>(_endpointService, "requestavailablevehicles", HttpMethod.Get).Data?.Select(VehicleIdMapper.Map);
            }
            catch (Exception exception)
            {
                Log.ErrorException(Log.CurrentMethod(), exception);
                throw;
            }
        }
    }
}