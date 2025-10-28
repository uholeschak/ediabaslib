using BMW.Rheingold.Psdz.Model.Events;
using BMW.Rheingold.Psdz.Model.Exceptions;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzEventListener))]
    public interface IEventManagerService
    {
        bool Listening { get; }

        void PrepareListening();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StartListening();

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StopListening();

        IConnectionLossEventListener AddPsdzEventListenerForConnectionLoss();

        void RemovePsdzEventListenerForConnectionLoss();

        void SendInternalEvent(IPsdzEvent psdzEvent);

        void AddEventListener(IPsdzEventListener psdzEventListener);

        void RemoveEventListener(IPsdzEventListener psdzEventListener);

        void RemoveAllEventListeners();
    }
}
