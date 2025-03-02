using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzEventListener))]
    public interface IEventManagerService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StartListening();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StopListening();
    }
}
