using System.Collections.Generic;
using System.ServiceModel;
using BMW.Rheingold.Psdz.Model;
using BMW.Rheingold.Psdz.Model.Ecu;
using BMW.Rheingold.Psdz.Model.Exceptions;

namespace BMW.Rheingold.Psdz
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceKnownType(typeof(PsdzSgbmId))]
    [ServiceKnownType(typeof(PsdzEcuIdentifier))]
    [ServiceKnownType(typeof(PsdzFa))]
    [ServiceKnownType(typeof(PsdzIstufe))]
    [ServiceKnownType(typeof(PsdzConnection))]
    public interface IMacrosService
    {
        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzSgbmId> CheckSoftwareEntries(IEnumerable<IPsdzSgbmId> sgbmIds);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuList(IPsdzFa fa, IPsdzIstufe iStufe);

        [OperationContract]
        [FaultContract(typeof(PsdzRuntimeException))]
        IEnumerable<IPsdzEcuIdentifier> GetInstalledEcuListWithConnection(IPsdzConnection connection, IPsdzFa fa, IPsdzIstufe iStufe);
    }
}
