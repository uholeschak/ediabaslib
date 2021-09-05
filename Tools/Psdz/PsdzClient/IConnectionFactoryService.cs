using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
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
