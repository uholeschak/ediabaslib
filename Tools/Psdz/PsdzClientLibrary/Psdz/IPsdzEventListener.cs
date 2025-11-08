using System.ServiceModel;
using BMW.Rheingold.Psdz.Model.Events;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzEventListener
    {
        // [UH] Keep operation contract for backward compatibility
        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(PsdzEvent))]
        [ServiceKnownType(typeof(PsdzProgressEvent))]
        [ServiceKnownType(typeof(PsdzTransactionEvent))]
        [ServiceKnownType(typeof(PsdzTransactionProgressEvent))]
        [ServiceKnownType(typeof(PsdzMcdDiagServiceEvent))]
        void SetPsdzEvent(IPsdzEvent psdzEvent);
    }
}
