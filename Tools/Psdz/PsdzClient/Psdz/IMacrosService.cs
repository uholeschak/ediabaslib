using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [ServiceKnownType(typeof(PsdzIstufe))]
    [ServiceKnownType(typeof(PsdzSgbmId))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IMacrosService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSgbmId> CheckSoftwareEntries(IEnumerable<IPsdzSgbmId> sgbmIds);

        [FaultContract(typeof(PsdzRuntimeException))]
        [OperationContract]
        IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuList(IPsdzFa fa, IPsdzIstufe iStufe);
    }
}
