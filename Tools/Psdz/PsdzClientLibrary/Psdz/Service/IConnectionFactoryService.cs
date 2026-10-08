using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RheingoldPsdzWebApi.Adapter.Contracts.DomainObjects;
using RheingoldPsdzWebApi.Adapter.Contracts.Model;

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
