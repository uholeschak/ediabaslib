using BMW.Rheingold.Psdz.Model.Events;
using BMW.Rheingold.Psdz.Model.Exceptions;
using PsdzClient;
using System.ServiceModel;
using BMW.Rheingold.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace RheingoldPsdzWebApi.Adapter.Contracts.Services
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