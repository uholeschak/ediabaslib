using BMW.Rheingold.Psdz.Model.Events;
using PsdzClient;
using System.ServiceModel;

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
