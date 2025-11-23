using BMW.Rheingold.Psdz.Model.Events;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.ServiceModel;

namespace BMW.Rheingold.Psdz
{
    [PreserveSource(AttributesModified = true)]
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IPsdzEventListener))]
    public interface IEventManagerService
    {
        bool Listening { get; }

        void PrepareListening();
        IConnectionLossEventListener AddPsdzEventListenerForConnectionLoss();
        void RemovePsdzEventListenerForConnectionLoss();
        void SendInternalEvent(IPsdzEvent psdzEvent);
        void AddEventListener(IPsdzEventListener psdzEventListener);
        void RemoveEventListener(IPsdzEventListener psdzEventListener);
        void RemoveAllEventListeners();
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StartListening();
        [PreserveSource(KeepAttribute = true)]
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        void StopListening();
    }
}