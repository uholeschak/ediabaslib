using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzEventListener))]
    public interface IEventManagerService
    {
        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        void StartListening();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StopListening();
    }
}
