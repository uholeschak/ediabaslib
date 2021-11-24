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
    [ServiceKnownType(typeof(PsdzTargetSelector))]
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IConnectionFactoryService
    {
        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IEnumerable<IPsdzTargetSelector> GetTargetSelectors();
    }
}
