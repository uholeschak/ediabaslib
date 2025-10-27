using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzTargetSelector))]
    public interface IConnectionFactoryService
    {
        IEnumerable<VehicleId> RequestAvailableVehicles();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzTargetSelector> GetTargetSelectors();
    }
}
