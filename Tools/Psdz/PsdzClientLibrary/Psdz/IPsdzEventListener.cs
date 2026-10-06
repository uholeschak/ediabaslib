using PsdzClient;
using System.ServiceModel;
using RheingoldPsdzWebApi.Adapter.Contracts.Model.Events;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface IPsdzEventListener
    {
        [PreserveSource(KeepAttribute = true)]
        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(PsdzEvent))]
        [ServiceKnownType(typeof(PsdzProgressEvent))]
        [ServiceKnownType(typeof(PsdzTransactionEvent))]
        [ServiceKnownType(typeof(PsdzTransactionProgressEvent))]
        [ServiceKnownType(typeof(PsdzMcdDiagServiceEvent))]
        void SetPsdzEvent(IPsdzEvent psdzEvent);
    }
}
