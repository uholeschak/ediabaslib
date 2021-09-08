using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public interface IPsdzEventListener
    {
        [ServiceKnownType(typeof(PsdzEvent))]
        [ServiceKnownType(typeof(PsdzProgressEvent))]
        [ServiceKnownType(typeof(PsdzTransactionEvent))]
        [ServiceKnownType(typeof(PsdzMcdDiagServiceEvent))]
        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(PsdzTransactionProgressEvent))]
        void SetPsdzEvent(IPsdzEvent psdzEvent);
    }
}
