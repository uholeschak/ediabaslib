using PsdzClient;
using System.Collections.Generic;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts.DomainObjects;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Exceptions;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzTargetSelector))]
    public interface IConnectionFactoryService
    {
        IEnumerable<VehicleId> RequestAvailableVehicles();

        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzTargetSelector> GetTargetSelectors();
    }
}
