using System.ServiceModel;
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
